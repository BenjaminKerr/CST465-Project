using System.ComponentModel.DataAnnotations;

namespace CST465_project.Models
{
    public class CommentCreateDto
    {
        [Required]
        public int VisualizationId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
