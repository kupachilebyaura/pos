using System;
using System.Collections.Generic;

namespace POS.Application.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<SaleDetailDto> Details { get; set; } = new();
    }

    public class SaleDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}