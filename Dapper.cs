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
                //return connection.Query<Country>("SELECT * FROM countries");
                return connection.Query<Country>("GetAllCountries", commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<State> GetStatesByCountry(int countryId)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                //return connection.Query<State>("SELECT * FROM states WHERE CountryId = @CountryId", new { CountryId = countryId });
                return connection.Query<State>("GetStatesByCountry", new { in_countryId = countryId }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<City> GetCitiesByState(int stateId)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                //return connection.Query<City>("SELECT * FROM cities WHERE StateId = @StateId", new { StateId = stateId });
                return connection.Query<City>("GetCitiesByState", new { in_stateId = stateId }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<product> GetAllProducts()
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();

                //return connection.Query<product>("select person.id, person.name,person.phone,person.address,person.file_name,person.file_content_type,person.FileContent,countries.Name AS country,states.Name AS state,cities.Name AS city from person\r\ninner join countries on person.countryId = countries.Id\r\ninner join `states` on person.stateId = `states`.Id\r\ninner join `cities` on person.cityId = `cities`.Id;");
                return connection.Query<product>("GetAllProducts", commandType: CommandType.StoredProcedure);
            }
        }

        public product GetProductById(int id)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                //return connection.QuerySingleOrDefault<product>("SELECT * FROM person WHERE Id = @Id", new { Id = id });
                return connection.QuerySingleOrDefault<product>("GetProductById", new { productId = id }, commandType: CommandType.StoredProcedure);
            }
        }

        public void InsertProduct(product product,IFormFile productFile)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                //string query = "INSERT INTO person (name, phone,address,file_name,file_content_type,FileContent,countryId,stateId,cityId) VALUES (@name, @phone,@address,@file_name,@file_content_type,@FileContent,@countryId,@stateId,@cityId)";
                //var parameters = new
                //{
                //    product_name = product.name,
                //    product_phone = product.phone,
                //    product_address = product.address,
                //    file_name = productFile?.FileName,
                //    file_content_type = productFile?.ContentType,
                //    file_content = product.FileContent,
                //    country_id = product.CountryId,
                //    state_id = product.StateId,
                //    city_id = product.cityId
                //};

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@product_name", product.name);
                parameters.Add("@product_phone", product.phone);
                parameters.Add("@product_address", product.address);
                parameters.Add("@file_name", product.file_name);
                parameters.Add("@file_content_type", product.file_content_type);
                
                parameters.Add("@countryId", product.CountryId);
                parameters.Add("@stateId", product.StateId);
                parameters.Add("@cityId", product.cityId);


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
                            parameters.Add("@fileContent", product.FileContent);
                        }

                    }
                    else
                    {
                        Console.WriteLine("ERROR goes to here");
                    }
                }
                //connection.Execute(query, product);
                connection.Execute("InsertProductSP", parameters, commandType: CommandType.StoredProcedure);
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
                //string query = "DELETE FROM person WHERE id = @id";
                //connection.Execute(query, new { Id = id });
                connection.Execute("DeleteProductById", new { productId = id }, commandType: CommandType.StoredProcedure);
            }
        }

    }
}
