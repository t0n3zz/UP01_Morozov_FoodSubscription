using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>
    /// Генерация и проверка капчи, которая выводится на странице авторизации
    /// после трёх неверных попыток входа.
    /// </summary>
    public static class CaptchaHelper
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int Length = 5;
        private const int Width = 220;
        private const int Height = 60;

        private static readonly Random Rnd = new Random();

        /// <summary>Генерирует новый текст капчи.</summary>
        public static string GenerateText()
        {
            char[] symbols = new char[Length];
            for (int i = 0; i < Length; i++)
                symbols[i] = Alphabet[Rnd.Next(Alphabet.Length)];
            return new string(symbols);
        }

        /// <summary>Рисует изображение капчи: символы под разным наклоном и линии-помехи.</summary>
        public static ImageSource Draw(string text)
        {
            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, Width, Height));

                for (int i = 0; i < 6; i++)
                {
                    Pen pen = new Pen(RandomBrush(120), Rnd.Next(1, 3));
                    dc.DrawLine(pen,
                        new Point(Rnd.Next(Width), Rnd.Next(Height)),
                        new Point(Rnd.Next(Width), Rnd.Next(Height)));
                }

                double x = 15;
                foreach (char symbol in text)
                {
                    FormattedText formatted = new FormattedText(
                        symbol.ToString(),
                        CultureInfo.GetCultureInfo("ru-RU"),
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Open Sans, Segoe UI"),
                                     FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                        Rnd.Next(28, 36),
                        RandomBrush(200),
                        96);

                    double y = Rnd.Next(2, 12);
                    double angle = Rnd.Next(-25, 26);

                    dc.PushTransform(new RotateTransform(angle, x + formatted.Width / 2, y + formatted.Height / 2));
                    dc.DrawText(formatted, new Point(x, y));
                    dc.Pop();

                    x += formatted.Width + Rnd.Next(4, 12);
                }

                for (int i = 0; i < 90; i++)
                    dc.DrawEllipse(RandomBrush(160), null,
                        new Point(Rnd.Next(Width), Rnd.Next(Height)), 1.2, 1.2);
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>Проверяет введённый текст капчи без учёта регистра.</summary>
        public static bool Check(string input, string expected)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(expected))
                return false;
            return string.Equals(input.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static Brush RandomBrush(int maxChannel)
        {
            return new SolidColorBrush(Color.FromRgb(
                (byte)Rnd.Next(maxChannel),
                (byte)Rnd.Next(maxChannel),
                (byte)Rnd.Next(maxChannel)));
        }
    }
}
