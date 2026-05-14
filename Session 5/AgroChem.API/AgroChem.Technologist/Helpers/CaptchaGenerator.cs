using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AgroChem.Technologist.Helpers
{
    public static class CaptchaGenerator
    {
        private static readonly string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private static readonly Random Rnd = new Random();

        public static string Code { get; private set; }
        public static BitmapSource Image { get; private set; }

        public static void Generate(int length = 5, int width = 180, int height = 60)
        {
            var codeArray = new char[length];
            for (int i = 0; i < length; i++)
                codeArray[i] = Chars[Rnd.Next(Chars.Length)];
            Code = new string(codeArray);

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                // фон
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

                // линии
                for (int i = 0; i < 20; i++)
                {
                    var p1 = new Point(Rnd.Next(width), Rnd.Next(height));
                    var p2 = new Point(Rnd.Next(width), Rnd.Next(height));
                    dc.DrawLine(new Pen(Brushes.LightGray, 1), p1, p2);
                }

                // текст
                var ft = new FormattedText(Code, System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface("Arial"), 28, Brushes.DarkBlue,
                    new NumberSubstitution(), TextFormattingMode.Display, 96.0);
                dc.DrawText(ft, new Point(10, 10));

                // точки
                for (int i = 0; i < 100; i++)
                    dc.DrawRectangle(Brushes.Gray, null, new Rect(Rnd.Next(width), Rnd.Next(height), 2, 2));
            }

            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(visual);
            Image = renderTarget;
        }
    }
}