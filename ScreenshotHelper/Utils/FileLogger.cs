using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.Utils
{
    public static class FileLogger
    {
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 将文本同时输出到控制台和文件
        /// </summary>
        public static void WriteLine(string message, string logFile = "output.log")
        {
            if (!Path.IsPathRooted(logFile)) 
            {
                logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
            }
            lock (_lockObject)
            {
                // 输出到控制台
                Console.WriteLine(message);

                // 输出到文件
                try
                {
                    using (var writer = new StreamWriter(logFile, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"写入日志文件失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 将文本输出到文件（不显示在控制台）
        /// </summary>
        public static void WriteToFile(string message, string logFile = "output.log")
        {
            lock (_lockObject)
            {
                try
                {
                    using (var writer = new StreamWriter(logFile, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"写入日志文件失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 重定向所有Console.WriteLine到文件
        /// </summary>
        public static void RedirectConsoleToFile(string logFile = "console_output.log")
        {
            lock (_lockObject)
            {
                try
                {
                    var fileStream = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                    var writer = new StreamWriter(fileStream, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                    Console.SetOut(writer);
                    Console.WriteLine($"=== 控制台输出重定向开始于 {DateTime.Now} ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重定向失败: {ex.Message}");
                }
            }
        }
    }
}
