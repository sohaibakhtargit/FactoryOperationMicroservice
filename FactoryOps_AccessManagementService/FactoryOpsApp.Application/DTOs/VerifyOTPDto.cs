namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs
{
    public class VerifyOTPDto
    {
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public string Otp { get; set; }

    }
    public class VerifyOtpByEmailDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
