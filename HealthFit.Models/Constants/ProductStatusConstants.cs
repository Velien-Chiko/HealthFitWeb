namespace HealthFit.Models.Constants
{
    public class ProductStatusConstants
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        public static readonly List<string> All = new()
    {
        Pending,
        Approved,
        Rejected
    };
    }
}
