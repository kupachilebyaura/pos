using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ventas agrupadas por producto para Excel/PDF y frontend
        [HttpGet("sales-by-product")]
        public async Task<IActionResult> GetSalesByProduct([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var productSales = await _context.SaleDetails
                .Include(sd => sd.Product)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Date >= from && sd.Sale.Date <= to)
                .GroupBy(sd => new { sd.ProductId, sd.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalIncome = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            return Ok(productSales);
        }

        [HttpGet("sales-by-product-excel")]
        public async Task<IActionResult> GetSalesByProductExcel([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var productSales = await _context.SaleDetails
                .Include(sd => sd.Product)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Date >= from && sd.Sale.Date <= to)
                .GroupBy(sd => new { sd.ProductId, sd.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalIncome = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ventas por producto");
            ws.Cell(1, 1).Value = "Producto";
            ws.Cell(1, 2).Value = "Cantidad Vendida";
            ws.Cell(1, 3).Value = "Ingresos";

            int row = 2;
            foreach (var sale in productSales)
            {
                ws.Cell(row, 1).Value = sale.ProductName;
                ws.Cell(row, 2).Value = sale.TotalSold;
                ws.Cell(row, 3).Value = sale.TotalIncome;
                row++;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var fileName = $"ventas_por_producto_{from:yyyyMMdd}_a_{to:yyyyMMdd}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet("sales-by-product-pdf")]
        public async Task<IActionResult> GetSalesByProductPdf([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var productSales = await _context.SaleDetails
                .Include(sd => sd.Product)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Date >= from && sd.Sale.Date <= to)
                .GroupBy(sd => new { sd.ProductId, sd.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalIncome = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            var bytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"Ventas por Producto: {from:yyyy-MM-dd} a {to:yyyy-MM-dd}")
                        .FontSize(18).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(100);
                            columns.ConstantColumn(120);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Producto").Bold();
                            header.Cell().Text("Cantidad Vendida").Bold();
                            header.Cell().Text("Ingresos").Bold();
                        });

                        foreach (var sale in productSales)
                        {
                            table.Cell().Text(sale.ProductName);
                            table.Cell().Text(sale.TotalSold.ToString());
                            table.Cell().Text($"${sale.TotalIncome.ToString("N2", CultureInfo.InvariantCulture)}");
                        }
                    });
                });
            }).GeneratePdf();

            var fileName = $"ventas_por_producto_{from:yyyyMMdd}_a_{to:yyyyMMdd}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}