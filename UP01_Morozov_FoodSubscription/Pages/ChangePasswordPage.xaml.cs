using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Страница смены пароля пользователя.</summary>
    public partial class ChangePasswordPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public ChangePasswordPage()
        {
            InitializeComponent();

            if (CurrentSession.IsAuthorized)
            {
                TbLogin.Text = CurrentSession.User.Login;
                TbLogin.IsReadOnly = true;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string oldPassword = PbOld.Password;
            string newPassword = PbNew.Password;
            string confirm = PbConfirm.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageHelper.Warning("Заполните все поля формы!");
                return;
            }

            string oldHash = HashHelper.GetHash(oldPassword);
            User user = _context.User
                .FirstOrDefault(u => u.Login == login && u.Password == oldHash);

            if (user == null)
            {
                MessageHelper.Error("Неверный логин или текущий пароль!");
                return;
            }

            string error;
            if (!ValidationHelper.IsPasswordValid(newPassword, out error))
            {
                MessageHelper.Warning(error);
                return;
            }

            if (newPassword == oldPassword)
            {
                MessageHelper.Warning("Новый пароль не должен совпадать со старым!");
                return;
            }

            if (newPassword != confirm)
            {
                MessageHelper.Warning("Новый пароль и его подтверждение не совпадают!");
                return;
            }

            user.Password = HashHelper.GetHash(newPassword);
            _context.SaveChanges();

            MessageHelper.Info("Пароль успешно изменён! Войдите в систему с новым паролем.");
            CurrentSession.SignOut();
            Manager.MainFrame.Navigate(new AuthPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSession.IsAuthorized)
            {
                if (Manager.MainFrame.CanGoBack)
                    Manager.MainFrame.GoBack();
                return;
            }

            Manager.MainFrame.Navigate(new AuthPage());
        }

        #region Заполнители (placeholder) полей ввода

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhLogin.Visibility = string.IsNullOrEmpty(TbLogin.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PbOld_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PhOld.Visibility = string.IsNullOrEmpty(PbOld.Password)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PbNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PhNew.Visibility = string.IsNullOrEmpty(PbNew.Password)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PbConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PhConfirm.Visibility = string.IsNullOrEmpty(PbConfirm.Password)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PhLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbLogin.Focus(); }
        private void PhOld_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbOld.Focus(); }
        private void PhNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbNew.Focus(); }
        private void PhConfirm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbConfirm.Focus(); }

        #endregion
    }
}
