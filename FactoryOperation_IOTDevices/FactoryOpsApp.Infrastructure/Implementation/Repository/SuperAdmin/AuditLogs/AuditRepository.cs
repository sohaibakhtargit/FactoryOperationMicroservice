using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs;
using FactoryOps_IOTDeviceService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using static FactoryOperation_IOTDevices.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.AuditLogs
{
    public class AuditRepository : IAuditRepository
    {

        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IExceptionLoggerService _exceptionLogger;
        public AuditRepository(MasterFactoryOpsDbContext masterDbcontext, IExceptionLoggerService exceptionLogger)
        {
            _masterDbcontext = masterDbcontext;
            _exceptionLogger = exceptionLogger;
        }

        public GetAllRecord<GetAllAuditsDto> GetAuditRecords()
        {
            GetAllRecord<GetAllAuditsDto> response = new();
            try
            {
                var auditList = (from aud in _masterDbcontext.Audit_Log_MasterDb
                                 where aud.IsDeleted == false
                                 orderby aud.AuditLogID descending
                                 select new GetAllAuditsDto
                                 {
                                     AuditLogID = aud.AuditLogID,
                                     Timestamp = aud.Timestamp,
                                     Email = aud.Email,
                                     Tenant = _masterDbcontext.FactoryTenants
                                         .Where(t => t.TenantId == aud.TenantId)
                                         .Select(t => t.TenantName)
                                         .FirstOrDefault() ?? "SuperAdmin",
                                     UserName = aud.UserName,
                                     Ipaddress = aud.Ipaddress,
                                     Action = aud.Action,
                                     Details = aud.Details,
                                 }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AuditStatusMessage.AuditFetched;
                response.GetAllData = auditList;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "AuditModule",
                   apiName: "get-Audits",
                   tenantId: null,
                   userId: null
               );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public GetAllRecord<GetAllAuditsDto> GetTenantAuditRecords(int TenantId)
        {
            GetAllRecord<GetAllAuditsDto> response = new();
            try
            {
                var tenantList = (from aud in _masterDbcontext.Audit_Log_MasterDb
                                  join t in _masterDbcontext.FactoryTenants
                                  on aud.TenantId equals t.TenantId
                                  where aud.IsDeleted == false && aud.TenantId == TenantId
                                  orderby aud.AuditLogID descending
                                  select new GetAllAuditsDto
                                  {
                                      AuditLogID = aud.AuditLogID,
                                      Timestamp = aud.Timestamp,
                                      Email = aud.Email,
                                      Tenant = t.TenantName,
                                      UserName = aud.UserName,
                                      Ipaddress = aud.Ipaddress,
                                      Action = aud.Action,
                                      Details = aud.Details,
                                  }).ToList();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AuditStatusMessage.AuditFetched;
                response.GetAllData = tenantList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AuditModule",
                    apiName: "get-Tenant-AuditTrials",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
    }
}
