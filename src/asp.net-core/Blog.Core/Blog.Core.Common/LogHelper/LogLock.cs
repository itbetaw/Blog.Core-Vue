using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blog.Core.Common
{
    public class LogLock
    {
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        static int WritedCount = 0;
        static int FailedCount = 0;
        static string _contentRoot = string.Empty;

        public LogLock(string contentPath)
        {
            _contentRoot = contentPath;
        }
        public static void OutSql2Log(string prefix, string[] dataParas, bool isHeader = true)
        {
            try
            {
                LogWriteLock.EnterWriteLock();
                var folderPath = Path.Combine(_contentRoot, "Log");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var logFilePath = "";
                var now = DateTime.Now;
                string logContent = string.Join("\r\n", dataParas);
                if (isHeader)
                {
                    logContent = "--------------------------------\r\n" +
                       DateTime.Now + "|\r\n" +
                       String.Join("\r\n", dataParas) + "\r\n";
                }
                File.AppendAllText(logFilePath, logContent);
                WritedCount++;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                WritedCount++;
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }
    }
}
