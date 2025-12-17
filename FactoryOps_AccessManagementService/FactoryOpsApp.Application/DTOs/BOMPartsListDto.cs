using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{

    public class BOMPartsListResponseDto
    {
        public BOMPartsCard SummaryCards { get; set; }
        public List<BOMBPartsListDto> PartsList { get; set; }
    }

    public class BOMBPartsListDto
    {
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public string Supplier { get; set; }
        public string Asset { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }

    public class BOMPartsCard
    {
        public int TotalParts { get; set; }
        public decimal GrandTotalValue { get; set; }
        public int LowStockParts { get; set; }
        public int OutStockParts { get; set; }
    }


}
