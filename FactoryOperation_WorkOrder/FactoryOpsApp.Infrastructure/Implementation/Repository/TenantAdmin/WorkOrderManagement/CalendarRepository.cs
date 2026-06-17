using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs.TechnicianLoadDto;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.WorkOrderManagement;

public class CalendarRepository : ICalendarRepository
{
    private readonly TenantDbContextFactory _tenantDbContext;

    public CalendarRepository(TenantDbContextFactory tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }
    private DateTime ToUtc(DateTime date)
    {
        return date.Kind == DateTimeKind.Utc
            ? date
            : DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
    }

    public async Task<GetSpecificRecord<CalendarDataDto>> GetCalendarAsync(
      int tenantId,
      DateOnly? from = null,
      DateOnly? to = null)
    {
        var response = new GetSpecificRecord<CalendarDataDto>();

        try
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var baseQuery = tenantDb.WorkOrders
                .Where(w => w.TenantId == tenantId && w.IsActive && !w.IsDeleted);

    
            var scheduledQuery = baseQuery.Where(w => w.ScheduleDate != null);

            if (from.HasValue && to.HasValue)
            {
                var fromDateTime = from.Value.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
                var toDateTime = to.Value.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();
                scheduledQuery = scheduledQuery
                    .Where(w => w.ScheduleDate.HasValue &&
                                w.ScheduleDate.Value >= fromDateTime &&
                                w.ScheduleDate.Value <= toDateTime);
            }

            var scheduledList = await scheduledQuery
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new
                {
                    w.WorkOrderId,
                    w.WorkOrderNumber,
                    w.Title,
                    w.Description, 

                    Priority = w.Priority.HasValue ? w.Priority.Value.ToString() : null,
                    Status = w.Status.HasValue ? w.Status.Value.ToString() : null,
                    WorkOrderType = w.WorkOrderType.HasValue ? w.WorkOrderType.Value.ToString() : null,

                    AssetName = w.Asset != null ? w.Asset.AssetName : null,
                    LocationName = w.Location != null ? w.Location.LocationName : null,

                    w.ScheduleDate,
                    w.EstimatedDurationMinutes,
                    w.AssignedToUserId,

                    w.LaborCost,  
                    w.PartCost,  
                    w.WorkOrderProgressMedia,
                    w.WorkOrderProgressMediaPath,

                    TechnicianName = w.AssignedToUser != null
                        ? w.AssignedToUser.FirstName + " " + w.AssignedToUser.LastName
                        : null
                })
                .ToListAsync();


