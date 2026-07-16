using ECommerceAPI.DTO_s;

using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<int> InsertAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<CategoryDto>> GetAllForAdminAsync();
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var list = new List<CategoryDto>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM Categories WHERE IsActive = 1", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new CategoryDto
                {
                    CategoryId = (int)reader["CategoryId"],
                    Name = reader["CategoryName"].ToString(),
                    Description = reader["Description"].ToString(),
                    IsActive = (bool)reader["IsActive"]
                });
            }
            return list;
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM Categories WHERE CategoryId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CategoryDto
                {
                    CategoryId = (int)reader["CategoryId"],
                    Name = reader["CategoryName"].ToString(),
                    Description = reader["Description"].ToString(),
                    IsActive = (bool)reader["IsActive"]
                };
            }
            return null;
        }

        public async Task<int> InsertAsync(CreateCategoryDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"INSERT INTO Categories (CategoryName, Description, IsActive) 
                  VALUES (@Name, @Desc, 1); 
                  SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Desc", dto.Description ?? "");
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"UPDATE Categories 
                  SET CategoryName=@Name, Description=@Desc, IsActive=@IsActive 
                  WHERE CategoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Desc", dto.Description ?? "");
            cmd.Parameters.AddWithValue("@IsActive", dto.IsActive);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "UPDATE Categories SET IsActive=0 WHERE CategoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllForAdminAsync()
        {
            var list = new List<CategoryDto>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM Categories", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new CategoryDto
                {
                    CategoryId = (int)reader["CategoryId"],
                    Name = reader["CategoryName"].ToString(),
                    Description = reader["Description"].ToString(),
                    IsActive = (bool)reader["IsActive"]
                });
            }
            return list;
        }
    }
}