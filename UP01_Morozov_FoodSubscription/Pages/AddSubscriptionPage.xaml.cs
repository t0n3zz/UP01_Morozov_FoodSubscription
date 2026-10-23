using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Добавление и редактирование подписки на продуктовый набор.</summary>
    public partial class AddSubscriptionPage : Page
    {
        private const int MinNum = 1;
        private const int MaxNum = 100;
        private const decimal MinPrice = 0m;
        private const decimal MaxPrice = 100000m;

        private readonly FoodSubscriptionEntities _context = AppData.GetContext();
        private readonly Subscription _subscription;
        private readonly bool _isNew;

        public AddSubscriptionPage(Subscription subscription)
        {
            InitializeComponent();

            _isNew = subscription == null;
            _subscription = subscription ?? new Subscription { Date = DateTime.Today };

            CmbUser.ItemsSource = _context.User.OrderBy(u => u.FIO).ToList();
            CmbCategory.ItemsSource = _context.Category.OrderBy(c => c.Name).ToList();
            DpDate.DisplayDateEnd = DateTime.Today;

            if (_isNew)
            {
                CmbUser.SelectedIndex = 0;
                CmbCategory.SelectedIndex = 0;
                DpDate.SelectedDate = DateTime.Today;
            }
            else
            {
                Title = "Редактирование подписки";
                TbHeader.Text = "Редактирование подписки";

                CmbUser.SelectedItem = (CmbUser.ItemsSource as System.Collections.Generic.List<User>)
                    .FirstOrDefault(u => u.ID == _subscription.UserID);
                CmbCategory.SelectedItem = (CmbCategory.ItemsSource as System.Collections.Generic.List<Category>)
                    .FirstOrDefault(c => c.ID == _subscription.CategoryID);

                TbName.Text = _subscription.Name;
                TbNum.Text = _subscription.Num.ToString();
                TbPrice.Text = _subscription.Price.ToString("0.00");
                DpDate.SelectedDate = _subscription.Date;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            User user = CmbUser.SelectedItem as User;
            Category category = CmbCategory.SelectedItem as Category;
            string name = TbName.Text.Trim();

            if (user == null || category == null || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(TbNum.Text) || string.IsNullOrWhiteSpace(TbPrice.Text) ||
                DpDate.SelectedDate == null)
            {
                MessageHelper.Warning("Заполните все поля формы!");
                return;
            }

            if (name.Length < 3)
            {
                MessageHelper.Warning("Название набора должно содержать не менее 3 символов!");
                return;
            }

            int num;
            if (!int.TryParse(TbNum.Text, out num) ||
                !ValidationHelper.IsInRange(num, MinNum, MaxNum))
            {
                MessageHelper.Warning(string.Format(
                    "Количество наборов должно быть целым числом от {0} до {1}!", MinNum, MaxNum));
                return;
            }

            decimal price;
            if (!ValidationHelper.TryParsePrice(TbPrice.Text, out price) ||
                !ValidationHelper.IsInRange(price, MinPrice, MaxPrice))
            {
                MessageHelper.Warning(string.Format(
                    "Цена должна быть числом от {0:N0} до {1:N0} руб.!", MinPrice, MaxPrice));
                return;
            }

            DateTime date = DpDate.SelectedDate.Value;
            if (date > DateTime.Today)
            {
                MessageHelper.Warning("Дата оформления не может быть позднее сегодняшнего дня!");
                return;
            }

            _subscription.UserID = user.ID;
            _subscription.CategoryID = category.ID;
            _subscription.Name = name;
            _subscription.Num = num;
            _subscription.Price = price;
            _subscription.Date = date;

            if (_isNew)
                _context.Subscription.Add(_subscription);

            _context.SaveChanges();

            MessageHelper.Info(_isNew ? "Подписка добавлена." : "Подписка сохранена.");
            Manager.MainFrame.Navigate(new SubscriptionTabPage());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbName.Clear();
            TbNum.Clear();
            TbPrice.Clear();
            CmbUser.SelectedIndex = 0;
            CmbCategory.SelectedIndex = 0;
            DpDate.SelectedDate = DateTime.Today;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new SubscriptionTabPage());
        }

        /// <summary>Ограничение ввода: только цифры.</summary>
        private void TbNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ValidationHelper.AllowOnlyDigits(sender, e);
        }

        /// <summary>Ограничение ввода: число с двумя знаками после запятой.</summary>
        private void TbPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ValidationHelper.AllowOnlyPrice(sender, e);
        }
    }
}
