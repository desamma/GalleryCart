namespace GalleryCart.Utilities.Utils
{
    // Helper methods to parse the List<string> data
    public static class CommissionResultHelper
    {
        public static List<(string FilePath, string OriginalName)> GetFiles(List<string> commissionResultLinks)
        {
            var files = new List<(string FilePath, string OriginalName)>();

            if (commissionResultLinks == null) return files;

            foreach (var item in commissionResultLinks.Where(x => x.StartsWith("FILE:")))
            {
                var content = item.Substring(5); // Remove "FILE:" prefix
                var parts = content.Split('|');
                if (parts.Length >= 2)
                {
                    files.Add((parts[0], parts[1]));
                }
            }

            return files;
        }

        public static List<string> GetLinks(List<string> commissionResultLinks)
        {
            if (commissionResultLinks == null) return new List<string>();

            return commissionResultLinks
                .Where(x => x.StartsWith("LINK:"))
                .Select(x => x.Substring(5)) // Remove "LINK:" prefix
                .ToList();
        }

        public static DateTime? GetUploadedAt(List<string> commissionResultLinks)
        {
            if (commissionResultLinks == null) return null;

            var uploadedItem = commissionResultLinks.FirstOrDefault(x => x.StartsWith("UPLOADED_AT:"));
            if (uploadedItem != null && DateTime.TryParse(uploadedItem.Substring(12), out var date))
            {
                return date;
            }

            return null;
        }
    }
}