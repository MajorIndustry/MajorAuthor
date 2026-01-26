// Project: MajorAuthor.Web
// File: Models/ExternalLoginSessionData.cs

using System.Text.Json.Serialization;

namespace MajorAuthor.Models
{
    public class ExternalLoginTempData
    {
        public string LoginProvider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? ReturnUrl { get; set; }
    }
}