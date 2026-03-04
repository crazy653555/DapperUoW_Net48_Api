namespace DapperUoW_Net48_Api.Core.Interfaces
{
    /// <summary>
    /// 工作單元工廠介面 (工廠模式)
    /// 用於建立並提供獨立的資料庫連線實例，以解決同一 Request 中非同步並發查詢 (Concurrency) 的資源佔用問題
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// 建立一條新的、獨立的資料庫連線 (UnitOfWork)
        /// </summary>
        /// <returns>實作 IUnitOfWork 的實例，使用者需自行確保使用完畢後將其 Dispose</returns>
        IUnitOfWork Create();
    }
}
