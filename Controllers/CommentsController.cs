using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using System.Threading.Tasks;

namespace project.Controllers
{
    // только залогиненные пользователи могут работать с комментами
    public class CommentsController : Controller
    {
        private readonly RecipesDbContext _context;

        public CommentsController(RecipesDbContext context)
        {
            _context = context;
        }

    // add comment (AJAX)
    [HttpPost]
    public async Task<IActionResult> Add(int recipeID, string text, int? parentID)
    {
if (string.IsNullOrWhiteSpace(text))
        return BadRequest("Коментар пустий");

    // Берём userId из сессии
    int? userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return StatusCode(401, "Користувач не авторизований або UserId не знайдено в сесії");

    // Создаём новый комментарий
    var comment = new Comments
    {
        recipeID = recipeID,
        userID = userId.Value,
        text = text,
        parentID = parentID
    };

    _context.Comments.Add(comment);
    await _context.SaveChangesAsync();

    // Загружаем пользователя вручную
    User[] allUsers = _context.Users.ToArray();
    for (int i = 0; i < allUsers.Length; i++)
    {
        if (allUsers[i].userID == comment.userID)
        {
            comment.User = allUsers[i];
            break;
        }
    }

    // Если есть родительский комментарий — загружаем вручную
    if (comment.parentID != null)
    {
        Comments[] allComments = _context.Comments.ToArray();
        for (int i = 0; i < allComments.Length; i++)
        {
            if (allComments[i].rewiewID == comment.parentID)
            {
                comment.Parent = allComments[i];
                break;
            }
        }
    }

    // Возвращаем partial для одного комментария
    return PartialView("~/Views/Comments/Comments.cshtml", comment);
    }


        // delete comment
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized();

                // ищем комментарий вручную
                Comments[] allComments = _context.Comments.ToArray();
                Comments? comment = null;
                for (int i = 0; i < allComments.Length; i++)
                {
                    if (allComments[i].rewiewID == id)
                    {
                        comment = allComments[i];
                        break;
                    }
                }

                if (comment == null)
                    return NotFound();

                if (comment.userID != userId.Value && !IsAdmin(userId.Value))
                    return Forbid();

                _context.Comments.Remove(comment);
                _context.SaveChanges();

                return Ok();

        }

        // здесь твоя логика проверки на администратора
        private bool IsAdmin(int userId)
        {
            // пример — проверка по таблице Users
            var user = _context.Users.FirstOrDefault(u => u.userID == userId);
            return user != null && user.username == "Admin";
        }
    }
}
