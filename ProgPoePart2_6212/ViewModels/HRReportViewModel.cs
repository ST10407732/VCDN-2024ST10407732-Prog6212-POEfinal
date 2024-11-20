using ProgPoePart2_6212.Models;

namespace ProgPoePart2_6212.ViewModels
{

    public class HRReportViewModel
    {
        public List<LecturerClaim> Claims { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalAmountClaimed { get; set; }
        public decimal AverageClaim { get; set; }
        public decimal HighestClaim { get; set; }
        public decimal LowestClaim { get; set; }
    }
}