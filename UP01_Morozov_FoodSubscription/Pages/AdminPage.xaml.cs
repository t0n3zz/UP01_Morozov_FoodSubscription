using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Главное меню администратора.</summary>
    public partial class AdminPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public AdminPage()
        {
            InitializeComponent();

            if (CurrentSession.IsAuthorized)
                TbWelcome.Text = CurrentSession.User.FIO;

            TbStats.Text = string.Format(
                "Пользователей: {0}   •   Категорий: {1}   •   Подписок: {2}",
                _context.User.Count(),
                _context.Category.Count(),
                _context.Subscription.Count());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new UsersTabPage());
        }

        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new CategoryTabPage());
        }

        private void BtnSubscriptions_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new SubscriptionTabPage());
        }

        private void BtnDiagramm_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new DiagrammPage());
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePasswordPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти из учётной записи?"))
                return;

            CurrentSession.SignOut();
            Manager.MainFrame.Navigate(new AuthPage());
        }
    }
}
