﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.DTOs.UserDTOs
{
    public class UserStatsDTO
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalSales { get; set; }
    }
}
