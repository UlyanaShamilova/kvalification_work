using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Services;
using project.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        {
            HttpContext.Session.Clear();
        }

        var categories = _categoryService.GetCategories();
        var recipes = _context.Recipes.Include("Category").ToList();

        // 🔍 Поиск
        if (!string.IsNullOrEmpty(query))
        {
            string queryLower = query.ToLower();
            var filtered = new List<Recipe>();

            foreach (var r in recipes)
            {
                bool matches = false;

                // По названию
                if (!string.IsNullOrEmpty(r.recipe_name) && r.recipe_name.ToLower().Contains(queryLower))
                    matches = true;

                // По ингредиентам
                if (!matches && !string.IsNullOrEmpty(r.ingredients) && r.ingredients.ToLower().Contains(queryLower))
                    matches = true;

                if (matches)
                    filtered.Add(r);
            }

            recipes = filtered;
        }

        // Формируем карточки категорий (как у тебя было)
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

        ViewData["Query"] = query; // чтобы заполнить поле поиска в Razor
        
        return View(model);
    }

    public IActionResult ShowCategories(string category_name)
    {
        var allCategories = _categoryService.GetCategories();
        Category? selectedCategory = null;

        // Найдём нужную категорию по имени
        foreach (var category in allCategories)
        {
            if (category.category_name == category_name)
            {
                selectedCategory = category;
                break;
            }
        }

        // Если такой категории нет — 404
        if (selectedCategory == null)
        {
            return NotFound();
        }

        var allRecipes = _recipeService.GetRecipes();
        var recipes = new List<Recipe>();

        // Добавим только рецепты с нужным CategoryId
        foreach (var recipe in allRecipes)
        {
            if (recipe.categoryID == selectedCategory.categoryID)
            {
                recipes.Add(recipe);
            }
        }

        ViewBag.CategoryName = category_name;
        return View(recipes);
    }


    public IActionResult RecipeCards(int id)
    {
        var recipe = _recipeService.GetRecipeById(id);
        if (recipe == null)
        {
            return NotFound();
        }
        return View(recipe); // Передаёт рецепт в представление
    }

    // public IActionResult details(int id)
    // {
    //     var recipe = _recipeService.GetRecipeById(id); // реализуй метод в сервисе

    //     if (recipe == null) return NotFound();

    //     return View(recipe);
    // }

    public IActionResult details(int id)
    {
            // 1. Загружаем все рецепты
            Recipe[] allRecipes = _context.Recipes.ToArray();
            Recipe? recipe = null;
            for (int i = 0; i < allRecipes.Length; i++)
            {
                if (allRecipes[i].recipeID == id)
                {
                    recipe = allRecipes[i];
                    break;
                }
            }

            if (recipe == null)
                return NotFound();

            // 2. Загружаем категорию рецепта
            Category[] allCategories = _context.Categories.ToArray();
            for (int i = 0; i < allCategories.Length; i++)
            {
                if (allCategories[i].categoryID == recipe.categoryID)
                {
                    recipe.Category = allCategories[i];
                    break;
                }
            }

            // 3. Загружаем все комментарии и всех пользователей
            Comments[] allComments = _context.Comments.ToArray();
            User[] allUsers = _context.Users.ToArray();

            // массив для корневых комментариев
            Comments[] recipeComments = new Comments[allComments.Length];
            int rootCount = 0;

            for (int i = 0; i < allComments.Length; i++)
            {
                if (allComments[i].recipeID == id && allComments[i].parentID == null)
                {
                    // привязываем пользователя
                    for (int j = 0; j < allUsers.Length; j++)
                    {
                        if (allUsers[j].userID == allComments[i].userID)
                        {
                            allComments[i].User = allUsers[j];
                            break;
                        }
                    }

                    // находим ответы на этот комментарий
                    Comments[] repliesTemp = new Comments[allComments.Length];
                    int replyCount = 0;

                    for (int k = 0; k < allComments.Length; k++)
                    {
                        // Проверяем, что parentID есть и совпадает с текущим корневым комментарием
                        if (allComments[k].parentID.GetValueOrDefault() == allComments[i].rewiewID)
                        {
                            // Привязываем пользователя к комментарию, если User null
                            allComments[k].User ??= allUsers.FirstOrDefault(u => u.userID == allComments[k].userID) ?? new User { username = "Неизвестный" };


                            // Добавляем в массив ответов
                            repliesTemp[replyCount++] = allComments[k];
                        }
                    }


                    // обрезаем массив ответов
                    if (replyCount > 0)
                    {
                        Comments[] finalReplies = new Comments[replyCount];
                        for (int r = 0; r < replyCount; r++)
                            finalReplies[r] = repliesTemp[r];
                        allComments[i].Replies = finalReplies;
                    }
                    else
                    {
                        allComments[i].Replies = new Comments[0];
                    }

                    recipeComments[rootCount] = allComments[i];
                    rootCount++;
                }
            }

            // обрезаем массив корневых комментариев
            Comments[] finalComments = new Comments[rootCount];
for (int i = 0; i < rootCount; i++)
{
    finalComments[i] = recipeComments[i];

    if (finalComments[i].Replies == null)
        finalComments[i].Replies = new Comments[0];

    if (finalComments[i].User == null)
        finalComments[i].User = new User { username = "Неизвестный" };
}

            ViewBag.Comments = finalComments;

            return View(recipe);
        
    }



    [HttpGet]
