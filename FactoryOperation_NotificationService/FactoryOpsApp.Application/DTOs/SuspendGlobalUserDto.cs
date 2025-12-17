using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class SuspendGlobalUserDto
    {
        public int GlobalUserId { get; set; }
        public int? SuspendedBy { get; set; }
        public string? SuspensionReason { get; set; }
    }

    public class SuspendUserDto
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int? SuspendedBy { get; set; }
        public string? SuspensionReason { get; set; }
    }
}
