// Project: MajorAuthor.Web
// File: Models/ExternalLoginJsonResult.cs

using System.Collections.Generic;

namespace MajorAuthor.Models
{
    public class ExternalLoginJsonResult
    {
        public string Status { get; set; } = string.Empty; // "success", "error", "email_missing", "redirect"
        public string? Message { get; set; }
        public string? RedirectUrl { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public string? LoginProvider { get; set; }
        public string ReturnUrl { get; set; }
    }
}