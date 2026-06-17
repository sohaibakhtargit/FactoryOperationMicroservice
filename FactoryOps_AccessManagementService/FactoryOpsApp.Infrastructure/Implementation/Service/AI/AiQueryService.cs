using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Data;

public class AiQueryService : IAiQueryService
{
    private readonly TenantDbContextFactory _tenantDbFactory;

    public AiQueryService(TenantDbContextFactory tenantDbFactory)
    {
        _tenantDbFactory = tenantDbFactory;
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, int tenantId)
    {
        try
        {
            sql = CleanSql(sql);
            sql = FixAliases(sql);
            sql = FixPostgresNaming(sql);

            // Force TenantId
            if (!sql.Contains("\"TenantId\""))
            {
                if (sql.ToLower().Contains("where"))
                    sql += $" AND \"TenantId\" = {tenantId}";
                else
                    sql += $" WHERE \"TenantId\" = {tenantId}";
            }

            if (!sql.ToLower().Contains("limit"))
                sql += " LIMIT 50";

            Console.WriteLine("FINAL SQL: " + sql);

            if (!IsSafeQuery(sql))
                throw new Exception("Only SELECT queries allowed");

            using var db = _tenantDbFactory.GetTenantDbContext(tenantId);
            var conn = db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var reader = await cmd.ExecuteReaderAsync();

            var result = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader[i];

                result.Add(row);
            }

            return result;
        }
        catch (Exception ex)
        {
            return new List<Dictionary<string, object>>
            {
                new() { { "error", ex.Message } }
            };
        }
    }

    private string CleanSql(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        text = text.Replace("```sql", "").Replace("```", "").Trim();

        var start = text.IndexOf("select", StringComparison.OrdinalIgnoreCase);
        if (start < 0) return "";

        text = text.Substring(start);

        var end = text.IndexOf(";");
        if (end >= 0)
            text = text.Substring(0, end);

        return text.Trim();
    }

  
    private string FixAliases(string sql)
    {
        if (sql.Contains("JOIN"))
        {
            sql = sql.Replace("FROM \"WorkOrders\"", "FROM \"WorkOrders\" w")
                     .Replace("JOIN \"FactoryUsers\"", "JOIN \"FactoryUsers\" u")

                     .Replace("\"WorkOrderId\"", "w.\"WorkOrderId\"")
                     .Replace("\"Title\"", "w.\"Title\"")
                     .Replace("\"Status\"", "w.\"Status\"")
                     .Replace("\"CreatedAt\"", "w.\"CreatedAt\"")
                     .Replace("\"AssignedToUserId\"", "w.\"AssignedToUserId\"");
        }

        return sql;
    }

    private string FixPostgresNaming(string sql)
    {
        return sql
            .Replace("workorders", "\"WorkOrders\"")
            .Replace("tenantid", "\"TenantId\"")
            .Replace("status", "\"Status\"")
            .Replace("\"\"", "\"")
            .Replace("'Active'", "'Assigned'");
    }

    private bool IsSafeQuery(string sql)
    {
        var s = sql.ToLower().Trim();

        if (!s.StartsWith("select")) return false;

        string[] forbidden = { "insert", "update", "delete", "drop", "alter" };

        return !forbidden.Any(x => s.Contains(x));
    }
}