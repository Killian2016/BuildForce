#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class ProjectPhotosPage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private int _selectedProjectId = 0;
    private bool _ready = false;

    // Viewer state [PP2]
    private ProjectPhoto? _viewerPhoto;
    private readonly Dictionary<int, byte[]> _imageCache = new();
    private double _vScale = 1, _vStartScale = 1;
    private double _vX = 0, _vY = 0, _vStartX = 0, _vStartY = 0;

    public ProjectPhotosPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        InitAsync();
    }

    private async void InitAsync()
    {
        SetStatus(null);
        SetLoading(true);

        // Load the project list and the active (clocked-in) timesheet in parallel.
        var projectsTask = _api.GetProjectsAsync();
        var activeTask = _api.GetActiveTimesheetAsync();
        await Task.WhenAll(projectsTask, activeTask);

        _projects = projectsTask.Result ?? new List<ProjectSummary>();
        var active = activeTask.Result;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProjectPicker.Items.Clear();
            foreach (var p in _projects)
                ProjectPicker.Items.Add(p.Name ?? ("Project " + p.Id));

            if (_projects.Count == 0)
            {
                SubLabel.Text = "No projects found";
                CaptureBtn.IsEnabled = false;
                SetLoading(false);
                ShowEmpty("Create a project on Mezano CM first.");
                _ready = true;
                return;
            }

            // Default to the active clocked-in project if we have one, else first.
            int defaultIndex = 0;
            if (active != null && active.ProjectId > 0)
            {
                var idx = _projects.FindIndex(p => p.Id == active.ProjectId);
                if (idx >= 0) defaultIndex = idx;
            }

            _ready = true;
            ProjectPicker.SelectedIndex = defaultIndex; // fires OnProjectChanged -> loads photos
        });
    }

    private void OnProjectChanged(object sender, EventArgs e)
    {
        if (!_ready) return;
        var idx = ProjectPicker.SelectedIndex;
        if (idx < 0 || idx >= _projects.Count) return;

        _selectedProjectId = _projects[idx].Id;
        var name = _projects[idx].Name ?? ("Project " + _selectedProjectId);
        HeaderLabel.Text = name;
        SubLabel.Text = "Site photos [v21]";
        LoadPhotos();
    }

    private async void LoadPhotos()
    {
        if (_selectedProjectId <= 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(true);
            EmptyLabel.IsVisible = false;
            PhotoList.Children.Clear();
        });

        var photos = await _api.GetProjectPhotosAsync(_selectedProjectId);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(false);

            if (photos == null)
            {
                ShowEmpty(_api.LastError ?? "Could not load photos.");
                return;
            }
            if (photos.Count == 0)
            {
                ShowEmpty("No photos yet. Tap Take photo to add the first.");
                return;
            }

            foreach (var ph in photos)
                PhotoList.Children.Add(BuildPhotoCard(ph));
        });
    }

    private View BuildPhotoCard(ProjectPhoto ph)
    {
        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#0d1117"),
            Stroke = Color.FromArgb("#1c2330"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = 0
        };

        var stack = new VerticalStackLayout { Spacing = 0 };

        var img = new Image
        {
            Aspect = Aspect.AspectFill,
            HeightRequest = 220,
            BackgroundColor = Color.FromArgb("#080b10")
        };
        stack.Children.Add(img);
        LoadCardImage(img, ph.Id);

        var meta = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(14, 12) };

        if (!string.IsNullOrWhiteSpace(ph.Category))
        {
            meta.Children.Add(new Label
            {
                Text = ph.Category!.ToUpperInvariant(),
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0ea5e9"),
                CharacterSpacing = 1.5
            });
        }

        var desc = !string.IsNullOrWhiteSpace(ph.Caption) ? ph.Caption
                 : !string.IsNullOrWhiteSpace(ph.AIDescription) ? ph.AIDescription
                 : "Site photo";
        meta.Children.Add(new Label
        {
            Text = desc,
            FontSize = 13,
            TextColor = Color.FromArgb("#e6edf3")
        });

        var by = string.IsNullOrWhiteSpace(ph.TakenByName) ? "" : ph.TakenByName + "  \u2022  ";
        meta.Children.Add(new Label
        {
            Text = by + ph.CreatedDate.ToLocalTime().ToString("MMM d, h:mm tt"),
            FontSize = 11,
            TextColor = Color.FromArgb("#7d8590")
        });

        stack.Children.Add(meta);
        border.Content = stack;

        // Tap card -> full-screen viewer [PP2]
        var phLocal = ph;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => OpenViewer(phLocal);
        border.GestureRecognizers.Add(tap);

        return border;
    }

    private async void LoadCardImage(Image img, int photoId)
    {
        var bytes = await GetImageBytesAsync(photoId);
        if (bytes == null || bytes.Length == 0) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            img.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        });
    }

    private async Task<byte[]?> GetImageBytesAsync(int photoId)
    {
        if (_imageCache.TryGetValue(photoId, out var cached)) return cached;
        var bytes = await _api.GetProjectPhotoImageAsync(photoId);
        if (bytes != null && bytes.Length > 0) _imageCache[photoId] = bytes;
        return bytes;
    }

    // ===================== Viewer [PP2] =====================

    private void OpenViewer(ProjectPhoto ph)
    {
        _viewerPhoto = ph;
        ResetViewerTransform();

        var desc = !string.IsNullOrWhiteSpace(ph.Caption) ? ph.Caption
                 : !string.IsNullOrWhiteSpace(ph.AIDescription) ? ph.AIDescription
                 : "Site photo";
        var by = string.IsNullOrWhiteSpace(ph.TakenByName) ? "" : ph.TakenByName + "  \u2022  ";
        ViewerCaption.Text = desc + "\n" + by + ph.CreatedDate.ToLocalTime().ToString("MMM d, h:mm tt");

        if (_imageCache.TryGetValue(ph.Id, out var bytes))
        {
            ViewerImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        else
        {
            ViewerImage.Source = null;
            LoadViewerImage(ph.Id);
        }

        ViewerOverlay.IsVisible = true;
    }

    private async void LoadViewerImage(int photoId)
    {
        var bytes = await GetImageBytesAsync(photoId);
        if (bytes == null || bytes.Length == 0) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewerPhoto != null && _viewerPhoto.Id == photoId)
                ViewerImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        });
    }

    private void ResetViewerTransform()
    {
        _vScale = 1; _vX = 0; _vY = 0;
        ViewerImage.Scale = 1;
        ViewerImage.TranslationX = 0;
        ViewerImage.TranslationY = 0;
    }

    private void OnViewerPinch(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _vStartScale = _vScale;
        }
        else if (e.Status == GestureStatus.Running)
        {
            _vScale = Math.Clamp(_vStartScale * e.Scale, 1.0, 5.0);
            ViewerImage.Scale = _vScale;
            if (_vScale <= 1.01)
            {
                _vX = 0; _vY = 0;
                ViewerImage.TranslationX = 0;
                ViewerImage.TranslationY = 0;
            }
        }
    }

    private void OnViewerPan(object sender, PanUpdatedEventArgs e)
    {
        if (_vScale <= 1.01) return;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _vStartX = _vX; _vStartY = _vY;
                break;
            case GestureStatus.Running:
                var maxX = Math.Max(0, ViewerImage.Width * (_vScale - 1) / 2);
                var maxY = Math.Max(0, ViewerImage.Height * (_vScale - 1) / 2);
                _vX = Math.Clamp(_vStartX + e.TotalX, -maxX, maxX);
                _vY = Math.Clamp(_vStartY + e.TotalY, -maxY, maxY);
                ViewerImage.TranslationX = _vX;
                ViewerImage.TranslationY = _vY;
                break;
        }
    }

    private void OnViewerDoubleTap(object sender, TappedEventArgs e)
    {
        if (_vScale > 1.01)
        {
            ResetViewerTransform();
        }
        else
        {
            _vScale = 2.5;
            ViewerImage.Scale = 2.5;
        }
    }

    private void OnViewerClose(object sender, EventArgs e)
    {
        ViewerOverlay.IsVisible = false;
        _viewerPhoto = null;
    }

    private async void OnViewerDelete(object sender, EventArgs e)
    {
        if (_viewerPhoto == null) return;

        var sure = await ConfirmAsync("Delete photo",
            "This removes the photo for everyone on the project. Are you sure?",
            "Delete", "Cancel");
        if (!sure) return;

        ViewerDeleteBtn.IsEnabled = false;
        var error = await _api.DeleteProjectPhotoAsync(_viewerPhoto.Id);
        ViewerDeleteBtn.IsEnabled = true;

        if (error != null)
        {
            await AlertAsync("Delete failed", error);
            return;
        }

        _imageCache.Remove(_viewerPhoto.Id);
        ViewerOverlay.IsVisible = false;
        _viewerPhoto = null;
        LoadPhotos();
    }

    protected override bool OnBackButtonPressed()
    {
        if (ViewerOverlay.IsVisible)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ViewerOverlay.IsVisible = false;
                _viewerPhoto = null;
            });
            return true;
        }
        return base.OnBackButtonPressed();
    }

    // ===================== Capture (unchanged from v20) =====================

    private async void OnCapture(object sender, EventArgs e)
    {
        if (_selectedProjectId <= 0)
        {
            await AlertAsync("No project", "Pick a project first.");
            return;
        }

        // Native camera app capture (reliable exposure on this device), then 800px downscale + EXIF fix.
        string? base64 = await CapturePhotoAsync("SITE PHOTO");
        if (string.IsNullOrEmpty(base64)) return;

        // Best-effort GPS stamp (never blocks the upload).
        double? lat = null, lng = null;
        try
        {
            var loc = await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLastKnownLocationAsync();
            if (loc != null) { lat = loc.Latitude; lng = loc.Longitude; }
        }
        catch { }

        SetStatus("Uploading photo...");
        CaptureBtn.IsEnabled = false;

        var result = await _api.UploadProjectPhotoAsync(
            _selectedProjectId, base64, caption: null, category: null,
            analyzeWithAI: true, latitude: lat, longitude: lng);

        CaptureBtn.IsEnabled = true;

        if (result == null)
        {
            SetStatus(null);
            await AlertAsync("Upload failed", _api.LastError ?? "Could not upload the photo.");
            return;
        }

        SetStatus("Photo added");
        await Task.Delay(1200);
        SetStatus(null);
        LoadPhotos();
    }

    private async Task<string?> CapturePhotoAsync(string label)
    {
        try
        {
            if (!Microsoft.Maui.Media.MediaPicker.Default.IsCaptureSupported)
            {
                await AlertAsync("No camera", "Photo capture is not supported on this device.");
                return null;
            }

            var shot = await Microsoft.Maui.Media.MediaPicker.Default.CapturePhotoAsync(
                new Microsoft.Maui.Media.MediaPickerOptions { Title = label });
            if (shot == null) return null; // user cancelled

            byte[] bytes;
            using (var src = await shot.OpenReadAsync())
            using (var ms = new MemoryStream())
            {
                await src.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            if (bytes.Length == 0) return null;

            // Same downscale + EXIF-bake as clock-in selfies.
            try { bytes = PunchCameraPage.NormalizePhoto(bytes, 800, 75); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Normalize failed: " + ex.Message); }

            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Photo capture error: " + ex.Message);
            await AlertAsync("Camera error", ex.Message);
            return null;
        }
    }

    private void SetLoading(bool on)
    {
        Loading.IsRunning = on;
        Loading.IsVisible = on;
    }

    private void ShowEmpty(string text)
    {
        EmptyLabel.Text = text;
        EmptyLabel.IsVisible = true;
    }

    private void SetStatus(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            StatusLabel.IsVisible = false;
            StatusLabel.Text = "";
        }
        else
        {
            StatusLabel.Text = text;
            StatusLabel.IsVisible = true;
        }
    }

    private static async Task AlertAsync(string title, string message)
    {
        var host = Application.Current?.MainPage;
        if (host != null)
            await host.DisplayAlert(title, message, "OK");
    }

    private static async Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        var host = Application.Current?.MainPage;
        if (host == null) return false;
        return await host.DisplayAlert(title, message, accept, cancel);
    }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}
