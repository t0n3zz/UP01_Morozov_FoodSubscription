using UP01_Morozov_FoodSubscription.Data;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>Сведения о пользователе, вошедшем в систему.</summary>
    public static class CurrentSession
    {
        public static User User { get; private set; }

        public static bool IsAuthorized
        {
            get { return User != null; }
        }

        public static bool IsAdmin
        {
            get { return User != null && User.Role == Data.User.RoleAdmin; }
        }

        public static void SignIn(User user)
        {
            User = user;
        }

        public static void SignOut()
        {
            User = null;
        }
    }
}
