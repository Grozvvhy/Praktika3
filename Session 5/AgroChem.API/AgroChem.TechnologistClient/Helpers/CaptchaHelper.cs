using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AgroChem.TechnologistClient.Helpers
{
    public static class CaptchaHelper
    {
        private static readonly Random _random = new Random();

        public static (byte[] imageBytes, string code) GenerateCaptcha(int width = 200, int height = 80)
        {
            string code = GenerateRandomCode(6);
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(System.Drawing.Color.WhiteSmoke);
                using (var font = new Font("Arial", 28, FontStyle.Bold | FontStyle.Italic))
                {
                    var textSize = graphics.MeasureString(code, font);
                    var x = (width - (int)textSize.Width) / 2;
                    var y = (height - (int)textSize.Height) / 2;
                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, width, height),
                        System.Drawing.Color.DarkBlue, System.Drawing.Color.DarkOrange, 45f))
                    {
                        graphics.DrawString(code, font, brush, x, y);
                    }
                }
                // Шум
                for (int i = 0; i < 100; i++)
                {
                    int x = _random.Next(width);
                    int y = _random.Next(height);
                    bitmap.SetPixel(x, y, System.Drawing.Color.Gray);
                }
                // Линии
                for (int i = 0; i < 5; i++)
                {
                    using (var pen = new Pen(System.Drawing.Color.Gray, 1))
                    {
                        graphics.DrawLine(pen, _random.Next(width), _random.Next(height), _random.Next(width), _random.Next(height));
                    }
                }
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return (ms.ToArray(), code);
                }
            }
        }

        private static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            char[] code = new char[length];
            for (int i = 0; i < length; i++)
                code[i] = chars[_random.Next(chars.Length)];
            return new string(code);
        }
    }
}