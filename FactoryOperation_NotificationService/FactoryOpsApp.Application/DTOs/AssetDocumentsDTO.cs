using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.AspNetCore.Http;
using System;

namespace FactoryOpsApp.Application.DTOs
{
    public class CreateAssetDocumentDto
    {
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public AssetDocumentTypeEnum DocumentType { get; set; }
        public DocumentCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public DocumentStatus? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ReminderDaysBeforeExpiry { get; set; }
        public bool ComplianceFlag { get; set; }
        public IFormFile? DocumentFile { get; set; }
        public int CreatedBy { get; set; }
    }

    public class UpdateAssetDocumentDto
    {
        public long DocumentId { get; set; }
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public AssetDocumentTypeEnum DocumentType { get; set; }
        public DocumentCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public int? ReminderDaysBeforeExpiry { get; set; }
        public bool ComplianceFlag { get; set; }
        public IFormFile? DocumentFile { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class AssetDocumentResponseDto
    {
        public long DocumentId { get; set; }
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string DocumentTitle { get; set; } = string.Empty;
        public AssetDocumentTypeEnum? DocumentType { get; set; }
        public DocumentCategory? Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ReminderDaysBeforeExpiry { get; set; }
        public DocumentStatus? Status { get; set; }
        public bool ComplianceFlag { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedOn { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ComplianceAlertDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssetCode { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Severity { get; set; } = "Medium";
        public int DaysRemaining { get; set; }
        public int DaysOverdue { get; set; }
        public long DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
    }
}