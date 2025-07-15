using Bikya.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bikya.Data.Models
{
    public class ExchangeRequest
    {
        public int Id { get; set; }

        public int OfferedProductId { get; set; }
        public Product OfferedProduct { get; set; }

        public int RequestedProductId { get; set; }
        public Product RequestedProduct { get; set; }

        public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Message { get; set; }
    }

}