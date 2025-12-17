using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class ForgetPasswordDTO
    {
        public string email { get; set; }
        public string newPassword { get; set; }
    }
}
