using System.ComponentModel.DataAnnotations;

namespace CST465_project.Models
{
    public class EmailChangeDto
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string NewEmail { get; set; } = string.Empty;
    }
}
