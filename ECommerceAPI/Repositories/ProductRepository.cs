using ECommerceAPI.DTOs;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public interface IProductRepository
    {
        Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<ProductDto> GetByIdAsync(int id);
        Task<int> InsertAsync(CreateProductDto dto);
        Task<bool> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> SoftDeleteAsync(int id);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            var list = new List<ProductDto>();
            int totalCount = 0;
            int offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Total count
            using (var countCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Products WHERE IsActive = 1", conn))
            {
                totalCount = (int)await countCmd.ExecuteScalarAsync();
            }

            // Paged data
            using var cmd = new SqlCommand(@"
                SELECT ProductId, ProductName, Description, Price, DiscountPrice,
                       Stock, ImageUrl, CategoryId, IsActive, CreatedAt
                FROM Products
                WHERE IsActive = 1
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProduct(reader));
            }

            return (list, totalCount);
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT ProductId, ProductName, Description, Price, DiscountPrice,
                       Stock, ImageUrl, CategoryId, IsActive, CreatedAt
                FROM Products 
                WHERE ProductId = @Id AND IsActive = 1", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapProduct(reader);
            return null;
        }

        public async Task<int> InsertAsync(CreateProductDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Products 
                    (ProductName, Description, Price, DiscountPrice, Stock, ImageUrl, CategoryId, IsActive)
                VALUES 
                    (@Name, @Desc, @Price, @DiscountPrice, @Stock, @ImageUrl, @CategoryId, 1);
                SELECT SCOPE_IDENTITY();", conn);

            cmd.Parameters.AddWithValue("@Name", dto.ProductName);
            cmd.Parameters.AddWithValue("@Desc", dto.Description ?? "");
            cmd.Parameters.AddWithValue("@Price", dto.Price);
            cmd.Parameters.AddWithValue("@DiscountPrice", (object?)dto.DiscountPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Stock", dto.Stock);
            cmd.Parameters.AddWithValue("@ImageUrl", dto.ImageUrl ?? "");
            cmd.Parameters.AddWithValue("@CategoryId", dto.CategoryId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE Products SET
                    ProductName = @Name,
                    Description = @Desc,
                    Price = @Price,
                    DiscountPrice = @DiscountPrice,
                    Stock = @Stock,
                    ImageUrl = @ImageUrl,
                    CategoryId = @CategoryId,
                    IsActive = @IsActive
                WHERE ProductId = @Id", conn);

            cmd.Parameters.AddWithValue("@Name", dto.ProductName);
            cmd.Parameters.AddWithValue("@Desc", dto.Description ?? "");
            cmd.Parameters.AddWithValue("@Price", dto.Price);
            cmd.Parameters.AddWithValue("@DiscountPrice", (object?)dto.DiscountPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Stock", dto.Stock);
            cmd.Parameters.AddWithValue("@ImageUrl", dto.ImageUrl ?? "");
            cmd.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
            cmd.Parameters.AddWithValue("@IsActive", dto.IsActive);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "UPDATE Products SET IsActive = 0 WHERE ProductId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private ProductDto MapProduct(SqlDataReader reader)
        {
            return new ProductDto
            {
                ProductId = (int)reader["ProductId"],
                ProductName = reader["ProductName"].ToString(),
                Description = reader["Description"].ToString(),
                Price = (decimal)reader["Price"],
                DiscountPrice = reader["DiscountPrice"] == DBNull.Value ? null : (decimal?)reader["DiscountPrice"],
                Stock = (int)reader["Stock"],
                ImageUrl = reader["ImageUrl"].ToString(),
                CategoryId = (int)reader["CategoryId"],
                IsActive = (bool)reader["IsActive"],
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }
    }
}