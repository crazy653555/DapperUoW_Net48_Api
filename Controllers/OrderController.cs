using System;
using System.Threading.Tasks;
using System.Web.Http;
using DapperUoW_Net48_Api.Core.Interfaces;

namespace DapperUoW_Net48_Api.Controllers
{
    [RoutePrefix("api/order")]
    public class OrderController : ApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

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
