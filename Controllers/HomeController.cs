using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Services;
using project.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net;

namespace project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly RecipeService _recipeService;

    private readonly CategoryService _categoryService;

    private readonly RecipesDbContext _context;

    public HomeController(ILogger<HomeController> logger, RecipeService recipeService, CategoryService categoryService, RecipesDbContext context)
    {
        _logger = logger;
        _recipeService = recipeService;
        _categoryService = categoryService;
        _context = context;
    }

    public IActionResult title_page()
    {
        return View();
    }

    public IActionResult main_page(string query)
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
            HttpContext.Session.Clear();

        var categories = _categoryService.GetCategories();
        var recipes = _context.Recipes.Include("Category").ToList();

        int userId;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
        {
            var savedRecipes = _context.SavedRecipes.ToList();

            var userSaved = new List<SavedRecipe>();
            for (int i = 0; i < savedRecipes.Count; i++)
            {
                if (savedRecipes[i].userID == userId)
                {
                    userSaved.Add(savedRecipes[i]);
                }
            }

            for (int i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];

                bool found = false;
                for (int j = 0; j < userSaved.Count; j++)
                {
                    if (userSaved[j].recipeID == recipe.recipeID)
                    {
                        found = true;
                        break;
                    }
                }

                if (found) recipe.IsSaved = true;
            }
        }

        if (!string.IsNullOrEmpty(query))
        {
            string queryLower = query.ToLower();
            var filtered = new List<Recipe>();

            foreach (var r in recipes)
            {
                bool matches = false;

                if (!string.IsNullOrEmpty(r.recipe_name) && r.recipe_name.ToLower().Contains(queryLower)) matches = true;

                if (!matches && !string.IsNullOrEmpty(r.ingredients) && r.ingredients.ToLower().Contains(queryLower)) matches = true;

                if (matches) filtered.Add(r);
            }
            recipes = filtered;
        }

        var categoryCards = new List<CategoryWithRecipes>();

        foreach (var category in categories)
        {
            var exampleRecipes = new List<Recipe>();
            int count = 0;

            foreach (var recipe in recipes)
            {
                if (recipe.Category != null && recipe.Category.category_name == category.category_name)
                {
                    exampleRecipes.Add(recipe);
                    count++;
                    if (count == 3) break;
                }
            }

            categoryCards.Add(new CategoryWithRecipes
            {
                Category = category,
                Recipes = exampleRecipes
            });
        }

        var model = new MainPageViewModel
        {
            CategoryCards = categoryCards,
            Recipes = recipes
        };

        ViewData["Query"] = query;

        return View(model);
    }

    public IActionResult ShowCategories(string category_name)
    {
        var allCategories = _categoryService.GetCategories();
        Category? selectedCategory = null;

        foreach (var category in allCategories)
        {
            if (category.category_name == category_name)
            {
                selectedCategory = category;
                break;
            }
        }

        if (selectedCategory == null) return NotFound();

        var allRecipes = _recipeService.GetRecipes();
        var recipes = new List<Recipe>();

        foreach (var recipe in allRecipes)
        {
            if (recipe.categoryID == selectedCategory.categoryID) recipes.Add(recipe);
        }

        ViewBag.CategoryName = category_name;
        return View(recipes);
    }

    public IActionResult RecipeCards(int id)
    {
        var recipe = _recipeService.GetRecipeById(id);
        if (recipe == null) return NotFound();
        return View(recipe);
    }

    public IActionResult Details(int id)
    {
        Recipe recipe = _context.Recipes.Include(r => r.Author).FirstOrDefault(r => r.recipeID == id);
        if (recipe == null) return NotFound();

        bool isAuthor = false;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int currentUserId))
        {
            isAuthor = recipe.authorUserID == currentUserId;
        }

        ViewBag.IsAuthor = isAuthor;

        recipe.Category = _context.Categories.FirstOrDefault(c => c.categoryID == recipe.categoryID);

        string[] currentIngredients = recipe.IngredientsArr;

        var allCategoryRecipes = _context.Recipes.Where(r => r.categoryID == recipe.categoryID && r.recipeID != id).ToList();

        var similarRecipes = new List<Recipe>();
        foreach (var r in allCategoryRecipes)
        {
            r.Category = _context.Categories.FirstOrDefault(c => c.categoryID == r.categoryID);
            string[] rIngredients = r.IngredientsArr;

            int commonCount = rIngredients.Intersect(currentIngredients).Count();
            if (commonCount > 0) similarRecipes.Add(r);
        }
        ViewBag.SimilarRecipes = similarRecipes;

        var comments = _context.Comments.Where(c => c.recipeID == id && c.parentID == null).Include(c => c.User).Include(c => c.Replies).ThenInclude(r => r.User).ToList();

        ViewBag.Comments = comments;

        return View(recipe);
    }

    [HttpGet]
    public IActionResult AddRecipe()
    {
        var categoriesFromService = _categoryService.GetCategories();

        var filteredCategories = new List<Category>();

        if (categoriesFromService != null)
        {
            foreach (var c in categoriesFromService)
            {
                if (c == null) throw new Exception("знайдено null-елемент в категоріях");

                if (string.IsNullOrEmpty(c.category_name)) throw new Exception($"null категорії у ID {c.categoryID}");

                filteredCategories.Add(c);
            }
        }
        else throw new Exception("метод повернув null");

        var model = new AddRecipeModel { Categories = filteredCategories};

        return View(model);
    }

    [HttpPost]
    public IActionResult AddRecipe(AddRecipeModel model)
    {
        if (!ModelState.IsValid)
        {
            var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.ModelErrors = allErrors;

            model.Categories = _categoryService.GetCategories() ?? new List<Category>();
            return View(model);
        }

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            TimeSpan parsedTime = TimeSpan.Zero;
            if (!string.IsNullOrEmpty(model.TimeCooking)) TimeSpan.TryParse(model.TimeCooking, out parsedTime);

            var recipe = new Recipe
            {
                recipe_name = model.RecipeName,
                ingredients = model.Ingredients,
                instruction = model.Instruction,
                categoryID = model.CategoryID,
                time_cooking = parsedTime,
                authorUserID = userId
            };

            if (model.Photo != null && model.Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                var uploadPath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }

                recipe.Photo = "/uploads/" + fileName;
            }

            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            return RedirectToAction("main_page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "помилка при додаванні рецепта");

            ViewBag.ModelErrors = new List<string> { "помилка при збереженні рецепта" };

            model.Categories = _categoryService.GetCategories() ?? new List<Category>();
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult SaveRecipe(int recipeId)
    {
        var user = User;
        if (user?.Identity == null || !user.Identity.IsAuthenticated) return Json(new { success = false, message = "Авторизуйтесь для збереження рецепту" });

        if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId)) return Json(new { success = false, message = "Некорректний ID" });

        var savedList = _context.SavedRecipes.ToList();

        bool exists = savedList.Any(s => s.userID == userId && s.recipeID == recipeId);

        if (!exists)
        {
            var newSaved = new SavedRecipe
            {
                userID = userId,
                recipeID = recipeId
            };

            _context.SavedRecipes.Add(newSaved);
            _context.SaveChanges();
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult UnsaveRecipe(int recipeId)
    {
        if (!User.Identity?.IsAuthenticated ?? false) return Json(new { success = false, message = "Авторизуйтесь для збереження рецепту" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Json(new { success = false, message = "Некоректний ID" });

        if (!int.TryParse(userIdClaim.Value, out int userId)) return Json(new { success = false, message = "Некоректний ID" });

        var savedList = _context.SavedRecipes.ToList();

        var toRemove = savedList.FirstOrDefault(s => s.userID == userId && s.recipeID == recipeId);

        if (toRemove != null)
        {
            _context.SavedRecipes.Remove(toRemove);
            _context.SaveChanges();
        }

        return Json(new { success = true });
    }

    public IActionResult saved()
    {
        if (User?.Identity?.IsAuthenticated != true) return RedirectToAction("Login", "Acc");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return Json(new { success = false, message = "Некорректний ID" });

        var allSavedRecipes = _context.SavedRecipes.ToList();

        var savedRecipes = new List<Recipe>();

        foreach (var saved in allSavedRecipes)
        {
            if (saved.userID == userId)
            {
                if (saved.Recipe != null) savedRecipes.Add(saved.Recipe);
                else
                {
                    var recipe = _context.Recipes.Find(saved.recipeID);
                    if (recipe != null) savedRecipes.Add(recipe);
                }
            }
        }

        return View(savedRecipes);
    }

    [HttpGet]
    public IActionResult Questions()
    {
        return View();
    }

    [HttpPost]
    [Route("Home/Questions")]
    public async Task<IActionResult> Questions(string question, string email, int recipeId)
    {
        if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Будь ласка, заповніть всі поля.");
            return View();
        }

        try
        {
            var fromAddress = new MailAddress("ulanasamileva1@gmail.com", "Питання");
            var toAddress = new MailAddress("ulanasamileva1@gmail.com");
            const string fromPassword = "xmsy mnzu zaxx qgwe";
            string subject = "Питання від користувача";
            string body = $"Питання: {question}\nEmail для зворотнього зв'язку: {email}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                await smtp.SendMailAsync(message);
            }

            TempData["Success"] = "Ваше питання надіслано, дякуємо!";
            return RedirectToAction("Details", "Home", new { id = recipeId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Помилка при відправці повідомлення: " + ex.Message);
            return RedirectToAction("Details", "Home", new { id = recipeId });
        }
    }

    public IActionResult DownloadRecipe(int id)
    {
        var recipe = _context.Recipes.Include(r => r.Category).FirstOrDefault(r => r.recipeID == id);

        if (recipe == null) return NotFound();

        string content = $"Назва: {recipe.recipe_name}\n" +
                         $"Категорія: {recipe.Category?.category_name}\n" +
                         $"Час приготування: {recipe.time_cooking}\n" +
                         $"Інгредієнти: {recipe.ingredients}\n" +
                         $"Інструкція: {recipe.instruction}";

        byte[] contentArr = System.Text.Encoding.UTF8.GetBytes(content);
        return File(contentArr, "text/plain", $"{recipe.recipe_name}.txt");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var recipe = _context.Recipes.FirstOrDefault(r => r.recipeID == id);
        if (recipe == null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        int userId = int.Parse(userIdClaim);

        if (recipe.authorUserID != userId) return Forbid();

        var model = new AddRecipeModel
        {
            RecipeID = recipe.recipeID,
            RecipeName = recipe.recipe_name,
            Ingredients = recipe.ingredients,
            Instruction = recipe.instruction,
            CategoryID = recipe.categoryID,
            TimeCooking = recipe.time_cooking?.ToString(@"hh\:mm"),
            Categories = _categoryService.GetCategories()
        };

        return View("AddRecipe", model);
    }

    [HttpPost]
    public IActionResult Edit(AddRecipeModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = _categoryService.GetCategories();
            return View("AddRecipe", model);
        }

        var recipe = _context.Recipes.FirstOrDefault(r => r.recipeID == model.RecipeID);
        if (recipe == null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        int userId = int.Parse(userIdClaim);
        if (recipe.authorUserID != userId) return Forbid();

        recipe.recipe_name = model.RecipeName;
        recipe.ingredients = model.Ingredients;
        recipe.instruction = model.Instruction;
        recipe.categoryID = model.CategoryID;

        if (!string.IsNullOrEmpty(model.TimeCooking))
            recipe.time_cooking = TimeSpan.Parse(model.TimeCooking);

        if (model.Photo != null && model.Photo.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.Photo.FileName);
            var path = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            model.Photo.CopyTo(stream);

            recipe.Photo = "/uploads/" + fileName;
        }

        _context.SaveChanges();

        return RedirectToAction("Details", new { id = recipe.recipeID });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}