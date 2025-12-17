namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common
{
    public class CommonResponseModel
    {
        public string? StatusCode { get; set; }
        public string? StatusMessage { get; set; }
    }
    public class ResponseToken
    {
        public string? StatusCode { get; set; }
        public string? StatusMessage { get; set; }
        public string? Token { get; set; }
    }

    public class GetSpecificRecord<T> : CommonResponseModel
    {
        public T? Data { get; set; }
    }
    public class GetAllRecord<T> : CommonResponseModel
    {
        public List<T> GetAllData { get; set; } = new List<T>();
    }
    public class CountResponseModel
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public int Count { get; set; }
    }

    public class UserRoleCountResponseModel
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public int AdminCount { get; set; }
        public int SupervisorCount { get; set; }
        public int TechnicianCount { get; set; }
        public int OperatorCount { get; set; }
    }

    public class EmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MessageId { get; set; }
    }
    public class GetAllPagedRecord<T> : CommonResponseModel
    {
        public List<T> GetAllDataPaged { get; set; } = new List<T>();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

}
