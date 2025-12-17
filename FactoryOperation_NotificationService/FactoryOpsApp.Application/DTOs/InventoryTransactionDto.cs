using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }

        [Required]
        public int PerformedById { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetInventoryTransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public TransactionType TransactionType { get; set; }
        public string TransactionTypeDisplay { get; set; } = string.Empty;
        public int PartId { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int? FromLocationId { get; set; }
        public string? FromLocationName { get; set; }
        public int? ToLocationId { get; set; }
        public string? ToLocationName { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public int PerformedById { get; set; }
        public string PerformedByName { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public bool IsActive { get; set; }
        public string TransactionDateFormatted { get; set; } = string.Empty;
    }

    public class TransactionQueryDto
    {
        public int TenantId { get; set; }
        public int? PartId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? LocationId { get; set; }
    }
}