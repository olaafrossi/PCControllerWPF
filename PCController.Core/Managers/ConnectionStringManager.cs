using Serilog;

namespace PCController.Core.Managers
{
    public static class ConnectionStringManager
    {
        public enum DataBases
        {
            /// <summary>
            /// These are the tables in the Logs.db database
            /// Should probably clean this up a bit
            /// </summary>
            Logs,
            PCControllerDB,
        }

        public static string GetConnectionString(DataBases dB)
        {
            string output = string.Empty;

            if (dB == DataBases.Logs)
            {
                output = Properties.Settings.Default.ConnectionStringLogs;
                Log.Logger.Information("Getting SQL Connection String for LogDB {output}", output);
            }
            else if (dB == DataBases.PCControllerDB)
            {
                output = Properties.Settings.Default.ConnectionStringApp;
                Log.Logger.Information("Getting SQL Connection String for NetworkDB {output}", output);
            }
            else
            {
                output = "Not a valid DB";
            }
            return output;
        }
    }
}

