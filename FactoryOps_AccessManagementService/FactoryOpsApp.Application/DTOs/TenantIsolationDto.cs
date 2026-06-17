using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddTenantIsolationDto
    {
        public int TenantId { get; set; }
        public string DataEncryption { get; set; } = "Disabled";
        public string? EncryptionKeyId { get; set; }
        public string? TenantName { get; set; }
        public bool CustomBranding { get; set; } = false;
        public IFormFile? Logo { get; set; }
        public IFormFile? LogoText { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorScheme { get; set; }
        public string? DataPartitionId { get; set; }
    }

    public class GetTenantIsolationDto
    {
        public int TenantId { get; set; }
        public string? TenantName { get; set; }
        public string DataEncryption { get; set; } = "Disabled";
        public string? EncryptionKeyId { get; set; }
        public bool CustomBranding { get; set; }
        public string? LogoUrl { get; set; }
        public string? LogoText { get; set; }
        public string? ColorScheme { get; set; }
        public string? DataPartitionId { get; set; }
    }

    public class AddComplianceAuditDto
    {
        public int TenantId { get; set; }
        public string ComplianceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateComplianceAuditDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string ComplianceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? LastReviewedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateAuditComplianceMetricsDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        // Isolation Configuration
        public string? IsolationLevel { get; set; }
        public string? DataRegion { get; set; }
        public string? ComplianceLevel { get; set; }
        public DateTime? LastAuditDate { get; set; }
        public decimal? MonthlyMaintenanceCost { get; set; }

        // Validation Tests
        public string? ValidationIsolationLevel { get; set; }
        public string? ValidationSchemaIsolation { get; set; }
        public string? ValidationFileStorage { get; set; }
        public string? ValidationApiAccessControl { get; set; }
        public DateTime? ValidationTimestamp { get; set; }

        // Isolation Metrics
        public decimal? MetricDataIntegrityScore { get; set; }
        public decimal? MetricAccessControl { get; set; }
        public decimal? MetricSchemaIsolation { get; set; }
        public decimal? MetricNetworkSegmentation { get; set; }
        public DateTime? MetricsTimestamp { get; set; }

        // Security Stats (last 24h)
        public int? AccessAttempts24H { get; set; }
        public int? BlockedRequests24H { get; set; }
        public int? PolicyViolations24H { get; set; }
        public int? DataQueries24H { get; set; }
        public DateTime? SecurityStatsTimestamp { get; set; }

    }

    public class GetAuditComplianceMetricsDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        // Budget Cost Tracking
        public string IsolationLevel { get; set; }
        public string DataRegion { get; set; }
        public string ComplianceLevel { get; set; }
        public DateTime? LastAuditDate { get; set; }
        public decimal? MonthlyMaintenanceCost { get; set; }

        // Validation Tests
        public string ValidationIsolationLevel { get; set; }
        public string ValidationSchemaIsolation { get; set; }
        public string ValidationFileStorage { get; set; }
        public string ValidationApiAccessControl { get; set; }
        public DateTime? ValidationTimestamp { get; set; }

        // Isolation Metrics
        public decimal? MetricDataIntegrityScore { get; set; }
        public decimal? MetricAccessControl { get; set; }
        public decimal? MetricSchemaIsolation { get; set; }
        public decimal? MetricNetworkSegmentation { get; set; }
        public DateTime? MetricsTimestamp { get; set; }

        // Security Events (24h)
        public int? AccessAttempts24h { get; set; }
        public int? BlockedRequests24h { get; set; }
        public int? PolicyViolations24h { get; set; }
        public int? DataQueries24h { get; set; }
        public DateTime? SecurityStatsTimestamp { get; set; }
    }


    public class GetComplianceAuditDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string ComplianceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastReviewedOn { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

