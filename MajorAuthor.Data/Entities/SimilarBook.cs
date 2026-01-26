// Models/SimilarBook.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    public class SimilarBook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int SimilarToBookId { get; set; }

        [Required]
        [Range(0.0, 1.0)]
        public double SimilarityScore { get; set; }

        [Required]
        public DateTime CalculationDate { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }

        [ForeignKey("SimilarToBookId")]
        public virtual Book SimilarToBook { get; set; }

        // Дополнительные метрики для уточнения похожести
        public int? CommonGenresCount { get; set; }
        public int? CommonTagsCount { get; set; }
        public bool? SameAuthor { get; set; }

        public SimilarBook()
        {
            CalculationDate = DateTime.UtcNow;
        }
    }
}