using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Feedback_Support
{
    public class SuperAdminSupportFeedbackRepository : ISuperAdminSupportFeedbackRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;

        public SuperAdminSupportFeedbackRepository(
            MasterFactoryOpsDbContext masterDbContext,
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger)
        {
            _masterDbContext = masterDbContext;
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<GetAllRecord<SuperAdminSupportFeedbackDto>> GetAllSupportFeedbackAsync()
        {
            var response = new GetAllRecord<SuperAdminSupportFeedbackDto>();
            try
            {
                var tenants = await _masterDbContext.FactoryTenants
                    .Where(t => !t.IsDeleted && t.IsActive)
                    .ToListAsync();

                var tasks = tenants.Select(async tenant =>
                {
                    try
                    {
                        using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);

                        var feedbacks = await tenantDb.SupportFeedback
                            .AsNoTracking()
                            .Where(f => !f.IsDeleted)
                            .GroupJoin(
                                tenantDb.FactoryUsers,
                                feedback => feedback.SubmittedBy,
                                user => user.UserId,
                                (feedback, users) => new { feedback, users }
                            )
                            .SelectMany(
                                x => x.users.DefaultIfEmpty(),
                                (x, user) => new SuperAdminSupportFeedbackDto
                                {
                                    FeedbackId = x.feedback.FeedbackId,
                                    TenantId = tenant.TenantId,
                                    TenantName = tenant.TenantName,
                                    Rating = x.feedback.Rating,
                                    Comments = x.feedback.Comments,
                                    SubmittedBy = user != null ? $"{user.FirstName} {user.LastName}" : null,
                                    SubmittedAt = x.feedback.SubmittedAt,
                                    Acknowledged = x.feedback.Acknowledged
                                }
                            )
                            .ToListAsync();

                        return feedbacks;
                    }
                    catch
                    {
                        return new List<SuperAdminSupportFeedbackDto>();
                    }
                });

                var allFeedbacks = (await Task.WhenAll(tasks)).SelectMany(fb => fb).OrderByDescending(fb => fb.SubmittedAt).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SuperAdminSupportFeedbackStatusMessage.DataFetched;
                response.GetAllData = allFeedbacks;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportModule",
                    apiName: "GetAllSupportFeedbackAsync",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = SuperAdminSupportFeedbackStatusMessage.FetchFailed;
                response.GetAllData = new List<SuperAdminSupportFeedbackDto>();
            }

            return response;
        }
    }
}
