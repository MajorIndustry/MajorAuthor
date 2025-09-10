// Проект: MajorAuthor.Data
// Файл: Entities/BookInvitation.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Сущность для хранения приглашений соавторов к книге.
    /// </summary>
    public class BookInvitation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public string InviteeEmail { get; set; }

        [Required]
        public string InvitationToken { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsAccepted { get; set; } = false;

        public Book Book { get; set; }
    }
}
