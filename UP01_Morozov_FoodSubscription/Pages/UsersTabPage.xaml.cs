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
    /// Таблица «Пользователи» для администратора:
    /// просмотр, поиск, фильтрация, сортировка, добавление, редактирование и удаление.
    /// </summary>
    public partial class UsersTabPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public UsersTabPage()
        {
            InitializeComponent();
            UpdateData();
        }

        /// <summary>Обновляет список при возврате со страницы добавления/редактирования.</summary>
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                UpdateData();
        }

        private void UpdateData()
        {
            if (DgUsers == null)
                return;

            IEnumerable<User> users = _context.User.ToList();

            string search = TbSearch == null ? string.Empty : TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                users = users.Where(u => u.FIO.ToLower().Contains(search)
                                      || u.Login.ToLower().Contains(search));

            if (CmbRole != null && CmbRole.SelectedIndex > 0)
            {
                string role = (CmbRole.SelectedItem as ComboBoxItem).Content.ToString();
                users = users.Where(u => u.Role == role);
            }

            switch (CmbSort == null ? 0 : CmbSort.SelectedIndex)
            {
                case 1: users = users.OrderByDescending(u => u.FIO); break;
                case 2: users = users.OrderByDescending(u => u.TotalSum); break;
                case 3: users = users.OrderBy(u => u.TotalSum); break;
                default: users = users.OrderBy(u => u.FIO); break;
            }

            List<User> result = users.ToList();
            DgUsers.ItemsSource = result;
            TbCount.Text = string.Format("Найдено пользователей: {0}", result.Count);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddUserPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            User user = DgUsers.SelectedItem as User;
            if (user == null)
            {
                MessageHelper.Warning("Выберите пользователя в таблице!");
                return;
            }

            Manager.MainFrame.Navigate(new AddUserPage(user));
        }

        /// <summary>Каскадное удаление пользователя вместе с его подписками.</summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            User user = DgUsers.SelectedItem as User;
            if (user == null)
            {
                MessageHelper.Warning("Выберите пользователя в таблице!");
                return;
            }

            if (CurrentSession.IsAuthorized && CurrentSession.User.ID == user.ID)
            {
                MessageHelper.Warning("Нельзя удалить учётную запись, под которой выполнен вход!");
                return;
            }

            int subscriptionsCount = user.Subscription.Count;
            if (!MessageHelper.Confirm(string.Format(
                    "Вы действительно хотите удалить пользователя «{0}»?\n" +
                    "Вместе с ним будут удалены его подписки ({1} шт.).",
                    user.FIO, subscriptionsCount)))
                return;

            _context.Subscription.RemoveRange(user.Subscription.ToList());
            _context.User.Remove(user);
            _context.SaveChanges();

            MessageHelper.Info("Пользователь удалён.");
            UpdateData();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AdminPage());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
            CmbRole.SelectedIndex = 0;
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
