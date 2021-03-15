using System.Collections.Generic;
using PCController.DataAccess.Models;

namespace PCController.DataAccess
{
    public class SQLiteCRUD
    {
        private readonly string connectionString;

        private readonly SQLiteDataAccess db = new();

        public SQLiteCRUD(string connString)
        {
            this.connectionString = connString;
        }

        public IList<LogModel> GetAllLogs()
        {
            string sql = $"SELECT * FROM Logs ORDER BY ID DESC LIMIT";
            return this.db.LoadData<LogModel, dynamic>(sql, new { }, this.connectionString);
        }

        public IList<LogModel> GetSomeLogs(int logCount)
        {
            string sql = $"SELECT * FROM Logs ORDER BY ID DESC LIMIT {logCount}";
            return this.db.LoadData<LogModel, dynamic>(sql, new { }, this.connectionString);
        }

        public IList<NetworkMessageModel> GetSomeNetData(int msgCount)
        {
            string sql = $"SELECT * FROM Network ORDER BY iD DESC LIMIT {msgCount}";
            return this.db.LoadData<NetworkMessageModel, dynamic>(sql, new { }, this.connectionString);
        }

        public void InsertNetMessage(NetworkMessageModel msg)
        {
            string sql =
                "insert into Network (Timestamp, UDPPort, RemoteIP, RemotePort, IncomingMessage, OutgoingMessage) values (@Timestamp, @UDPPort, @RemoteIP, @RemotePort, @IncomingMessage, @OutgoingMessage);";
            this.db.SaveData(
                sql,
                new
                    {
                        msg.Timestamp,
                        msg.UDPPort,
                        msg.RemoteIP,
                        msg.RemotePort,
                        msg.IncomingMessage,
                        msg.OutgoingMessage
                    },
                this.connectionString);
        }
    }
}
