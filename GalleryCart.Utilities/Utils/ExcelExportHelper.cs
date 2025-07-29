using GalleryCart.Models.ViewModels;
using OfficeOpenXml;

namespace GalleryCart.Utilities.Utils;

public class ExcelExportHelper
{
    public static byte[] ExportSellingHistoryToExcel(List<SellingHistoryDto> data)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("SellingHistory");

        // Header
        worksheet.Cells[1, 1].Value = "Title";
        worksheet.Cells[1, 2].Value = "Price";
        worksheet.Cells[1, 3].Value = "Quantity";
        worksheet.Cells[1, 4].Value = "Amount";
        worksheet.Cells[1, 5].Value = "Purchase Date";

        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cells[row, 1].Value = item.Title;
            worksheet.Cells[row, 2].Value = item.Price;
            worksheet.Cells[row, 3].Value = item.Quantity;
            worksheet.Cells[row, 4].Value = item.Amount;
            worksheet.Cells[row, 5].Value = item.PurchaseDate.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }
}