using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using project.Models;

namespace project.Services
{
    public class CategoryService
    {
        private readonly string _connectionString;

        public CategoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");
        }

        public List<Category> GetCategories()
        {
            var list = new List<Category>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("SELECT categoryID, category_name, photo FROM category", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    categoryID = reader.GetInt32(0),
                    category_name = reader.GetString(1),
                    Photo = reader["photo"] != DBNull.Value ? (byte[])reader["photo"] : null
                });
            }
            return list;
        }
    }
}