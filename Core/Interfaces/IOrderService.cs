using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Models.Dtos;

namespace DapperUoW_Net48_Api.Core.Interfaces
{
    /// <summary>
    /// 訂單服務層介面
    /// 定義對外部 (如 Controller) 提供的訂單相關商業邏輯流程
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// (情境 1) 建立完整訂單流程，展示大交易、共生共死
        /// </summary>
        /// <param name="customerName">客戶名稱</param>
        /// <returns>非同步工作完成</returns>
        Task CreateOrderProcessAsync(string customerName);

        /// <summary>
        /// (情境 2) 取得獨立訂單資料，展示單純查詢免管連線
        /// </summary>
        /// <param name="orderId">訂單編號</param>
        /// <returns>包含明細的訂單完整資料</returns>
        Task<OrderWithDetailsDto> GetOrderInfoAsync(int orderId);

        /// <summary>
        /// (情境 3) 處理儀表板並發請求，展示多條連線與非同步的混合操作
        /// </summary>
        /// <returns>非同步工作完成</returns>
        Task DashboardProcessConcurrentAsync();
    }
}
