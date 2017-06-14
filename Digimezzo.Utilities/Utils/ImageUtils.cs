using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Digimezzo.Utilities.Utils
{
    public static class ImageUtils
    {
        private static readonly ImageCodecInfo JpegCodec;

        static ImageUtils()
        {
            JpegCodec = ImageCodecInfo.GetImageEncoders().First(encoder => encoder.MimeType == "image/jpeg");
        }

        public static byte[] Image2GrayScaleByteArray(string filename)
        {
            byte[] byteArray = null;

            try
            {
                if (string.IsNullOrEmpty(filename)) return null;

                Bitmap bmp = default(Bitmap);

                using (Bitmap tempBmp = new Bitmap(filename))
                {

                    bmp = MakeGrayscale(new Bitmap(tempBmp));
                }

                ImageConverter converter = new ImageConverter();

                byteArray = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
            }
            catch (Exception)
            {
                throw;
            }

            return byteArray;
        }

        public static Bitmap MakeGrayscale(Bitmap original)
        {
            // Create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            // Get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            // Create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {new float[] {0.3f,0.3f,0.3f,0,0},
                                                                     new float[] {0.59f,0.59f,0.59f,0,0},
                                                                     new float[] {0.11f,0.11f,0.11f,0,0},
                                                                     new float[] {0,0,0,1,0},
                                                                     new float[] {0,0,0,0,1}
                                                                    });

            // Create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            // Set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            // Draw the original image on the new image using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            // Dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static byte[] Image2ByteArray(string filename)
        {
            byte[] byteArray = null;

            try
            {
                if (string.IsNullOrEmpty(filename))
                    return null;

                Bitmap bmp = default(Bitmap);

                using (Bitmap tempBmp = new Bitmap(filename))
                {
                    bmp = new Bitmap(tempBmp);
                }

                ImageConverter converter = new ImageConverter();

                byteArray = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
            }
            catch (Exception)
            {
                throw;
            }

            return byteArray;
        }

        public static void Image2File(Image img, ImageCodecInfo codec, string filename,int width, int height,
            long qualityPercent)
        {
            var encoderParams = new EncoderParameters
            {
                Param = { [0] = new EncoderParameter(Encoder.Quality, qualityPercent) }
            };

            Image scaledImg = null;
            try
            {
                if (width > 0 && height > 0)
                {
                    scaledImg = img.GetThumbnailImage(width, height, null, IntPtr.Zero);
                    img = scaledImg;
                }

                if (File.Exists(filename))
                    File.Delete(filename);
                img.Save(filename, codec, encoderParams);
            }
            finally
            {
                scaledImg?.Dispose();
            }
        }

        public static void Byte2ImageFile(byte[] imageData, ImageCodecInfo codec, string filename, int width, int height,
            long qualityPercent)
        {
            using (var ms = new MemoryStream(imageData))
            {
                using (var img = Image.FromStream(ms))
                {
                    Image2File(img, codec, filename, width, height, qualityPercent);
                }
            }
        }

        public static void Byte2Jpg(byte[] imageData, string filename, int width, int height, long qualityPercent)
        {
            Byte2ImageFile(imageData, JpegCodec, filename, width, height, qualityPercent);
        }

        public static long GetImageDataSize(byte[] imageData)
        {
            int size = 0;

            try
            {
                using (System.Drawing.Image img = System.Drawing.Image.FromStream(new MemoryStream(imageData)))
                {
                    size = img.Width * img.Height;
                }

            }
            catch (Exception)
            {
            }

            return size;
        }

        public static BitmapImage PathToBitmapImage(string path, int imageWidth, int imageHeight)
        {
            if (System.IO.File.Exists(path))
            {
                BitmapImage bi = new BitmapImage();

                bi.BeginInit();

                if (imageWidth > 0 && imageHeight > 0)
                {
                    bi.DecodePixelWidth = imageWidth;
                    bi.DecodePixelHeight = imageHeight;
                }

                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(path);
                bi.EndInit();
                bi.Freeze();

                return bi;
            }

            return null;
        }

        public static BitmapImage ByteToBitmapImage(byte[] byteData, int imageWidth, int imageHeight, int maxLength)
        {
            if (byteData != null && byteData.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(byteData))
                {

                    BitmapImage bi = new BitmapImage();

                    bi.BeginInit();

                    if (imageWidth > 0 && imageHeight > 0)
                    {
                        var size = new Size(imageWidth, imageHeight);
                        if (maxLength > 0) size = GetScaledSize(new Size(imageWidth, imageHeight), maxLength);

                        bi.DecodePixelWidth = size.Width;
                        bi.DecodePixelHeight = size.Height;
                    }

                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();

                    return bi;
                }
            }

            return null;
        }

        public static Size GetScaledSize(Size originalSize, int maxLength)
        {
            var scaledSize = new Size();

            if (originalSize.Height > originalSize.Width)
            {
                scaledSize.Height = maxLength;
                scaledSize.Width = Convert.ToInt32(((double)originalSize.Width / maxLength) * 100);
            }
            else
            {
                scaledSize.Width = maxLength;
                scaledSize.Height = Convert.ToInt32(((double)originalSize.Height / maxLength) * 100);
            }

            return scaledSize;
        }

        public static System.Windows.Media.Color GetDominantColor(string path)
        {
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(path);

            return GetDominantColor(bitmap);
        }

        public static System.Windows.Media.Color GetDominantColor(byte[] imageData)
        {
            System.Drawing.Bitmap bitmap;

            using (var ms = new MemoryStream(imageData))
            {
                bitmap = new Bitmap(ms);
            }

            return GetDominantColor(bitmap);
        }

        private static System.Windows.Media.Color GetDominantColor(System.Drawing.Bitmap bitmap)
        {
            var newBitmap = new System.Drawing.Bitmap(1, 1);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap))
            {
                // Interpolation mode needs to be HighQualityBilinear or HighQualityBicubic
                // or this method doesn't work. With either setting, the averaging result is
                // slightly different.
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, 1, 1));
            }

            System.Drawing.Color color = newBitmap.GetPixel(0, 0);

            return System.Windows.Media.Color.FromRgb(color.R, color.G, color.B); ;
        }
    }
}
