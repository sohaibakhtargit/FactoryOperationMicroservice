using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class LoginDto
    {
        [DefaultValue("")]
        [Required(ErrorMessage = "Email is required.")]

        public string? Email { get; set; }

        [DefaultValue("")]
        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }
    }

    public class SwitchTenantRequestDto
    {
        public int TenantId { get; set; }
    }

}
