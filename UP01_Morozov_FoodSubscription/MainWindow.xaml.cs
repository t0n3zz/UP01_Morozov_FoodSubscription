using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UP01_Morozov_FoodSubscription.Helpers;
using UP01_Morozov_FoodSubscription.Pages;

namespace UP01_Morozov_FoodSubscription
{
    /// <summary>
    /// Главное окно приложения: шапка с динамическим заголовком страницы,
    /// фрейм для WPF-страниц и строка состояния с датой и временем.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            Manager.MainFrame = MainFrame;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, args) =>
                TbDateTime.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
            _timer.Start();
            TbDateTime.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");

            UpdateThemeButton();
            MainFrame.Navigate(new AuthPage());
        }

        /// <summary>Обновляет заголовок окна и строку состояния при переходе между страницами.</summary>
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Page page = e.Content as Page;
            string title = page != null && !string.IsNullOrWhiteSpace(page.Title)
                ? page.Title
                : "Онлайн-сервис подписки на продукты питания";

            TbPageTitle.Text = title;
            Title = "Онлайн-сервис подписки на продукты питания — " + title;

            BtnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            TbUserInfo.Text = CurrentSession.IsAuthorized
                ? string.Format("Вы вошли как: {0} ({1})",
                                CurrentSession.User.FIO, CurrentSession.User.Role)
                : "Пользователь не авторизован";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        /// <summary>Смена темы приложения по нажатию на кнопку.</summary>
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Toggle();
            UpdateThemeButton();
        }

        private void UpdateThemeButton()
        {
            BtnTheme.Content = ThemeManager.IsCustomTheme ? "Светлая тема" : "Тёмная тема";
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>Выход из приложения с подтверждением действия.</summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти из приложения?"))
                e.Cancel = true;
            else
                e.Cancel = false;
        }
    }
}
