using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Таблица «Категории продуктовых наборов» для администратора.</summary>
    public partial class CategoryTabPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public CategoryTabPage()
        {
            InitializeComponent();
            UpdateData();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                UpdateData();
        }

        private void UpdateData()
        {
            if (DgCategories == null)
                return;

            IEnumerable<Category> categories = _context.Category.ToList();

            string search = TbSearch == null ? string.Empty : TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                categories = categories.Where(c => c.Name.ToLower().Contains(search));

            switch (CmbSort == null ? 0 : CmbSort.SelectedIndex)
            {
                case 1: categories = categories.OrderByDescending(c => c.Name); break;
                case 2: categories = categories.OrderByDescending(c => c.Subscription.Count); break;
                default: categories = categories.OrderBy(c => c.Name); break;
            }

            List<Category> result = categories.ToList();
            DgCategories.ItemsSource = result;
            TbCount.Text = string.Format("Найдено категорий: {0}", result.Count);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddCategoryPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Category category = DgCategories.SelectedItem as Category;
            if (category == null)
            {
                MessageHelper.Warning("Выберите категорию в таблице!");
                return;
            }

            Manager.MainFrame.Navigate(new AddCategoryPage(category));
        }

        /// <summary>Каскадное удаление категории вместе со связанными подписками.</summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Category category = DgCategories.SelectedItem as Category;
            if (category == null)
            {
                MessageHelper.Warning("Выберите категорию в таблице!");
                return;
            }

            if (!MessageHelper.Confirm(string.Format(
                    "Вы действительно хотите удалить категорию «{0}»?\n" +
                    "Вместе с ней будут удалены связанные подписки ({1} шт.).",
                    category.Name, category.Subscription.Count)))
                return;

            _context.Subscription.RemoveRange(category.Subscription.ToList());
            _context.Category.Remove(category);
            _context.SaveChanges();

            MessageHelper.Info("Категория удалена.");
            UpdateData();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AdminPage());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
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
