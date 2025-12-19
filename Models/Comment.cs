using System.ComponentModel.DataAnnotations;

namespace CST465_project.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VisualizationId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}