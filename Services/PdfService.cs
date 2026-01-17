using Mess_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mess_Management_System.Services
{
    public class PdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Generate User Bill Invoice as HTML (can be printed as PDF)
        public string GenerateBillInvoiceHtml(int billId, int userId)
        {
            try
            {
                // Get bill details
                var bill = _context.Bills
                    .Include(b => b.User)
                    .FirstOrDefault(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                    throw new Exception("Bill not found");

                // Get attendance breakdown
                var billMonth = bill.Date.Month;
                var billYear = bill.Date.Year;
                var attendances = _context.Attendances
                    .Include(a => a.Menu)
                    .Where(a => a.UserId == userId &&
                               a.Attended &&
                               a.Menu.Date.Month == billMonth &&
                               a.Menu.Date.Year == billYear)
                    .OrderBy(a => a.Menu.Date)
                    .ToList();

                var html = new StringBuilder();

                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine("<meta charset='UTF-8'>");
                html.AppendLine($"<title>Invoice_{bill.Id}_{bill.Date:yyyyMM}</title>");
                html.AppendLine("<style>");
                html.AppendLine(@"
                    @media print {
                        .no-print { display: none; }
                        body { margin: 0; }
                    }
                    body { font-family: Arial, sans-serif; padding: 40px; }
                    .header { text-align: center; color: #7B2CBF; margin-bottom: 30px; }
                    .header h1 { margin: 0; font-size: 28px; }
                    .header h2 { margin: 5px 0; font-size: 20px; color: #333; }
                    .invoice-details { display: flex; justify-content: space-between; margin: 30px 0; }
                    .bill-to { flex: 1; }
                    .invoice-info { flex: 1; text-align: right; }
                    table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                    th { background-color: #7B2CBF; color: white; padding: 12px; text-align: left; }
                    td { padding: 10px; border-bottom: 1px solid #ddd; }
                    .total-row { background-color: #f0f0f0; font-weight: bold; }
                    .footer { text-align: center; margin-top: 40px; color: #666; font-size: 12px; }
                    .status-paid { color: green; font-weight: bold; }
                    .status-unpaid { color: red; font-weight: bold; }
                    .action-buttons { text-align: center; margin: 20px 0; display: flex; justify-content: center; gap: 12px; flex-wrap: wrap; }
                    .btn { display: inline-flex; align-items: center; justify-content: center; padding: 10px 20px; margin: 0; border-radius: 10px; text-decoration: none; font-weight: 500; cursor: pointer; border: none; font-size: 14px; transition: all 0.2s ease; }
                    .btn-download { background-color: #f5f5f7; color: #1d1d1f; border: 1px solid #d2d2d7; }
                    .btn-download:hover { background-color: #e8e8eb; }
                    .btn-print { background-color: #f5f5f7; color: #1d1d1f; border: 1px solid #d2d2d7; }
                    .btn-print:hover { background-color: #e8e8eb; }
                    .btn-close { background-color: #f5f5f7; color: #1d1d1f; border: 1px solid #d2d2d7; }
                    .btn-close:hover { background-color: #e8e8eb; }
                ");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                // Action Buttons (only visible on screen, not in print)
                html.AppendLine("<div class='action-buttons no-print'>");
                html.AppendLine("<button class='btn btn-download' onclick='downloadPDF()'><span style='margin-right: 6px;'>↓</span> Download PDF</button>");
                html.AppendLine("<button class='btn btn-print' onclick='window.print()'><span style='margin-right: 6px;'>⎗</span> Print</button>");
                html.AppendLine("<button class='btn btn-close' onclick='closeWindow()'><span style='margin-right: 6px;'>×</span> Close</button>");
                html.AppendLine("</div>");

                // Header
                html.AppendLine("<div class='header'>");
                html.AppendLine("<h1>MESS MANAGEMENT SYSTEM</h1>");
                html.AppendLine("<h2>Invoice</h2>");
                html.AppendLine("</div>");

                // Invoice Details
                html.AppendLine("<div class='invoice-details'>");
                html.AppendLine("<div class='bill-to'>");
                html.AppendLine("<strong>BILL TO:</strong><br>");
                html.AppendLine($"{bill.User.FullName}<br>");
                html.AppendLine($"{bill.User.Email}");
                html.AppendLine("</div>");
                html.AppendLine("<div class='invoice-info'>");
                html.AppendLine($"<strong>Invoice #:</strong> {bill.Id}<br>");
                html.AppendLine($"<strong>Date:</strong> {bill.Date:MMM dd, yyyy}<br>");
                html.AppendLine($"<strong>Period:</strong> {bill.Date:MMMM yyyy}<br>");
                html.AppendLine($"<strong>Status:</strong> <span class='{(bill.Paid ? "status-paid" : "status-unpaid")}'>{(bill.Paid ? "PAID" : "UNPAID")}</span>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");

                // Attendance Table
                html.AppendLine("<table>");
                html.AppendLine("<thead>");
                html.AppendLine("<tr>");
                html.AppendLine("<th>Date</th>");
                html.AppendLine("<th>Item</th>");
                html.AppendLine("<th>Type</th>");
                html.AppendLine("<th style='text-align: right;'>Price</th>");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");

                foreach (var attendance in attendances)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{attendance.Menu.Date:MMM dd}</td>");
                    html.AppendLine($"<td>{attendance.Menu.Name}</td>");
                    html.AppendLine($"<td>{(attendance.Menu.IsFood ? "Food" : "Drink")}</td>");
                    html.AppendLine($"<td style='text-align: right;'>Rs {attendance.Menu.Price:N2}</td>");
                    html.AppendLine("</tr>");
                }

                // Total Row
                html.AppendLine("<tr class='total-row'>");
                html.AppendLine("<td colspan='3' style='text-align: right;'>TOTAL AMOUNT</td>");
                html.AppendLine($"<td style='text-align: right; font-size: 16px;'>Rs {bill.Amount:N2}</td>");
                html.AppendLine("</tr>");

                html.AppendLine("</tbody>");
                html.AppendLine("</table>");

                // Footer
                html.AppendLine("<div class='footer'>");
                html.AppendLine("<p>Thank you for using our Mess Management System!</p>");
                html.AppendLine($"<p>Generated on: {DateTime.Now:MMM dd, yyyy HH:mm}</p>");
                html.AppendLine("</div>");

                // JavaScript for PDF download
                html.AppendLine("<script>");
                html.AppendLine(@"
                    function downloadPDF() {
                        // Hide buttons before printing
                        document.querySelectorAll('.no-print').forEach(el => el.style.display = 'none');
                        
                        // Trigger print dialog with PDF option
                        window.print();
                        
                        // Show buttons again after print
                        setTimeout(() => {
                            document.querySelectorAll('.no-print').forEach(el => el.style.display = 'block');
                        }, 1000);
                    }

                    function closeWindow() {
                        // Try to close the window (works for windows opened via window.open)
                        try { window.close(); } catch (e) { /* ignore */ }

                        // If window didn't close (most browsers block close on tabs not opened by script),
                        // navigate back or redirect to the app home as a fallback.
                        setTimeout(function() {
                            // If the window is still open, try history back first
                            if (!window.closed) {
                                if (window.history.length > 1) {
                                    window.history.back();
                                } else {
                                    window.location.href = '/UserBill/Index';
                                }
                            }
                        }, 150);
                    }
                ");
                html.AppendLine("</script>");

                html.AppendLine("</body>");
                html.AppendLine("</html>");

                return html.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating invoice: {ex.Message}", ex);
            }
        }

        // ✅ Generate Monthly Report as HTML
        public string GenerateMonthlyReportHtml(int month, int year)
        {
            try
            {
                var bills = _context.Bills
                    .Include(b => b.User)
                    .Where(b => b.Date.Month == month && b.Date.Year == year)
                    .OrderBy(b => b.User.FullName)
                    .ToList();

                var totalAmount = bills.Sum(b => b.Amount);
                var paidAmount = bills.Where(b => b.Paid).Sum(b => b.Amount);
                var unpaidAmount = bills.Where(b => !b.Paid).Sum(b => b.Amount);

                var html = new StringBuilder();

                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine("<style>");
                html.AppendLine(@"
                    body { font-family: Arial, sans-serif; padding: 40px; }
                    .header { text-align: center; color: #7B2CBF; margin-bottom: 30px; }
                    .summary { display: flex; justify-content: space-around; margin: 20px 0; }
                    .summary-box { text-align: center; padding: 20px; background: #f5f5f5; border-radius: 8px; }
                    table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                    th { background-color: #7B2CBF; color: white; padding: 12px; text-align: left; }
                    td { padding: 10px; border-bottom: 1px solid #ddd; }
                    .footer { text-align: center; margin-top: 40px; color: #666; font-size: 12px; }
                ");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                // Header
                html.AppendLine("<div class='header'>");
                html.AppendLine("<h1>MESS MANAGEMENT SYSTEM</h1>");
                html.AppendLine($"<h2>Monthly Report - {new DateTime(year, month, 1):MMMM yyyy}</h2>");
                html.AppendLine("</div>");

                // Summary
                html.AppendLine("<div class='summary'>");
                html.AppendLine("<div class='summary-box'>");
                html.AppendLine($"<strong>Total Bills</strong><br><h2>{bills.Count}</h2>");
                html.AppendLine("</div>");
                html.AppendLine("<div class='summary-box'>");
                html.AppendLine($"<strong>Total Amount</strong><br><h2>Rs {totalAmount:N2}</h2>");
                html.AppendLine("</div>");
                html.AppendLine("<div class='summary-box'>");
                html.AppendLine($"<strong>Paid</strong><br><h2 style='color:green;'>Rs {paidAmount:N2}</h2>");
                html.AppendLine("</div>");
                html.AppendLine("<div class='summary-box'>");
                html.AppendLine($"<strong>Unpaid</strong><br><h2 style='color:red;'>Rs {unpaidAmount:N2}</h2>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");

                // Bills Table
                html.AppendLine("<table>");
                html.AppendLine("<thead>");
                html.AppendLine("<tr>");
                html.AppendLine("<th>User</th>");
                html.AppendLine("<th>Date</th>");
                html.AppendLine("<th style='text-align: right;'>Amount</th>");
                html.AppendLine("<th>Status</th>");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");

                foreach (var bill in bills)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{bill.User.FullName}</td>");
                    html.AppendLine($"<td>{bill.Date:MMM dd, yyyy}</td>");
                    html.AppendLine($"<td style='text-align: right;'>Rs {bill.Amount:N2}</td>");
                    html.AppendLine($"<td>{(bill.Paid ? "PAID" : "UNPAID")}</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</tbody>");
                html.AppendLine("</table>");

                // Footer
                html.AppendLine("<div class='footer'>");
                html.AppendLine($"<p>Generated on: {DateTime.Now:MMM dd, yyyy HH:mm}</p>");
                html.AppendLine("</div>");

                html.AppendLine("</body>");
                html.AppendLine("</html>");

                return html.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating report: {ex.Message}", ex);
            }
        }
    }
}