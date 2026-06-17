using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Analytic_Reports
{
    public class AnalyticsAndReportsRepository : IAnalyticsAndReportsRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public AnalyticsAndReportsRepository(
           TenantDbContextFactory tenantDbContext,
           IExceptionLoggerService exceptionLogger,
           IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }
        public async Task<GetAllRecord<AnalyticsAndReportsDto>> GetAnalyticsAndReportsAsync(
          int tenantId, DateTime startDate, DateTime endDate, CategoryType category = CategoryType.All)

        {
            var response = new GetAllRecord<AnalyticsAndReportsDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var result = new AnalyticsAndReportsDto
                {
                    Category = category
                };

                var workOrders = await tenantDb.WorkOrders
                    .Where(x => !x.IsDeleted && x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                    .ToListAsync();

                var tasks = await tenantDb.MaintenanceTasks
                    .Where(x => !x.IsDeleted && x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                    .ToListAsync();


                var schedules = await tenantDb.MaintenanceSchedules
                    .Where(x => !x.IsDeleted && x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                    .ToListAsync();
                if (!workOrders.Any() && !tasks.Any() && !schedules.Any())
                {
                    response.GetAllData = new List<AnalyticsAndReportsDto>();
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AnalyticsAndReportsStatusMessage.NoRecordsFound;
                    return response;
                }

                switch (category)
                {
                    case CategoryType.Cost:
                        result.TotalCost = workOrders.Sum(x => x.PartCost + x.LaborCost);
                        break;

                    case CategoryType.Compliance:
                        int totalSchedules = schedules.Count();
                        int completedTasks = tasks.Count(t => t.Status == MaintenanceTaskStatus.Completed);
                        result.ComplianceRate = totalSchedules > 0 ? (double)completedTasks / totalSchedules * 100 : 0;
                        break;

                    case CategoryType.Downtime:
                        result.TotalDowntimeHours = workOrders.Sum(x => x.EstimatedDurationMinutes) / 60.0;
                        break;

                    case CategoryType.Reliability:
                        int failures = workOrders.Count(w => w.Status == WorkOrderStatus.Cancelled || w.Status == WorkOrderStatus.Overdue);
                        double totalOperatingTime = (endDate - startDate).TotalHours;
                        result.MTBF = failures > 0 ? totalOperatingTime / failures : totalOperatingTime;
                        break;

                    case CategoryType.All:
                    default:
                        
                        int totalSchedulesAll = schedules.Count();
                        int completedTasksAll = tasks.Count(t => t.Status == MaintenanceTaskStatus.Completed);
                        int totalFailures = workOrders.Count(w => w.Status == WorkOrderStatus.Cancelled || w.Status == WorkOrderStatus.Overdue);
                        double totalOpTime = (endDate - startDate).TotalHours;

                        result.TotalCost = workOrders.Sum(x => x.PartCost + x.LaborCost);
                        result.ComplianceRate = totalSchedulesAll > 0 ? (double)completedTasksAll / totalSchedulesAll * 100 : 0;
                        result.TotalDowntimeHours = workOrders.Sum(x => x.EstimatedDurationMinutes) / 60.0;
                        result.MTBF = totalFailures > 0 ? totalOpTime / totalFailures : totalOpTime;

                        
                        decimal sumMetrics = (decimal)result.TotalCost + (decimal)result.ComplianceRate + (decimal)result.TotalDowntimeHours + (decimal)result.MTBF;
                        result.MetricsDistribution = new MetricsDistributionsDto
                        {
                            CostPercentage = sumMetrics > 0 ? result.TotalCost / sumMetrics * 100 : 0,
                            CompliancePercentage = sumMetrics > 0 ? (decimal)result.ComplianceRate / sumMetrics * 100 : 0,
                            DowntimePercentage = sumMetrics > 0 ? (decimal)result.TotalDowntimeHours / sumMetrics * 100 : 0,
                            ReliabilityPercentage = sumMetrics > 0 ? (decimal)result.MTBF / sumMetrics * 100 : 0
                        };
                        break;
                }

                result.WorkOrdersCount = workOrders.Count;
                result.MaintenanceTasksCount = tasks.Count;
                result.MaintenanceSchedules = schedules.Count;
                result.Category = category;
                

                response.GetAllData = new List<AnalyticsAndReportsDto> { result };
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AnalyticsAndReportsStatusMessage.DataFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Analytics-Module", "GetAnalyticsAndReports", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AnalyticsAndReportsStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<byte[]> ExportAnalyticsAndReportsAsync(
        int tenantId,
        DateTime startDate,
        DateTime endDate,
        CategoryType category = CategoryType.All)
        {
            var data = await GetAnalyticsAndReportsAsync(tenantId, startDate, endDate, category);

            if (data.GetAllData == null || !data.GetAllData.Any())
                return Array.Empty<byte>();

            using (var ms = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

             
                var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
                var bodyFont = new XFont("Arial", 10, XFontStyle.Regular);

                double margin = 40;
                double yPoint = margin;

                gfx.DrawString("Analytics and Reports", titleFont, XBrushes.DarkBlue,
                    new XRect(0, yPoint, page.Width, 30), XStringFormats.TopCenter);

                yPoint += 40;

                string[] headers = {
            "Category", "Compliance Rate", "Total Downtime (Hrs)", "Total Cost",
            "MTBF", "Work Orders Count", "Tasks Count", "Schedules Count"
        };

                double tableWidth = page.Width - 2 * margin;
                double colWidth = tableWidth / headers.Length;
                double headerRowHeight = 40;

                List<string> WrapText(string text, XFont font, double maxWidth, XGraphics gfx)
                {
                    var words = text.Split(' ');
                    var lines = new List<string>();
                    var currentLine = "";

                    foreach (var word in words)
                    {
                        var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                        var size = gfx.MeasureString(testLine, font);

                        if (size.Width > maxWidth)
                        {
                            if (!string.IsNullOrEmpty(currentLine))
                                lines.Add(currentLine);
                            currentLine = word;
                        }
                        else
                        {
                            currentLine = testLine;
                        }
                    }

                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);

                    return lines;
                }

                double xStart = margin;
                for (int i = 0; i < headers.Length; i++)
                {
                    var rect = new XRect(xStart, yPoint, colWidth, headerRowHeight);
                    gfx.DrawRectangle(XPens.Black, rect);

                    var lines = WrapText(headers[i], headerFont, colWidth - 4, gfx);

                    double lineHeight = headerFont.GetHeight();
                    double yText = yPoint + (headerRowHeight - (lines.Count * lineHeight)) / 2;

                    foreach (var line in lines)
                    {
                        gfx.DrawString(line, headerFont, XBrushes.DarkBlue,
                            new XRect(xStart + 2, yText, colWidth - 4, lineHeight), XStringFormats.TopCenter);
                        yText += lineHeight;
                    }

                    xStart += colWidth;
                }

                yPoint += headerRowHeight;

                foreach (var r in data.GetAllData)
                {
                    xStart = margin;
                    string[] rowData = {
                r.Category.ToString(),
                r.ComplianceRate.ToString(),
                r.TotalDowntimeHours.ToString(),
                r.TotalCost.ToString(),
                r.MTBF.ToString(),
                r.WorkOrdersCount.ToString(),
                r.MaintenanceTasksCount.ToString(),
                r.MaintenanceSchedules.ToString(),
               // r.AlertNotificationsCount.ToString()
            };

                    double rowHeight = 25;

                    for (int i = 0; i < rowData.Length; i++)
                    {
                        var rect = new XRect(xStart, yPoint, colWidth, rowHeight);
                        gfx.DrawRectangle(XPens.Black, rect);

                        gfx.DrawString(rowData[i], bodyFont, XBrushes.Black,
                            new XRect(xStart + 2, yPoint, colWidth - 4, rowHeight), XStringFormats.Center);

                        xStart += colWidth;
                    }

                    yPoint += rowHeight;
                }

                document.Save(ms);
                return ms.ToArray();
            }

        }
    }
}