#pragma warning disable CA1416
using System.Linq;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace BuildForce.Views;

public partial class PunchCameraPage : ContentPage
{
    public TaskCompletionSource<string?> Result { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private bool _completed = false;
    private bool _captureRequested = false;
    private bool _mediaReceived = false;
    private bool _resultSet = false;

    public PunchCameraPage(string punchLabel)
    {
        InitializeComponent();
        TitleLabel.Text = punchLabel + " [v14]";
        Camera.MediaCaptured += OnMediaCaptured;
        Camera.MediaCaptureFailed += OnMediaCaptureFailed;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var camStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (camStatus != PermissionStatus.Granted)
            {
                await ShowStatus("Camera permission denied");
                await Complete(null);
                return;
            }

            var cameras = await Camera.GetAvailableCameras(CancellationToken.None);
            var front = cameras.FirstOrDefault(c => c.Position.ToString() == "Front")
                        ?? cameras.FirstOrDefault();
            if (front != null)
                Camera.SelectedCamera = front;

            try { Camera.StopCameraPreview(); } catch { }
            await Task.Delay(150);
            await Camera.StartCameraPreview(CancellationToken.None);

            int waited = 0;
            while (!Camera.IsAvailable && waited < 5000)
            {
                await Task.Delay(250);
                waited += 250;
            }

            if (!Camera.IsAvailable)
            {
                await ShowStatus("Camera not available");
                await Complete(null);
                return;
            }

            _ = RunCountdownAndCapture();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PunchCamera OnAppearing error: {ex.Message}");
            await ShowStatus("Camera error: " + ex.Message);
            await Complete(null);
        }
    }

