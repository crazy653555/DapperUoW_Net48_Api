using System;
using System.Data;
using System.Threading.Tasks;
using DapperUoW_Net48_Api.Core.Interfaces;

namespace DapperUoW_Net48_Api.Infrastructure.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;

        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDbConnection Connection => _connection;
        public IDbTransaction Transaction => _transaction;

        public Task BeginAsync()
        {
            // 在 .NET Framework 4.8 中，IDbConnection 尚未原生支援非同步 BeginTransactionAsync
            // 故此處使用同步 BeginTransaction 封裝
            _transaction = _connection.BeginTransaction();
            return Task.CompletedTask;
        }

        public void Commit()
        {
            _transaction?.Commit();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
