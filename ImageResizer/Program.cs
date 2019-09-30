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
            int numTotalRound = 10;
            double avgNormal = 0.0;
            double avgAsync = 0.0;
            double avgAsync2 = 0.0;
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;
            List<long> normalProcessTimes = new List<long>();
            List<long> asyncProcessTimes = new List<long>();
            ImageProcess imageProcess = new ImageProcess();
            Stopwatch sw = new Stopwatch();

            #region 執行普通轉換，並記下平均
            Console.WriteLine($"開始執行一般處理，共 {numTotalRound} 輪");
            for (int i = 0; i < numTotalRound; i++)
            {
                imageProcess.Clean(destinationPath);

                Console.WriteLine($"正在執行第 { i + 1 } 輪處理...");
                sw.Restart();
                imageProcess.ResizeImages(i + 1, sourcePath, destinationPath, 2.0);
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
                Console.WriteLine();

                normalProcessTimes.Add(sw.ElapsedMilliseconds);
                System.GC.Collect();//強制 GC 回收記憶體，否則會爆
            }

            avgNormal = normalProcessTimes.Average(x => x);
            Console.WriteLine($"平均處理時間：{avgNormal} ms \n");
            Console.WriteLine();
            #endregion

            #region 執行第一種非同步轉換，並記下平均
            Console.WriteLine($"\n\n開始執行非同步處理，共 {numTotalRound} 輪");
            for (int i = 0; i < numTotalRound; i++)
            {
                imageProcess.Clean(destinationPath);

                Console.WriteLine($"正在執行第 { i + 1 } 輪處理...");
                sw.Restart();
                await imageProcess.ResizeImageAsync(i + 1, sourcePath, destinationPath, 2.0);
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
                Console.WriteLine();

                asyncProcessTimes.Add(sw.ElapsedMilliseconds);
                System.GC.Collect();//強制 GC 回收記憶體，否則會爆
            }

            avgAsync = asyncProcessTimes.Average(x => x);
            Console.WriteLine($"平均處理時間：{avgAsync} ms");
            #endregion

            #region 第二種非同步轉換(根據邏輯處理器數量做批次處理)，並記下平均
            Console.WriteLine($"\n\n開始執行第二種非同步處理，共 {numTotalRound} 輪");
            Console.WriteLine("Number Of Logical Processors: {0}", Environment.ProcessorCount);
            asyncProcessTimes.Clear();

            for (int i = 0; i < numTotalRound; i++)
            {
                imageProcess.Clean(destinationPath);

                Console.WriteLine($"正在執行第 { i + 1 } 輪處理...");
                sw.Restart();
                await imageProcess.ResizeImageBoundHyperThreadAsync(i + 1, sourcePath, destinationPath, 2.0);
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
                Console.WriteLine();

                asyncProcessTimes.Add(sw.ElapsedMilliseconds);
                System.GC.Collect();//強制 GC 回收記憶體，否則會爆
            }

            avgAsync2 = asyncProcessTimes.Average(x => x);
            Console.WriteLine($"平均處理時間：{avgAsync2} ms \n");
            Console.WriteLine();
            #endregion

            Console.WriteLine($"\n\n比較結果：");
            Console.WriteLine($"普通處理平均 {avgNormal} ms;");
            Console.WriteLine($"第一種非同步處理平均 {avgAsync} ms; 改善率 {(avgNormal - avgAsync) / avgNormal * 100, 0:F3} %");
            Console.WriteLine($"第二種非同步處理平均 {avgAsync2} ms; 改善率 {(avgNormal - avgAsync2) / avgNormal * 100,0:F3} %");
            Console.ReadKey();
        }
    }
}
