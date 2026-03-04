using System.Data;
using System.Data.SQLite;
using Dapper;

namespace DapperUoW_Net48_Api.Infrastructure.DataAccess
{
    public class DbConnectionFactory
    {
        // 使用實體檔案作為 SQLite 模擬資料庫，確保跨連線或並行測試時資料持久性
        private readonly string _connectionString = "Data Source=DemoDB.sqlite;Version=3;";

        public DbConnectionFactory()
        {
            InitializeDatabase();
        }

        public IDbConnection CreateConnection()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                // 建立模擬用的 Table
                // 註：這使用 SQLite 語法，但我們後續的 Dapper 操作會盡量展演 Oracle 風格的寫法 (如 :param)
                string createTablesSql = @"
                    CREATE TABLE IF NOT EXISTS Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerName TEXT NOT NULL,
                        OrderDate DATETIME NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS OrderDetails (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL,
                        ProductName TEXT NOT NULL,
                        Price DECIMAL NOT NULL,
                        FOREIGN KEY (OrderId) REFERENCES Orders(Id)
                    );
                ";
                conn.Execute(createTablesSql);
            }
        }
    }
}
