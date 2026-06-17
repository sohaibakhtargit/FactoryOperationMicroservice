using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI;
using System.Net.Http.Headers;
using System.Text.Json;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OpenAiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;

        var apiKey = _config["OpenRouter:Key"];

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "FactoryOps AI");
    }

    public async Task<string> GenerateSqlAsync(string prompt, int tenantId, int userId)
    {
        try
        {
            var systemPrompt = $@"
You are a PostgreSQL SQL generator.

STRICT RULES:
- ONLY return SQL
- ONLY SELECT queries
- ALWAYS use aliases (w for WorkOrders, u for FactoryUsers)
- ALWAYS use DOUBLE QUOTES
- ALWAYS prefix columns with alias (w. or u.)
- ALWAYS apply w.""TenantId"" = {tenantId}

VALID STATUS:
Started, Assigned, InProgress, Completed, Cancelled, Open

IMPORTANT RULES:
- NEVER use tenant name or database name in WHERE clause
- Ignore tenant name in prompt completely
- DO NOT use LOWER()
- DO NOT add unnecessary filters

🚨 COLUMN RULE (STRICT - NEVER BREAK):
- If user says ""show all"" or ""list all""
  → MUST use: SELECT w.*
  → DO NOT select specific columns
  → DO NOT optimize columns
  → This rule is mandatory

QUERY RULES:
- Use JOIN only when needed
- Use LIMIT 50
- Return SQL only

SCHEMA:
""WorkOrders"" w
""FactoryUsers"" u

RELATION:
w.""AssignedToUserId"" = u.""UserId""
";

            var body = new
            {
                model = "openai/gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                body
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return "";

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()?
                .Replace("```sql", "")
                .Replace("```", "")
                .Trim() ?? "";
        }
        catch
        {
            return "";
        }
    }
}