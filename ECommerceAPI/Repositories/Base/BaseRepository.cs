using ECommerceAPI.Data;
using Microsoft.Data.SqlClient;
namespace ECommerceAPI.Repositories.Base
{
    public class BaseRepository
    {
        protected readonly DatabaseHelper _db;

        public BaseRepository(DatabaseHelper db)
        {
            _db = db;
        }

        protected async Task<List<T>> ExecuteQueryAsync<T>(
           string query,
           SqlParameter[]? parameters,
           Func<SqlDataReader, T> mapper)
        {
            var list = new List<T>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(mapper(reader));
            return list;
        }
        protected async Task<int> ExecuteNonQueryAsync(
        string query,
        SqlParameter[]? parameters = null)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        protected async Task<object?> ExecuteScalarAsync(
           string query,
           SqlParameter[]? parameters = null)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteScalarAsync();
        }

        protected async Task ExecuteInTransactionAsync(Func<SqlConnection, SqlTransaction, Task> action)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var tx = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                await action(conn, tx);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
