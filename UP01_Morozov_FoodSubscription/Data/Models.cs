using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace UP01_Morozov_FoodSubscription.Data
{
    /// <summary>
    /// Пользователь онлайн-сервиса подписки на продукты питания.
    /// Роль определяет доступные разделы приложения.
    /// </summary>
    [Table("User")]
    public class User
    {
        public const string RoleAdmin = "Администратор";
        public const string RoleUser = "Пользователь";

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(50)]
        public string Login { get; set; }

        /// <summary>SHA-256 хэш пароля в формате Base64. Открытый пароль не хранится.</summary>
        [Required, StringLength(100)]
        public string Password { get; set; }

        [Required, StringLength(20)]
        public string Role { get; set; }

        [Required, StringLength(150)]
        public string FIO { get; set; }

        /// <summary>Имя файла фотографии из каталога res.</summary>
        [StringLength(255)]
        public string Photo { get; set; }

        public virtual ICollection<Subscription> Subscription { get; set; }
            = new List<Subscription>();

        /// <summary>Итоговая сумма всех подписок пользователя — для списков и отчётов.</summary>
        [NotMapped]
        public decimal TotalSum
        {
            get { return Subscription == null ? 0 : Subscription.Sum(s => s.Price * s.Num); }
        }

        [NotMapped]
        public bool IsAdmin
        {
            get { return Role == RoleAdmin; }
        }

        public override string ToString()
        {
            return FIO;
        }
    }

    /// <summary>Категория продуктового набора (бокса).</summary>
    [Table("Category")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Subscription> Subscription { get; set; }
            = new List<Subscription>();

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>Оформленная подписка пользователя на продуктовый набор.</summary>
    [Table("Subscription")]
    public class Subscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int CategoryID { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required, StringLength(150)]
        public string Name { get; set; }

        /// <summary>Количество наборов в доставке.</summary>
        public int Num { get; set; }

        /// <summary>Стоимость одного набора, руб.</summary>
        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        /// <summary>Итоговая стоимость подписки (количество * цена).</summary>
        [NotMapped]
        public decimal Total
        {
            get { return Price * Num; }
        }
    }
}
