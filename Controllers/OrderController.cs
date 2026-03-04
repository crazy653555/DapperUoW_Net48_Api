using System;
using System.Threading.Tasks;
using System.Web.Http;
using DapperUoW_Net48_Api.Core.Interfaces;

namespace DapperUoW_Net48_Api.Controllers
{
    /// <summary>
    /// 訂單 Web API 控制器
    /// 負責接收外部 HTTP 請求，並呼叫服務層 (IOrderService) 展示不同連線與交易情境
    /// </summary>
    [RoutePrefix("api/order")]
    public class OrderController : ApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// 建立新訂單 (展示情境 1：大交易共生共死)
        /// </summary>
        /// <param name="customerName">客戶名稱，未傳入則使用預設值</param>
        /// <returns>HTTP 狀態碼及結果訊息</returns>
        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateOrder(string customerName = "預設測試客戶")
        {
            try
            {
                // 情境 1：大交易共生共死 
                await _orderService.CreateOrderProcessAsync(customerName);
                return Ok(new { Message = "訂單與明細建立成功 (情境1: Transaction 成功)" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 取得訂單主副表資料 (展示情境 2：單表操作，由 Repo 自動管理連線)
        /// </summary>
        /// <param name="id">要查詢的訂單 ID</param>
        /// <returns>包含明細的訂單完整資料</returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IHttpActionResult> GetOrder(int id)
        {
            // 情境 2：單表操作，Repository 自動管理連線
            var data = await _orderService.GetOrderInfoAsync(id);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        /// <summary>
        /// 執行儀表板更新流程 (展示情境 3：並發查詢加事後寫入，連線不衝突)
        /// </summary>
        /// <returns>HTTP 狀態碼及結果訊息</returns>
        [HttpPost]
        [Route("dashboard")]
        public async Task<IHttpActionResult> DashboardProcess()
        {
            try
            {
                // 情境 3：同時多連線並發查詢，最後才寫入
                await _orderService.DashboardProcessConcurrentAsync();
                return Ok(new { Message = "儀表板背景並發與寫入成功 (情境3: Task.WhenAll 無資源衝突)" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
