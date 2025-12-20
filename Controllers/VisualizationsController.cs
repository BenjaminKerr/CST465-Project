using Microsoft.AspNetCore.Mvc;
using CST465_project.Models;
using CST465_project.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CST465_project.Controllers;

[Route("[controller]")]
public class VisualizationsController : Controller
{
    private readonly IVisualizationRepository _repo;
    private readonly ILogger<VisualizationsController> _logger;

    public VisualizationsController(IVisualizationRepository repo, ILogger<VisualizationsController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _repo.GetAllAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("api/list")]
    public async Task<IActionResult> GetApi()
    {
        try
        {
            var visualizations = await _repo.GetAllAsync();
            return Ok(visualizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching visualizations");
            return Problem(detail: "An internal error occurred while retrieving visualizations.", statusCode: 500);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var visualization = await _repo.GetByIdAsync(id);
            if (visualization == null)
                return NotFound();

            return Ok(visualization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching visualization {Id}", id);
            return Problem(detail: "An internal error occurred while retrieving the visualization.", statusCode: 500);
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostApi([FromBody] Visualization v)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            await _repo.AddAsync(v);
            return CreatedAtAction(nameof(GetById), new { id = v.Id }, v);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating visualization");
            return Problem(detail: "An internal error occurred while creating the visualization.", statusCode: 500);
        }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        // 1. Async Data Access
        var viz = await _repo.GetByIdAsync(id);

        if (viz == null) return NotFound();

        // 2. Business Logic: Constructing the file content
        var content = new System.Text.StringBuilder();
        content.AppendLine("--- QUANTUM VISUALIZATION REPORT ---");
        content.AppendLine($"ID: {viz.Id}");
        content.AppendLine($"Name: {viz.Name}");
        content.AppendLine($"Qubits: {viz.BitSize}");
        content.AppendLine($"Date Run: {viz.CreatedAt}");
        content.AppendLine($"Notes: {viz.Description}");
        content.AppendLine("------------------------------------");
        content.AppendLine("Simulation Status: Success");

        // 3. File Generation
        var bytes = System.Text.Encoding.UTF8.GetBytes(content.ToString());
        var fileName = $"Grovers_Run_{viz.Id}.txt";

        // Returns a standard file download response
        return File(bytes, "text/plain", fileName);
    }

    [HttpPost("api/save")]
    public async Task<IActionResult> Save([FromBody] VisualizationCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var viz = new Visualization
        {
            Name = dto.Name,
            BitSize = dto.BitSize,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        // Assuming you have a standard AddAsync method in your repo
        await _repo.AddAsync(viz);

        // Return JSON with the new ID so the frontend can generate a download link
        return Ok(new { id = viz.Id, message = "Saved successfully" });
    }
}