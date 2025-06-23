// Проект: MajorAuthor.Data
// Файл: Entities/Promotion.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет запись о продвижении книги.
    /// </summary>
    public class Promotion
    {
        /// <summary>
        /// Уникальный идентификатор акции продвижения.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к книге, которая продвигается.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// Внешний ключ к тарифному плану продвижения.
        /// </summary>
        public int PromotionPlanId { get; set; }

        /// <summary>
        /// Навигационное свойство к тарифному плану продвижения.
        /// </summary>
        public PromotionPlan PromotionPlan { get; set; }

        /// <summary>
        /// Дата и время начала продвижения.
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата и время окончания продвижения.
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
