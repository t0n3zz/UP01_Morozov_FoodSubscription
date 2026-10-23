using System.Windows;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>Единый вывод сообщений пользователю.</summary>
    public static class MessageHelper
    {
        private const string Caption = "Сервис подписки на продукты питания";

        public static void Info(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Warning(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Error(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>Запрос подтверждения действия. Возвращает true, если пользователь нажал «Да».</summary>
        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, Caption, MessageBoxButton.YesNo, MessageBoxImage.Question)
                   == MessageBoxResult.Yes;
        }
    }
}
