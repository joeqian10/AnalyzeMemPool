using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace AnalyzeMemPool
{
    class Program
    {
        private static IConfigurationSection mainSection => new ConfigurationBuilder().AddJsonFile("appConfig.json").Build().GetSection("ApplicationConfiguration");
        private static string AnalysisPath => mainSection.GetSection("AnalysisPath").Value;
        private static string TableHeader => mainSection.GetSection("TableHeader").Value;

        static void Main(string[] args)
        {
            string[] seeds = mainSection.GetSection("SeedList").GetChildren().Select(p => p.Value).ToArray();
            int NodeNum = seeds.Length;
            Console.WriteLine("Mempool analysis started...");

            if (!File.Exists(AnalysisPath))
            {
                WriteLineToCSV(TableHeader);
            }

            int[] txNums = new int[NodeNum];
            string[] urls = new string[NodeNum];

            for (int i = 0; i < NodeNum; i++)
            {
                urls[i] = @"http://" + seeds[i];
            }

            bool stop = false;

            Task task = Task.Run(() =>
            {
                while (!stop)
                {
                    for (int i = 0; i < NodeNum; i++)
                    {
                        txNums[i] = Tools.GetMemPoolTransNum(urls[i]);
                    }
                    DateTime time = DateTime.Now;
                    WriteLineToCSV(time, txNums);
//#if DEBUG
                    WriteToConsole(time, txNums);
//#endif
                    Thread.Sleep(60000); // sleep for 1 minute 
                }
            });
            Console.ReadLine();
            stop = true;
            task.Wait();

            Console.WriteLine("Mempool analysis stopped...");
        }

        private static void WriteLineToCSV(string line)
        {
            using (FileStream fs = new FileStream(AnalysisPath, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.ASCII))
                sw.WriteLine(line);
        }

        private static void WriteLineToCSV(DateTime datetime, int[] txNum)
        {
            int len = txNum.Length;
            StringBuilder sb = new StringBuilder();
            sb.Append(datetime).Append(",");
            for (int i = 0; i <= len-2; i++)
            {
                sb.Append(txNum[i]).Append(",");
            }
            sb.Append(txNum[len - 1]);
            WriteLineToCSV(sb.ToString());
        }

        private static void WriteToConsole(DateTime time, int[] txNum)
        {
            int len = txNum.Length;
            StringBuilder sb = new StringBuilder();
            sb.Append(time).Append("\t");
            for (int i = 0; i <= len - 2; i++)
            {
                sb.Append(txNum[i]).Append("\t");
            }
            sb.Append(txNum[len - 1]);

            Console.WriteLine(sb.ToString());
        }
    }
}
