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
    public class CashController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CashController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consolidated cash sessions summary, grouped by day and user
        [HttpGet("consolidated")]
        public async Task<IActionResult> GetConsolidated(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day") // "day" or "user"
        {
            var sessions = await _context.CashSessions
                .Include(s => s.Movements)
                .Include(s => s.User)
                .Where(s => s.OpenedAt >= from && s.OpenedAt <= to)
                .ToListAsync();

            // Load sales in one go for the range to avoid N+1
            var sessionDates = sessions
                .Select(s => new { s.Id, s.OpenedAt, s.ClosedAt })
                .ToList();

            var sales = await _context.Sales
                .Where(s => s.Date >= from && s.Date <= to && s.PaymentMethod == "Efectivo")
                .ToListAsync();

            var consolidated = groupBy == "user"
                ? sessions.GroupBy(s => s.User?.UserName ?? $"Usuario {s.UserId}")
                : sessions.GroupBy(s => s.OpenedAt.Date);

            var result = new List<object>();
            foreach (var group in consolidated)
            {
                var groupLabel = groupBy == "user"
                    ? group.Key
                    : ((DateTime)group.Key).ToString("yyyy-MM-dd");

                decimal efectivoInicial = group.Sum(s => s.InitialAmount);
                decimal ingresos = group.SelectMany(s => s.Movements.Where(m => m.Type == "Ingreso")).Sum(m => m.Amount);
                decimal retiros = group.SelectMany(s => s.Movements.Where(m => m.Type == "Retiro")).Sum(m => m.Amount);

                // Ventas en efectivo sólo si la venta está en el rango de apertura/cierre de la sesión
                decimal ventasEfectivo = 0;
                foreach (var s in group)
                {
                    ventasEfectivo += sales
                        .Where(v => v.Date >= s.OpenedAt && v.Date <= (s.ClosedAt ?? to))
                        .Sum(v => v.Total);
                }

                decimal montoFinal = group.Sum(s => s.ClosingAmount ?? 0);
                decimal diferencia = group.Sum(s => s.Difference ?? 0);

                result.Add(new {
                    Group = groupLabel,
                    EfectivoInicial = efectivoInicial,
                    VentasEfectivo = ventasEfectivo,
                    Ingresos = ingresos,
                    Retiros = retiros,
                    MontoFinal = montoFinal,
                    Diferencia = diferencia,
                    CantidadArqueos = group.Count()
                });
            }

            return Ok(result);
        }

        [HttpGet("consolidated-excel")]
        public async Task<IActionResult> GetConsolidatedExcel(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day")
        {
            var r = await GetConsolidated(from, to, groupBy) as OkObjectResult;
            var data = r.Value as List<object>;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Consolidado Arqueos");
            ws.Cell(1, 1).Value = groupBy == "user" ? "Usuario" : "Fecha";
            ws.Cell(1, 2).Value = "Cantidad Arqueos";
            ws.Cell(1, 3).Value = "Efectivo Inicial";
            ws.Cell(1, 4).Value = "Ventas Efectivo";
            ws.Cell(1, 5).Value = "Ingresos";
            ws.Cell(1, 6).Value = "Retiros";
            ws.Cell(1, 7).Value = "Monto Final";
            ws.Cell(1, 8).Value = "Diferencia";

            int row = 2;
            foreach (dynamic item in data)
            {
                ws.Cell(row, 1).Value = item.Group;
                ws.Cell(row, 2).Value = item.CantidadArqueos;
                ws.Cell(row, 3).Value = item.EfectivoInicial;
                ws.Cell(row, 4).Value = item.VentasEfectivo;
                ws.Cell(row, 5).Value = item.Ingresos;
                ws.Cell(row, 6).Value = item.Retiros;
                ws.Cell(row, 7).Value = item.MontoFinal;
                ws.Cell(row, 8).Value = item.Diferencia;
                row++;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var fileName = $"consolidado_arqueos_{from:yyyyMMdd}_a_{to:yyyyMMdd}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet("consolidated-pdf")]
        public async Task<IActionResult> GetConsolidatedPdf(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day")
        {
            var r = await GetConsolidated(from, to, groupBy) as OkObjectResult;
            var data = r.Value as List<object>;

            var bytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"Consolidado de Arqueos de Caja ({from:yyyy-MM-dd} a {to:yyyy-MM-dd})")
                        .FontSize(16).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Fecha/Usuario
                            columns.ConstantColumn(60); // Cantidad Arqueos
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text(groupBy == "user" ? "Usuario" : "Fecha").Bold();
                            header.Cell().Text("Cantidad").Bold();
                            header.Cell().Text("Inicial").Bold();
                            header.Cell().Text("Ventas Ef.").Bold();
                            header.Cell().Text("Ingresos").Bold();
                            header.Cell().Text("Retiros").Bold();
                            header.Cell().Text("Final").Bold();
                            header.Cell().Text("Diferencia").Bold();
                        });

                        foreach (dynamic item in data)
                        {
                            table.Cell().Text(item.Group.ToString());
                            table.Cell().Text(item.CantidadArqueos.ToString());
                            table.Cell().Text($"${item.EfectivoInicial:N2}");
                            table.Cell().Text($"${item.VentasEfectivo:N2}");
                            table.Cell().Text($"${item.Ingresos:N2}");
                            table.Cell().Text($"${item.Retiros:N2}");
                            table.Cell().Text($"${item.MontoFinal:N2}");
                            table.Cell().Text($"${item.Diferencia:N2}");
                        }
                    });
                });
            }).GeneratePdf();

            var fileName = $"consolidado_arqueos_{from:yyyyMMdd}_a_{to:yyyyMMdd}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}