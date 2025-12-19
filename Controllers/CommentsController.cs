using Microsoft.AspNetCore.Mvc;
using CST465_project.Models;
using CST465_project.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CST465_project.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _repo;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentRepository repo, ILogger<CommentsController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET api/comments?visualizationId=1
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int visualizationId)
    {
        try
        {
            var comments = await _repo.GetCommentsByVisualizationIdAsync(visualizationId);
            var shaped = comments.Select(c => new { c.Id, c.VisualizationId, c.Author, c.Content, c.CreatedAt });
            return Ok(shaped);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // POST api/comments
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] CommentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var comment = new Comment
            {
                VisualizationId = dto.VisualizationId,
                Author = dto.Author,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddCommentAsync(comment);

            return CreatedAtAction(nameof(Get), new { visualizationId = comment.VisualizationId }, new { comment.Id, comment.VisualizationId, comment.Author, comment.Content, comment.CreatedAt });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
