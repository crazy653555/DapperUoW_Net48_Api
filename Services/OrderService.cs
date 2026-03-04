using System;
using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Interfaces;
using DapperUoW_Net48_Api.Core.Models.Dtos;
using DapperUoW_Net48_Api.Core.Models.Entities;

namespace DapperUoW_Net48_Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IOrderRepository _orderRepo;

        public OrderService(IUnitOfWorkFactory factory, IOrderRepository orderRepo)
        {
            _uowFactory = factory;
            _orderRepo = orderRepo;
        }

        /// <summary>
        /// 情境 1: 多 Table 寫入，共生共死，全成功才 Commit
        /// 此處展示了在 Service 中建立一個主力的 UnitOfWork，並將其一路往下傳遞給多個 Repository 呼叫
        /// </summary>
        /// <param name="customerName">客戶名稱，將作為訂單主表的欄位</param>
        /// <returns>非同步操作任務</returns>
        public async Task CreateOrderProcessAsync(string customerName)
        {
            using (var uow = _uowFactory.Create())
            {
                await uow.BeginAsync();

                try
                {
                    var order = new Order { CustomerName = customerName, OrderDate = DateTime.Now };

                    // 將 uow 傳入，確保在同一個 Transaction 內
                    int newOrderId = await _orderRepo.InsertOrderAsync(order, uow);

                    var item1 = new OrderDetail { OrderId = newOrderId, ProductName = "【模擬 Oracle】鍵盤", Price = 3499 };
                    var item2 = new OrderDetail { OrderId = newOrderId, ProductName = "【模擬 Oracle】滑鼠", Price = 2000 };

                    await _orderRepo.InsertOrderDetailAsync(item1, uow);
                    await _orderRepo.InsertOrderDetailAsync(item2, uow);

                    uow.Commit(); // 全部成功才寫入
                    Console.WriteLine($"[Service 成功] 訂單編號 {newOrderId} 寫入成功。");
                }
                catch (Exception)
                {
                    uow.Rollback(); // 發生錯誤防呆回滾
                    throw;
                }
            }
        }

        /// <summary>
        /// 情境 2: 單純查詢，不用手動傳入 UoW，交給 Repository 自己開關連線
        /// 外部直接呼叫此方法，由底層 BaseRepository 在沒有收到外部 UoW 時，自行建立臨時連線並於查畢後釋放
        /// </summary>
        /// <param name="orderId">欲查詢的訂單主鍵</param>
        /// <returns>帶有明細的訂單傳輸物件 DTO</returns>
        public async Task<OrderWithDetailsDto> GetOrderInfoAsync(int orderId)
        {
            // 非常乾淨，Service 就像不知道有資料庫連線這回事
            return await _orderRepo.GetOrderWithDetailsAsync(orderId);
        }

        /// <summary>
        /// 情境 3: 混和操作。先並發兩條連線查詢，算完結果再開第三條連線寫入
        /// 此處展示 Factory 模式的核心價值：可以輕鬆取得多條互不干涉的連線，滿足 Task.WhenAll 並發查詢需求，
        /// 避免 ADO.NET 同一連線被多執行緒存取引發的錯誤。
        /// </summary>
        /// <returns>非同步操作任務</returns>
        public async Task DashboardProcessConcurrentAsync()
        {
            // 為了展示並行，我們同時去抓不同的資料庫資訊 (此處以同樣是查 Order 代替示範)
            using (var readUow1 = _uowFactory.Create())
            using (var readUow2 = _uowFactory.Create())
            {
                // 這兩條連線是互相獨立的，因此使用 Task.WhenAll 也不會引發連線資源衝突或錯亂
                var task1 = _orderRepo.GetOrderWithDetailsAsync(1, readUow1);
                var task2 = _orderRepo.GetOrderWithDetailsAsync(2, readUow2);

                await Task.WhenAll(task1, task2);

                var data1 = task1.Result;
                var data2 = task2.Result;
                Console.WriteLine($"平行查詢完成：從兩條獨立連線取得結果。");
            }
            // 離開 using 區塊後自動 Dispose，連線歸還

            // 接著根據查出來的資料，我們可能需要寫入彙整報表
            using (var writeUow = _uowFactory.Create())
            {
                await writeUow.BeginAsync();
                try
                {
                    var reportOrder = new Order { CustomerName = "系統報表批次寫入", OrderDate = DateTime.Now };
                    await _orderRepo.InsertOrderAsync(reportOrder, writeUow);
                    writeUow.Commit();
                }
                catch
                {
                    writeUow.Rollback();
                    throw;
                }
            }
        }
    }
}
