using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>
    /// Страница авторизации пользователей.
    /// После трёх неверных попыток входа выводится капча.
    /// </summary>
    public partial class AuthPage : Page
    {
        /// <summary>Количество неверных попыток входа.</summary>
        private int _failedAttempts = 0;

        /// <summary>Максимальное число попыток до появления капчи.</summary>
        private const int MaxAttempts = 3;

        /// <summary>Текст капчи, сгенерированный для текущей попытки входа.</summary>
        private string _captchaText = string.Empty;

        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public AuthPage()
        {
            InitializeComponent();
            CurrentSession.SignOut();
        }

        /// <summary>Вход в систему: проверка данных, хэша пароля и капчи.</summary>
        private void BtnAuth_Click(object sender, RoutedEventArgs e)
        {
            Authorize();
        }

        private void Authorize()
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageHelper.Warning("Введите логин и пароль!");
                return;
            }

            if (SpCaptcha.Visibility == Visibility.Visible)
            {
                if (!CaptchaHelper.Check(TbCaptcha.Text, _captchaText))
                {
                    MessageHelper.Warning("Символы с картинки введены неверно!");
                    GenerateCaptcha();
                    return;
                }
            }

            string hash = HashHelper.GetHash(password);
            User user = _context.User
                .FirstOrDefault(u => u.Login == login && u.Password == hash);

            if (user == null)
            {
                _failedAttempts++;

                if (_failedAttempts >= MaxAttempts)
                {
                    ShowCaptcha();
                    MessageHelper.Error(string.Format(
                        "Неверный логин или пароль!\nКоличество неудачных попыток: {0}.\n" +
                        "Для продолжения введите символы с картинки.", _failedAttempts));
                }
                else
                {
                    MessageHelper.Error(string.Format(
                        "Неверный логин или пароль!\nОсталось попыток до проверки: {0}.",
                        MaxAttempts - _failedAttempts));
                }

                PbPassword.Clear();
                return;
            }

            _failedAttempts = 0;
            HideCaptcha();
            CurrentSession.SignIn(user);

            MessageHelper.Info(string.Format("Добро пожаловать, {0}!", user.FIO));

            if (user.Role == User.RoleAdmin)
                Manager.MainFrame.Navigate(new AdminPage());
            else
                Manager.MainFrame.Navigate(new UserPage());
        }

        /// <summary>Показывает капчу и генерирует её текст.</summary>
        private void ShowCaptcha()
        {
            SpCaptcha.Visibility = Visibility.Visible;
            GenerateCaptcha();
        }

        private void HideCaptcha()
        {
            SpCaptcha.Visibility = Visibility.Collapsed;
            TbCaptcha.Clear();
        }

        /// <summary>Генерирует новый текст капчи и выводит его изображение.</summary>
        private void GenerateCaptcha()
        {
            _captchaText = CaptchaHelper.GenerateText();
            ImgCaptcha.Source = CaptchaHelper.Draw(_captchaText);
            TbCaptcha.Clear();
        }

        private void BtnRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new RegPage());
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePasswordPage());
        }

        /// <summary>Вход по нажатию клавиши Enter в поле пароля.</summary>
        private void PbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Authorize();
        }

        #region Заполнители (placeholder) полей ввода

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

        private void TbCaptcha_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhCaptcha.Visibility = string.IsNullOrEmpty(TbCaptcha.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PhLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TbLogin.Focus();
        }

        private void PhPassword_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PbPassword.Focus();
        }

        private void PhCaptcha_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TbCaptcha.Focus();
        }

        #endregion
    }
}
