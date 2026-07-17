using System.Text;
using MAUI_Assignment.Models;
using MAUI_Assignment.Services;

namespace MAUI_Assignment.Platforms.Windows;

/// <summary>
/// Windows PDF export. Builds a minimal, valid single-page PDF (no external
/// dependency) and writes it directly to the user's Downloads folder.
/// </summary>
public class PdfService : IPdfService
{
    public Task<string?> ExportOrdersAsync(IReadOnlyList<OrderItem> orders)
    {
        string downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloads);
        string fileName = $"OrderStatus_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string fullPath = Path.Combine(downloads, fileName);

        // Build the page's text content stream.
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 18 Tf");
        content.AppendLine("40 800 Td");
        content.AppendLine("(Order Status) Tj");
        content.AppendLine("/F1 11 Tf");
        content.AppendLine("0 -22 Td");
        content.AppendLine("(Overview of Latest Month) Tj");
        content.AppendLine("0 -28 Td");
        content.AppendLine("(INVOICE    CUSTOMER            FROM        PRICE     STATUS) Tj");

        foreach (var o in orders)
        {
            string line = $"{Pad(o.Invoice, 10)} {Pad(o.Customer, 20)} {Pad(o.From, 12)} {Pad(o.Price, 8)} {o.Status}";
            content.AppendLine("0 -20 Td");
            content.AppendLine($"({Escape(line)}) Tj");
        }

        content.AppendLine("0 -28 Td");
        content.AppendLine($"(Total: {orders.Count} orders) Tj");
        content.AppendLine("ET");

        string stream = content.ToString();
        byte[] streamBytes = Encoding.ASCII.GetBytes(stream);

        // Assemble the PDF objects.
        var pdf = new StringBuilder();
        var offsets = new List<int>();
        void AddObj(string body)
        {
            offsets.Add(pdf.Length);
            pdf.Append(body);
        }

        pdf.Append("%PDF-1.4\n");
        AddObj("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        AddObj("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        AddObj("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] " +
               "/Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>\nendobj\n");
        AddObj($"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n{stream}endstream\nendobj\n");
        AddObj("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>\nendobj\n");

        int xrefPos = pdf.Length;
        pdf.Append("xref\n");
        pdf.Append($"0 {offsets.Count + 1}\n");
        pdf.Append("0000000000 65535 f \n");
        foreach (var off in offsets)
            pdf.Append(off.ToString("D10") + " 00000 n \n");
        pdf.Append("trailer\n");
        pdf.Append($"<< /Size {offsets.Count + 1} /Root 1 0 R >>\n");
        pdf.Append("startxref\n");
        pdf.Append($"{xrefPos}\n");
        pdf.Append("%%EOF");

        try
        {
            File.WriteAllText(fullPath, pdf.ToString(), Encoding.ASCII);
            return Task.FromResult<string?>(fullPath);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    private static string Pad(string? s, int width)
    {
        s ??= "";
        if (s.Length > width) s = s.Substring(0, width);
        return s.PadRight(width);
    }

    private static string Escape(string s) =>
        s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
