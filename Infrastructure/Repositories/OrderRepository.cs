using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperUoW_Net48_Api.Core.Interfaces;
using DapperUoW_Net48_Api.Core.Models.Dtos;
using DapperUoW_Net48_Api.Core.Models.Entities;

namespace DapperUoW_Net48_Api.Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(IUnitOfWorkFactory factory) : base(factory)
        {
        }

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
