using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using System.Text.Json;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Announcements
{
    public class AnnouncementTemplateService : IAnnouncementTemplateService
    {
        private readonly IAnnouncementTemplateRepository _repository;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AnnouncementTemplateService(IAnnouncementTemplateRepository repository,
            IExceptionLoggerService exceptionLogger)
        {
            _repository = repository;
            _exceptionLogger = exceptionLogger;
        }
        public async Task<GetAllRecord<AnnouncementTemplate>> GetAllAsync()
        {
            var result = new GetAllRecord<AnnouncementTemplate>();
            try
            {
                var templates = await _repository.GetAllAsync();

                result.StatusCode = "200";
                result.StatusMessage = "Templates retrieved successfully.";
                result.GetAllData = templates.ToList();
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementTemplateModule",
                    apiName: "get-AllTemplate",
                    tenantId: null,
                    userId: null
                );

                result.StatusCode = "500";
                result.StatusMessage = $"Failed to retrieve templates: {ex.Message}";
            }

            return result;
        }
        public async Task<GetSpecificRecord<AnnouncementTemplate>> GetByIdAsync(int id)
        {
            var response = new GetSpecificRecord<AnnouncementTemplate>();
            try
            {
                var template = await _repository.GetByIdAsync(id);

                if (template == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Template not found.";
                    return response;
                }

                response.StatusCode = "200";
                response.StatusMessage = "Template retrieved successfully.";
                response.Data = template;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementTemplateModule",
                    apiName: "get-TemplateById",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = "500";
                response.StatusMessage = $"Internal Server Error: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> CreateAsync(AnnouncementTemplateCreateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                var template = new AnnouncementTemplate
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Type = dto.Type,
                    TitleTemplate = dto.TitleTemplate,
                    MessageTemplate = dto.MessageTemplate,
                    DefaultChannels = JsonSerializer.Serialize(dto.DefaultChannels),
                    CreatedBy = dto.CreatedBy ?? 1,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(template);

                response.StatusCode = "200";
                response.StatusMessage = "Template created successfully.";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementTemplateModule",
                    apiName: "create-Template",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = "500";
                response.StatusMessage = $"Failed to create template: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAsync(AnnouncementTemplateUpdateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                var template = await _repository.GetByIdAsync(dto.TemplateId);

                if (template == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Template not found.";
                    return response;
                }

                if (dto.Name != null) template.Name = dto.Name;
                if (dto.Description != null) template.Description = dto.Description;
                if (dto.Type.HasValue) template.Type = dto.Type.Value;
                if (dto.TitleTemplate != null) template.TitleTemplate = dto.TitleTemplate;
                if (dto.MessageTemplate != null) template.MessageTemplate = dto.MessageTemplate;
                if (dto.DefaultChannels != null) template.DefaultChannels = JsonSerializer.Serialize(dto.DefaultChannels);

                template.UpdatedBy = dto.UpdatedBy ?? 1;
                template.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(template);

                response.StatusCode = "200";
                response.StatusMessage = "Template updated successfully.";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementTemplateModule",
                    apiName: "update-Template",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = "500";
                response.StatusMessage = $"Failed to update template: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteAsync(int id)
        {
            var response = new CommonResponseModel();
            try
            {
                var template = await _repository.GetByIdAsync(id);

                if (template == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Template not found.";
                    return response;
                }

                if (template.IsSystem)
                {
                    response.StatusCode = "400";
                    response.StatusMessage = "System templates cannot be deleted.";
                    return response;
                }

                await _repository.DeleteAsync(template);
                response.StatusCode = "200";
                response.StatusMessage = "Template deleted successfully.";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementTemplateModule",
                    apiName: "delete-Template",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = "500";
                response.StatusMessage = $"Failed to delete template: {ex.Message}";
            }

            return response;
        }

    }
}
