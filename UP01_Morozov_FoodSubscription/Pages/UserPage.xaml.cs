using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>
    /// Страница пользователя: просмотр собственных подписок
    /// с поиском, фильтрацией по категории и сортировкой.
    /// </summary>
    public partial class UserPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();
        private readonly PhotoConverter _photoConverter = new PhotoConverter();

        public UserPage()
        {
            InitializeComponent();
            LoadUserCard();
            LoadCategories();
            UpdateData();
        }

        /// <summary>Выводит сведения о текущем пользователе.</summary>
        private void LoadUserCard()
        {
            User user = CurrentSession.User;
            if (user == null)
                return;

            TbFio.Text = user.FIO;
            TbRole.Text = "Роль: " + user.Role;
            ImgPhoto.Source = _photoConverter.Convert(
                user.Photo, typeof(object), null,
                System.Globalization.CultureInfo.CurrentCulture) as System.Windows.Media.ImageSource;
        }

        /// <summary>Загружает категории в фильтр, добавляя вариант «Все категории».</summary>
        private void LoadCategories()
        {
            List<Category> categories = _context.Category.OrderBy(c => c.Name).ToList();
            categories.Insert(0, new Category { ID = 0, Name = "Все категории" });
            CmbCategory.ItemsSource = categories;
            CmbCategory.SelectedIndex = 0;
        }

        /// <summary>Применяет поиск, фильтр и сортировку к списку подписок пользователя.</summary>
        private void UpdateData()
        {
            if (DgSubscriptions == null)
                return;

            if (CurrentSession.User == null)
                return;

            int userId = CurrentSession.User.ID;

            IEnumerable<Subscription> subscriptions = _context.Subscription
                .Where(s => s.UserID == userId)
                .ToList();

            string search = TbSearch == null ? string.Empty : TbSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
                subscriptions = subscriptions.Where(s =>
                    s.Name.ToLower().Contains(search.ToLower()));

            Category category = CmbCategory == null ? null : CmbCategory.SelectedItem as Category;
            if (category != null && category.ID != 0)
                subscriptions = subscriptions.Where(s => s.CategoryID == category.ID);

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

            TbCount.Text = string.Format("Найдено подписок: {0}", result.Count);
            TbTotal.Text = string.Format("Сумма подписок: {0:N2} руб.",
                result.Sum(s => s.Price * s.Num));
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

        /// <summary>Очищает поиск, фильтр и сортировку.</summary>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
            CmbCategory.SelectedIndex = 0;
            CmbSort.SelectedIndex = 0;
            UpdateData();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePasswordPage());
        }

        /// <summary>Завершение сеанса с подтверждением действия.</summary>
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти из учётной записи?"))
                return;

            CurrentSession.SignOut();
            Manager.MainFrame.Navigate(new AuthPage());
        }

        private void PhSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TbSearch.Focus();
        }
    }
}
