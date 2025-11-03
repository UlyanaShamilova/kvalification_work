using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using project.Models;

namespace project.Services
{
    public class RecipeService
    {
        private readonly string _connectionString;

        public RecipeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");
        }

        public List<Recipe> GetRecipes()
        {
            var list = new List<Recipe>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT r.recipeID, r.recipe_name, r.categoryID, r.Photo, r.time_cooking, c.category_name
                FROM recipes r
                JOIN category c ON r.categoryID = c.categoryID", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Recipe
                {
                    recipeID = reader.GetInt32(0),
                    recipe_name = reader.GetString(1),
                    categoryID = reader.GetInt32(2),
                    Category = new Category
                    {
                        category_name = reader.GetString(5)
                    },
                    Photo = reader.IsDBNull(3) ? "/images/default.jpg" : reader.GetString(3),
                    time_cooking = reader.IsDBNull(4) ? (TimeSpan?)null : reader.GetTimeSpan(4),
                    ingredients = null,
                    instruction = null
                });
            }
            return list;
        }

        public Recipe GetRecipeById(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT r.recipeID, r.recipe_name, c.category_name, r.Photo, r.time_cooking, r.ingredients, r.instruction
                FROM recipes r
                JOIN category c ON r.categoryID = c.categoryID
                WHERE r.recipeID = @id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                TimeSpan? timeCooking = null;
                if (!reader.IsDBNull(4)) timeCooking = reader.GetTimeSpan(4);

                return new Recipe
                {
                    recipeID = reader.GetInt32(0),
                    recipe_name = reader.GetString(1),
                    Category = new Category
                    {
                        category_name = reader.GetString(2)
                    },
                    time_cooking = reader.IsDBNull(4) ? (TimeSpan?)null : reader.GetTimeSpan(4),
                    ingredients = reader.IsDBNull(5) ? null : reader.GetString(5),
                    instruction = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }
            return null;
        }
    }
}
