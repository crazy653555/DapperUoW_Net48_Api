using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using DapperUoW_Net48_Api.Core.Interfaces;
using DapperUoW_Net48_Api.Infrastructure.DataAccess;
using DapperUoW_Net48_Api.Infrastructure.Repositories;
using DapperUoW_Net48_Api.Services;

namespace DapperUoW_Net48_Api.App_Start
{
    public static class AutofacConfig
    {
        public static IContainer Configure(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            // 1. 註冊 Web API Controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // 2. 註冊資料庫連線產生器與 UoW 工廠
            // DbConnectionFactory 與 UnitOfWorkFactory 是產生連線的基礎建設，註冊為 SingleInstance (Singleton)
            builder.RegisterType<DbConnectionFactory>().AsSelf().SingleInstance();
            builder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();

            // 3. 註冊 Repository 與 Service
            // 每個 Request 共用同一個 Service 與 Repo 實例 (InstancePerRequest)
            // 在 Owin Self-Host 環境下，通常使用 InstancePerRequest 或是 LifetimeScope
            builder.RegisterType<OrderRepository>().As<IOrderRepository>().InstancePerRequest();
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerRequest();

            var container = builder.Build();

            // 設定 Web API 的 Dependency Resolver
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return container;
        }
    }
}
