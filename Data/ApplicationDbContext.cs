using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CST465_project.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CST465_project.Models.Comment> Comments { get; set; } = null!;
    public DbSet<CST465_project.Models.Visualization> Visualizations { get; set; } = null!;
}
