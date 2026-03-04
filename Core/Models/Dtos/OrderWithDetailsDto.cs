using System;
using System.Collections.Generic;
using DapperUoW_Net48_Api.Core.Models.Entities;

namespace DapperUoW_Net48_Api.Core.Models.Dtos
{
    public class OrderWithDetailsDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }
}
