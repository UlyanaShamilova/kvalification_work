using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using project.Models;
using System.Collections.Generic;

[Route("api/chat")]
[ApiController]
public class ChatBotApiController : ControllerBase
{
    private readonly string _connectionString;

    public ChatBotApiController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new ArgumentNullException("Connection string not found.");
    }

[HttpPost("ask")]
public IActionResult Ask([FromBody] ChatRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Question))
        return BadRequest(new { answer = "Введіть свій запит" });

    string query = request.Question.Trim().ToLower();
    var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (words.Length == 0)
        return Ok(new { answer = "Запит занадто короткий" });

    using var conn = new MySqlConnection(_connectionString);
    conn.Open();

    // Получаем все рецепты, где хотя бы одно слово совпадает с названием или ингредиентами
    var whereParts = new List<string>();
    for (int i = 0; i < words.Length; i++)
        whereParts.Add($"LOWER(recipe_name) LIKE @w{i} OR LOWER(ingredients) LIKE @w{i}");

    string whereClause = string.Join(" OR ", whereParts);

    var cmd = new MySqlCommand($@"
        SELECT recipe_name, ingredients, instruction
        FROM recipes
        WHERE {whereClause};", conn);

    for (int i = 0; i < words.Length; i++)
        cmd.Parameters.AddWithValue($"@w{i}", $"%{words[i]}%");

    var recipes = new List<Recipe>();
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        recipes.Add(new Recipe
        {
            recipe_name = reader.GetString(0),
            ingredients = reader.IsDBNull(1) ? "" : reader.GetString(1),
            instruction = reader.IsDBNull(2) ? "" : reader.GetString(2)
        });
    }

    if (recipes.Count == 0)
        return Ok(new { answer = "Перепрошую, я поки не знаю цього рецепту." });

    // Выбираем наиболее релевантный рецепт по количеству совпадений слов
    Recipe bestMatch = null;
    int maxMatches = -1;
    foreach (var r in recipes)
    {
        int count = 0;
        string content = (r.recipe_name + " " + r.ingredients).ToLower();
        foreach (var w in words)
            if (content.Contains(w)) count++;

        if (count > maxMatches)
        {
            maxMatches = count;
            bestMatch = r;
        }
    }

    return Ok(new
    {
        ingredients = bestMatch.ingredients,
        instructions = bestMatch.instruction
    });
}


private List<Recipe> SearchRecipesByKeywords(string query)
{
    var list = new List<Recipe>();
    using var conn = new MySqlConnection(_connectionString);
    conn.Open();

    // Сначала ищем точное совпадение в названии блюда
    var cmdExact = new MySqlCommand(@"
        SELECT ingredients, instruction
        FROM recipes
        WHERE LOWER(recipe_name) = LOWER(@query)
        LIMIT 1;", conn);
    cmdExact.Parameters.AddWithValue("@query", query.Trim());

    using var readerExact = cmdExact.ExecuteReader();
    if (readerExact.Read())
    {
        list.Add(new Recipe
        {
            ingredients = readerExact.IsDBNull(0) ? null : readerExact.GetString(0),
            instruction = readerExact.IsDBNull(1) ? null : readerExact.GetString(1)
        });
        return list; // если нашли точное название — возвращаем сразу
    }
    readerExact.Close();

    // Если точного названия нет — ищем по ключевым словам
    string[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (words.Length == 0)
        return list;

    var whereParts = new List<string>();
    for (int i = 0; i < words.Length; i++)
    {
        whereParts.Add($"recipe_name LIKE @w{i} OR ingredients LIKE @w{i}");
    }
    string whereClause = string.Join(" OR ", whereParts);

    var cmd = new MySqlCommand($@"
        SELECT ingredients, instruction
        FROM recipes
        WHERE {whereClause}
        LIMIT 10;", conn);

    for (int i = 0; i < words.Length; i++)
    {
        cmd.Parameters.AddWithValue($"@w{i}", $"%{words[i]}%");
    }

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        list.Add(new Recipe
        {
            ingredients = reader.IsDBNull(0) ? null : reader.GetString(0),
            instruction = reader.IsDBNull(1) ? null : reader.GetString(1)
        });
    }

    return list;
}


}
