using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int numTotalRound = 5;
            double avgNormal = 0.0;
            double avgAsync = 0.0;
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;
            List<long> normalProcessTimes = new List<long>();
            List<long> asyncProcessTimes = new List<long>();
            ImageProcess imageProcess = new ImageProcess();
            Stopwatch sw = new Stopwatch();

            //執行 N 次普通轉換，並記下平均
            Console.WriteLine($"開始執行一般處理，共 {numTotalRound} 次");
            for (int i=0; i< numTotalRound; i++)
            {
                imageProcess.Clean(destinationPath);

                Console.Write($"正在執行第 { i+1 } 次處理...");
                sw.Restart();
                imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");

                normalProcessTimes.Add(sw.ElapsedMilliseconds);
                System.GC.Collect();
            }

            avgNormal = normalProcessTimes.Average(x => x);
            Console.WriteLine($"平均處理時間：{avgNormal} ms");

            //執行 N 次非同步轉換，並記下平均
            Console.WriteLine($"\n\n開始執行非同步處理，共 {numTotalRound} 次");
            for (int i = 0; i < numTotalRound; i++)
            {
                imageProcess.Clean(destinationPath);

                Console.Write($"正在執行第 { i + 1 } 次處理...");
                sw.Restart();
                await imageProcess.ResizeImageAsync(sourcePath, destinationPath, 2.0);
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");

                asyncProcessTimes.Add(sw.ElapsedMilliseconds);
                System.GC.Collect();
            }

            avgAsync = asyncProcessTimes.Average(x => x);
            Console.WriteLine($"平均處理時間：{avgAsync} ms");

            Console.WriteLine($"\n\n比較結果：普通處理平均 {avgNormal} ms; " +
                $"非同步處理平均 {avgAsync} ms; " +
                $"改善率 {(avgNormal - avgAsync) / avgNormal * 100, 0:F3} %");
            Console.ReadKey();
        }
    }
}
