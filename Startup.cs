using System.Web.Http;
using DapperUoW_Net48_Api.App_Start;
using Owin;

namespace DapperUoW_Net48_Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            // 1. 設定 Autofac 依賴注入 (將 Resolver 註冊到 config 內)
            AutofacConfig.Configure(config);
            
            // 2. 設定 Web API 路由
            WebApiConfig.Register(config);

            appBuilder.UseWebApi(config);
        }
    }
}
