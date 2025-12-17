using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.RestoreBackup;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.RestoreBackup
{
    /// <summary>
    /// Backup and Restore Management API
    /// Manages database backups, restoration processes, and backup statistics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BackupRestoreController : ControllerBase
    {
        private readonly IBackupService _iBackupService;
        public BackupRestoreController(IBackupService iBackupService)
        {
            _iBackupService = iBackupService;
        }

        /// <summary>
        /// Create tenant database backup
        /// Generates backup for specific tenant database with configurable options
        /// </summary>
        /// <param name="request">Backup configuration details</param>
        /// <returns>Backup creation result</returns>
        /// <response code="200">Tenant backup successfully created</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("Create-DbBackup_TenantAdmin")]
        public async Task<IActionResult> CreateTenantBackupAsync([FromBody] BackupRequestDto request)
        {
            var data = await _iBackupService.CreateTenantBackupAsync(request);
            return Ok(data);
        }

        /// <summary>
        /// Create super admin database backup
        /// Generates system-wide backup including all tenants and configurations
        /// </summary>
        /// <param name="request">Backup configuration details</param>
        /// <returns>Backup creation result</returns>
        /// <response code="200">System backup successfully created</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("Create-DbBackup_SuperAdmin")]
        public async Task<IActionResult> CreateBackupAsync([FromBody] BackupRequestDto request)
        {
            var data = await _iBackupService.CreateBackupAsync(request);
            return Ok(data);
        }

        /// <summary>
        /// Restore database from backup
        /// Restores tenant or system database from existing backup file
        /// </summary>
        /// <param name="request">Restore configuration details</param>
        /// <returns>Restore operation result</returns>
        /// <response code="200">Backup successfully restored</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreBackup([FromBody] RestoreBackupRequestDto request)
        {
            var result = await _iBackupService.RestoreTenantBackupAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get all backup jobs
        /// Retrieves complete list of all backup jobs with their status and details
        /// </summary>
        /// <returns>List of all backup jobs</returns>
        /// <response code="200">Successfully retrieved backup jobs</response>
        [HttpGet("GetAllBackups")]
        public async Task<IActionResult> GetAllBackups()
        {
            var result = await _iBackupService.GetAllBackupJobsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get backup statistics
        /// Retrieves backup performance metrics and storage statistics
        /// </summary>
        /// <returns>Backup statistics and metrics</returns>
        /// <response code="200">Successfully retrieved backup statistics</response>
        [HttpGet]
        [Route("GetBackupStatistics")]
        public async Task<IActionResult> GetBackupStatistics()
        {
            var result = await _iBackupService.GetBackupStatisticsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Delete backup job
        /// Removes backup job and associated backup files from system
        /// </summary>
        /// <param name="backupJobId">Backup job identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Backup job successfully deleted</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("DeleteBackupJob")]
        public async Task<IActionResult> DeleteBackupJob(int backupJobId)
        {
            var result = await _iBackupService.DeleteBackupJobAsync(backupJobId);
            return Ok(result);
        }
    }
}