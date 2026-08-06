using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminAuditService
    {
        Task<AdminAuditViewModel> GetAuditLogAsync();
    }
}
