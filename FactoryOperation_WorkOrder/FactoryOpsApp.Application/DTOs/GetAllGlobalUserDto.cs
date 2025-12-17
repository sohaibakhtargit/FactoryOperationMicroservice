using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class GetAllGlobalUserDto
    {
        public int GlobalUserId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string Email { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public string IpAddress { get; set; }
        public string Roles { get; set; }
        public int RoleId { get; set; }
        public int? SuspendedBy { get; set; }
        public bool Suspend { get; set; }
        public bool ForceLogout { get; set; }
        public string SuspensionReason { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }

    public class GetAllSuspendUserDto
    {
        public int GlobalUserId { get; set; }
        public string Email { get; set; }
        public int? SuspendedBy { get; set; }
        public bool Suspend { get; set; }
        public string SuspensionReason { get; set; }
    }
    public class GetInfoOfUserDto
    {

        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public string? Role { get; set; }
        public string? Contact { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? LocationName { get; set; }
        public string? JobTitle { get; set; }
        public string? Bio { get; set; }
        public string? IndustryType { get; set; }
        public string? ProfileUrl { get; set; }  
        public DateTime? CreatedAt { get; set; }
    }
    public class UpdateUserProfileDto
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string? Bio { get; set; }
        public string? Email {get; set; }
        public string? ContactNumber { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class UpdateSuperAdminProfileDto
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? Bio { get; set; }
        public string? LastName { get; set; }
        public string? ContactNumber { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class UpdateTenantProfileDto
    {

        public int TenantId { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Bio { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public int? UpdatedBy { get; set; }
    }
}