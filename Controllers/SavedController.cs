using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class SavedController : Controller
{
    private readonly RecipesDbContext _db;

    public SavedController(RecipesDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var savedRecipes = await (
            from sr in _db.SavedRecipes
            join r in _db.Recipes on sr.recipeID equals r.recipeID
            where sr.userID == userId
            select r
        ).ToListAsync();

        return View("~/Views/Home/main_page.cshtml", savedRecipes);
    }

    [HttpPost]
    public async Task<IActionResult> Save(int recipeId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var alreadySaved = await (
            from sr in _db.SavedRecipes
            where sr.userID == userId && sr.recipeID == recipeId
            select sr
        ).AnyAsync();

        if (!alreadySaved)
        {
            var newSaved = new SavedRecipe
            {
                userID = userId,
                recipeID = recipeId
            };

            _db.SavedRecipes.Add(newSaved);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("main_page", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int recipeId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return Json(new { success = false, message = "Некоректний ID" });

        var saved = await (
            from sr in _db.SavedRecipes
            where sr.userID == userId && sr.recipeID == recipeId
            select sr
        ).FirstOrDefaultAsync();

        if (saved != null)
        {
            _db.SavedRecipes.Remove(saved);
            await _db.SaveChangesAsync();
        }

        var savedRecipes = await (
            from sr in _db.SavedRecipes
            join r in _db.Recipes on sr.recipeID equals r.recipeID
            where sr.userID == userId
            select r
        ).ToListAsync();

        return View("~/Views/Home/saved.cshtml", savedRecipes);
    }
}