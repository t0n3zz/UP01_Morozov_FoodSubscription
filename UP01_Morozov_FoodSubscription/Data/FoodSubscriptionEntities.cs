using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace UP01_Morozov_FoodSubscription.Data
{
    /// <summary>
    /// Контекст базы данных DB_Morozov_FoodSubscription.
    /// Строка подключения задаётся в App.config.
    /// </summary>
    public class FoodSubscriptionEntities : DbContext
    {
        public FoodSubscriptionEntities()
            : base("name=FoodSubscriptionEntities")
        {
            Database.SetInitializer<FoodSubscriptionEntities>(null);
        }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Subscription> Subscription { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Subscription>()
                .Property(s => s.Price)
                .HasPrecision(10, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
