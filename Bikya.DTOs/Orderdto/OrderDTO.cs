using Bikya.DTOs.ShippingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.DTOs.Orderdto
{
    public class OrderDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductTitle { get; set; }

        public int BuyerId { get; set; }
        public string BuyerName { get; set; }

        public int SellerId { get; set; }
        public string SellerName { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal SellerAmount { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public ShippingInfoDTO ShippingInfo { get; set; } 
    }

}
