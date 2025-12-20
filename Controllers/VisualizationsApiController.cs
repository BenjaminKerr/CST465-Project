using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CST465_project.Models;
using CST465_project.Repositories;

namespace CST465_project.Controllers;

/// <summary>
/// API Controller - Returns JSON for AJAX/frontend consumption
/// </summary>
[ApiController]
[Route("api/visualizations")]
[Produces("application/json")]
public class VisualizationsApiController : ControllerBase
{
    private readonly IVisualizationRepository _repo;
    private readonly ILogger<VisualizationsApiController> _logger;

    public VisualizationsApiController(IVisualizationRepository repo, ILogger<VisualizationsApiController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// GET: /api/visualizations
    /// Returns list of all visualizations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var visualizations = await _repo.GetAllAsync();
            return Ok(visualizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all visualizations");
            return Problem(
                detail: "An internal error occurred while retrieving visualizations.",
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// GET: /api/visualizations/5
    /// Returns a single visualization by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var visualization = await _repo.GetByIdAsync(id);
            if (visualization == null)
            {
                return NotFound(new { error = $"Visualization with ID {id} not found." });
            }

            return Ok(visualization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching visualization {Id}", id);
            return Problem(
                detail: "An internal error occurred while retrieving the visualization.",
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// POST: /api/visualizations
    /// Creates a new visualization from JSON
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] VisualizationCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var viz = new Visualization
            {
                Name = dto.Name,
                BitSize = dto.BitSize,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(viz);

            // Return 201 Created with location header
            return CreatedAtAction(
                nameof(GetById),
                new { id = viz.Id },
                new { id = viz.Id, message = "Visualization created successfully." }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating visualization");
            return Problem(
                detail: "An internal error occurred while creating the visualization.",
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// DELETE: /api/visualizations/5
    /// Deletes a visualization (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var viz = await _repo.GetByIdAsync(id);
            if (viz == null)
            {
                return NotFound(new { error = $"Visualization with ID {id} not found." });
            }

            await _repo.DeleteAsync(id);
            
            return Ok(new { message = $"Visualization '{viz.Name}' deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting visualization {Id}", id);
            return Problem(
                detail: "An internal error occurred while deleting the visualization.",
                statusCode: 500
            );
        }
    }
}