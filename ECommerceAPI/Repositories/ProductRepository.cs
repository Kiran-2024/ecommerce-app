using ECommerceAPI.DTOs;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public interface IProductRepository
    {
        Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetAllAsync(
            int page = 1, int pageSize = 10,
            string? search = null, int? categoryId = null,
            decimal? minPrice = null, decimal? maxPrice = null);
        Task<ProductDto?> GetByIdAsync(int id);
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

        public async Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetAllAsync(
     int page = 1, int pageSize = 10,
     string? search = null, int? categoryId = null,
     decimal? minPrice = null, decimal? maxPrice = null)
        {
            var list = new List<ProductDto>();
            int totalCount = 0;
            int offset = (page - 1) * pageSize;

            // Dynamic WHERE build
            var where = new List<string> { "IsActive = 1" };
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                where.Add("(ProductName LIKE @Search OR Description LIKE @Search)");
                parameters.Add(new SqlParameter("@Search", $"%{search}%"));
            }
            if (categoryId.HasValue)
            {
                where.Add("CategoryId = @CategoryId");
                parameters.Add(new SqlParameter("@CategoryId", categoryId.Value));
            }
            if (minPrice.HasValue)
            {
                where.Add("Price >= @MinPrice");
                parameters.Add(new SqlParameter("@MinPrice", minPrice.Value));
            }
            if (maxPrice.HasValue)
            {
                where.Add("Price <= @MaxPrice");
                parameters.Add(new SqlParameter("@MaxPrice", maxPrice.Value));
            }

            string whereClause = string.Join(" AND ", where);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Total count
            using (var countCmd = new SqlCommand(
                $"SELECT COUNT(*) FROM Products WHERE {whereClause}", conn))
            {
                countCmd.Parameters.AddRange(parameters.Select(p =>
                    new SqlParameter(p.ParameterName, p.Value)).ToArray());
                totalCount = (int)await countCmd.ExecuteScalarAsync();
            }

            // Paged data
            using var cmd = new SqlCommand($@"
        SELECT ProductId, ProductName, Description, Price, DiscountPrice,
               Stock, ImageUrl, CategoryId, IsActive, CreatedAt
        FROM Products
        WHERE {whereClause}
        ORDER BY CreatedAt DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

            cmd.Parameters.AddRange(parameters.Select(p =>
                new SqlParameter(p.ParameterName, p.Value)).ToArray());
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