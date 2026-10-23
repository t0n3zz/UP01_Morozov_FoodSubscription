using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Таблица «Подписки» для администратора.</summary>
    public partial class SubscriptionTabPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public SubscriptionTabPage()
        {
            InitializeComponent();
            LoadFilters();
            UpdateData();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                UpdateData();
        }

        /// <summary>Загружает списки категорий и пользователей в фильтры.</summary>
        private void LoadFilters()
        {
            List<Category> categories = _context.Category.OrderBy(c => c.Name).ToList();
            categories.Insert(0, new Category { ID = 0, Name = "Все категории" });
            CmbCategory.ItemsSource = categories;
            CmbCategory.SelectedIndex = 0;

            List<User> users = _context.User.OrderBy(u => u.FIO).ToList();
            users.Insert(0, new User { ID = 0, FIO = "Все пользователи" });
            CmbUser.ItemsSource = users;
            CmbUser.SelectedIndex = 0;
        }

        private void UpdateData()
        {
            if (DgSubscriptions == null)
                return;

            IEnumerable<Subscription> subscriptions = _context.Subscription.ToList();

            string search = TbSearch == null ? string.Empty : TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                subscriptions = subscriptions.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    (s.User != null && s.User.FIO.ToLower().Contains(search)));

            Category category = CmbCategory == null ? null : CmbCategory.SelectedItem as Category;
            if (category != null && category.ID != 0)
                subscriptions = subscriptions.Where(s => s.CategoryID == category.ID);

            User user = CmbUser == null ? null : CmbUser.SelectedItem as User;
            if (user != null && user.ID != 0)
                subscriptions = subscriptions.Where(s => s.UserID == user.ID);

            switch (CmbSort == null ? 0 : CmbSort.SelectedIndex)
            {
                case 1: subscriptions = subscriptions.OrderBy(s => s.Date); break;
                case 2: subscriptions = subscriptions.OrderByDescending(s => s.Price * s.Num); break;
                case 3: subscriptions = subscriptions.OrderBy(s => s.Price * s.Num); break;
                case 4: subscriptions = subscriptions.OrderBy(s => s.Name); break;
                default: subscriptions = subscriptions.OrderByDescending(s => s.Date); break;
            }

            List<Subscription> result = subscriptions.ToList();
            DgSubscriptions.ItemsSource = result;
            TbCount.Text = string.Format("Найдено подписок: {0}   •   Общая сумма: {1:N2} руб.",
                result.Count, result.Sum(s => s.Price * s.Num));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddSubscriptionPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Subscription subscription = DgSubscriptions.SelectedItem as Subscription;
            if (subscription == null)
            {
                MessageHelper.Warning("Выберите подписку в таблице!");
                return;
            }

            Manager.MainFrame.Navigate(new AddSubscriptionPage(subscription));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Subscription subscription = DgSubscriptions.SelectedItem as Subscription;
            if (subscription == null)
            {
                MessageHelper.Warning("Выберите подписку в таблице!");
                return;
            }

            if (!MessageHelper.Confirm(string.Format(
                    "Вы действительно хотите удалить подписку «{0}»?", subscription.Name)))
                return;

            _context.Subscription.Remove(subscription);
            _context.SaveChanges();

            MessageHelper.Info("Подписка удалена.");
            UpdateData();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AdminPage());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
            CmbCategory.SelectedIndex = 0;
            CmbUser.SelectedIndex = 0;
            CmbSort.SelectedIndex = 0;
            UpdateData();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhSearch.Visibility = string.IsNullOrEmpty(TbSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            UpdateData();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void PhSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TbSearch.Focus();
        }
    }
}
