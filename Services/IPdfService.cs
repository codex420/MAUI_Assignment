using MAUI_Assignment.Models;

namespace MAUI_Assignment.Services;

/// <summary>
/// Exports the orders table to a PDF file saved directly to the device's
/// Downloads folder (Android) or the user's Downloads folder (Windows).
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF of the given orders and saves it to Downloads.
    /// Returns the saved file path, or null if the export failed.
    /// </summary>
    Task<string?> ExportOrdersAsync(IReadOnlyList<OrderItem> orders);
}
