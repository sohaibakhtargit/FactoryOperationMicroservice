using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssetDocumentTypeEnum
    {
        Manual,
        SOP,
        Warranty,
        Certificate,
        Insurance,
        CheckList
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentStatus
    {
        Active,
        Expired,
        Pending,
        Archived
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentCategory
    {
        Operations,
        Compliance,
        Legal,
        Safety,
        Maintenance
    }

    public class AssetDocuments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Column("DocumentId")]  // Ensure this matches the DB column name exactly
        public long DocumentId { get; set; }
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public AssetRegistry Asset { get; set; }
        public AssetDocumentTypeEnum? DocumentType { get; set; }
        public string? DocumentTitle { get; set; }
        public DocumentCategory? Category { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public string? FileExtension { get; set; }
        public long? FileSize { get; set; } 
        public DateTime? ExpiryDate { get; set; }
        public int? ReminderDaysBeforeExpiry { get; set; }
        public DocumentStatus? Status { get; set; }
        public bool ComplianceFlag { get; set; } = false;
        public string? UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
