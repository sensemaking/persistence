using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Test.Cosmos
{
    public static class CosmosEmulator
    {
        private const string ProcessName = "CosmosDB.Emulator";

        public static readonly string FileName =
            $@"C:\Program Files\Azure Cosmos DB Emulator\{ProcessName}.exe";

        public static void Start(CosmosClient client, ProcessStartInfo? processStartInfo = null)
        {
            if (!bool.Parse(Environment.GetEnvironmentVariable("SkipCheckEmulatorBeforeRun") ?? "false") && CheckRespondsToRequests(client)) return;

            StartEmulator(client, processStartInfo);

            var emulatorHasInitialised = false;
            while (!emulatorHasInitialised)
                emulatorHasInitialised = CheckRespondsToRequests(client);
        }

        private static void StartEmulator(CosmosClient client, ProcessStartInfo? processStartInfo)
        {
            var ciIsManagingEmulator = bool.Parse(Environment.GetEnvironmentVariable("CiIsManagingEmulator") ?? "false");
            if (ciIsManagingEmulator) return;

            var processes = Process.GetProcessesByName(ProcessName);

            if (processes.Any()) { Stop(processes.Single(), client); }

            processStartInfo ??= new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = "/NoExplorer /NoUi /Port=8081 /PartitionCount=50",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process.Start(processStartInfo);
        }

        public static void Stop(Process process, CosmosClient client)
        {
            process.Close();
            Thread.Sleep(TimeSpan.FromSeconds(30).Milliseconds);
            var emulatorHasShutdown = false;
            while (!emulatorHasShutdown)
                emulatorHasShutdown = !CheckRespondsToRequests(client);
        }

        private static bool CheckRespondsToRequests(CosmosClient client)
        {
            try
            {
                var _ = client.GetDatabaseQueryIterator<DatabaseProperties>().ReadNextAsync().Result;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}