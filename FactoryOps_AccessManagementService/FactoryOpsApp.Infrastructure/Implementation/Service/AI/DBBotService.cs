using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

public class DBBotService : IDBBotService
{
    private readonly IOpenAiService _openAi;
    private readonly IAiQueryService _queryService;
    private readonly MasterFactoryOpsDbContext _masterDb;

    public DBBotService(
        IOpenAiService openAi,
        IAiQueryService queryService,
        MasterFactoryOpsDbContext masterDb)
    {
        _openAi = openAi;
        _queryService = queryService;
        _masterDb = masterDb;
    }

    public async Task<DbBotResponseDto> AskAsync(DbBotRequestDto request)
    {
        int finalTenantId = 0; // must declare

        //  1. Detect tenant using DbName or TenantName
        var tenant = _masterDb.TenantMasterMapping
            .AsNoTracking()
            .FirstOrDefault(x =>
                x.IsActive &&
                (
                    request.Prompt.ToLower().Contains(x.DbName.ToLower()) ||
                    request.Prompt.ToLower().Contains(x.TenantName.ToLower())
                )
            );

        if (tenant != null)
        {
            finalTenantId = tenant.TenantId;
        }
        else
        {
            //  fallback (IMPORTANT)
            finalTenantId = _masterDb.TenantMasterMapping
                .Where(x => x.IsActive)
                .Select(x => x.TenantId)
                .FirstOrDefault();
        }

        // 2. Remove tenant info from prompt
        var cleanedPrompt = request.Prompt;

        if (tenant != null)
        {
            if (!string.IsNullOrEmpty(tenant.DbName))
                cleanedPrompt = cleanedPrompt.Replace(tenant.DbName, "", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(tenant.TenantName))
                cleanedPrompt = cleanedPrompt.Replace(tenant.TenantName, "", StringComparison.OrdinalIgnoreCase);
        }

        //  3. Generate SQL
        var sql = await _openAi.GenerateSqlAsync(
            cleanedPrompt,
            finalTenantId,
            0 // userId removed, pass dummy
        );

        Console.WriteLine("AI SQL: " + sql);

        //  4. Fallback SQL
        if (string.IsNullOrWhiteSpace(sql) || !sql.ToLower().Contains("select"))
        {
            sql = $"SELECT * FROM \"WorkOrders\" WHERE \"TenantId\" = {finalTenantId} LIMIT 10";
        }

        //  5. Execute
        var data = await _queryService.ExecuteQueryAsync(sql, finalTenantId);

        return new DbBotResponseDto
        {
            SqlQuery = sql,
            UsedTenantId = finalTenantId,
            Summary = $"{data.Count} records found",
            Data = data
        };
    }
}