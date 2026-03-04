using System;
using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Interfaces;

namespace DapperUoW_Net48_Api.Infrastructure.Repositories
{
    /// <summary>
    /// 這是一個抽象基底類別，透過泛型委派封裝 Dapper 的 UoW 生命週期管理。
    /// 目的：消除重複的 try...finally 程式碼，並防止連線遺漏關閉。
    /// </summary>
    public abstract class BaseRepository
    {
        protected readonly IUnitOfWorkFactory _factory;

        protected BaseRepository(IUnitOfWorkFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// 執行資料庫操作的核心防呆方法
        /// </summary>
        /// <typeparam name="T">回傳型別(例如 int 或是實體類別)</typeparam>
        /// <param name="externalUow">外部傳入的 UnitOfWork (可能有也可能為 null)</param>
        /// <param name="action">包含實際 Dapper 操作的委派方法 (也就是熟悉的 db => { ... } 區塊)</param>
        /// <returns>T 型別的結果</returns>
        protected async Task<T> ExecuteWithDbAsync<T>(IUnitOfWork externalUow, Func<IUnitOfWork, Task<T>> action)
        {
            // 判斷這個連線是否是由外部 (Service) 傳遞進來的
            bool isExternal = externalUow != null;

            // 如果外部有傳，就用外部的；否則我們自己向工廠要一個新的連線
            var uow = externalUow ?? _factory.Create();

            try
            {
                // 把準備好的連線 (uow) 餵給外部撰寫的 Lambda 方法區塊
                return await action(uow);
            }
            finally
            {
                // 如果是我們自己建的連線，我們用完了就要負責 Dispose 掉
                // 如果是別人傳的，就不關我們的事 (不要在此釋放)
                if (!isExternal)
                {
                    uow.Dispose();
                }
            }
        }
    }
}
