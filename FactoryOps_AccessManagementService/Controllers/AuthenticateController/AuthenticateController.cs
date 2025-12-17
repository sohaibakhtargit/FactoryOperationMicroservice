using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Authentication;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.AuthenticateController
{  /// <summary>
   /// Authentication API
   /// Manages user authentication, tenant switching, and password recovery
   /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IFactoryAuthenticationService _iAuthService;
        public AuthenticateController(IFactoryAuthenticationService iAuthService)
        {
            _iAuthService = iAuthService;
        }

        /// <summary>
        /// User login
        /// Authenticates user credentials and returns access token
        /// </summary>
        /// <param name="login">User login credentials</param>
        /// <returns>Authentication token and user information</returns>
        /// <response code="200">Successfully authenticated</response>
        /// <response code="400">Invalid login data provided</response>
        /// <response code="401">Authentication failed</response>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new ResponseToken
                {
                    StatusCode = "400",
                    StatusMessage = "Email and Password are required."
                });
            }

            var result = await _iAuthService.UnifiedAuthenticate(login);

            if (result.StatusCode == "200")
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }

        /// <summary>
        /// Switch tenant
        /// Allows super admin to switch between different tenant contexts
        /// </summary>
        /// <param name="request">Tenant switching request</param>
        /// <returns>Tenant switch confirmation</returns>
        /// <response code="200">Successfully switched tenant</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("switch-to-tenant")]
        public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantRequestDto request)
        {
            var response = await _iAuthService.SwitchTenantAsync(request.TenantId);
            return Ok(response);
        }

        /// <summary>
        /// Forget password
        /// Initiates password recovery process for user account
        /// </summary>
        /// <param name="dto">Password recovery request data</param>
        /// <returns>Password recovery initiation result</returns>
        /// <response code="200">Password recovery process initiated</response>
        [AllowAnonymous]
        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDTO dto)
        {
            var result = await _iAuthService.CheckEmailExistence(dto);
            return Ok(result);
        }
    }
}