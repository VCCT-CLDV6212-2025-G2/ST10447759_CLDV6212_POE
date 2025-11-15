using System;
using System.Collections.Generic;

namespace AzureRetailHub.Functions.Data
{
    public class OrderDto
    {
        public string Id { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
        public string UserId { get; set; } = "";
        public UserDto? User { get; set; } // Nested DTO
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>(); // Nested DTO list
    }
}