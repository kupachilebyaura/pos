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

        // DTO interno para resultados consolidados (fuertemente tipado)
        private class ConsolidatedResult
        {
            public string Group { get; set; } = string.Empty;
            public decimal EfectivoInicial { get; set; }
            public decimal VentasEfectivo { get; set; }
            public decimal Ingresos { get; set; }
            public decimal Retiros { get; set; }
            public decimal MontoFinal { get; set; }
            public decimal Diferencia { get; set; }
            public int CantidadArqueos { get; set; }
        }

        public CashController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consolidated cash sessions summary, grouped by day and user
        [AllowAnonymous]
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

            var result = new List<ConsolidatedResult>();
            if (groupBy == "user")
            {
                var consolidated = sessions.GroupBy(s => s.User?.Username ?? $"Usuario {s.UserId}");
                foreach (var group in consolidated)
                {
                    var groupLabel = group.Key; 

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

                    result.Add(new ConsolidatedResult {
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
            }
            else // groupBy == "day"
            {
                var consolidated = sessions.GroupBy(s => s.OpenedAt.Date);
                foreach (var group in consolidated)
                {
                    var groupLabel = group.Key.ToString("yyyy-MM-dd");
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

                    result.Add(new ConsolidatedResult {
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
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        [AllowAnonymous]
        [HttpGet("consolidated-excel")]
        public async Task<IActionResult> GetConsolidatedExcel(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day")
        {
            var r = await GetConsolidated(from, to, groupBy) as OkObjectResult;
            var data = r.Value as List<ConsolidatedResult> ?? new List<ConsolidatedResult>();

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
            foreach (var item in data)
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

        [AllowAnonymous]
        [HttpGet("consolidated-pdf")]
        public async Task<IActionResult> GetConsolidatedPdf(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day")
        {
            var r = await GetConsolidated(from, to, groupBy) as OkObjectResult;
            var data = r.Value as List<ConsolidatedResult> ?? new List<ConsolidatedResult>();

            var bytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Row(row => {
                        row.RelativeItem().AlignCenter().Text($"Consolidado de Arqueos de Caja ({from:yyyy-MM-dd} a {to:yyyy-MM-dd})")
                            .FontSize(16).Bold();
                    });
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
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span(groupBy == "user" ? "Usuario" : "Fecha").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Cantidad").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Inicial").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Ventas Ef.").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Ingresos").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Retiros").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Final").Bold());
                            });
                            header.Cell().Element(cell =>
                            {
                                cell.AlignCenter();
                                cell.Text(text => text.Span("Diferencia").Bold());
                            });
                        });

                        foreach (var item in data)
                        {
                            var group = item.Group;
                            var cantidad = item.CantidadArqueos.ToString();
                            var inicial = $"${item.EfectivoInicial:N2}";
                            var ventas = $"${item.VentasEfectivo:N2}";
                            var ingresos = $"${item.Ingresos:N2}";
                            var retiros = $"${item.Retiros:N2}";
                            var final = $"${item.MontoFinal:N2}";
                            var diferencia = $"${item.Diferencia:N2}";

                            table.Cell().Element(cell => { cell.AlignLeft(); cell.Text(group); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(cantidad); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(inicial); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(ventas); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(ingresos); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(retiros); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(final); });
                            table.Cell().Element(cell => { cell.AlignRight(); cell.Text(diferencia); });
                        }
                    });
                });
            }).GeneratePdf();

            var fileName = $"consolidado_arqueos_{from:yyyyMMdd}_a_{to:yyyyMMdd}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}