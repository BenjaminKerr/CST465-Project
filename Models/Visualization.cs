using System.ComponentModel.DataAnnotations;

namespace CST465_project.Models
{
    public class VisualizationCreateDto
    {
        public string Name { get; set; }
        public int BitSize { get; set; }
        public string Description { get; set; }
    }
    public class Visualization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(2, 10)]
        public int BitSize { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
