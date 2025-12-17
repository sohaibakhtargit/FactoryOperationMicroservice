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
    public class SupportFeedbackRepository : ISupportFeedbackRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        public SupportFeedbackRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> AcknowledgeFeedbackAsync(AcknowledgeFeedbackDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var feedback = await tenantDb.SupportFeedback
                    .FirstOrDefaultAsync(f => f.FeedbackId == dto.FeedbackId && !f.IsDeleted);

                if (feedback == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = SupportFeedbackStatusMessage.FeedbackNotFound;
                    return response;
                }

                if (dto.AcknowledgedBy == null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = SupportFeedbackStatusMessage.AcknowledgedByRequired;
                    return response;
                }

                feedback.Acknowledged = true;
                feedback.AcknowledgedBy = dto.AcknowledgedBy;
                feedback.AcknowledgedAt = DateTime.UtcNow;


                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: $"Acknowledged feedback ID {dto.FeedbackId}",
                    email: null,
                    eventType: "SupportFeedback",
                    tenantId: dto.TenantId
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SupportFeedbackStatusMessage.FeedbackAcknowledged;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "SupportFeedbackModule",
                        apiName: "AcknowledgeFeedbackAsync",
                        tenantId: dto?.TenantId,
                        userId: null
                    );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportFeedbackStatusMessage.FeedbackAcknowledgeFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> AddFeedback(AddSupportFeedbackDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                var ticketExists = await tenantDb.FactorySupportTickets
                    .AnyAsync(t => t.TicketId == dto.TicketId && t.IsActive && !t.IsDeleted);

                if (!ticketExists)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = SupportFeedbackStatusMessage.FeedbackTicketNotFound;
                    return response;
                }

                SupportFeedback feedback;

                if (dto.FeedbackId > 0)
                {
                    feedback = await tenantDb.SupportFeedback
                        .FirstOrDefaultAsync(f => f.FeedbackId == dto.FeedbackId && f.IsActive && !f.IsDeleted);

                    if (feedback == null)
                    {
                        response.StatusCode = StatusCode.NotFound;
                        response.StatusMessage = SupportFeedbackStatusMessage.FeedbackNotFound;
                        return response;
                    }

                    feedback.Rating = dto.Rating;
                    feedback.Comments = dto.Comments;
                    feedback.SubmittedBy = dto.SubmittedBy;
                    feedback.SubmittedAt = DateTime.UtcNow;

                    tenantDb.SupportFeedback.Update(feedback);
                }
                else
                {
                    feedback = new SupportFeedback
                    {
                        TenantId = dto.TenantId,
                        TicketId = dto.TicketId,
                        Rating = dto.Rating,
                        Comments = dto.Comments,
                        SubmittedBy = dto.SubmittedBy,
                        SubmittedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    await tenantDb.SupportFeedback.AddAsync(feedback);
                }

                await _auditLogger.LogAuditAsync(
                        action: "Create",
                        details: $"Added new feedback for ticket ID {dto.TicketId}",
                        eventType: "SupportFeedback",
                        tenantId: dto.TenantId,
                        email: null
                    );

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = dto.FeedbackId > 0
                    ? SupportFeedbackStatusMessage.FeedbackAdded
                    : SupportFeedbackStatusMessage.FeedbackUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportFeedbackModule",
                    apiName: "AddOrUpdateFeedback",
                    tenantId: dto?.TenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportFeedbackStatusMessage.FeedbackAddOrUpdateFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> AddFeedbackAsync(AddSupportFeedbackDto dto)
        {
            return await AddFeedback(dto);
        }

        public async Task<CommonResponseModel> DeleteFeedbackAsync(DeleteSupportFeedbackDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var feedback = await tenantDb.SupportFeedback
                    .FirstOrDefaultAsync(f => f.FeedbackId == dto.FeedbackId && !f.IsDeleted);

                if (feedback == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = SupportFeedbackStatusMessage.FeedbackNotFound;
                    return response;
                }

                feedback.IsDeleted = true;
                feedback.IsActive = false;

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: $"Deleted feedback ID {dto.FeedbackId}",
                    eventType: "SupportFeedback",
                    tenantId: dto.TenantId,
                    email: null
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SupportFeedbackStatusMessage.FeedbackDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportFeedbackModule",
                    apiName: "DeleteFeedbackAsync",
                    tenantId: dto?.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{SupportFeedbackStatusMessage.FeedbackDeleteFailed}: {ex.Message}";
            }

            return response;
        }


        public async Task<GetAllRecord<GetSupportFeedbackDto>> GetAllFeedbackByTenant(int tenantId)
        {
            {
                var response = new GetAllRecord<GetSupportFeedbackDto>();
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                try
                {
                    var list = (from f in tenantDb.SupportFeedback
                                join user in tenantDb.FactoryUsers on f.SubmittedBy equals user.UserId into userGroup
                                from user in userGroup.DefaultIfEmpty()
                                join ticket in tenantDb.FactorySupportTickets on f.TicketId equals ticket.TicketId into ticketGroup
                                from ticket in ticketGroup.DefaultIfEmpty()
                                where f.TenantId == tenantId && !f.IsDeleted
                                orderby f.SubmittedAt descending
                                select new GetSupportFeedbackDto
                                {
                                    FeedbackId = f.FeedbackId,
                                    TicketId = f.TicketId,
                                    Comments = f.Comments,
                                    Rating = f.Rating,
                                    SubmittedByName = user != null ? user.FirstName + " " + user.LastName : null,
                                    SubmittedAt = f.SubmittedAt,
                                    Acknowledged = f.Acknowledged,
                                    AcknowledgedByName = null,
                                    AcknowledgedAt = f.AcknowledgedAt,
                                    TicketSubject = ticket != null ? ticket.Subject : null
                                }).ToList();

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = SupportFeedbackStatusMessage.FeedbackFetched;
                    response.GetAllData = list;
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "SupportFeedbackModule",
                        apiName: "GetAllFeedbackByTenant",
                        tenantId: tenantId,
                        userId: null
                    );
                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = $"{SupportFeedbackStatusMessage.FeedbackFetchFailed}: {ex.Message}";
                }

                return response;
            }


        }
        public async Task<FeedbackMetricDto> GetFeedbackMetricsAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var feedbacks = await tenantDb.SupportFeedback
                    .Where(f => f.TenantId == tenantId && !f.IsDeleted)
                    .ToListAsync();

                if (feedbacks == null || feedbacks.Count == 0)
                {
                    return new FeedbackMetricDto
                    {
                        TenantId = tenantId,
                        AvgRating = 0,
                        SatisfactionRate = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                }

                var avgRating = (decimal)Math.Round(feedbacks.Average(f => f.Rating), 2);
                var satisfactionRate = (decimal)Math.Round(feedbacks.Count(f => f.Rating >= 4) * 100.0 / feedbacks.Count, 2);
                var lastUpdated = feedbacks.Max(f => f.SubmittedAt);

                return new FeedbackMetricDto
                {
                    TenantId = tenantId,
                    AvgRating = avgRating,
                    SatisfactionRate = satisfactionRate,
                    LastUpdated = lastUpdated
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportFeedbackModule",
                    apiName: "GetFeedbackMetricsAsync",
                    tenantId: tenantId,
                    userId: null
                );

                return new FeedbackMetricDto
                {
                    TenantId = tenantId,
                    AvgRating = 0,
                    SatisfactionRate = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }
    }
}
