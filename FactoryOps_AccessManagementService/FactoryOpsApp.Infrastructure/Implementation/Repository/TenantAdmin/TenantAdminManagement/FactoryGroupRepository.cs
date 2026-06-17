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
    public class FactoryGroupRepository : IFactoryGroupRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public FactoryGroupRepository(
            TenantDbContextFactory tenantDbContext,
            IAuditLogService auditLogger,
            IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddGroupAsync(FactoryGroupDto dto)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var group = new FactoryGroup
                {
                    Name = dto.Name,
                    Type = dto.Type,
                    TenantId = dto.TenantId,
                    LocationId = dto.LocationId,
                    Description = dto.Description,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                tenantDb.FactoryGroups.Add(group);
                await tenantDb.SaveChangesAsync();

                if (dto.UserIds?.Any() == true)
                {
                    foreach (var userId in dto.UserIds)
                    {
                        tenantDb.FactoryGroupUsers.Add(new FactoryGroupUser
                        {
                            GroupId = group.GroupId,
                            UserId = userId,
                            TenantId = dto.TenantId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await tenantDb.SaveChangesAsync();
                }
                await transaction.CommitAsync();
                await _auditLogger.LogAuditAsync("Create", $"Created Group '{group.Name}'", dto.CreatedBy, "", "CreateGroup");
                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryGroupStatusMessage.GroupCreated };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "Group-Module", "AddGroupAsync", dto.CreatedBy, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryGroupStatusMessage.GroupCreateFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> UpdateGroupAsync(FactoryGroupDto dto)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                var group = await tenantDb.FactoryGroups
                    .Include(g => g.GroupUsers)
                    .FirstOrDefaultAsync(g => g.GroupId == dto.GroupId && g.IsDeleted == false);

                if (group == null)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = FactoryGroupStatusMessage.GroupNotFound
                    };
                }

                group.Name = dto.Name;
                group.LocationId = dto.LocationId;
                group.Description = dto.Description;
                group.Type = dto.Type;
                group.UpdatedAt = DateTime.UtcNow;
                group.UpdatedBy = dto.UpdatedBy;

                var existingUsers = group.GroupUsers!.ToList();

                if (dto.UserIds != null && dto.UserIds.Any())
                {
                    foreach (var userId in dto.UserIds)
                    {
                        var existingUserMapping = existingUsers.FirstOrDefault(eu => eu.UserId == userId);

                        if (existingUserMapping != null)
                        {
                            if (existingUserMapping.IsDeleted || !existingUserMapping.IsActive)
                            {
                                existingUserMapping.IsActive = true;
                                existingUserMapping.IsDeleted = false;
                                existingUserMapping.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                        else
                        {
                            var newGroupUser = new FactoryGroupUser
                            {
                                GroupId = group.GroupId,
                                UserId = userId,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow,
                            };
                            await tenantDb.FactoryGroupUsers.AddAsync(newGroupUser);
                        }
                    }

                    foreach (var existingUser in existingUsers)
                    {
                        if (!dto.UserIds.Contains(existingUser.UserId) && existingUser.IsActive && !existingUser.IsDeleted)
                        {
                            existingUser.IsActive = false;
                            existingUser.IsDeleted = true;
                            existingUser.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated Group '{group.Name}'", dto.UpdatedBy, "", "UpdateGroup");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryGroupStatusMessage.GroupUpdated
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Group-Module", "UpdateGroupAsync", dto.UpdatedBy, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryGroupStatusMessage.GroupUpdateFailed} {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> DeleteGroupAsync(int tenantId, int groupId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var group = await tenantDb.FactoryGroups
                    .Include(g => g.GroupUsers)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted);

                if (group == null)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = FactoryGroupStatusMessage.GroupNotFound
                    };

                group.IsDeleted = true;
                group.IsActive = false;
                group.DeletedAt = DateTime.UtcNow;
                group.DeletedBy = tenantId;

                foreach (var mapping in group.GroupUsers!)
                {
                    mapping.IsDeleted = true;
                    mapping.IsActive = false;
                }

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Group '{group.Name}'", tenantId, "", "DeleteGroup");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryGroupStatusMessage.GroupDeleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Group-Module", "DeleteGroupAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryGroupStatusMessage.GroupDeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<FactoryGroupGetDto>> GetAllGroupsAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var groups = await tenantDb.FactoryGroups
                    .Where(g => !g.IsDeleted).Include(l => l.Location)
                    .OrderByDescending(g => g.GroupId)
                    .Include(g => g.GroupUsers)!
                        .ThenInclude(gu => gu.User)
                    .Select(g => new FactoryGroupGetDto
                    {
                        GroupId = g.GroupId,
                        TenantId = g.TenantId,
                        Name = g.Name,
                        Type = g.Type,
                        LocationId = g.LocationId,
                        LocationName = g.Location!.LocationName,
                        Description = g.Description,
                        Users = g.GroupUsers!.Select(u => new FactoryGroupUserDto
                        {
                            UserId = u.UserId,
                            FullName = u.User!.FirstName + " " + u.User!.LastName,
                        }).ToList()
                    })
                    .ToListAsync();

                return new GetAllRecord<FactoryGroupGetDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryGroupStatusMessage.GroupsFetched,
                    GetAllData = groups
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "Group-Module",
                    "GetAllGroupsAsync",
                    tenantId,
                    null
                );

                return new GetAllRecord<FactoryGroupGetDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryGroupStatusMessage.GroupsFetchFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<FactoryGroupGetDto>> GetGroupByIdAsync(int tenantId, int groupId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var group = await tenantDb.FactoryGroups.Include(l => l.Location)
                    .Include(g => g.GroupUsers!)
                        .ThenInclude(gu => gu.User)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted);

                if (group == null)
                    return new GetSpecificRecord<FactoryGroupGetDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = FactoryGroupStatusMessage.GroupNotFound
                    };

                var result = new FactoryGroupGetDto
                {
                    GroupId = group.GroupId,
                    TenantId = group.TenantId,
                    Name = group.Name,
                    Type = group.Type,
                    LocationId = group.LocationId,
                    LocationName = group.Location!.LocationName,
                    Description = group.Description,
                    Users = group.GroupUsers!.Select(u => new FactoryGroupUserDto
                    {
                        UserId = u.UserId,
                        FullName = u.User!.FirstName + " " + u.User!.LastName,
                    }).ToList()
                };

                return new GetSpecificRecord<FactoryGroupGetDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryGroupStatusMessage.GroupFetched,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "Group-Module",
                    "GetGroupByIdAsync",
                    tenantId,
                    null
                );

                return new GetSpecificRecord<FactoryGroupGetDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryGroupStatusMessage.GroupFetchFailed}: {ex.Message}"
                };
            }
        }

    }
}
