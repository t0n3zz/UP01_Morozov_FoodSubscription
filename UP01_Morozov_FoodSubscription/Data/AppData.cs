namespace UP01_Morozov_FoodSubscription.Data
{
    /// <summary>
    /// Единая точка доступа к контексту базы данных.
    /// Контекст создаётся один раз и используется всеми страницами приложения.
    /// </summary>
    public static class AppData
    {
        private static FoodSubscriptionEntities _context;

        /// <summary>Возвращает текущий контекст базы данных.</summary>
        public static FoodSubscriptionEntities GetContext()
        {
            if (_context == null)
                _context = new FoodSubscriptionEntities();
            return _context;
        }

        /// <summary>
        /// Пересоздаёт контекст — используется после отмены изменений,
        /// чтобы неподтверждённые правки не попали в базу данных.
        /// </summary>
        public static void ResetContext()
        {
            if (_context != null)
                _context.Dispose();
            _context = new FoodSubscriptionEntities();
        }
    }
}
