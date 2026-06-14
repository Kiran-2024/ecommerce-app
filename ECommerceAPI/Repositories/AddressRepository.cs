using ECommerceAPI.DTO_s;
using ECommerceAPI.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

public class AddressRepository : IAddressRepository
{
    private readonly string _connectionString;

    public AddressRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId)
    {
        var addresses = new List<AddressDto>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT * FROM Addresses WHERE UserId = @UserId ORDER BY IsDefault DESC, CreatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            addresses.Add(MapToDto(reader));
        }
        return addresses;
    }

    public async Task<AddressDto?> GetByIdAsync(int addressId, int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT * FROM Addresses WHERE AddressId = @AddressId AND UserId = @UserId", conn);
        cmd.Parameters.AddWithValue("@AddressId", addressId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync()) return MapToDto(reader);
        return null;
    }

    public async Task<int> CreateAsync(int userId, CreateAddressDto dto)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (dto.IsDefault)
        {
            using var resetCmd = new SqlCommand("UPDATE Addresses SET IsDefault = 0 WHERE UserId = @UserId", conn);
            resetCmd.Parameters.AddWithValue("@UserId", userId);
            await resetCmd.ExecuteNonQueryAsync();
        }

        using var cmd = new SqlCommand(@"
            INSERT INTO Addresses (UserId, FullName, PhoneNumber, AddressLine1, AddressLine2, City, State, PinCode, IsDefault, CreatedAt)
            VALUES (@UserId, @FullName, @PhoneNumber, @AddressLine1, @AddressLine2, @City, @State, @PinCode, @IsDefault, GETDATE());
            SELECT SCOPE_IDENTITY();", conn);

        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FullName", dto.FullName);
        cmd.Parameters.AddWithValue("@PhoneNumber", dto.PhoneNumber);
        cmd.Parameters.AddWithValue("@AddressLine1", dto.AddressLine1);
        cmd.Parameters.AddWithValue("@AddressLine2", (object?)dto.AddressLine2 ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", dto.City);
        cmd.Parameters.AddWithValue("@State", dto.State);
        cmd.Parameters.AddWithValue("@PinCode", dto.PinCode);
        cmd.Parameters.AddWithValue("@IsDefault", dto.IsDefault);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(int addressId, int userId, CreateAddressDto dto)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (dto.IsDefault)
        {
            using var resetCmd = new SqlCommand("UPDATE Addresses SET IsDefault = 0 WHERE UserId = @UserId", conn);
            resetCmd.Parameters.AddWithValue("@UserId", userId);
            await resetCmd.ExecuteNonQueryAsync();
        }

        using var cmd = new SqlCommand(@"
            UPDATE Addresses SET
                FullName = @FullName,
                PhoneNumber = @PhoneNumber,
                AddressLine1 = @AddressLine1,
                AddressLine2 = @AddressLine2,
                City = @City,
                State = @State,
                PinCode = @PinCode,
                IsDefault = @IsDefault
            WHERE AddressId = @AddressId AND UserId = @UserId", conn);

        cmd.Parameters.AddWithValue("@AddressId", addressId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FullName", dto.FullName);
        cmd.Parameters.AddWithValue("@PhoneNumber", dto.PhoneNumber);
        cmd.Parameters.AddWithValue("@AddressLine1", dto.AddressLine1);
        cmd.Parameters.AddWithValue("@AddressLine2", (object?)dto.AddressLine2 ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", dto.City);
        cmd.Parameters.AddWithValue("@State", dto.State);
        cmd.Parameters.AddWithValue("@PinCode", dto.PinCode);
        cmd.Parameters.AddWithValue("@IsDefault", dto.IsDefault);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int addressId, int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("DELETE FROM Addresses WHERE AddressId = @AddressId AND UserId = @UserId", conn);
        cmd.Parameters.AddWithValue("@AddressId", addressId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await conn.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> SetDefaultAsync(int addressId, int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var resetCmd = new SqlCommand("UPDATE Addresses SET IsDefault = 0 WHERE UserId = @UserId", conn);
        resetCmd.Parameters.AddWithValue("@UserId", userId);
        await resetCmd.ExecuteNonQueryAsync();

        using var cmd = new SqlCommand("UPDATE Addresses SET IsDefault = 1 WHERE AddressId = @AddressId AND UserId = @UserId", conn);
        cmd.Parameters.AddWithValue("@AddressId", addressId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private AddressDto MapToDto(SqlDataReader reader)
    {
        return new AddressDto
        {
            AddressId = (int)reader["AddressId"],
            FullName = reader["FullName"].ToString()!,
            PhoneNumber = reader["PhoneNumber"].ToString()!,
            AddressLine1 = reader["AddressLine1"].ToString()!,
            AddressLine2 = reader["AddressLine2"] as string,
            City = reader["City"].ToString()!,
            State = reader["State"].ToString()!,
            PinCode = reader["PinCode"].ToString()!,
            IsDefault = (bool)reader["IsDefault"]
        };
    }
}