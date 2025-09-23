using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Globalization;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CashRegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CashRegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ... (otros endpoints de apertura, cierre y movimientos)

        [HttpGet("arqueo-excel")]
        public async Task<IActionResult> GetArqueoExcel([FromQuery] int sessionId)
        {
            var session = await _context.CashRegisterSessions
                .Include(s => s.Movements)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            // Total de ventas en efectivo de la sesión
            var efectivoVentas = await _context.Sales
                .Where(s => s.Date >= session.OpenedAt && s.Date <= (session.ClosedAt ?? DateTime.UtcNow)
                            && s.PaymentMethod == "Efectivo")
                .SumAsync(s => (decimal?)s.Total) ?? 0;

            var ingresos = session.Movements.Where(m => m.Type == "Ingreso").Sum(m => m.Amount);
            var retiros = session.Movements.Where(m => m.Type == "Retiro").Sum(m => m.Amount);
            var esperado = session.InitialAmount + ingresos - retiros + efectivoVentas;
            var diferencia = (session.ClosingAmount ?? 0) - esperado;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Arqueo de Caja");
            ws.Cell(1, 1).Value = "Apertura";
            ws.Cell(1, 2).Value = session.OpenedAt.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(2, 1).Value = "Monto Inicial";
            ws.Cell(2, 2).Value = session.InitialAmount;
            ws.Cell(3, 1).Value = "Ventas en Efectivo";
            ws.Cell(3, 2).Value = efectivoVentas;
            ws.Cell(4, 1).Value = "Ingresos";
            ws.Cell(4, 2).Value = ingresos;
            ws.Cell(5, 1).Value = "Retiros";
            ws.Cell(5, 2).Value = retiros;
            ws.Cell(6, 1).Value = "Esperado";
            ws.Cell(6, 2).Value = esperado;
            ws.Cell(7, 1).Value = "Cierre";
            ws.Cell(7, 2).Value = session.ClosedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            ws.Cell(8, 1).Value = "Monto Final";
            ws.Cell(8, 2).Value = session.ClosingAmount ?? 0;
            ws.Cell(9, 1).Value = "Diferencia";
            ws.Cell(9, 2).Value = diferencia;

            ws.Cell(11, 1).Value = "Movimientos";
            ws.Cell(12, 1).Value = "Fecha/Hora";
            ws.Cell(12, 2).Value = "Tipo";
            ws.Cell(12, 3).Value = "Monto";
            ws.Cell(12, 4).Value = "Descripción";

            int row = 13;
            foreach (var m in session.Movements.OrderBy(m => m.Timestamp))
            {
                ws.Cell(row, 1).Value = m.Timestamp.ToString("yyyy-MM-dd HH:mm");
                ws.Cell(row, 2).Value = m.Type;
                ws.Cell(row, 3).Value = m.Amount;
                ws.Cell(row, 4).Value = m.Description ?? "";
                row++;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var fileName = $"arqueo_caja_{session.OpenedAt:yyyyMMdd_HHmm}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet("arqueo-pdf")]
        public async Task<IActionResult> GetArqueoPdf([FromQuery] int sessionId)
        {
            var session = await _context.CashRegisterSessions
                .Include(s => s.Movements)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            var efectivoVentas = await _context.Sales
                .Where(s => s.Date >= session.OpenedAt && s.Date <= (session.ClosedAt ?? DateTime.UtcNow)
                            && s.PaymentMethod == "Efectivo")
                .SumAsync(s => (decimal?)s.Total) ?? 0;

            var ingresos = session.Movements.Where(m => m.Type == "Ingreso").Sum(m => m.Amount);
            var retiros = session.Movements.Where(m => m.Type == "Retiro").Sum(m => m.Amount);
            var esperado = session.InitialAmount + ingresos - retiros + efectivoVentas;
            var diferencia = (session.ClosingAmount ?? 0) - esperado;

            var bytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"Arqueo de Caja — {session.OpenedAt:yyyy-MM-dd HH:mm}")
                        .FontSize(18).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Apertura: {session.OpenedAt:yyyy-MM-dd HH:mm}");
                        col.Item().Text($"Monto Inicial: ${session.InitialAmount:N2}");
                        col.Item().Text($"Ventas en Efectivo: ${efectivoVentas:N2}");
                        col.Item().Text($"Ingresos: ${ingresos:N2}");
                        col.Item().Text($"Retiros: ${retiros:N2}");
                        col.Item().Text($"Esperado: ${esperado:N2}");
                        col.Item().Text($"Cierre: {(session.ClosedAt.HasValue ? session.ClosedAt.Value.ToString("yyyy-MM-dd HH:mm") : "-")}");
                        col.Item().Text($"Monto Final: ${(session.ClosingAmount ?? 0):N2}");
                        col.Item().Text($"Diferencia: ${diferencia:N2}");
                        col.Item().PaddingVertical(10).Text("Movimientos:").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(110);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Fecha/Hora").Bold();
                                header.Cell().Text("Tipo").Bold();
                                header.Cell().Text("Monto").Bold();
                                header.Cell().Text("Descripción").Bold();
                            });

                            foreach (var m in session.Movements.OrderBy(m => m.Timestamp))
                            {
                                table.Cell().Text(m.Timestamp.ToString("yyyy-MM-dd HH:mm"));
                                table.Cell().Text(m.Type);
                                table.Cell().Text($"${m.Amount:N2}");
                                table.Cell().Text(m.Description ?? "");
                            }
                        });
                    });
                });
            }).GeneratePdf();

            var fileName = $"arqueo_caja_{session.OpenedAt:yyyyMMdd_HHmm}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}