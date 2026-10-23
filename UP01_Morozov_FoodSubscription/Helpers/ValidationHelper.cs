using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>Проверки формата вводимых данных и ограничения ввода.</summary>
    public static class ValidationHelper
    {
        private static readonly Regex OnlyDigits = new Regex("^[0-9]+$");
        private static readonly Regex OnlyPrice = new Regex(@"^[0-9]+([.,][0-9]{0,2})?$");
        private static readonly Regex LoginFormat = new Regex("^[A-Za-z0-9_]{3,50}$");
        private static readonly Regex FioFormat = new Regex(@"^[А-Яа-яЁё\-]{2,}(\s[А-Яа-яЁё\-]{2,}){1,2}$");

        /// <summary>Логин: 3-50 символов, только латиница, цифры и знак подчёркивания.</summary>
        public static bool IsLoginValid(string login)
        {
            return !string.IsNullOrWhiteSpace(login) && LoginFormat.IsMatch(login.Trim());
        }

        /// <summary>Ф.И.О.: кириллица, два или три слова через пробел.</summary>
        public static bool IsFioValid(string fio)
        {
            return !string.IsNullOrWhiteSpace(fio) && FioFormat.IsMatch(fio.Trim());
        }

        /// <summary>
        /// Пароль: не менее 6 символов, минимум одна цифра, одна заглавная
        /// и одна строчная буква, один спецсимвол.
        /// Текст ошибки возвращается через параметр error.
        /// </summary>
        public static bool IsPasswordValid(string password, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                error = "Пароль должен содержать не менее 6 символов!";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                error = "Добавьте хотя бы одну цифру!";
                return false;
            }
            if (!password.Any(char.IsUpper))
            {
                error = "Добавьте хотя бы одну заглавную букву!";
                return false;
            }
            if (!password.Any(char.IsLower))
            {
                error = "Добавьте хотя бы одну строчную букву!";
                return false;
            }
            if (password.All(char.IsLetterOrDigit))
            {
                error = "Добавьте хотя бы один специальный символ (!, @, #, $ и т.д.)!";
                return false;
            }

            return true;
        }

        /// <summary>Обработчик PreviewTextInput для полей, принимающих только целые числа.</summary>
        public static void AllowOnlyDigits(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !OnlyDigits.IsMatch(e.Text);
        }

        /// <summary>Обработчик PreviewTextInput для полей с денежной суммой.</summary>
        public static void AllowOnlyPrice(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            string result = textBox == null
                ? e.Text
                : textBox.Text.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !OnlyPrice.IsMatch(result);
        }

        /// <summary>Проверка числа на попадание в допустимый диапазон.</summary>
        public static bool IsInRange(decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        /// <summary>Преобразование строки в денежную сумму с учётом запятой и точки.</summary>
        public static bool TryParsePrice(string text, out decimal price)
        {
            return decimal.TryParse(
                (text ?? string.Empty).Replace('.', ','),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
                out price);
        }
    }
}
