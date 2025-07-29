namespace GalleryCart.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalArtists { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Growth { get; set; }

    public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<SellingHistoryDto> SellingHistories { get; set; } = new();

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
}

public class SellingHistoryDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount => Price * Quantity;
    public DateTime PurchaseDate { get; set; }
}