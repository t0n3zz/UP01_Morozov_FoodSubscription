using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Страница регистрации новых пользователей сервиса.</summary>
    public partial class RegPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public RegPage()
        {
            InitializeComponent();
        }

        /// <summary>Регистрация пользователя со всеми проверками введённых данных.</summary>
        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            string fio = TbFio.Text.Trim();
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;
            string confirm = PbConfirm.Password;

            if (string.IsNullOrWhiteSpace(fio) || string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageHelper.Warning("Заполните все поля формы!");
                return;
            }

            if (!ValidationHelper.IsFioValid(fio))
            {
                MessageHelper.Warning("Ф.И.О. должно быть записано кириллицей: Фамилия Имя Отчество.");
                return;
            }

            if (!ValidationHelper.IsLoginValid(login))
            {
                MessageHelper.Warning("Логин должен содержать от 3 до 50 символов: " +
                                      "латинские буквы, цифры и знак подчёркивания.");
                return;
            }

            if (_context.User.Any(u => u.Login == login))
            {
                MessageHelper.Warning("Пользователь с таким логином уже существует! " +
                                      "Придумайте другой логин.");
                return;
            }

            string error;
            if (!ValidationHelper.IsPasswordValid(password, out error))
            {
                MessageHelper.Warning(error);
                return;
            }

            if (password != confirm)
            {
                MessageHelper.Warning("Введённые пароли не совпадают!");
                return;
            }

            User user = new User
            {
                FIO = fio,
                Login = login,
                Password = HashHelper.GetHash(password),
                Role = (CmbRole.SelectedItem as ComboBoxItem).Content.ToString(),
                Photo = null
            };

            _context.User.Add(user);
            _context.SaveChanges();

            MessageHelper.Info("Регистрация успешно завершена! Войдите в систему, используя свой логин и пароль.");
            Manager.MainFrame.Navigate(new AuthPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AuthPage());
        }

        #region Заполнители (placeholder) полей ввода

        private void TbFio_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhFio.Visibility = string.IsNullOrEmpty(TbFio.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhLogin.Visibility = string.IsNullOrEmpty(TbLogin.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PhPassword.Visibility = string.IsNullOrEmpty(PbPassword.Password)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PbConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PhConfirm.Visibility = string.IsNullOrEmpty(PbConfirm.Password)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PhFio_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbFio.Focus(); }
        private void PhLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbLogin.Focus(); }
        private void PhPassword_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbPassword.Focus(); }
        private void PhConfirm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbConfirm.Focus(); }

        #endregion
    }
}