    private async Task ShowStatus(string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HintLabel.Text = text;
            HintLabel.TextColor = Color.FromArgb("#ef4444");
        });
        await Task.Delay(3000);
    }

    private async Task RunCountdownAndCapture()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CountdownBadge.IsVisible = true;
            HintLabel.Text = "Hold still...";
        });

        for (int i = 3; i >= 1; i--)
        {
            if (_completed) return;
            int n = i;
            MainThread.BeginInvokeOnMainThread(() => CountdownLabel.Text = n.ToString());
            await Task.Delay(1000);
        }

        MainThread.BeginInvokeOnMainThread(() => CountdownBadge.IsVisible = false);
        await TriggerCaptureAsync();
    }

    private async void OnCaptureNow(object sender, EventArgs e)
    {
        if (_completed) return;
        MainThread.BeginInvokeOnMainThread(() => CountdownBadge.IsVisible = false);
        await TriggerCaptureAsync();
    }

    private async Task TriggerCaptureAsync()
    {
        if (_completed || _captureRequested) return;
        _captureRequested = true;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProcessingOverlay.IsVisible = true;
            RetakeBtn.IsEnabled = false;
        });

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            await Camera.CaptureImage(cts.Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PunchCamera capture trigger error: {ex.Message}");
            await ShowStatus("Capture error: " + ex.Message);
            await Complete(null);
            return;
        }

        await Task.Delay(6000);
        if (!_mediaReceived && !_completed)
        {
            await ShowStatus("No photo received from camera");
            await Complete(null);
        }
    }

    private async void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e)
    {
        if (_completed) return;
        System.Diagnostics.Debug.WriteLine($"PunchCamera MediaCaptureFailed: {e.FailureReason}");
        await ShowStatus("Capture failed: " + e.FailureReason);
        await Complete(null);
    }

    private async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
    {
        if (_completed || _mediaReceived) return;
        _mediaReceived = true;

        try
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await e.Media.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            if (bytes.Length == 0)
            {
                await ShowStatus("Camera returned empty photo");
                await Complete(null);
                return;
            }

            try
            {
                bytes = NormalizePhoto(bytes, 800, 75);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Photo normalize failed: " + ex.Message);
            }

            var base64 = Convert.ToBase64String(bytes);
            await Complete(base64);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PunchCamera media error: {ex.Message}");
            await ShowStatus("Photo error: " + ex.Message);
            await Complete(null);
        }
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await Complete(null);
    }

    private Task Complete(string? base64)
    {
        if (_completed) return Task.CompletedTask;
        _completed = true;

        _resultSet = true;
        Result.TrySetResult(base64);

        try { Camera.MediaCaptured -= OnMediaCaptured; } catch { }
        try { Camera.MediaCaptureFailed -= OnMediaCaptureFailed; } catch { }
        try { Camera.StopCameraPreview(); } catch { }

        // Do NOT pop here. The caller (CapturePunchSelfieAsync) owns the modal
        // and pops it after the result is read, to avoid a modal-stack race.
        return Task.CompletedTask;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!_resultSet)
        {
            _resultSet = true;
            Result.TrySetResult(null);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        _ = Complete(null);
        return true;
    }

    // Decodes the JPEG, applies the EXIF orientation to the actual pixels,
    // downsizes to maxDim, and re-encodes. Re-encoding drops EXIF, so the
    // rotation MUST be baked into the pixels before saving.
    private static byte[] NormalizePhoto(byte[] input, int maxDim, int quality)
    {
        using var codec = SkiaSharp.SKCodec.Create(new MemoryStream(input));
        if (codec == null) return input;

        var origin = codec.EncodedOrigin;

        using var original = SkiaSharp.SKBitmap.Decode(codec);
        if (original == null) return input;

        using var upright = ApplyOrigin(original, origin);

        var w = upright.Width;
        var h = upright.Height;
        var scale = 1.0;
        if (w > maxDim || h > maxDim)
            scale = (double)maxDim / Math.Max(w, h);

        var targetW = Math.Max(1, (int)Math.Round(w * scale));
        var targetH = Math.Max(1, (int)Math.Round(h * scale));

        using var final = (scale < 1.0)
            ? upright.Resize(new SkiaSharp.SKImageInfo(targetW, targetH), SkiaSharp.SKFilterQuality.Medium)
            : upright.Copy();

        if (final == null) return input;

        using var image = SkiaSharp.SKImage.FromBitmap(final);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, quality);
        return data.ToArray();
    }

    private static SkiaSharp.SKBitmap ApplyOrigin(SkiaSharp.SKBitmap src, SkiaSharp.SKEncodedOrigin origin)
    {
        if (origin == SkiaSharp.SKEncodedOrigin.TopLeft || origin == SkiaSharp.SKEncodedOrigin.Default)
            return src.Copy();

        var swapsDims = origin == SkiaSharp.SKEncodedOrigin.LeftTop
                     || origin == SkiaSharp.SKEncodedOrigin.RightTop
                     || origin == SkiaSharp.SKEncodedOrigin.RightBottom
                     || origin == SkiaSharp.SKEncodedOrigin.LeftBottom;

        var outW = swapsDims ? src.Height : src.Width;
        var outH = swapsDims ? src.Width : src.Height;

        var dst = new SkiaSharp.SKBitmap(outW, outH, src.ColorType, src.AlphaType);
        using var canvas = new SkiaSharp.SKCanvas(dst);

        switch (origin)
        {
            case SkiaSharp.SKEncodedOrigin.TopRight:
                canvas.Translate(outW, 0); canvas.Scale(-1, 1); break;
            case SkiaSharp.SKEncodedOrigin.BottomRight:
                canvas.Translate(outW, outH); canvas.RotateDegrees(180); break;
            case SkiaSharp.SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, outH); canvas.Scale(1, -1); break;
            case SkiaSharp.SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
            case SkiaSharp.SKEncodedOrigin.RightTop:
                canvas.Translate(outW, 0); canvas.RotateDegrees(90); break;
            case SkiaSharp.SKEncodedOrigin.RightBottom:
                canvas.Translate(outW, outH); canvas.RotateDegrees(-90); canvas.Scale(1, -1); break;
            case SkiaSharp.SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, outH); canvas.RotateDegrees(-90); break;
        }

        canvas.DrawBitmap(src, 0, 0);
        canvas.Flush();
        return dst;
    }
}