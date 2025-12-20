using Microsoft.AspNetCore.Mvc;
using CST465_project.Models;
using CST465_project.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CST465_project.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _repo;
    private readonly ILogger<CommentsController> _logger;
    private readonly IConfiguration _config;

    public CommentsController(ICommentRepository repo, ILogger<CommentsController> logger, IConfiguration config)
    {
        _repo = repo;
        _logger = logger;
        _config = config;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int visualizationId)
    {
        try
        {
            var comments = await _repo.GetCommentsByVisualizationIdAsync(visualizationId);
            // Selecting only necessary fields to keep the API response lean
            var shaped = comments.Select(c => new { c.Id, c.VisualizationId, c.Content, c.CreatedAt, c.AuthorEmail });
            return Ok(shaped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching comments for visualization {Id}", visualizationId);
            return Problem(detail: "An internal error occurred while retrieving comments.", statusCode: 500);
        }
    }

    [HttpPost]
    [Authorize] // Only logged-in users can comment
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post([FromBody] CommentCreateDto dto)
    {
        var isEnabled = _config.GetValue<bool>("SiteSettings:EnablePublicComments");
        if (!isEnabled)
        {
            return BadRequest(new { error = "Comments are currently disabled." });
        }
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "Anonymous";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = new Comment
            {
                VisualizationId = dto.VisualizationId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                AuthorEmail = email ?? "Anonymous"
            };

            await _repo.AddCommentAsync(comment);

            // Returns 201 Created with a link to the GET method
            return CreatedAtAction(
                nameof(Get),
                new { visualizationId = comment.VisualizationId },
                comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for visualization {Id} by user {User}", dto.VisualizationId, User.Identity?.Name);
            return Problem(detail: "Could not save comment.", statusCode: 500);
        }
    }
}