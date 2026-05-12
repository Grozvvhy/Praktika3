using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace AgroChem.Technologist.Helpers
{
    public static class CaptchaGenerator
    {
        private static readonly string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private static readonly Random Rnd = new Random();

        public static (string Code, byte[] ImageBytes) Generate(int length = 5, int width = 180, int height = 60)
        {
            var code = new char[length];
            for (int i = 0; i < length; i++)
                code[i] = Chars[Rnd.Next(Chars.Length)];
            string codeStr = new string(code);

            using (var bitmap = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                // Линии шума
                for (int i = 0; i < 20; i++)
                {
                    int x1 = Rnd.Next(width);
                    int y1 = Rnd.Next(height);
                    int x2 = Rnd.Next(width);
                    int y2 = Rnd.Next(height);
                    g.DrawLine(new Pen(Color.LightGray), x1, y1, x2, y2);
                }
                // Текст
                using (var font = new Font("Arial", 24, FontStyle.Bold))
                using (var brush = new LinearGradientBrush(new Point(0, 0), new Point(width, height),
                    Color.DarkBlue, Color.DarkRed))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(codeStr, font, brush, new RectangleF(0, 0, width, height), format);
                }
                // Точки
                for (int i = 0; i < 100; i++)
                    bitmap.SetPixel(Rnd.Next(width), Rnd.Next(height), Color.Gray);

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return (codeStr, ms.ToArray());
                }
            }
        }
    }
}