using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using TfsToGit.Base.Contracts.Helpers;

namespace TfsToGit.Utils.Helpers
{
    internal class CommandlineArgsHelper : ICommandlineArgsHelper
    {
        private readonly IConfiguration _configuration;

        public CommandlineArgsHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TValue GetCommandlineArgumentValue<TValue>(string commandlineArgumentName, bool required = true)
        {
            TValue argValue = _configuration.GetValue<TValue>(commandlineArgumentName);
            if (required && argValue == null)
                throw new ArgumentNullException($"Commandline argument {commandlineArgumentName} is null.");
            return argValue;
        }

        public TValue GetCommandlineArgumentValue<TValue>(string commandlineArgumentName, bool required = true, params TValue[] options)
        {
            TValue argValue = _configuration.GetValue<TValue>(commandlineArgumentName);
            if (required && argValue == null)
                throw new ArgumentNullException($"Commandline argument {commandlineArgumentName} is null.");

            if (argValue != null && !options.ToList().Contains(argValue))
                throw new ArgumentOutOfRangeException($"Commandline argument {commandlineArgumentName} is invalid. Should be in options: [{string.Join(",", options)}]");

            return argValue;
        }

        public string GetCommandlineUsageText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Please provide the needed information:");
            sb.AppendLine();
            sb.AppendLine("Usage: cassandrabulkupdater.exe /CassandraHostName=\"<CassandraHostName>\" /CassandraUserName=\"<CassandraUserName>\" /CassandraPassword=\"<CassandraPassword>\" /Keyspace=\"<Keyspace>\" /Table=\"<Table>\" /ColumnToUpdateName=\"<ColumnToUpdateName>\" /ColumnToUpdateType=\"<ColumnToUpdateType>\" /ColumnToUpdateValue=\"<ColumnToUpdateValue>\" /PrimaryKeyColumnName=\"<PrimaryKeyColumnName>\" /PrimaryKeyColumnType=\"<PrimaryKeyColumnType>\" /NumberOfThreads=<NumberOfThreads>");
            sb.AppendLine();
            sb.AppendLine("CassandraHostName: The address where to reach cassandra (just provide one of the reachable nodes)");
            sb.AppendLine("CassandraUserName: The cassandra username to use (OPTIONAL)");
            sb.AppendLine("CassandraPassword: The cassandra password to use (OPTIONAL)");
            sb.AppendLine("Keyspace: The keyspace to use");
            sb.AppendLine("Table: The table to use");
            sb.AppendLine("ColumnToUpdateName: The column name of the field that should be updated");
            sb.AppendLine("ColumnToUpdateType: The column type of the field that should be updated (Supported: System.String, System.Int16, System.Int32, System.Int64, System.Boolean)");
            sb.AppendLine("ColumnToUpdateValue: The value to insert in the COLUMNTOUPDATE field");
            sb.AppendLine("PrimaryKeyColumnName: The column name of the primarykey");
            sb.AppendLine("PrimaryKeyColumnType: The column type of the primarykey  (Supported: System.String, System.Int16, System.Int32, System.Int64)");
            sb.AppendLine("NumberOfThreads: The number of threads to use which the bulkupdater uses against Cassandra. Advise is to use 20 here.");
            return sb.ToString();
        }
    }
}
