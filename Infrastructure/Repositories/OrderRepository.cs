using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperUoW_Net48_Api.Core.Interfaces;
using DapperUoW_Net48_Api.Core.Models.Dtos;
using DapperUoW_Net48_Api.Core.Models.Entities;

namespace DapperUoW_Net48_Api.Infrastructure.Repositories
{
    /// <summary>
    /// 訂單倉儲實作
    /// 負責與底層 SQLite (模擬 Oracle 架構) 進行實際資料存取
    /// 繼承 BaseRepository 獲得連線生命週期管控能力
    /// </summary>
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(IUnitOfWorkFactory factory) : base(factory)
        {
        }

        /// <summary>
        /// 新增訂單資料
        /// 透過 SQLite 的 RETURNING (類似 Oracle) 取得剛建立的 Id，並支援參數綁定 :param
        /// </summary>
        /// <param name="order">訂單實體，包含客戶名稱與日期</param>
        /// <param name="uow">若被傳入，則加入目前交易；若無則自動新建連線</param>
        /// <returns>新增成功後的訂單 ID</returns>
        public Task<int> InsertOrderAsync(Order order, IUnitOfWork uow = null)
        {
            return ExecuteWithDbAsync(uow, async db =>
            {
                // 這裡示範使用 SQLite 支援的 RETURNING 語法來取得新建立的 ID
                // 在 Oracle 中經常使用 RETURNING Id INTO :Id 的類似語法
                // 同時注意，參數使用 :param 的 Oracle 風格，Dapper 同樣支援自動對應 SQLite
                string sql = @"
                    INSERT INTO Orders (CustomerName, OrderDate) 
                    VALUES (:CustomerName, :OrderDate) 
                    RETURNING Id;";

                return await db.Connection.ExecuteScalarAsync<int>(sql, order, db.Transaction);
            });
        }

        /// <summary>
        /// 新增訂單明細資料
        /// </summary>
        /// <param name="detail">明細實體，需依附於有效的主訂單 ID</param>
        /// <param name="uow">若被傳入，則加入目前交易；若無則自動新建連線</param>
        /// <returns>非同步工作完成 (無回傳值)</returns>
        public Task InsertOrderDetailAsync(OrderDetail detail, IUnitOfWork uow = null)
        {
            return ExecuteWithDbAsync(uow, async db =>
            {
                // Oracle Style 參數 (:param)
                string sql = @"
                    INSERT INTO OrderDetails (OrderId, ProductName, Price) 
                    VALUES (:OrderId, :ProductName, :Price)";

                await db.Connection.ExecuteAsync(sql, detail, db.Transaction);
                // 為了滿足 Task<T> 委派規範，沒有回傳值的方法通常可以回傳一個 Dummy value (如0)
                return 0;
            });
        }

        /// <summary>
        /// 根據訂單編號，使用 Dapper 的 QueryMultipleAsync 一次取得主副表資料
        /// </summary>
        /// <param name="orderId">欲查詢的訂單 ID</param>
        /// <param name="uow">若被傳入，則加入目前交易；若無則自動新建連線</param>
        /// <returns>對外傳輸的 DTO 物件，內含明細集合</returns>
        public Task<OrderWithDetailsDto> GetOrderWithDetailsAsync(int orderId, IUnitOfWork uow = null)
        {
            return ExecuteWithDbAsync(uow, async db =>
            {
                string sql = @"
                    SELECT Id as OrderId, CustomerName, OrderDate FROM Orders WHERE Id = :OrderId;
                    
                    SELECT Id, OrderId, ProductName, Price FROM OrderDetails WHERE OrderId = :OrderId;
                ";

                using (var multi = await db.Connection.QueryMultipleAsync(sql, new { OrderId = orderId }, db.Transaction))
                {
                    var order = await multi.ReadSingleOrDefaultAsync<OrderWithDetailsDto>();

                    if (order != null)
                    {
                        var details = await multi.ReadAsync<OrderDetail>();
                        order.Details = details.ToList();
                    }

                    return order;
                }
            });
        }
    }
}
