using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class GlobalDropdownDto
    {
        public int GlobalDropdownsId { get; set; }
        public int? TenantId { get; set; }

        [Required]
        public string Module { get; set; } = string.Empty;

        [Required]
        public string SubModule { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GetGlobalDropdownDto : GlobalDropdownDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DropdownFilterDto
    {
        public string? Module { get; set; }
        public string? SubModule { get; set; }
        public int? TenantId { get; set; }
    }
}