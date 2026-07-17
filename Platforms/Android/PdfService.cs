using Android.Content;
using Android.Graphics.Pdf;
using MAUI_Assignment.Models;
using MAUI_Assignment.Services;
using AndroidUri = Android.Net.Uri;
using APaint = Android.Graphics.Paint;
using AColor = Android.Graphics.Color;

namespace MAUI_Assignment.Platforms.Android;

/// <summary>
/// Android PDF export. Draws the orders table onto a PdfDocument page and writes
/// it to the public Downloads collection via MediaStore (no storage permission
/// needed on API 29+).
/// </summary>
public class PdfService : IPdfService
{
    private const int PageWidth = 595;   // A4 @ 72dpi
    private const int PageHeight = 842;
    private const int Margin = 40;

    public Task<string?> ExportOrdersAsync(IReadOnlyList<OrderItem> orders)
    {
        var context = global::Android.App.Application.Context;
        string fileName = $"OrderStatus_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        var doc = new PdfDocument();
        var pageInfo = new PdfDocument.PageInfo.Builder(PageWidth, PageHeight, 1).Create();
        var page = doc.StartPage(pageInfo);
        var canvas = page.Canvas;

        var titlePaint = new APaint { Color = AColor.Black, TextSize = 20, FakeBoldText = true };
        var headerPaint = new APaint { Color = AColor.Argb(255, 40, 40, 40), TextSize = 12, FakeBoldText = true };
        var cellPaint = new APaint { Color = AColor.Argb(255, 60, 60, 60), TextSize = 12 };
        var linePaint = new APaint { Color = AColor.Argb(255, 210, 210, 210), StrokeWidth = 1 };

        float y = Margin;
        canvas.DrawText("Order Status", Margin, y, titlePaint);
        y += 22;
        canvas.DrawText("Overview of Latest Month", Margin, y, cellPaint);
        y += 28;

        // Column x positions
        float[] cols = { Margin, Margin + 90, Margin + 230, Margin + 340, Margin + 420 };
        canvas.DrawText("INVOICE", cols[0], y, headerPaint);
        canvas.DrawText("CUSTOMER", cols[1], y, headerPaint);
        canvas.DrawText("FROM", cols[2], y, headerPaint);
        canvas.DrawText("PRICE", cols[3], y, headerPaint);
        canvas.DrawText("STATUS", cols[4], y, headerPaint);
        y += 8;
        canvas.DrawLine(Margin, y, PageWidth - Margin, y, linePaint);
        y += 20;

        foreach (var o in orders)
        {
            canvas.DrawText(o.Invoice ?? "", cols[0], y, cellPaint);
            canvas.DrawText(Truncate(o.Customer, 20), cols[1], y, cellPaint);
            canvas.DrawText(Truncate(o.From, 14), cols[2], y, cellPaint);
            canvas.DrawText(o.Price ?? "", cols[3], y, cellPaint);
            canvas.DrawText(o.Status ?? "", cols[4], y, cellPaint);
            y += 6;
            canvas.DrawLine(Margin, y, PageWidth - Margin, y, linePaint);
            y += 20;

            if (y > PageHeight - Margin) break; // single-page export
        }

        canvas.DrawText($"Total: {orders.Count} orders", Margin, y + 10, cellPaint);

        doc.FinishPage(page);

        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                // API 29+: write into the public Downloads collection via MediaStore.
                var values = new ContentValues();
                values.Put(global::Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                values.Put(global::Android.Provider.MediaStore.IMediaColumns.MimeType, "application/pdf");
                values.Put(global::Android.Provider.MediaStore.IMediaColumns.RelativePath, global::Android.OS.Environment.DirectoryDownloads);

                var resolver = context.ContentResolver!;
                var collection = global::Android.Provider.MediaStore.Downloads.ExternalContentUri!;
                AndroidUri? uri = resolver.Insert(collection, values);

                if (uri != null)
                {
                    using var stream = resolver.OpenOutputStream(uri);
                    if (stream != null)
                    {
                        doc.WriteTo(stream);
                        stream.Flush();
                    }
                }
            }
            else
            {
                // Older devices: write straight to the app's external files dir.
                var dir = context.GetExternalFilesDir(global::Android.OS.Environment.DirectoryDownloads);
                string path = System.IO.Path.Combine(dir?.AbsolutePath ?? global::Android.OS.Environment.ExternalStorageDirectory!.AbsolutePath, fileName);
                using var fs = System.IO.File.Create(path);
                doc.WriteTo(fs);
                fs.Flush();
            }

            doc.Close();
            return Task.FromResult<string?>($"Downloads/{fileName}");
        }
        catch
        {
            doc.Close();
            return Task.FromResult<string?>(null);
        }
    }

    private static string Truncate(string? s, int max)
    {
        s ??= "";
        return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
    }
}
