using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>
    /// Добавление нового пользователя и редактирование существующего.
    /// Если в конструктор передан null, создаётся новая запись.
    /// </summary>
    public partial class AddUserPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();
        private readonly User _user;
        private readonly bool _isNew;

        public AddUserPage(User user)
        {
            InitializeComponent();

            _isNew = user == null;
            _user = user ?? new User { Role = User.RoleUser };

            if (_isNew)
            {
                Title = "Добавление пользователя";
                TbHeader.Text = "Добавление пользователя";
            }
            else
            {
                Title = "Редактирование пользователя";
                TbHeader.Text = "Редактирование пользователя";

                TbFio.Text = _user.FIO;
                TbLogin.Text = _user.Login;
                TbPhoto.Text = _user.Photo;
                CmbRole.SelectedIndex = _user.Role == User.RoleAdmin ? 1 : 0;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fio = TbFio.Text.Trim();
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;
            string role = (CmbRole.SelectedItem as ComboBoxItem).Content.ToString();
            string photo = TbPhoto.Text.Trim();

            if (string.IsNullOrWhiteSpace(fio) || string.IsNullOrWhiteSpace(login))
            {
                MessageHelper.Warning("Заполните Ф.И.О. и логин!");
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

            if (_context.User.Any(u => u.Login == login && u.ID != _user.ID))
            {
                MessageHelper.Warning("Пользователь с таким логином уже существует!");
                return;
            }

            if (_isNew || !string.IsNullOrWhiteSpace(password))
            {
                string error;
                if (!ValidationHelper.IsPasswordValid(password, out error))
                {
                    MessageHelper.Warning(error);
                    return;
                }

                _user.Password = HashHelper.GetHash(password);
            }

            _user.FIO = fio;
            _user.Login = login;
            _user.Role = role;
            _user.Photo = string.IsNullOrWhiteSpace(photo) ? null : photo;

            if (_isNew)
                _context.User.Add(_user);

            _context.SaveChanges();

            MessageHelper.Info(_isNew
                ? "Пользователь добавлен."
                : "Данные пользователя сохранены.");

            Manager.MainFrame.Navigate(new UsersTabPage());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbFio.Clear();
            TbLogin.Clear();
            TbPhoto.Clear();
            PbPassword.Clear();
            CmbRole.SelectedIndex = 0;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new UsersTabPage());
        }
    }
}
