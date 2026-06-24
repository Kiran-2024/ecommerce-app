using ECommerceAPI.DTO_s;
using ECommerceAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ECommerceAPI.Services
{
    public class InvoiceService
    {
        public byte[] GenerateInvoice(OrderResponseDto order)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // ── HEADER ──
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("INVOICE").FontSize(28).Bold().FontColor("#1D9E75");
                            col.Item().Text($"Order #{order.OrderId}").FontSize(13).FontColor("#555555");
                        });

                        row.ConstantItem(160).AlignRight().Column(col =>
                        {
                            col.Item().Text("ECommerce App").Bold().FontSize(14);
                            col.Item().Text("support@ecommerceapp.com").FontSize(10).FontColor("#777777");
                            col.Item().Text($"Date: {order.CreatedAt:dd MMM yyyy}").FontSize(10);
                        });
                    });

                    // ── CONTENT ──
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // Billing Address
                        col.Item().Background("#F5F5F5").Padding(12).Column(addr =>
                        {
                            addr.Item().Text("Bill To:").Bold().FontSize(12);
                            addr.Item().Text(order.DeliveryAddress ?? "N/A").FontColor("#333333");
                        });

                        col.Item().Height(16);

                        // Items Table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3); // Product
                                cols.RelativeColumn(1); // Qty
                                cols.RelativeColumn(1); // Price
                                cols.RelativeColumn(1); // Total
                            });

                            // Table Header
                            static IContainer HeaderCell(IContainer c) =>
                                c.Background("#1D9E75").Padding(8).AlignCenter();

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Product").Bold().FontColor("#FFFFFF");
                                header.Cell().Element(HeaderCell).Text("Qty").Bold().FontColor("#FFFFFF");
                                header.Cell().Element(HeaderCell).Text("Unit Price").Bold().FontColor("#FFFFFF");
                                header.Cell().Element(HeaderCell).Text("Total").Bold().FontColor("#FFFFFF");
                            });

                            // Table Rows
                            bool alternate = false;
                            foreach (var item in order.OrderItems)
                            {
                                string bg = alternate ? "#F9F9F9" : "#FFFFFF";
                                alternate = !alternate;

                                static IContainer DataCell(IContainer c, string bg) =>
                                    c.Background(bg).BorderBottom(1).BorderColor("#EEEEEE").Padding(8);

                                table.Cell().Element(c => DataCell(c, bg)).Text(item.ProductName);
                                table.Cell().Element(c => DataCell(c, bg)).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(c => DataCell(c, bg)).AlignRight().Text($"₹{item.UnitPrice:F2}");
                                table.Cell().Element(c => DataCell(c, bg)).AlignRight().Text($"₹{item.Quantity * item.UnitPrice:F2}");
                            }
                        });

                        col.Item().Height(16);

                        // Total
                        col.Item().AlignRight().Column(total =>
                        {
                            total.Item().BorderTop(2).BorderColor("#1D9E75").PaddingTop(8)
                                .Text($"Grand Total: ₹{order.TotalAmount:F2}")
                                .Bold().FontSize(14).FontColor("#1D9E75");
                        });

                        col.Item().Height(20);

                        // Payment & Status
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Payment Method: {order.PaymentMethod ?? "COD"}").FontSize(10).FontColor("#555555");
                                c.Item().Text($"Payment Status: {order.PaymentStatus ?? "Unpaid"}").FontSize(10).FontColor("#555555");
                                c.Item().Text($"Order Status: {order.OrderStatus}").FontSize(10).FontColor("#555555");
                            });
                        });
                    });

                    // ── FOOTER ──
                    page.Footer().AlignCenter()
                        .Text("Thank you for shopping with us! For support: support@ecommerceapp.com")
                        .FontSize(9).FontColor("#999999");
                });
            }).GeneratePdf();
        }
    }
}