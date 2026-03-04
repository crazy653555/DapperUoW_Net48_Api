using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperUoW_Net48_Api.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }

        Task BeginAsync();
        void Commit();
        void Rollback();
    }
}
