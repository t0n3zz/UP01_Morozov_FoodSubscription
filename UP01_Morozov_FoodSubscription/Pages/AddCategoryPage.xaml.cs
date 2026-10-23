using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>Добавление и редактирование категории продуктовых наборов.</summary>
    public partial class AddCategoryPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();
        private readonly Category _category;
        private readonly bool _isNew;

        public AddCategoryPage(Category category)
        {
            InitializeComponent();

            _isNew = category == null;
            _category = category ?? new Category();

            if (!_isNew)
            {
                Title = "Редактирование категории";
                TbHeader.Text = "Редактирование категории";
                TbName.Text = _category.Name;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = TbName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageHelper.Warning("Введите название категории!");
                return;
            }

            if (name.Length < 3)
            {
                MessageHelper.Warning("Название категории должно содержать не менее 3 символов!");
                return;
            }

            if (_context.Category.Any(c => c.Name == name && c.ID != _category.ID))
            {
                MessageHelper.Warning("Такая категория уже существует!");
                return;
            }

            _category.Name = name;

            if (_isNew)
                _context.Category.Add(_category);

            _context.SaveChanges();

            MessageHelper.Info(_isNew ? "Категория добавлена." : "Категория сохранена.");
            Manager.MainFrame.Navigate(new CategoryTabPage());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbName.Clear();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new CategoryTabPage());
        }
    }
}
