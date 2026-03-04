using DapperUoW_Net48_Api.Core.Interfaces;

namespace DapperUoW_Net48_Api.Infrastructure.DataAccess
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly DbConnectionFactory _connectionFactory;

        public UnitOfWorkFactory(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IUnitOfWork Create()
        {
            var connection = _connectionFactory.CreateConnection();
            return new UnitOfWork(connection);
        }
    }
}
