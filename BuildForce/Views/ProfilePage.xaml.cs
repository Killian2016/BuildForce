#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;

    public ProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Load();
    }

    private async void Load()
    {
        Loading.IsRunning = true; Loading.IsVisible = true;
        var p = await _api.GetMyProfileAsync();
        Loading.IsRunning = false; Loading.IsVisible = false;

        if (p == null)
        {
            HeaderLabel.Text = "Unavailable";
            InitialsLabel.Text = "!";
            AddInfo("", _api.LastError ?? "Could not load your profile.");
            return;
        }

        HeaderLabel.Text = p.FullName ?? "My Profile";
        InitialsLabel.Text = Initials(p.FullName);

        InfoStack.Children.Clear();
        AddInfo("POSITION", p.Position);
        AddInfo("EMAIL", p.Email);
        AddInfo("PHONE", p.Phone);

        if (p.HasPhoto) await LoadPhoto();
    }

    private async Task LoadPhoto()
    {
        var bytes = await _api.GetProfilePhotoImageAsync();
        if (bytes == null || bytes.Length == 0) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PhotoImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            PhotoImage.IsVisible = true;
            InitialsLabel.IsVisible = false;
        });
    }

    private void AddInfo(string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!string.IsNullOrEmpty(label))
            InfoStack.Children.Add(new Label { Text = label, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#7d8590"), CharacterSpacing = 1.5 });
        InfoStack.Children.Add(new Label { Text = value, FontSize = 14, TextColor = Color.FromArgb("#e6edf3"), Margin = new Thickness(0,0,0,4) });
    }

    private static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0,1) + parts[parts.Length-1].Substring(0,1)).ToUpper();
    }

    private async void OnCamera(object sender, EventArgs e) => await PickAndUpload(true);
    private async void OnGallery(object sender, EventArgs e) => await PickAndUpload(false);

    private async Task PickAndUpload(bool useCamera)
    {
        try
        {
            FileResult? shot = null;
            if (useCamera)
            {
                if (!Microsoft.Maui.Media.MediaPicker.Default.IsCaptureSupported)
                { await Alert("No camera", "Camera not supported on this device."); return; }
                shot = await Microsoft.Maui.Media.MediaPicker.Default.CapturePhotoAsync(new Microsoft.Maui.Media.MediaPickerOptions { Title = "PROFILE PHOTO" });
            }
            else
            {
                shot = await Microsoft.Maui.Media.MediaPicker.Default.PickPhotoAsync(new Microsoft.Maui.Media.MediaPickerOptions { Title = "PROFILE PHOTO" });
            }
            if (shot == null) return;

            byte[] bytes;
            using (var src = await shot.OpenReadAsync())
            using (var ms = new MemoryStream())
            {
                await src.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            if (bytes.Length == 0) return;
            try { bytes = PunchCameraPage.NormalizePhoto(bytes, 800, 75); } catch { }
            var b64 = Convert.ToBase64String(bytes);

            StatusLabel.Text = "Uploading..."; StatusLabel.IsVisible = true;
            CameraBtn.IsEnabled = false; GalleryBtn.IsEnabled = false;
            var ok = await _api.UploadProfilePhotoAsync(b64);
            CameraBtn.IsEnabled = true; GalleryBtn.IsEnabled = true;

            if (!ok)
            {
                StatusLabel.IsVisible = false;
                await Alert("Upload failed", _api.LastError ?? "Could not upload the photo.");
                return;
            }

            PhotoImage.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(b64)));
            PhotoImage.IsVisible = true; InitialsLabel.IsVisible = false;
            StatusLabel.Text = "Photo updated";
            await Task.Delay(1200);
            StatusLabel.IsVisible = false;
        }
        catch (Exception ex)
        {
            CameraBtn.IsEnabled = true; GalleryBtn.IsEnabled = true;
            await Alert("Error", ex.Message);
        }
    }

    private static async Task Alert(string t, string m)
    {
        var h = Application.Current?.MainPage;
        if (h != null) await h.DisplayAlert(t, m, "OK");
    }

    // [DISC1] Play requires the privacy policy and a deletion path to stay
    // reachable inside the app, not only on the one-time disclosure screen.
    private async void OnPrivacy(object sender, EventArgs e)
    { try { await Launcher.OpenAsync("https://mezanocm.com/privacy"); } catch { } }

    private async void OnTerms(object sender, EventArgs e)
    { try { await Launcher.OpenAsync("https://mezanocm.com/terms"); } catch { } }

    private async void OnDeletion(object sender, EventArgs e)
    { try { await Launcher.OpenAsync("https://mezanocm.com/account-deletion"); } catch { } }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}
