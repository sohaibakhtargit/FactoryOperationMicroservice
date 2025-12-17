using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class SupplierManagementDto
    {
        public int SupplierManagementId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ContactPerson { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int LeadTimeDays { get; set; }

        [Range(0, 5)]
        public decimal? PerformanceRating { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LastPrice { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetSupplierManagementDto : SupplierManagementDto
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int TotalReorderRules { get; set; }
        public int TotalPurchaseRequisitions { get; set; }
    }

    public class AddSupplierManagementDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ContactPerson { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int LeadTimeDays { get; set; }

        [Range(0, 5)]
        public decimal? PerformanceRating { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LastPrice { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public int CreatedBy { get; set; }
    }
}