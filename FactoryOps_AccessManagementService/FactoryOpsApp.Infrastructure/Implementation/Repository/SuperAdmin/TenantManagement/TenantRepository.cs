using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.Security;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StackExchange.Redis;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.TenantManagement
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantRepository> _logger;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _iEmailService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IPasswordHasher _hasher;
        private readonly IConnectionMultiplexer _redis;

        public TenantRepository(IConfiguration configuration,
            ILogger<TenantRepository> logger,
            MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor,
            TenantDbContextFactory tenantDbContext,
            IEmailService iEmailService,
            IFileStorageService fileStorageService, 
            IExceptionLoggerService exceptionLogger,
            IPasswordHasher hasher,
            IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _logger = logger;
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
            _iEmailService = iEmailService;
            _fileStorageService = fileStorageService;
            _exceptionLogger = exceptionLogger;
            _tenantDbContext = tenantDbContext;
            _hasher = hasher;
            _redis = redis;
        }
        public async Task<bool> CloneTenantDatabaseAsync(string newTenantDbName, string templateDbName = "FactoryOperation")
        {
            var masterConnectionString = _configuration.GetConnectionString("MasterDbConnection");

            try
            {
                await using var connection = new NpgsqlConnection(masterConnectionString);
                await connection.OpenAsync();

            
                var terminateConnectionsCommand = $@"
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{templateDbName}'
                  AND pid <> pg_backend_pid();";

                await using (var terminateCmd = new NpgsqlCommand(terminateConnectionsCommand, connection))
                {
                    await terminateCmd.ExecuteNonQueryAsync();
                }

                var createDbCommand = $@"CREATE DATABASE ""{newTenantDbName}"" WITH TEMPLATE ""{templateDbName}"";";

                _logger.LogInformation("Executing DB clone: {Sql}", createDbCommand);

                await using var cmd = new NpgsqlCommand(createDbCommand, connection);
                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation("Database cloned successfully for tenant {DB}", newTenantDbName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning tenant DB");
                return false;
            }
        }
        //public async Task<CommonResponseModel> AddTenant(AddTenantDto tenant)
        //{
        //    CommonResponseModel response = new();
        //    using var tran = await _masterDbcontext.Database.BeginTransactionAsync();
        //    int TenantId = 0;
        //    try
        //    {
        //        var existing = await _masterDbcontext.FactoryTenants
        //            .FirstOrDefaultAsync(t =>
        //                (t.TenantName == tenant.TenantName || t.AdminEmail == tenant.AdminEmail)
        //                && t.IsDeleted == false
        //                && t.IsActive == true);

        //        if (existing != null)
        //        {
        //            response.StatusCode = StatusCode.BadRequest;
        //            response.StatusMessage = TenantStatusMessage.BadRequest;
        //            return response;
        //        }

        //        string? relativePath = null;
        //        byte[]? imageBytes = null;

        //        if (tenant.EnableBranding)
        //        {
        //            if (tenant.ImageFile == null)
        //            {
        //                throw new ArgumentNullException(nameof(tenant.ImageFile), "Image file is required when branding is enabled");
        //            }

        //            relativePath = await _fileStorageService.SaveFileAsync(tenant.ImageFile, "Images");
        //            imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));
        //        }
        //        var tempPassword = tenant.TenantName + "@123";
        //        var hashedPassword = _hasher.Hash(tempPassword);

        //        var objTenant = new FactoryTenants
        //        {
        //            TenantName = tenant.TenantName,
        //            DomainOrSubdomain = tenant.DomainOrSubdomain,
        //            AdminEmail = tenant.AdminEmail,
        //            IndustryType = tenant.IndustryType ?? "",
        //            Plan = tenant.Plan,
        //            Status = tenant.Status,
        //            MaxUsers = tenant.MaxUsers,
        //            MaxAssets = tenant.MaxAssets,
        //            MaxStorage = tenant.MaxStorage,
        //            LastActiveDate = tenant.LastActiveDate,
        //            EnableBranding = tenant.EnableBranding,
        //            ImageName = imageBytes,
        //            BrandingLogoUrl = relativePath,
        //            TimeZone = tenant.TimeZone,
        //            DefaultLanguage = tenant.DefaultLanguage,
        //            IsActive = true,
        //            IsDeleted = false,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = 1,
        //        };
        //        await _masterDbcontext.FactoryTenants.AddAsync(objTenant);

        //        await _masterDbcontext.TenantIsolations.AddAsync(new TenantIsolation
        //        {
        //            TenantId = objTenant.TenantId,
        //            TenantName = objTenant.TenantName,
        //            CustomBranding = objTenant.EnableBranding,
        //            LogoUrl = objTenant.BrandingLogoUrl,
        //            LogoText = null, // if not available
        //            DataEncryption = "Disabled"
        //        });
        //        await _masterDbcontext.SaveChangesAsync();

        //        TenantId = objTenant.TenantId;
        //        var safeDbName = GenerateSafeDbName(tenant.TenantName);

        //        var TenantMasterMapping = new TenantMasterMapping()
        //        {
        //            TenantName = tenant.TenantName,
        //            DbName = safeDbName,
        //            TenantId = objTenant.TenantId,
        //            IsActive = true,
        //            IsDeleted = false,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = 1,
        //        };
        //        await _masterDbcontext.TenantMasterMapping.AddAsync(TenantMasterMapping);

        //        var TenantAdminLogin = new TenantAdminLogin()
        //        {
        //            Email = tenant.AdminEmail,
        //            PasswordHash = hashedPassword,
        //            RoleId = 2,
        //            TenantId = objTenant.TenantId,
        //            IsActive = true,
        //            IsDeleted = false,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = 1,
        //        };
        //        await _masterDbcontext.TenantAdminLogins.AddAsync(TenantAdminLogin);

        //        var allModules = await _masterDbcontext.ModuleMaster
        //            .Where(m => m.IsActive && !m.IsDeleted)
        //            .ToListAsync();

        //        if (allModules.Any())
        //        {
        //            var moduleMappings = allModules.Select(m => new ModuleMasterMapping
        //            {
        //                TenantId = TenantId,
        //                ModuleId = m.ModuleId,
        //                IsActive = true,
        //                IsDeleted = false,
        //                CreatedBy = 1,
        //                CreatedAt = DateTime.UtcNow
        //            }).ToList();

        //            await _masterDbcontext.ModuleMasterMapping.AddRangeAsync(moduleMappings);
        //        }

        //        var ctx = _httpContextAccessor.HttpContext;
        //        var auditData = new Audit_Log_MasterDb()
        //        {
        //            Action = "Create",
        //            Details = "Tenant-Added",
        //            EventType = "",
        //            TenantId = null,
        //            Email = "",
        //            Timestamp = DateTime.UtcNow,
        //            IsActive = true,
        //            IsDeleted = false,
        //            UserName = Environment.UserName,
        //            Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString() ?? "N/A",
        //        };

        //        await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
        //        await _masterDbcontext.SaveChangesAsync();
        //        await tran.CommitAsync();

        //        //var dbName = tenant.TenantName;

        //        var dbCreated = await CloneTenantDatabaseAsync(safeDbName);

        //        if (!dbCreated)
        //        {
        //            response.StatusCode = StatusCode.Error;
        //            response.StatusMessage = TenantStatusMessage.DBError;
        //            return response;
        //        }

        //        var emailDto = new EmailDTO
        //        {
        //            From = "shoaibmaliklenovo@gmail.com",
        //            To = tenant.AdminEmail ?? "factory.operation@yopmail.com",
        //            Subject = "Your New Account Credentials",
        //            Body = $@"<html>
        //        <body>
        //            <h2>Welcome, {tenant.TenantName}!</h2>
        //            <p>Your account has been successfully created.</p>
        //            <p><strong>Email:</strong> {tenant.AdminEmail}</p>
        //            <p><strong>Temporary Password:</strong> {tempPassword}</p>
        //            <p>Please change your password after first login.</p>
        //        </body>
        //        </html>"
        //        };

        //        var emailResponse = await _iEmailService.SendEmailAsync(emailDto);
        //        if (!emailResponse.Success)
        //        {
        //            response.StatusCode = StatusCode.Error;
        //            response.StatusMessage = TenantStatusMessage.EmailError;
        //        }

        //        response.StatusCode = StatusCode.Success;
        //        response.StatusMessage = TenantStatusMessage.TenantAdded;
        //    }
        //    catch (Exception ex)
        //    {
        //        await tran.RollbackAsync();
        //        await _exceptionLogger.LogExceptionAsync(
        //            ex,
        //            sourceModule: "TenantModule",
        //            apiName: "Add-Tenant",
        //            tenantId: TenantId,
        //            userId: null
        //        );
        //        response.StatusCode = StatusCode.Error;
        //        response.StatusMessage = ex.Message;
        //    }
        //    return response;
        //}

        public async Task<CommonResponseModel> AddTenant(AddTenantDto tenant)
        {
            CommonResponseModel response = new();
            using var tran = await _masterDbcontext.Database.BeginTransactionAsync();
            int TenantId = 0;

            try
            {
                var existing = await _masterDbcontext.FactoryTenants
                    .FirstOrDefaultAsync(t =>
                        (t.TenantName == tenant.TenantName || t.AdminEmail == tenant.AdminEmail)
                        && t.IsDeleted == false
                        && t.IsActive == true);

                if (existing != null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = TenantStatusMessage.BadRequest;
                    return response;
                }

                string? relativePath = null;
                byte[]? imageBytes = null;

                if (tenant.EnableBranding)
                {
                    if (tenant.ImageFile == null)
                        throw new ArgumentNullException(nameof(tenant.ImageFile));

                    relativePath = await _fileStorageService.SaveFileAsync(tenant.ImageFile, "Images");
                    imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));
                }

                var tempPassword = tenant.TenantName + "@123";
                var hashedPassword = _hasher.Hash(tempPassword);

                var objTenant = new FactoryTenants
                {
                    TenantName = tenant.TenantName,
                    DomainOrSubdomain = tenant.DomainOrSubdomain,
                    AdminEmail = tenant.AdminEmail,
                    IndustryType = tenant.IndustryType ?? "",
                    Plan = tenant.Plan,
                    Status = tenant.Status,
                    MaxUsers = tenant.MaxUsers,
                    MaxAssets = tenant.MaxAssets,
                    MaxStorage = tenant.MaxStorage,
                    LastActiveDate = tenant.LastActiveDate,
                    EnableBranding = tenant.EnableBranding,
                    ImageName = imageBytes,
                    BrandingLogoUrl = relativePath,
                    TimeZone = tenant.TimeZone,
                    DefaultLanguage = tenant.DefaultLanguage,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                };

                await _masterDbcontext.FactoryTenants.AddAsync(objTenant);
                await _masterDbcontext.SaveChangesAsync();

                TenantId = objTenant.TenantId;

                //ADD Isolation inside transaction
                await _masterDbcontext.TenantIsolations.AddAsync(new TenantIsolation
                {
                    TenantId = TenantId,
                    TenantName = objTenant.TenantName,
                    CustomBranding = objTenant.EnableBranding,
                    LogoUrl = objTenant.BrandingLogoUrl,
                    LogoText = null,
                    DataEncryption = "Disabled"
                });

                var safeDbName = GenerateSafeDbName(tenant.TenantName);

                await _masterDbcontext.TenantMasterMapping.AddAsync(new TenantMasterMapping
                {
                    TenantName = tenant.TenantName,
                    DbName = safeDbName,
                    TenantId = TenantId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                });

                await _masterDbcontext.TenantAdminLogins.AddAsync(new TenantAdminLogin
                {
                    Email = tenant.AdminEmail,
                    PasswordHash = hashedPassword,
                    RoleId = 2,
                    TenantId = TenantId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                });

                var allModules = await _masterDbcontext.ModuleMaster
                    .Where(m => m.IsActive && !m.IsDeleted)
                    .ToListAsync();

                if (allModules.Any())
                {
                    var moduleMappings = allModules.Select(m => new ModuleMasterMapping
                    {
                        TenantId = TenantId,
                        ModuleId = m.ModuleId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = 1,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _masterDbcontext.ModuleMasterMapping.AddRangeAsync(moduleMappings);
                }

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(new Audit_Log_MasterDb
                {
                    Action = "Create",
                    Details = "Tenant-Added",
                    TenantId = null,
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "N/A",
                });

                await _masterDbcontext.SaveChangesAsync();

                var dbCreated = await CloneTenantDatabaseAsync(safeDbName);

                if (!dbCreated)
                {
                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = TenantStatusMessage.DBError;
                    return response;
                }

                await tran.CommitAsync();
                var emailDto = new EmailDTO
                {
                    From = "shoaibmaliklenovo@gmail.com",
                    To = tenant.AdminEmail ?? "factory.operation@yopmail.com",
                    Subject = "Your New Account Credentials",
                    Body = $@"<html>
                <body>
                    <h2>Welcome, {tenant.TenantName}!</h2>
                    <p>Your account has been successfully created.</p>
                    <p><strong>Email:</strong> {tenant.AdminEmail}</p>
                    <p><strong>Temporary Password:</strong> {tempPassword}</p>
                    <p>Please change your password after first login.</p>
                </body>
                </html>"
                };

                var emailResponse = await _iEmailService.SendEmailAsync(emailDto);
                if (!emailResponse.Success)
                {
                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = TenantStatusMessage.EmailError;
                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.TenantAdded;
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "TenantModule", "Add-Tenant", TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }

        public async Task<GetAllRecord<GetAllTenantsDto>> GetAllTenants()
        {
            var response = new GetAllRecord<GetAllTenantsDto>();

            try
            {
                string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8128";

                var tenants = await _masterDbcontext.FactoryTenants
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.TenantId)
                    .ToListAsync();

                var tenantList = new List<GetAllTenantsDto>();

                foreach (var tenant in tenants)
                {
                    var tenantDatabase = await _masterDbcontext.TenantMasterMapping
                        .AsNoTracking()
                        .FirstOrDefaultAsync(tm => tm.TenantId == tenant.TenantId && !tm.IsDeleted && tm.IsActive);

                    if (tenantDatabase == null)
                        throw new Exception($"Tenant DB not found for TenantId {tenant.TenantId}");

                    string databaseName = tenantDatabase.DbName;

                    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);
                    var activeUsersCount = await tenantDb.FactoryUsers
                        .CountAsync(u => u.IsActive && !u.IsDeleted);

                    long useStorageBytes = 0;
                    try
                    {
                        await _masterDbcontext.Database.OpenConnectionAsync();
                        var command = _masterDbcontext.Database.GetDbConnection().CreateCommand();
                        command.CommandText = "SELECT pg_database_size(@dbName)";
                        command.Parameters.Add(new NpgsqlParameter("dbName", databaseName));

                        useStorageBytes = Convert.ToInt64(await command.ExecuteScalarAsync());
                    }
                    catch (Exception ex)
                    {
                        await _exceptionLogger.LogExceptionAsync(
                            ex,
                            sourceModule: "TenantModule",
                            apiName: "get-Tenants-db-size",
                            tenantId: tenant.TenantId,
                            userId: null
                        );
                    }
                    finally
                    {
                        await _masterDbcontext.Database.CloseConnectionAsync();
                    }
                    var tenantRole = await (from t in _masterDbcontext.TenantAdminLogins
                                            join r in _masterDbcontext.AdminRoles
                                                on t.RoleId equals r.RoleId
                                            where t.TenantId == tenant.TenantId
                                                  && t.IsActive
                                                  && !t.IsDeleted
                                            select new
                                            {
                                                RoleId = t.RoleId,
                                                RoleName = r.RoleName
                                            })
                        .FirstOrDefaultAsync();

                    var tenantModules = await (from map in _masterDbcontext.ModuleMasterMapping
                                               join mod in _masterDbcontext.ModuleMaster
                                                   on map.ModuleId equals mod.ModuleId
                                               where map.TenantId == tenant.TenantId
                                                     && map.IsActive
                                                     && !map.IsDeleted
                                                     && mod.IsActive
                                                     && !mod.IsDeleted
                                               select new ModuleDto
                                               {
                                                   ModuleId = mod.ModuleId,
                                                   ModuleName = mod.ModuleName
                                               })
                                              .ToListAsync();

                    tenantList.Add(new GetAllTenantsDto
                    {
                        TenantId = tenant.TenantId,
                        TenantName = tenant.TenantName,
                        DomainOrSubdomain = tenant.DomainOrSubdomain,
                        AdminEmail = tenant.AdminEmail,
                        IndustryType = tenant.IndustryType,
                        RoleId = tenantRole?.RoleId,
                        RoleName = tenantRole?.RoleName,
                        Plan = tenant.Plan,
                        Status = tenant.Status,
                        Suspend = tenant.Suspend,
                        ForceLogout = tenant.ForceLogout,
                        MaxUsers = tenant.MaxUsers,
                        MaxAssets = tenant.MaxAssets,
                        MaxStorage = tenant.MaxStorage,
                        ActiveUsers = activeUsersCount,
                        UseStorage = (double)useStorageBytes / (1024 * 1024 * 1024),
                        LastActiveDate = tenant.LastActiveDate,
                        EnableBranding = tenant.EnableBranding,
                        BrandingLogoUrl = tenant.BrandingLogoUrl != null
                            ? $"{baseUrl}/{tenant.BrandingLogoUrl.Replace("\\", "/")}"
                            : null,
                        TimeZone = tenant.TimeZone,
                        DefaultLanguage = tenant.DefaultLanguage,
                        CreatedAt = tenant.CreatedAt,
                        IsActive = tenant.IsActive,
                        IsDeleted = tenant.IsDeleted,
                        Modules = tenantModules
                    });
                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.DataFetched;
                response.GetAllData = tenantList;
            }
            catch (Exception ex) 
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TenantModule",
                    apiName: "get-Tenants",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public async Task<GetAllRecord<ModulelistDto>> GetAllModuleList()
        {
            var response = new GetAllRecord<ModulelistDto>();
            try
            {
                var list = _masterDbcontext.ModuleMaster
                    .Where(p => !p.IsDeleted)
                    .Select(p => new ModulelistDto
                    {
                        ModuleId = p.ModuleId,
                        ModuleName = p.ModuleName
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.AllModuleDataFetched;
                response.GetAllData = list;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "ModulePermission",
                   apiName: "GetAllModules",
                   tenantId: null,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TenantStatusMessage.PermissionsError}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateTenants(AddTenantDto tenant)
        {
            CommonResponseModel response = new();
            using var tran = await _masterDbcontext.Database.BeginTransactionAsync();

            try
            {
                var existing = await _masterDbcontext.FactoryTenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId && !t.IsDeleted);

                if (existing == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TenantStatusMessage.FetchFailed;
                    return response;
                }

                var originalTenantName = existing.TenantName;
                var originalAdminEmail = existing.AdminEmail;

                bool tenantNameExists = await _masterDbcontext.FactoryTenants
                    .AnyAsync(t => t.TenantName == tenant.TenantName && t.TenantId != tenant.TenantId && !t.IsDeleted && t.IsActive);

                bool emailExists = await _masterDbcontext.FactoryTenants
                    .AnyAsync(t => t.AdminEmail == tenant.AdminEmail && t.TenantId != tenant.TenantId && !t.IsDeleted && t.IsActive);

                if (tenantNameExists || emailExists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = tenantNameExists
                        ? TenantStatusMessage.TenantNameExists
                        : TenantStatusMessage.AdminEmailExists;
                    return response;
                }

                if (tenant.ImageFile != null)
                {
                    string relativePath = await _fileStorageService.SaveFileAsync(tenant.ImageFile, "Images");
                    existing.BrandingLogoUrl = relativePath;
                    existing.ImageName = await _fileStorageService.GetFileBytesAsync(relativePath);
                }

                existing.TenantName = tenant.TenantName;
                existing.DomainOrSubdomain = tenant.DomainOrSubdomain;
                existing.AdminEmail = tenant.AdminEmail;
                existing.IndustryType = tenant.IndustryType!;
                existing.Plan = tenant.Plan;
                existing.Status = tenant.Status;
                existing.MaxUsers = tenant.MaxUsers;
                existing.MaxAssets = tenant.MaxAssets;
                existing.MaxStorage = tenant.MaxStorage;
                existing.LastActiveDate = tenant.LastActiveDate;
                existing.EnableBranding = tenant.EnableBranding;
                existing.TimeZone = tenant.TimeZone;
                existing.DefaultLanguage = tenant.DefaultLanguage;
                existing.IsActive = true;
                existing.IsDeleted = false;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = 1;

                var mapping = await _masterDbcontext.TenantMasterMapping
                    .FirstOrDefaultAsync(m => m.TenantName == originalTenantName);

                if (mapping is not null && mapping.TenantName != tenant.TenantName)
                {
                    mapping.TenantName = tenant.TenantName;
                }

                var adminLogin = await _masterDbcontext.TenantAdminLogins
                    .FirstOrDefaultAsync(a => a.Email == originalAdminEmail && !a.IsDeleted);

                if (adminLogin is not null)
                {
                    

                    if (adminLogin.Email != tenant.AdminEmail)
                    {
                        adminLogin.Email = tenant.AdminEmail;
                    }

                    adminLogin.UpdatedAt = DateTime.UtcNow;
                    adminLogin.UpdatedBy = 1;
                }

                var isolation = await _masterDbcontext.TenantIsolations
           .FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId);

                if (isolation != null)
                {
                    isolation.TenantName = tenant.TenantName;
                    isolation.CustomBranding = tenant.EnableBranding;
                    isolation.LogoUrl = tenant.EnableBranding ? existing.BrandingLogoUrl : null;

                    _masterDbcontext.TenantIsolations.Update(isolation);
                }
                else
                {
                    await _masterDbcontext.TenantIsolations.AddAsync(new TenantIsolation
                    {
                        TenantId = tenant.TenantId,
                        TenantName = tenant.TenantName,
                        CustomBranding = tenant.EnableBranding,
                        LogoUrl = tenant.EnableBranding ? existing.BrandingLogoUrl : null,
                        DataEncryption = "Disabled"
                    });
                }

                await _masterDbcontext.SaveChangesAsync();

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Update",
                    Details = "Tenant-Updated",
                    EventType = "",
                    TenantId = tenant.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
                await _masterDbcontext.SaveChangesAsync();
                await tran.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.TenantUpdated;
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TenantModule",
                    apiName: "update-Tenants",
                    tenantId: tenant?.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateTenantModulesAsync(UpdateTenantModulesDto dto)
        {
            CommonResponseModel response = new();

            try
            {
                var tenantExists = await _masterDbcontext.FactoryTenants
                    .AnyAsync(t => t.TenantId == dto.TenantId && !t.IsDeleted && t.IsActive);

                if (!tenantExists)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TenantStatusMessage.TenantFailed;
                    return response;
                }

                var existingMappings = await _masterDbcontext.ModuleMasterMapping
                    .Where(x => x.TenantId == dto.TenantId)
                    .ToListAsync();

                var activeModuleIds = dto.ModuleIds.ToHashSet();

                foreach (var map in existingMappings)
                {
                    if (activeModuleIds.Contains(map.ModuleId ?? 0))
                    {
                        map.IsActive = true;
                        map.IsDeleted = false;
                        map.UpdatedAt = DateTime.UtcNow;
                        map.UpdatedBy = 1;
                    }
                    else
                    {
                        map.IsActive = false;
                        map.IsDeleted = true;
                        map.UpdatedAt = DateTime.UtcNow;
                        map.UpdatedBy = 1;
                    }
                }

                var existingIds = existingMappings.Select(m => m.ModuleId ?? 0).ToHashSet();
                var missingIds = activeModuleIds.Except(existingIds);

                if (missingIds.Any())
                {
                    var newMappings = missingIds.Select(mid => new ModuleMasterMapping
                    {
                        TenantId = dto.TenantId,
                        ModuleId = mid,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = 1,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _masterDbcontext.ModuleMasterMapping.AddRangeAsync(newMappings);
                }

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Update",
                    Details = "Tenant Modules Updated",
                    EventType = "",
                    TenantId = dto.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
                await _masterDbcontext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.TenantModulesUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TenantModule",
                    apiName: "update-tenant-modules",
                    tenantId: dto?.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteTenants(int Id)
        {
            CommonResponseModel response = new CommonResponseModel();
            try
            {
                var existingTenant = await _masterDbcontext.FactoryTenants
               .FirstOrDefaultAsync(t => t.TenantId == Id && t.IsDeleted == false);

                if (existingTenant == null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = TenantStatusMessage.DeletedBadRequest;
                }
                else
                {
                    existingTenant.IsDeleted = true;
                    existingTenant.IsActive = false;

                    var existingMapTanent = await _masterDbcontext.TenantMasterMapping.FirstOrDefaultAsync(t => t.TenantId == Id && t.IsDeleted == false);
                    existingMapTanent!.IsDeleted = true;
                    existingMapTanent.IsActive = false;

                    var existingLoginTanent = await _masterDbcontext.TenantAdminLogins.FirstOrDefaultAsync(t => t.TenantId == Id && t.IsDeleted == false);
                    existingLoginTanent!.IsDeleted = true;
                    existingLoginTanent.IsActive = false;

                    var tenantUsers = await _masterDbcontext.GlobalUsers
                    .Where(u => u.TenantId == Id && u.IsDeleted == false)
                    .ToListAsync();

                    foreach (var user in tenantUsers)
                    {
                        user.IsActive = false;
                        user.IsDeleted = true;
                        user.DeletedAt = DateTime.UtcNow;
                    }

                    var ctx = _httpContextAccessor.HttpContext;
                    var auditData = new Audit_Log_MasterDb()
                    {
                        Action = "Delete",
                        Details = "Tenant-Deleted",
                        EventType = "",
                        TenantId = null,
                        Email = "",
                        Timestamp = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                        UserName = Environment.UserName,
                        Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                    };

                    await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);

                    await _masterDbcontext.SaveChangesAsync();
                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = TenantStatusMessage.TenantDeleted;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TenantModule",
                    apiName: "Delete-Tenants",
                    tenantId: Id,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public async Task<CommonResponseModel> ChangeTenants(int Id)
        {
            CommonResponseModel response = new CommonResponseModel();
            try
            {
                var existingTenant = await _masterDbcontext.FactoryTenants
               .FirstOrDefaultAsync(t => t.TenantId == Id && t.IsDeleted == false);

                if (existingTenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TenantStatusMessage.TenantFailed;
                }
                else
                {
                    existingTenant.IsActive = !existingTenant.IsActive;

                    var ctx = _httpContextAccessor.HttpContext;
                    var auditData = new Audit_Log_MasterDb()
                    {
                        Action = "Update",
                        Details = "Tenant Status Changes",
                        EventType = "",
                        TenantId = null,
                        Email = "",
                        Timestamp = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                        UserName = Environment.UserName,
                        Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                    };

                    await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);

                    await _masterDbcontext.SaveChangesAsync();
                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = TenantStatusMessage.StatusUpdated;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TenantModule",
                    apiName: "Change-Status",
                    tenantId: Id,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public async Task<CommonResponseModel> ForceLogout(int Id)
        {
            CommonResponseModel response = new CommonResponseModel();

            try
            {
                var existingTenant = await _masterDbcontext.FactoryTenants
               .FirstOrDefaultAsync(t => t.TenantId == Id);
                if (existingTenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TenantStatusMessage.TenantFailed;
                    return response;
                }

                if (existingTenant.ForceLogout)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = TenantStatusMessage.PermissionBadRequest;
                    return response;
                }

                existingTenant.ForceLogout = true;

                var tenantAdmin = await _masterDbcontext.TenantAdminLogins
                    .FirstOrDefaultAsync(a => a.TenantId == Id && !a.IsDeleted);

                if (tenantAdmin != null)
                {
                    tenantAdmin.ForceLogout = true;
                    tenantAdmin.UpdatedAt = DateTime.UtcNow;
                }

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Update",
                    Details = "Force-Logout",
                    EventType = "",
                    TenantId = null,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);

                await _masterDbcontext.SaveChangesAsync();

                // Redis Cache Clear for TenantAdmin
                if (tenantAdmin != null)
                {
                    var redisDb = _redis.GetDatabase();

                    string cacheKey = $"user-status:{Id}:{tenantAdmin.Id}";

                    await redisDb.KeyDeleteAsync(cacheKey);
                }


                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.LogOutRequest;

            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "TenantModule",
                        apiName: "force-logout-tenant",
                        tenantId: Id,
                        userId: null
                    );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public async Task<CommonResponseModel> Suspend(int tenantId)
        {
            var response = new CommonResponseModel();

            try
            {
                var tenant = await _masterDbcontext.FactoryTenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TenantStatusMessage.TenantFailed;
                    return response;
                }

                tenant.Suspend = !tenant.Suspend;
                tenant.Status = !tenant.Suspend;
                var now = DateTime.UtcNow;
                var action = tenant.Suspend ? "Suspend" : "Unsuspend";
                if (!tenant.Suspend)
                {
                    tenant.ForceLogout = false;
                }

                var tenantAdmin = await _masterDbcontext.TenantAdminLogins
                    .FirstOrDefaultAsync(a => a.TenantId == tenantId && !a.IsDeleted);

                if (tenantAdmin != null)
                {
                    tenantAdmin.Suspend = tenant.Suspend;
                    tenantAdmin.Status = !tenant.Suspend;
                    tenantAdmin.UpdatedAt = now;

                    if (!tenantAdmin.Suspend)
                    {
                        tenantAdmin.ForceLogout = false;
                    }
                }

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(new Audit_Log_MasterDb
                {
                    Action = "Update",
                    Details = $"Tenant {action.ToLower()}ed",
                    EventType = "TenantStatusChange",
                    TenantId = tenant.TenantId,
                    Email = tenant.AdminEmail ?? string.Empty,
                    Timestamp = now,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });

                
                tenant.UpdatedAt = now;
                await _masterDbcontext.SaveChangesAsync();

                // Redis Cache Clear
                if (tenantAdmin != null)
                {
                    var redisDb = _redis.GetDatabase();

                    string cacheKey = $"user-status:{tenantId}:{tenantAdmin.Id}";

                    await redisDb.KeyDeleteAsync(cacheKey);
                }


                response.StatusCode = StatusCode.Success;
                response.StatusMessage = string.Format(
                    TenantStatusMessage.TenantAndAdminLoginsActionSuccess,
                    action.ToLower() + "ed"
                );
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TenantModule",
                    apiName: "suspend-tenant",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TenantStatusMessage.ToggleSuspensionFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<ModulelistDto>> GetAllModuleAsync(int tenantId)
        {
            var response = new GetAllRecord<ModulelistDto>();

            try
            {
                var list = await _masterDbcontext.ModuleMasterMapping
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
                    .Join(
                        _masterDbcontext.ModuleMaster,
                        x => x.ModuleId,
                        m => m.ModuleId,
                        (x, m) => new ModulelistDto
                        {
                            ModuleId = x.ModuleId,
                            ModuleName = m.ModuleName
                        })
                    .ToListAsync();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantStatusMessage.AllModuleDataFetched;
                response.GetAllData = list;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "ModulePermission",
                    apiName: "GetAllModules",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TenantStatusMessage.PermissionsError}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetSpecificRecord<TenantDashboardDto>>
        GetTenantDashboard(int tenantId)
        {
            var response = new GetSpecificRecord<TenantDashboardDto>();

            try
            {
                var tenant = await _masterDbcontext.FactoryTenants
                    .FirstOrDefaultAsync(t =>
                        t.TenantId == tenantId &&
                        !t.IsDeleted &&
                        t.IsActive);

                if (tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = "Tenant not found";
                    return response;
                }

                // Tenant DB Context
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int activeUsers = await tenantDb.FactoryUsers
                    .CountAsync(x => x.IsActive && !x.IsDeleted);

                int totalAssets = await tenantDb.AssetRegistry
                    .CountAsync(x => !x.IsDeleted);

                // Storage Calculation
                double usedStorageGB = 0;
                var mapping = await _masterDbcontext.TenantMasterMapping
                    .FirstAsync(x => x.TenantId == tenantId);

                await _masterDbcontext.Database.OpenConnectionAsync();
                var cmd = _masterDbcontext.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = "SELECT pg_database_size(@db)";
                cmd.Parameters.Add(new NpgsqlParameter("db", mapping.DbName));

                long sizeBytes = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                usedStorageGB = sizeBytes / (1024.0 * 1024 * 1024);
                await _masterDbcontext.Database.CloseConnectionAsync();

                response.Data = new TenantDashboardDto
                {
                    TenantId = tenant.TenantId,
                    TenantName = tenant.TenantName,
                    AdminEmail = tenant.AdminEmail,
                    Plan = tenant.Plan,

                    ActiveUsers = activeUsers,
                    MaxUsers = tenant.MaxUsers,

                    TotalAssets = totalAssets,
                    MaxAssets = tenant.MaxAssets,

                    UsedStorageGB = usedStorageGB,
                    MaxStorageGB = tenant.MaxStorage,

                    ContactNumber = tenant.ContactNumber,
                    LastActiveDate = tenant.LastActiveDate
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Dashboard fetched";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "TenantDashboard",
                    "GetDashboard",
                    tenantId,
                    null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }

        private string GenerateSafeDbName(string tenantName)
        {
            return "FactoryOperation_" + tenantName
                .Trim()
                .ToLower()
                .Replace(" ", "_")
                .Replace("-", "_");
        }

    }
}
