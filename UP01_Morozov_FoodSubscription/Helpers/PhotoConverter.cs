using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>
    /// Преобразует имя файла фотографии из базы данных в изображение.
    /// Если файл не найден, подставляется заглушка из каталога res.
    /// </summary>
    public class PhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fileName = value as string;
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res");

            string path = string.IsNullOrWhiteSpace(fileName)
                ? Path.Combine(folder, "logo.png")
                : Path.Combine(folder, fileName);

            if (!File.Exists(path))
                path = Path.Combine(folder, "logo.png");

            if (!File.Exists(path))
                return null;

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
