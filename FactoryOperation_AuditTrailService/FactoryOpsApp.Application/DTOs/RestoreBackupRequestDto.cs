using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class RestoreBackupRequestDto
    {
        public int TenantId { get; set; }
        public string BackupFileName { get; set; }
        public string RestoredBy { get; set; }
    }

}
