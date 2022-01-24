using System;

namespace Entities
{
    public class ResponseEntity
    {
        public decimal Cash { get; set; }
        public decimal NonCash { get; set; }
        public decimal GeneralLimit { get; set; }
        public DateTime FindeksDate { get; set; }
        public decimal TotalRisk { get; set; }
        public string RelationBanks { get; set; }
        public string LimitCurrency { get; set; }
        public double Share { get; set; }
    }
}
