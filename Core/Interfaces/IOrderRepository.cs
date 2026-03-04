using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Models.Dtos;
using DapperUoW_Net48_Api.Core.Models.Entities;

namespace DapperUoW_Net48_Api.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<int> InsertOrderAsync(Order order, IUnitOfWork uow = null);
        Task InsertOrderDetailAsync(OrderDetail detail, IUnitOfWork uow = null);
        Task<OrderWithDetailsDto> GetOrderWithDetailsAsync(int orderId, IUnitOfWork uow = null);
    }
}
