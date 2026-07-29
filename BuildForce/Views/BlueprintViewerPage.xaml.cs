#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class BlueprintViewerPage : ContentPage
{
    private readonly ApiService _api;
    private readonly BlueprintItem _item;
    private byte[]? _bytes;
    private double _vScale = 1, _vStartScale = 1;
    private double _vX = 0, _vY = 0, _vStartX = 0, _vStartY = 0;

    private const string PdfHtmlTemplate = @"<!DOCTYPE html><html><head>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=5.0, user-scalable=yes"">
<style>body{margin:0;background:#080b10;}canvas{display:block;margin:0 auto 8px auto;width:100%;height:auto;}</style>
<script src=""https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js""></script>
</head><body><div id=""c""></div>
<script>
pdfjsLib.GlobalWorkerOptions.workerSrc='https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
var data=atob('__B64__');
var arr=new Uint8Array(data.length);for(var i=0;i<data.length;i++)arr[i]=data.charCodeAt(i);
pdfjsLib.getDocument({data:arr}).promise.then(function(pdf){
  var cont=document.getElementById('c');
  var w=document.body.clientWidth||360;
  function render(n){ if(n>pdf.numPages)return; pdf.getPage(n).then(function(p){
    var v=p.getViewport({scale:1}); var scale=(w/v.width)*2; var vp=p.getViewport({scale:scale});
    var cv=document.createElement('canvas'); cv.width=vp.width; cv.height=vp.height;
    cont.appendChild(cv);
    p.render({canvasContext:cv.getContext('2d'),viewport:vp}).promise.then(function(){render(n+1);});
  });}
  render(1);
}).catch(function(e){document.body.innerHTML='<div style=""color:#7d8590;padding:24px;font-family:sans-serif;text-align:center"">Could not render this PDF here.<br>Use the Open button instead.</div>';});
</script></body></html>";

    public BlueprintViewerPage(ApiService api, BlueprintItem item)
    {
        InitializeComponent();
        _api = api;
        _item = item;
        TitleLabel.Text = item.Title ?? item.FileName ?? "Blueprint";
        PdfView.HandlerChanged += OnPdfHandlerChanged;
        LoadAsync();
    }

    private void OnPdfHandlerChanged(object? sender, EventArgs e)
    {
#if ANDROID
        if (PdfView.Handler?.PlatformView is Android.Webkit.WebView wv)
        {
            wv.Settings.BuiltInZoomControls = true;
            wv.Settings.DisplayZoomControls = false;
            wv.Settings.JavaScriptEnabled = true;
        }
#endif
    }

    private async void LoadAsync()
    {
        _bytes = await _api.GetBlueprintFileAsync(_item.Id);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Loading.IsRunning = false; Loading.IsVisible = false;
            if (_bytes == null || _bytes.Length == 0)
            {
                ErrorLabel.Text = "Could not load this sheet.\nCheck your connection and try again.";
                ErrorLabel.IsVisible = true;
                return;
            }
            OpenExtBtn.IsEnabled = true;

            if (_item.ContentType != null && _item.ContentType.StartsWith("image/"))
            {
                var local = _bytes;
                ImgView.Source = ImageSource.FromStream(() => new MemoryStream(local));
                ImgHost.IsVisible = true;
            }
            else
            {
                var html = PdfHtmlTemplate.Replace("__B64__", Convert.ToBase64String(_bytes));
                PdfView.Source = new HtmlWebViewSource { Html = html };
                PdfView.IsVisible = true;
            }
        });
    }

    private async void OnOpenExternal(object sender, EventArgs e)
    {
        if (_bytes == null) return;
        try
        {
            var ext = _item.ContentType == "application/pdf" ? ".pdf"
                    : _item.ContentType == "image/png" ? ".png" : ".jpg";
            var path = Path.Combine(FileSystem.CacheDirectory, "blueprint-" + _item.Id + ext);
            File.WriteAllBytes(path, _bytes);
            await Launcher.Default.OpenAsync(new OpenFileRequest(_item.Title ?? "Blueprint", new ReadOnlyFile(path)));
        }
        catch (Exception ex)
        {
            var host = Application.Current?.MainPage;
            if (host != null) await host.DisplayAlert("Open failed", ex.Message, "OK");
        }
    }

    private void OnPinch(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started) { _vStartScale = _vScale; }
        else if (e.Status == GestureStatus.Running)
        {
            _vScale = Math.Clamp(_vStartScale * e.Scale, 1.0, 6.0);
            ImgView.Scale = _vScale;
            if (_vScale <= 1.01)
            {
                _vX = 0; _vY = 0;
                ImgView.TranslationX = 0; ImgView.TranslationY = 0;
            }
        }
    }

    private void OnPan(object sender, PanUpdatedEventArgs e)
    {
        if (_vScale <= 1.01) return;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _vStartX = _vX; _vStartY = _vY;
                break;
            case GestureStatus.Running:
                var maxX = Math.Max(0, ImgView.Width * (_vScale - 1) / 2);
                var maxY = Math.Max(0, ImgView.Height * (_vScale - 1) / 2);
                _vX = Math.Clamp(_vStartX + e.TotalX, -maxX, maxX);
                _vY = Math.Clamp(_vStartY + e.TotalY, -maxY, maxY);
                ImgView.TranslationX = _vX; ImgView.TranslationY = _vY;
                break;
        }
    }

    private void OnDoubleTap(object sender, TappedEventArgs e)
    {
        if (_vScale > 1.01)
        {
            _vScale = 1; _vX = 0; _vY = 0;
            ImgView.Scale = 1; ImgView.TranslationX = 0; ImgView.TranslationY = 0;
        }
        else
        {
            _vScale = 2.5; ImgView.Scale = 2.5;
        }
    }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}