            var scheduledEvents = scheduledList.Select(w => new CalendarEventDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderNumber = w.WorkOrderNumber,
                Title = w.Title,
                Description = w.Description, 

                Priority = w.Priority,
                Status = w.Status,
                WorkOrderType = w.WorkOrderType,
                AssetName = w.AssetName,
                LocationName = w.LocationName,

                Start = w.ScheduleDate,
                EstimatedDurationMinutes = w.EstimatedDurationMinutes,

                End = w.ScheduleDate.HasValue && w.EstimatedDurationMinutes.HasValue
                    ? w.ScheduleDate.Value.AddMinutes(w.EstimatedDurationMinutes.Value)
                    : (DateTime?)null,

                AssignedToUserId = w.AssignedToUserId,
                TechnicianName = string.IsNullOrEmpty(w.TechnicianName) ? "Unassigned" : w.TechnicianName,
                LaborCost = w.LaborCost,
                PartsCost = w.PartCost,

                Attachments = string.IsNullOrEmpty(w.WorkOrderProgressMedia)
                    ? new List<AttachmentDto>()
                    : new List<AttachmentDto>
                    {
                    new AttachmentDto
                    {
                        FileName = w.WorkOrderProgressMedia,
                        FilePath = w.WorkOrderProgressMediaPath
                    }
                    }
            }).ToList();

            var conflictSource = await tenantDb.WorkOrders
       .Where(w => w.TenantId == tenantId
                && !w.IsDeleted
                && w.ScheduleDate != null
                && w.AssignedToUserId != null)
       .Select(w => new
       {
           w.WorkOrderId,
           w.AssignedToUserId,
           TechnicianName = w.AssignedToUser != null
            ? w.AssignedToUser.FirstName + " " + w.AssignedToUser.LastName
            : "Unassigned",
           Start = w.ScheduleDate,
           End = w.ScheduleDate!.Value.AddMinutes(w.EstimatedDurationMinutes ?? 0)
       })
       .ToListAsync();

            var conflicts = conflictSource
                .GroupBy(x => x.AssignedToUserId)
                .SelectMany(group =>
                {
                    var list = group.OrderBy(x => x.Start).ToList();
                    var conflictList = new List<ConflictDto>();
                    var technicianName = list.FirstOrDefault()?.TechnicianName ?? "Unassigned";

                    for (int i = 0; i < list.Count; i++)
                    {
                        for (int j = i + 1; j < list.Count; j++)
                        {
                            if (list[i].End > list[j].Start)
                            {
                                conflictList.Add(new ConflictDto
                                {
                                    WorkOrderId1 = list[i].WorkOrderId,
                                    WorkOrderId2 = list[j].WorkOrderId,
                                    TechnicianId = group.Key,
                                    TechnicianName = technicianName,
                                    Start1 = list[i].Start,
                                    End1 = list[i].End,
                                    Start2 = list[j].Start,
                                    End2 = list[j].End
                                });
                            }
                        }
                    }

                    return conflictList;
                })
                .ToList();


            var unscheduledItems = await baseQuery
                .Where(w => w.ScheduleDate == null)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new UnscheduledItemDto
                {
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    Description = w.Description, 

                    Priority = w.Priority.HasValue ? w.Priority.Value.ToString() : null,
                    Status = w.Status.HasValue ? w.Status.Value.ToString() : null,
                    WorkOrderType = w.WorkOrderType.HasValue ? w.WorkOrderType.Value.ToString() : null,

                    LocationName = w.Location != null ? w.Location.LocationName : null,

                    Category = null,
                    Inventory = null,

                    EstimatedDurationMinutes = w.EstimatedDurationMinutes,
                    DueDate = w.DueDate,
                })
                .ToListAsync();

            var techLoadRaw = await baseQuery
                .Where(w => w.AssignedToUserId != null)
                .GroupBy(w => w.AssignedToUserId)
                .Select(g => new
                {
                    TechnicianId = g.Key,
                    TotalMinutes = g.Sum(x => x.EstimatedDurationMinutes ?? 0)
                })
                .ToListAsync();

            var techIds = techLoadRaw
                .Where(x => x.TechnicianId.HasValue)
                .Select(x => x.TechnicianId!.Value)
                .Distinct()
                .ToList();

            var users = await tenantDb.FactoryUsers
                .Where(u => u.TenantId == tenantId && techIds.Contains(u.UserId))
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            var technicianLoads = techLoadRaw.Select(t =>
            {
                var user = t.TechnicianId.HasValue
                    ? users.FirstOrDefault(u => u.UserId == t.TechnicianId.Value)
                    : null;

                return new TechnicianLoadDto
                {
                    TechnicianId = t.TechnicianId,
                    TechnicianName = user != null
                        ? $"{user.FirstName} {user.LastName}"
                        : "Unassigned",
                    TotalAssignedMinutes = t.TotalMinutes
                };
            }).ToList();

          
            var resourceRaw = await baseQuery
                .Where(w => w.ScheduleDate != null)
                .Select(w => new
                {
                    AssetName = w.Asset != null ? w.Asset.AssetName : "Unknown",

                    w.WorkOrderId,
                    w.WorkOrderNumber,
                    w.Title,
                    WorkOrderType = w.WorkOrderType.HasValue ? w.WorkOrderType.Value.ToString() : null,

                    w.ScheduleDate,
                    w.EstimatedDurationMinutes,

                    TechnicianName = w.AssignedToUser != null
                        ? w.AssignedToUser.FirstName + " " + w.AssignedToUser.LastName
                        : "Unassigned"
                })
                .ToListAsync();

            var resources = resourceRaw
                .GroupBy(x => x.AssetName)
                .Select(g => new ResourceDto
                {
                    AssetName = g.Key,
                    Tasks = g.Select(w => new ResourceTaskDto
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNumber = w.WorkOrderNumber,
                        Title = w.Title,
                        WorkOrderType = w.WorkOrderType,
                        TechnicianName = w.TechnicianName,

                        Start = w.ScheduleDate,
                        End = w.ScheduleDate.HasValue && w.EstimatedDurationMinutes.HasValue
                            ? w.ScheduleDate.Value.AddMinutes(w.EstimatedDurationMinutes.Value)
                            : (DateTime?)null
                    }).ToList()
                })
                .ToList();

            foreach (var item in scheduledEvents)
            {
                if (item.Start.HasValue)
                    item.Start = ToUtc(item.Start.Value);

                if (item.End.HasValue)
                    item.End = ToUtc(item.End.Value);
            }

            
            foreach (var item in unscheduledItems)
            {
                if (item.DueDate.HasValue)
                    item.DueDate = ToUtc(item.DueDate.Value);
            }

            foreach (var resource in resources)
            {
                foreach (var task in resource.Tasks)
                {
                    if (task.Start.HasValue)
                        task.Start = ToUtc(task.Start.Value);

                    if (task.End.HasValue)
                        task.End = ToUtc(task.End.Value);
                }
            }

            response.Data = new CalendarDataDto
            {
                ScheduledEvents = scheduledEvents,
                UnscheduledItems = unscheduledItems,
                TechnicianLoads = technicianLoads,
                Resources = resources,
                Conflicts = conflicts
            };

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Calendar data fetched successfully";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCode.Error;
            response.StatusMessage = $"Error fetching calendar data: {ex.Message}";
        }

        return response;
    }
}