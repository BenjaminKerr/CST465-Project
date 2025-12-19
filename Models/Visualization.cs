using System.ComponentModel.DataAnnotations;

namespace CST465_project.Models
{
    public class Visualization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
