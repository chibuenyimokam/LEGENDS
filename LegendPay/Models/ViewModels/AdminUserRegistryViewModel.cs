namespace LegendPay.Models.ViewModels
{
    public class AdminUserRegistryViewModel
    {
        public int TotalUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int UnverifiedUsers { get; set; }

        public List<AdminUserRow> Users { get; set; } = new();

        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public int ShownCount => Users.Count;

        public string? Search { get; set; }
        public string? Status { get; set; }
        public decimal? MinBalance { get; set; }
    }

    public class AdminUserRow
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Reference => "LP-" + Id.ToString("N")[..6].ToUpperInvariant();

        public string Initials
        {
            get
            {
                var first = string.IsNullOrWhiteSpace(FirstName) ? "" : char.ToUpperInvariant(FirstName[0]).ToString();
                var last = string.IsNullOrWhiteSpace(LastName) ? "" : char.ToUpperInvariant(LastName[0]).ToString();
                var combined = first + last;
                return string.IsNullOrEmpty(combined) ? "?" : combined;
            }
        }
    }
}
