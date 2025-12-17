public class ReportsAnalyticsDto
{
    public PerformanceMetricsDto PerformanceMetrics { get; set; }
    public ApiManagementDto ApiManagement { get; set; }
    public WhiteLabelingDto WhiteLabeling { get; set; }
    public List<RecentTenantActivityDto> RecentTenantActivities { get; set; }
}

public class PerformanceMetricsDto
{
    public string DatabasePerformance { get; set; }
    public string ApiResponseTime { get; set; }
    public string StorageUsage { get; set; }
    public string ErrorRate { get; set; }
}

public class ApiManagementDto
{
    public int GlobalApiCalls24h { get; set; }
    public int ActiveApiKeys { get; set; }
    public int RateLimitViolations { get; set; }
}

public class WhiteLabelingDto
{
    public int CustomDomains { get; set; }
    public string BrandedTenants { get; set; }
    public int EmailTemplates { get; set; }
}

public class RecentTenantActivityDto
{
    public string TenantName { get; set; }
    public string ActivityDescription { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ReportsAnalyticsSuperAdminDto : ReportsAnalyticsDto
{
    public int ActiveTenants { get; set; }
    public int TotalUsers { get; set; }
    public int OpenIssues { get; set; }
    public double SystemLoad { get; set; }
}