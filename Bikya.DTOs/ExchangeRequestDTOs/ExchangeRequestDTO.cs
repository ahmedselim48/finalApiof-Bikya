using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.DTOs.ExchangeRequestDTOs
{
    public class ExchangeRequestDTO
    {
        public int Id { get; set; }

        public int OfferedProductId { get; set; }
        public string OfferedProductTitle { get; set; }

        public int RequestedProductId { get; set; }
        public string RequestedProductTitle { get; set; }

        public string? Message { get; set; }

        public string Status { get; set; } 

        public DateTime RequestedAt { get; set; }
    }
}