public IActionResult AddRecipe()
{
    // Получаем категории из сервиса
    var categoriesFromService = _categoryService.GetCategories();

    // Создаем новый список и проверяем каждую категорию
    var filteredCategories = new List<Category>();

    if (categoriesFromService != null)
    {
        foreach (var c in categoriesFromService)
        {
            if (c == null)
            {
                throw new Exception("Найден null-элемент в категориях!");
            }
if (string.IsNullOrEmpty(c.category_name))
{
    throw new Exception($"Пустое или null имя категории у ID {c.categoryID}");
}
            filteredCategories.Add(c);
        }
    }
    else
    {
        throw new Exception("Метод GetCategories() вернул null!");
    }

    // Создаем модель с проверенным списком категорий
    var model = new AddRecipeModel
    {
        Categories = filteredCategories
    };

    return View(model);
}


   [HttpPost]
public IActionResult AddRecipe(AddRecipeModel model)
{
    // 1️⃣ Проверяем валидацию модели
    if (!ModelState.IsValid)
    {
        var allErrors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
        ViewBag.ModelErrors = allErrors;

        // Загружаем категории заново для возврата в View
        model.Categories = _categoryService.GetCategories() ?? new List<Category>();
        return View(model);
    }

    try
    {
        // 2️⃣ Создаём объект Recipe
        var recipe = new Recipe
        {
            recipe_name = model.RecipeName,
            ingredients = model.Ingredients,
            instruction = model.Instruction,
            categoryID = model.CategoryID
        };

        // 3️⃣ Работа с фото
        if (model.Photo != null && model.Photo.Length > 0)
        {
            // Проверяем, существует ли папка uploads
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
            var uploadPath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                model.Photo.CopyTo(fileStream);
            }

            recipe.Photo = "/uploads/" + fileName;
        }

        // 4️⃣ Сохраняем рецепт в базе
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        // 5️⃣ Перенаправляем на главную страницу после успешного добавления
        return RedirectToAction("main_page");
    }
    catch (Exception ex)
    {
        // 6️⃣ Логируем ошибки
        _logger.LogError(ex, "Ошибка при добавлении рецепта");

        ViewBag.ModelErrors = new List<string> { "Произошла ошибка при сохранении рецепта. Попробуйте ещё раз." };
        
        // Загружаем категории заново для возврата в View
        model.Categories = _categoryService.GetCategories() ?? new List<Category>();
        return View(model);
    }
}


