using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class CreateCompanyBrandingDto
    {
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;
        public IFormFile? CompanyImageFile { get; set; }
        public string? CompanyLogo { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class UpdateCompanyBrandingDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        public IFormFile? CompanyImageFile { get; set; }
        public string? CompanyLogo { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class CompanyBrandingResponseDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyImage { get; set; }
        public string? CompanyLogo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
