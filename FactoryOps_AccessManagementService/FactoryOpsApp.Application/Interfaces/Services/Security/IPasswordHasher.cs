namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
        bool IsLegacyHash(string passwordHash);
    }
}
