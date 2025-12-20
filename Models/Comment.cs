using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace CST465_project.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VisualizationId { get; set; }

        public string UserId { get; set; }
        
        [Required]
        [StringLength(256)]
        public string AuthorEmail { get; set; } = string.Empty;
        //public virtual ApplicationUser User { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}