using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CST465_project.Models;
using CST465_project.Repositories;

namespace CST465_project.Controllers;

/// <summary>
/// MVC Controller - Returns Views for visualization management
/// </summary>
public class VisualizationsController : Controller
{
    private readonly IVisualizationRepository _repo;
    private readonly ILogger<VisualizationsController> _logger;

    public VisualizationsController(IVisualizationRepository repo, ILogger<VisualizationsController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET: /Visualizations
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await _repo.GetAllAsync();
            return View(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading visualizations list");
            TempData["ErrorMessage"] = "Error loading visualizations. Please try again.";
            return View(new List<Visualization>());
        }
    }

    // GET: /Visualizations/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var viz = await _repo.GetByIdAsync(id);
            if (viz == null)
            {
                TempData["ErrorMessage"] = "Visualization not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(viz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading visualization {Id}", id);
            TempData["ErrorMessage"] = "Error loading visualization details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Visualizations/Create
    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Visualizations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(VisualizationCreateDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var viz = new Visualization
            {
                Name = model.Name,
                BitSize = model.BitSize,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(viz);
            TempData["SuccessMessage"] = "Visualization created successfully!";
            return RedirectToAction(nameof(Details), new { id = viz.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating visualization");
            ModelState.AddModelError("", "Error creating visualization. Please try again.");
            return View(model);
        }
    }

    // POST: /Visualizations/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var viz = await _repo.GetByIdAsync(id);
            if (viz == null)
            {
                TempData["ErrorMessage"] = "Visualization not found.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Deleted visualization '{viz.Name}'.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting visualization {Id}", id);
            TempData["ErrorMessage"] = "Error deleting visualization.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Visualizations/Download/5
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var viz = await _repo.GetByIdAsync(id);
            if (viz == null)
            {
                return NotFound();
            }

            // Build the report content
            var content = new System.Text.StringBuilder();
            content.AppendLine("--- QUANTUM VISUALIZATION REPORT ---");
            content.AppendLine($"ID: {viz.Id}");
            content.AppendLine($"Name: {viz.Name}");
            content.AppendLine($"Qubits: {viz.BitSize}");
            content.AppendLine($"Date Run: {viz.CreatedAt}");
            content.AppendLine($"Notes: {viz.Description ?? "None"}");
            content.AppendLine("------------------------------------");
            content.AppendLine("Simulation Status: Success");

            var bytes = System.Text.Encoding.UTF8.GetBytes(content.ToString());
            var fileName = $"Grovers_Run_{viz.Id}_{DateTime.Now:yyyyMMdd}.txt";

            return File(bytes, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading visualization {Id}", id);
            TempData["ErrorMessage"] = "Error downloading visualization.";
            return RedirectToAction(nameof(Index));
        }
    }
}