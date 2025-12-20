using System.ComponentModel.DataAnnotations;

namespace CST465_project.Models
{

    public class VisualizationCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 20, ErrorMessage = "BitSize must be between 1 and 20")]
        
        public int BitSize { get; set; }
        [StringLength(2000)]
        public string? Description { get; set; }
    }
}