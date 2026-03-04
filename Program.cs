using System;
using Microsoft.Owin.Hosting;

namespace DapperUoW_Net48_Api
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9000/";

            try
            {
                // 啟動 Owin Self-Host
                using (WebApp.Start<Startup>(url: baseAddress))
                {
                    Console.WriteLine($"\n[Server] Web API 伺服器已成功啟動於：{baseAddress}");
                    Console.WriteLine("=========================================================");
                    Console.WriteLine(" 測試端點說明 (建議使用 Postman) \n");
                    Console.WriteLine("  1. 情境 1 (多表交易寫入): ");
                    Console.WriteLine("     [POST] http://localhost:9000/api/order/create?customerName=TestUser");
                    Console.WriteLine("\n  2. 情境 2 (查詢單表): ");
                    Console.WriteLine("     [GET]  http://localhost:9000/api/order/1");
                    Console.WriteLine("\n  3. 情境 3 (並發查詢與後續寫入): ");
                    Console.WriteLine("     [POST] http://localhost:9000/api/order/dashboard");
                    Console.WriteLine("=========================================================");
                    Console.WriteLine("\n請按任意鍵關閉伺服器...");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"啟動失敗：{ex}");
                Console.ReadLine();
            }
        }
    }
}
