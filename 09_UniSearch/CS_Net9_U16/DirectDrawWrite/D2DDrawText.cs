// Class D2DDrawText
// Draws a text using Direct2D and DirectWrite and returns a bitmap (Bgr32Bpp format as byte[])
// Emoji are rendered in color
//
// Origin: https://blogs.msdn.microsoft.com/dsui_team/2013/04/16/using-direct2d-from-a-service-in-c/
//
// 2018-09-14   PV
// 2023-01-22   PV      C#11, nullable enable
// 2023-11-20   PV      Net8 C#12; Transform this fromject from .Net FW 4.8 library to Net8 library (finally!!!)

using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using Microsoft.WindowsAPICodePack.DirectX.WindowsImagingComponent;

namespace DirectDrawWrite;

public static partial class D2DDrawText
{
    static D2DFactory? d2dFactory;
    static ImagingFactory? wicFactory;
    static DWriteFactory? dwriteFactory;
    static RenderTarget? wicRenderTarget;

    static RenderTargetProperties renderProps = new()
    {
        PixelFormat = new PixelFormat(
                 Microsoft.WindowsAPICodePack.DirectX.Graphics.Format.B8G8R8A8UNorm,
                 AlphaMode.Ignore),
        Usage = RenderTargetUsages.None,
        RenderTargetType = RenderTargetType.Software
    };

    public static byte[] TextToBytesArray(string s, string fontName, int fontSize = 72)
    {
        //Create Factories.
        d2dFactory = D2DFactory.CreateFactory(D2DFactoryType.Multithreaded);
        wicFactory = ImagingFactory.Create();
        dwriteFactory = DWriteFactory.CreateFactory();

        var size = new SizeU(400u, 400u);
        var wicBitmap = wicFactory.CreateImagingBitmap(
                  size.Width,
                  size.Height,
                  PixelFormats.Bgr32Bpp,
                  BitmapCreateCacheOption.CacheOnLoad);

        wicRenderTarget = d2dFactory.CreateWicBitmapRenderTarget(wicBitmap, renderProps);

        wicRenderTarget.BeginDraw();

        wicRenderTarget.Clear(new ColorF(1, 1, 1, 1));  // clear the background
        var textBrush = wicRenderTarget.CreateSolidColorBrush(new ColorF(0, 0, 0.5f, 1)); // Dark blue

        // Render text
        var textFormat_Value = dwriteFactory.CreateTextFormat(fontName, fontSize,
                  FontWeight.Regular,
                  FontStyle.Normal,
                  FontStretch.Normal);
        var renderTextValue = new RectF(50, 50, 400, 400);
        wicRenderTarget.DrawText(s, textFormat_Value, renderTextValue, textBrush, (DrawTextOptions)4);

        wicRenderTarget.EndDraw();

        byte[] result;
        try
        {
            result = wicBitmap.CopyPixels();
        }
        finally
        {
            // Clean up
            d2dFactory.Dispose();
            wicFactory.Dispose();
            dwriteFactory.Dispose();
        }

        return result;
    }
}
