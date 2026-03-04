using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Models.Dtos;

namespace DapperUoW_Net48_Api.Core.Interfaces
{
    public interface IOrderService
    {
        Task CreateOrderProcessAsync(string customerName);
        Task<OrderWithDetailsDto> GetOrderInfoAsync(int orderId);
        Task DashboardProcessConcurrentAsync();
    }
}