// [HttpPost]
// public IActionResult SaveRecipe(int recipeId)
// {
//     if (!User.Identity.IsAuthenticated)
//     {
//         return Json(new { success = false, message = "Авторизуйтесь для збереження рецепту" });
//     }

//     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

//     // Загружаем все сохранённые рецепты из БД
//     var savedList = _context.SavedRecipes.ToList();

//     // Проверяем, есть ли уже такой рецепт
//     bool exists = false;
//     foreach (var saved in savedList)
//     {
//         if (saved.userID == userId && saved.recipeID == recipeId)
//         {
//             exists = true;
//             break;
//         }
//     }

//     if (!exists)
//     {
//         var newSaved = new SavedRecipe
//         {
//             userID = userId,
//             recipeID = recipeId
//         };

//         _context.SavedRecipes.Add(newSaved);
//         _context.SaveChanges();
//     }

//     return Json(new { success = true });
// }

[HttpPost]
public IActionResult SaveRecipe(int recipeId)
{
    var user = User;
    if (user?.Identity == null || !user.Identity.IsAuthenticated)
    {
        return Json(new { success = false, message = "Авторизуйтесь для збереження рецепту" });
    }

    if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
    {
        return Json(new { success = false, message = "Некорректный User ID" });
    }




    // Загружаем все сохранённые рецепты из БД
    var savedList = _context.SavedRecipes.ToList();

    // Проверяем, есть ли уже такой рецепт
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


    // [HttpPost]
    // public IActionResult UnsaveRecipe(int recipeId)
    // {
    //     if (!User.Identity?.IsAuthenticated ?? false)
    //     {
    //         return Json(new { success = false, message = "Авторизуйтесь для збереження рецепту" });
    //     }

    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

    //     // Загружаем все сохранённые рецепты из БД
    //     var savedList = _context.SavedRecipes.ToList();

    //     SavedRecipe toRemove = null;
    //     foreach (var saved in savedList)
    //     {
    //         if (saved.userID == userId && saved.recipeID == recipeId)
    //         {
    //             toRemove = saved;
    //             break;
    //         }
    //     }

    //     if (toRemove != null)
    //     {
    //         _context.SavedRecipes.Remove(toRemove);
    //         _context.SaveChanges();
    //     }

    //     return Json(new { success = true });
    // }

    [HttpPost]
public IActionResult UnsaveRecipe(int recipeId)
{
    if (!User.Identity?.IsAuthenticated ?? false)  // проверяем безопасно
    {
        return Json(new { success = false, message = "Авторизуйтесь для збереження рецепту" });
    }

    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Json(new { success = false, message = "User not found" });
    }

    if (!int.TryParse(userIdClaim.Value, out int userId))
    {
        return Json(new { success = false, message = "Invalid User ID" });
    }

    // Загружаем все сохранённые рецепты из БД
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
        if (User?.Identity?.IsAuthenticated != true) return RedirectToAction("Login", "Account");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Json(new { success = false, message = "Некорректный User ID" });
        }

        // int userId = int.Parse(userIdClaim.Value);


        // Получаем все сохранённые рецепты из базы
        var allSavedRecipes = _context.SavedRecipes.ToList();

        // Создаём список рецептов, которые сохранил пользователь
        var savedRecipes = new List<Recipe>();

        foreach (var saved in allSavedRecipes)
        {
            if (saved.userID == userId)
            {
                if (saved.Recipe != null)
                {
                    savedRecipes.Add(saved.Recipe);
                }
                else
                {
                    // Если навигационное свойство Recipe не загружено,
                    // можно загрузить рецепт вручную из базы
                    var recipe = _context.Recipes.Find(saved.recipeID);
                    if (recipe != null)
                    {
                        savedRecipes.Add(recipe);
                    }
                }
            }
        }

        return View(savedRecipes);
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
