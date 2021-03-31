using System.Collections.Generic;
using PCController.DataAccess.Models;

namespace PCController.DataAccess
{
    public class SQLiteCRUD
    {
        private readonly string _connectionString;

        private readonly SQLiteDataAccess _db = new();

        public SQLiteCRUD(string connString)
        {
            _connectionString = connString;
        }

        public IList<LogModel> GetAllLogs()
        {
            string sql = $"SELECT * FROM Logs ORDER BY ID DESC LIMIT";
            return _db.LoadData<LogModel, dynamic>(sql, new { }, _connectionString);
        }

        public IList<LogModel> GetSomeLogs(int logCount)
        {
            string sql = $"SELECT * FROM Logs ORDER BY ID DESC LIMIT {logCount}";
            return _db.LoadData<LogModel, dynamic>(sql, new { }, _connectionString);
        }

        public IList<NetworkMessageModel> GetSomeNetData(int msgCount)
        {
            string sql = $"SELECT * FROM Network ORDER BY ID DESC LIMIT {msgCount}";
            return _db.LoadData<NetworkMessageModel, dynamic>(sql, new { }, _connectionString);
        }

        public IList<UdpSenderModel> GetSomeUdpData(int msgCount)
        {
            string sql = $"SELECT * FROM UDPSender ORDER BY ID DESC LIMIT {msgCount}";
            return _db.LoadData<UdpSenderModel, dynamic>(sql, new { }, _connectionString);
        }

        public IList<ProcMonitorModel> GetSomeProcData(int msgCount)
        {
            string sql = $"SELECT * FROM ProcMonitor ORDER BY ID DESC LIMIT {msgCount}";
            return _db.LoadData<ProcMonitorModel, dynamic>(sql, new { }, _connectionString);
        }

        public IList<string> GetUdpUsedIPAddresses(int ipCount)
        {
            string sql = $"SELECT LocalIP FROM UDPSender ORDER BY ID DESC LIMIT {ipCount}";
            return _db.LoadData<string, dynamic>(sql, new { }, _connectionString);
        }

        public void InsertUdpSentData(UdpSenderModel udp)
        {
            string sql = "insert into UDPSender (IncomingMessage, OutgoingMessage, RemoteIP, LocalIP, LocalPort, RemotePort, Timestamp) values(@IncomingMessage, @OutgoingMessage, @RemoteIP, @LocalIP, @LocalPort, @RemotePort, @Timestamp);";
            _db.SaveData(sql, new
                {
                    udp.IncomingMessage,
                    udp.OutgoingMessage,
                    udp.RemoteIP,
                    udp.LocalIP,
                    udp.LocalPort,
                    udp.RemotePort,
                    udp.Timestamp
                },
                _connectionString);
        }

        public void InsertNetMessage(NetworkMessageModel msg)
        {
            string sql =
                "insert into Network (Timestamp, UDPPort, RemoteIP, RemotePort, IncomingMessage, OutgoingMessage) values (@Timestamp, @UDPPort, @RemoteIP, @RemotePort, @IncomingMessage, @OutgoingMessage);";
            _db.SaveData(
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
                _connectionString);
        }

        public void InsertProcData(ProcMonitorModel data)
        {
            string sql =
                "insert into ProcMonitor (Timestamp, PeakPagedMemorySize, PeakWorkingSet, PrivateMemorySize, ThreadCount, HandleCount, IsNotResponding, Message) values (@Timestamp, @PeakPagedMemorySize, @PeakWorkingSet, @PrivateMemorySize, @ThreadCount, @HandleCount, @IsNotResponding, @Message);";
            _db.SaveData(
                sql,
                new {
                    data.Timestamp,
                    data.PeakPagedMemorySize,
                    data.PeakWorkingSet,
                    data.PrivateMemorySize,
                    data.ThreadCount,
                    data.HandleCount,
                    data.IsNotResponding,
                    data.Message
                },
                _connectionString);
        }
    }
}
