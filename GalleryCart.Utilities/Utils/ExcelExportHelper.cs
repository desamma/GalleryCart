using GalleryCart.Models.ViewModels;
using OfficeOpenXml;

namespace GalleryCart.Utilities.Utils;

public class ExcelExportHelper
{
    public static byte[] ExportSellingHistoryToExcel(List<SellingHistoryDto> data)
    {
        ExcelPackage.License.SetNonCommercialPersonal("My Name"); //This will also set the Author property to the name provided in the argument.
        using var package = new ExcelPackage(new FileInfo("SellingHistory.xlsx"));
        var worksheet = package.Workbook.Worksheets.Add("SellingHistory");

        // Header
        worksheet.Cells[1, 1].Value = "Title";
        worksheet.Cells[1, 2].Value = "Price";
        worksheet.Cells[1, 3].Value = "Purchase Date";

        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cells[row, 1].Value = item.Title;
            worksheet.Cells[row, 2].Value = item.Price;
            worksheet.Cells[row, 3].Value = item.PurchaseDate.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }
}