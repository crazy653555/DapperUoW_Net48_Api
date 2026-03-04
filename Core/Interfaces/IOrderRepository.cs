using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Models.Dtos;
using DapperUoW_Net48_Api.Core.Models.Entities;

namespace DapperUoW_Net48_Api.Core.Interfaces
{
    /// <summary>
    /// 訂單倉儲層介面
    /// 負責所有訂單與明細的資料庫存取操作
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// 新增單筆主訂單資料
        /// </summary>
        /// <param name="order">訂單實體物件</param>
        /// <param name="uow">可選的工作單元，若傳入則納入同一個交易中執行</param>
        /// <returns>回傳新建立的訂單 ID (由資料庫自動生成)</returns>
        Task<int> InsertOrderAsync(Order order, IUnitOfWork uow = null);

        /// <summary>
        /// 新增單筆訂單明細資料
        /// </summary>
        /// <param name="detail">訂單明細實體物件</param>
        /// <param name="uow">可選的工作單元，若傳入則納入同一個交易中執行</param>
        /// <returns>非同步工作完成</returns>
        Task InsertOrderDetailAsync(OrderDetail detail, IUnitOfWork uow = null);

        /// <summary>
        /// 根據訂單 ID 查詢包含主副表 (一對多) 的完整訂單資料
        /// </summary>
        /// <param name="orderId">訂單識別碼</param>
        /// <param name="uow">可選的工作單元 (若是單純查詢，通常由倉儲自行建立連線即可)</param>
        /// <returns>回傳包含明細列表的訂單傳輸物件 DTO</returns>
        Task<OrderWithDetailsDto> GetOrderWithDetailsAsync(int orderId, IUnitOfWork uow = null);
    }
}
