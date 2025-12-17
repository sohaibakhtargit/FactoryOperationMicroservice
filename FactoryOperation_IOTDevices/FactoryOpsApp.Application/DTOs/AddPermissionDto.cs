using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddPermissionDto
    {
        public int PermissionId { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
    }

}
