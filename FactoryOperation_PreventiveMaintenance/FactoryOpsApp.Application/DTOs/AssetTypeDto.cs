using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class CreateAssetTypeDto
    {
        [Required, MaxLength(100)]
        public string Type_Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal? Default_Depreciation_Rate { get; set; }

        public int? CreatedBy { get; set; }

        [Required]
        public int TenantId { get; set; }
    }

    public class UpdateAssetTypeDto
    {
        [Required]
        public int AssetTypeId { get; set; }

        [Required, MaxLength(100)]
        public string Type_Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal? Default_Depreciation_Rate { get; set; }

        public int? UpdatedBy { get; set; }

        [Required]
        public int TenantId { get; set; }
    }

    public class AssetTypeResponseDto
    {
        public int AssetTypeId { get; set; }
        public string Type_Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Default_Depreciation_Rate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public int TenantId { get; set; }
    }
}
