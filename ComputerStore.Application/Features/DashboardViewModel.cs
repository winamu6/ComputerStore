using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Features
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
