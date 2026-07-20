using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminReportService
    {
        Task<AdminReportsViewModel> GetReportsAsync();
    }
}
