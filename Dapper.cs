using System.Data;
using Dapper;
using join.Models;
using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyDapperMvcApp.Data
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public object ModelState { get; private set; }

        private bool IsAllowedFileType(string contentType)
        {
            // List of allowed content types for PDF and images
            string[] allowedTypes = { "application/pdf", "image/jpeg", "image/png", "image/gif" };

            return allowedTypes.Contains(contentType);
        }

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public IEnumerable<Country> GetAllCountries()
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                return connection.Query<Country>("SELECT * FROM countries");
            }
        }

        public IEnumerable<State> GetStatesByCountry(int countryId)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                return connection.Query<State>("SELECT * FROM states WHERE CountryId = @CountryId", new { CountryId = countryId });
            }
        }

        public IEnumerable<City> GetCitiesByState(int stateId)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                return connection.Query<City>("SELECT * FROM cities WHERE StateId = @StateId", new { StateId = stateId });
            }
        }
        public IEnumerable<product> GetAllProducts()
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();

                return connection.Query<product>("SELECT * FROM person");
            }
        }

        public product GetProductById(int id)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<product>("SELECT * FROM person WHERE Id = @Id", new { Id = id });
            }
        }

        public void InsertProduct(product product,IFormFile productFile)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO person (name, phone,address,file_name,file_content_type,FileContent,countryId,stateId) VALUES (@name, @phone,@address,@file_name,@file_content_type,@FileContent,@countryId,@stateId)";

                if (productFile != null && productFile.Length > 0)
                {
                    if (IsAllowedFileType(productFile.ContentType))
                    {
                        // Process and save the uploaded file
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", productFile.FileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                                productFile.CopyTo(stream);
                        }
                        using (var memoryStream = new MemoryStream())
                        {
                            productFile.CopyTo(memoryStream);
                            product.FileContent = memoryStream.ToArray();
                        }

                    }
                    else
                    {
                        Console.WriteLine("ERROR goes to here");
                    }
                }
                connection.Execute(query, product);
            }
        }

        public void UpdateProduct(product product, IFormFile productFile)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                string query = "UPDATE person SET Name = @name, phone = @phone,country=@country,file_name = @file_name,file_content_type=@file_content_type WHERE id = @id";
                connection.Execute(query, product);

                var parameters = new
                {
                    id = product.id,
                    name = product.name,
                    phone = product.phone,
                    country = product.country,
                    file_name = productFile != null ? productFile.FileName : product.file_name,
                    file_content_type = productFile != null ? product.file_content_type : productFile.FileName
                };

                connection.Execute(query, parameters);

                // Handle updated file if provided
                if (productFile != null && productFile.Length > 0)
                {
                    if (IsAllowedFileType(productFile.ContentType))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", productFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            productFile.CopyTo(stream);
                        }
                    }
                    else
                    {
                        // Handle invalid file type
                    }
                }
            }
        }
        public void DeleteProduct(int id)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM person WHERE id = @id";
                connection.Execute(query, new { Id = id });
            }
        }

    }
}
