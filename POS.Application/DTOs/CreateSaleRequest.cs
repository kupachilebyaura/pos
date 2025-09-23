using System.Collections.Generic;

namespace POS.Application.DTOs
{
    public class CreateSaleRequest
    {
        public int? CustomerId { get; set; } // Opcional
        public string PaymentMethod { get; set; } = null!; // Obligatorio en frontend
        public List<SaleProductRequest> Items { get; set; } = new();
    }
}