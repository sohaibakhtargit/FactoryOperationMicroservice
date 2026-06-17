namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs
{
    public class DbBotRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
    //    public int TenantId { get; set; } // fallback tenant
      //  public int UserId { get; set; }
    }

    public class DbBotResponseDto
    {
        public string SqlQuery { get; set; } = string.Empty;
        public int UsedTenantId { get; set; }   // 🔥 new
        public string Summary { get; set; } = string.Empty; // 🔥 new
        public List<Dictionary<string, object>> Data { get; set; } = new();
    }
}