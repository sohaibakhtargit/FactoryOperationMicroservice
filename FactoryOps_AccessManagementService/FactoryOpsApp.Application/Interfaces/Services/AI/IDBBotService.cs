using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI
{
    public interface IDBBotService
    {
        Task<DbBotResponseDto> AskAsync(DbBotRequestDto request);
    }
}
