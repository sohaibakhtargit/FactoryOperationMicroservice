using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement
{
    public class SupportTicketRepository : ISupportTicketRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        public SupportTicketRepository(TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> AddSupportTicket(AddSupportTicketDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var ticket = new FactorySupportTickets
                {
                    TenantId = dto.TenantId,
                    Subject = dto.Subject,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = dto.Status,
                    Module = dto.Module,
                    AssignedTo = dto.AssignedTo,
                    CreatedBy = dto.CreatedBy,
                    ResolutionNotes = dto.ResolutionNotes,
                    SatisfactionRating = dto.SatisfactionRating,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _auditLogger.LogAuditAsync(
                    action: "Create",
                    details: $"Added support ticket ID {ticket.TicketId}",
                    eventType: "SupportTicket",
                    tenantId: dto.TenantId,
                    email: null
                );
                await tenantDb.FactorySupportTickets.AddAsync(ticket);
                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SupportTicketStatusMessage.TicketAdded;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                ex,
                sourceModule: "SupportTicketModule",
                apiName: "AddSupportTicket",
                tenantId: dto?.TenantId,
                userId: null
            );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportTicketStatusMessage.TicketAddFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateSupportTicket(int ticketId, AddSupportTicketDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var ticket = await tenantDb.FactorySupportTickets
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);

                if (ticket == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = SupportTicketStatusMessage.TicketNotFound;
                    return response;
                }

                ticket.Subject = dto.Subject;
                ticket.Description = dto.Description;
                ticket.Status = dto.Status;
                ticket.Priority = dto.Priority;
                ticket.Module = dto.Module;
                ticket.AssignedTo = dto.AssignedTo;
                ticket.ResolutionNotes = dto.ResolutionNotes;
                ticket.SatisfactionRating = dto.SatisfactionRating;
                ticket.UpdatedAt = DateTime.UtcNow;
                ticket.UpdatedBy = dto.TenantId;

                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: $"Updated support ticket ID {ticket.TicketId}",
                    eventType: "SupportTicket",
                    tenantId: dto.TenantId,
                    email: null
                );
                await tenantDb.SaveChangesAsync();


                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SupportTicketStatusMessage.TicketUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportTicketModule",
                    apiName: "UpdateSupportTicket",
                    tenantId: dto?.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportTicketStatusMessage.TicketUpdated}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteSupportTicket(int ticketId, int TenantId)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);
            try
            {
                var ticket = await tenantDb.FactorySupportTickets
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);

                if (ticket == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = SupportTicketStatusMessage.TicketNotFound;
                    return response;
                }

                ticket.IsDeleted = true;
                ticket.IsActive = false;
                ticket.DeletedAt = DateTime.UtcNow;
                ticket.DeletedBy = TenantId;

                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: $"Deleted support ticket ID {ticket.TicketId}",
                    eventType: "SupportTicket",
                    tenantId: TenantId,
                    email: null
                );
                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SupportTicketStatusMessage.TicketDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportTicketModule",
                    apiName: "DeleteSupportTicket",
                    tenantId: TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportTicketStatusMessage.TicketDeleteFailed}: {ex.Message}";
            }

            return response;
        }
        public GetAllRecord<GetSupportTicketDto> GetAllTicketsByTenant(int tenantId)
        {
            var response = new GetAllRecord<GetSupportTicketDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var list = tenantDb.FactorySupportTickets
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .Include(t => t.AssignedTeam)
                    .Select(t => new GetSupportTicketDto
                    {
                        TicketId = t.TicketId,
                        TenantId = t.TenantId,
                        Subject = t.Subject,
                        Status = t.Status,
                        Priority = t.Priority,
                        Module = t.Module,
                        Description = t.Description,
                        AssignedTo = t.AssignedTo,
                        AssignedName = t.AssignedTeam != null
                            ? t.AssignedTeam.Name : null,
                        SatisfactionRating = t.SatisfactionRating,
                        CreatedBy = t.CreatedBy ?? null,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SupportTicketStatusMessage.TicketFetched;
                response.GetAllData = list;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                       ex,
                       sourceModule: "SupportTicketModule",
                       apiName: "GetAllTicketsByTenant",
                       tenantId: tenantId,
                       userId: null
                   );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportTicketStatusMessage.TicketFetchFailed}: {ex.Message}";
            }

            return response;
        }
    }
}
