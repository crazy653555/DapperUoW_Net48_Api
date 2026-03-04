using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperUoW_Net48_Api.Core.Interfaces
{
    /// <summary>
    /// 工作單元 (Unit of Work) 介面
    /// 負責維持單一資料庫連線，並控管其交易 (Transaction) 的生命週期，確保資料一致性
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 取得目前的資料庫連線實例
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 取得目前的資料庫交易實例 (若尚未開啟交易則為 null)
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// 非同步開啟資料庫交易
        /// </summary>
        Task BeginAsync();

        /// <summary>
        /// 提交目前的所有資料操作 (若成功則寫入資料庫)
        /// </summary>
        void Commit();

        /// <summary>
        /// 撤銷目前的所有資料操作 (還原至交易開啟前的狀態)
        /// </summary>
        void Rollback();
    }
}
