// Data/Entities/BookInvitation.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    public class BookInvitation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }

        // Email приглашенного пользователя
        [Required]
        [EmailAddress]
        public string InviteeEmail { get; set; }

        // ID пользователя, если он зарегистрирован
        public string? InviteeUserId { get; set; }
        public ApplicationUser? InviteeUser { get; set; }

        // Токен для принятия приглашения
        [Required]
        public string InvitationToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsAccepted { get; set; } = false;
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

        // Тип приглашения: для автора или для читателя
        public InvitationType InvitationType { get; set; } = InvitationType.CoAuthor;
    }

    public enum InvitationType
    {
        CoAuthor = 1,
        ReaderToAuthor = 2
    }
}