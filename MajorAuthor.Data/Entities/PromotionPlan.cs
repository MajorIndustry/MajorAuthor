// Проект: MajorAuthor.Data
// Файл: Entities/PromotionPlan.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет тарифный план продвижения для книг.
    /// </summary>
    public class PromotionPlan
    {
        /// <summary>
        /// Уникальный идентификатор тарифного плана.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название тарифного плана (например, "Базовое", "Премиум").
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Цена продвижения в денежных единицах (например, в рублях, долларах).
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// Срок продвижения в днях.
        /// </summary>
        [Required]
        public int DurationInDays { get; set; }

        /// <summary>
        /// Описание тарифного плана.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Коллекция акций продвижения, использующих этот тарифный план.
        /// </summary>
        public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}
