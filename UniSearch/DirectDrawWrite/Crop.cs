// DirectDrawWrite
// Draws a text using DirectDraw/DirectWrire, and returns a cropped image as a byte[] for BGRA32bpp bitmap
// 2018-09-14   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace DirectDrawWrite
{
    public static partial class D2DDrawText
    {

        // Main entry point, could be extended with more parameters...
        public static BitmapSource GetBitmapSource(string s)
        {
            byte[] xx = D2DDrawText.TextToBytesArray(s, 96);
            return CroppedBitmapSourceFromArray(xx, 400, 400);
        }


        // Converts a bytes array containing BRGA32 bytes for an image of width×height
        // Eliminate white borders
        // Assumes that stride = 4×width bytes
        public static BitmapSource CroppedBitmapSourceFromArray(byte[] pixels, int width, int height)
        {
            // Since we have the bytes array, we can directly compute crop area
            var cropColor = Colors.White;

            var bottom = 0;
            var left = width - 1; // Set the left crop point to the width so that the logic below will set the left value to the first non crop color pixel it comes across.
            var right = 0;
            var top = height - 1; // Set the top crop point to the height so that the logic below will set the top value to the first non crop color pixel it comes across.

            unsafe
            {
                fixed (byte* dataPtr = pixels)
                {
                    var rgbPtr = dataPtr;
                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var b = rgbPtr[0];
                            var g = rgbPtr[1];
                            var r = rgbPtr[2];
                            var a = rgbPtr[3];

                            // If any of the pixel RGBA values don't match and the crop color is not transparent, or if the crop color is transparent and the pixel A value is not transparent
                            if ((cropColor.A > 0 && (b != cropColor.B || g != cropColor.G || r != cropColor.R || a != cropColor.A)) || (cropColor.A == 0 && a != 0))
                            {
                                if (x < left) left = x;
                                if (x > right) right = x;
                                if (y < top) top = y;
                                if (y > bottom) bottom = y;
                            }

                            rgbPtr += 4;
                        }
                    }
                }
            }

            // Crop image while generating WritableBitmap at the same time
            int cropWidth = right - left + 1;
            int cropHeight = bottom - top + 1;
            Int32Rect sourceRect = new Int32Rect(left, top, cropWidth, cropHeight);
            WriteableBitmap bitmap = new WriteableBitmap(cropWidth, cropHeight, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(sourceRect, pixels, width * (bitmap.Format.BitsPerPixel / 8), 0, 0);
            bitmap.Freeze();
            return bitmap;
        }

    }
}
