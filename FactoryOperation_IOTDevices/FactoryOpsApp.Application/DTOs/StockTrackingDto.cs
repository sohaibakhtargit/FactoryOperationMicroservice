namespace FactoryOpsApp.Application.DTOs
{
    public class StockTrackingDto
    {
        public int ItemId { get; set; }
        public int TenantId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Available { get; set; }
        public int Reserved { get; set; }
        public int MinThreshold { get; set; }
        public int MaxThreshold { get; set; } = 100;
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
    public class StockTrackingSummaryDto
    {
        public int TotalItems { get; set; }
        public int LowStockAlerts { get; set; }
        public int OutOfStock { get; set; }
        public List<StockTrackingDto> StockLevels { get; set; } = new List<StockTrackingDto>();
    }

    public class StockReservationDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class StockReservationSummaryDto
    {
        public int TotalReservedItems { get; set; }
        public int TotalReservedQuantity { get; set; }
        public decimal TotalReservedValue { get; set; }
        public List<StockReservationDto> ReservationItems { get; set; } = new List<StockReservationDto>();
    }

    public class SerialBatchTrackingDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Available { get; set; }
        public string BatchInfo { get; set; } = string.Empty;
        public string SerialInfo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class SerialBatchSummaryDto
    {
        public int TotalTrackedItems { get; set; }
        public int ItemsWithBatchInfo { get; set; }
        public int ItemsWithSerialInfo { get; set; }
        public List<SerialBatchTrackingDto> TrackedItems { get; set; } = new List<SerialBatchTrackingDto>();
    }

    public class StockTrackingResponseDto
    {
        public StockTrackingType Type { get; set; }
        public StockTrackingSummaryDto Overview { get; set; }
        public StockReservationSummaryDto Reservations { get; set; }
        public SerialBatchSummaryDto SerialBatch { get; set; }
    }
    public enum StockTrackingType
    {
        Overview,
        Reservations,
        SerialBatch
    }


}