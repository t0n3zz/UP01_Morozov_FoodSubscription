using System;
using System.IO;
using System.Windows;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>
    /// Переключение словарей стилей приложения.
    /// Выбранная тема сохраняется в файл и восстанавливается при следующем запуске,
    /// в том числе после выхода пользователя из системы.
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>Фирменный словарь стилей (Style Guide Финансового университета).</summary>
        public const string Corporate = "Styles/Dictionary.xaml";

        /// <summary>Собственный (тёмный) словарь стилей.</summary>
        public const string Custom = "Styles/DarkDictionary.xaml";

        public static string CurrentTheme { get; private set; } = Corporate;

        public static bool IsCustomTheme
        {
            get { return CurrentTheme == Custom; }
        }

        private static string SettingsFile
        {
            get
            {
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "UP01_Morozov_FoodSubscription");
                Directory.CreateDirectory(folder);
                return Path.Combine(folder, "theme.txt");
            }
        }

        /// <summary>Применяет словарь стилей по относительному пути.</summary>
        public static void Apply(string theme)
        {
            Uri uri = new Uri(theme, UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            CurrentTheme = theme;
            Save();
        }

        /// <summary>Переключает тему на противоположную.</summary>
        public static void Toggle()
        {
            Apply(IsCustomTheme ? Corporate : Custom);
        }

        /// <summary>Загружает тему, сохранённую при прошлом запуске приложения.</summary>
        public static void Load()
        {
            string theme = Corporate;
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string saved = File.ReadAllText(SettingsFile).Trim();
                    if (saved == Corporate || saved == Custom)
                        theme = saved;
                }
            }
            catch (IOException)
            {
            }

            Apply(theme);
        }

        private static void Save()
        {
            try
            {
                File.WriteAllText(SettingsFile, CurrentTheme);
            }
            catch (IOException)
            {
            }
        }
    }
}
