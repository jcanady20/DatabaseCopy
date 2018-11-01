using System;
using System.IO;

namespace DatabaseCopy.BusinessObjects
{
    public class CopyLogger : IDisposable
    {
        private readonly string _tableName;
        private StreamWriter _writer = null;
        public CopyLogger(string tableName)
        {
            _tableName = tableName;
            CreateLogFile();
            _writer = new StreamWriter(this.LogFile, true);
        }

        private string DirectoryName => Directory.GetCurrentDirectory();

        public string LogFile => Path.Combine(DirectoryName, "Logs", $"{_tableName}.log");

        public void WriteLine(string message)
        {
            _writer?.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} :: {message}");
            _writer?.Flush();
        }

        public void Write(string message)
        {
            _writer?.Write($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} :: {message}");
            _writer?.Flush();
        }

        private void CreateDirectory()
        {
            var path = Path.Combine(DirectoryName, "TableLogs");
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        private void CreateLogFile()
        {
            CreateDirectory();
            if (File.Exists(LogFile)) return;
            using (var fs = new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Close();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer?.Flush();
                _writer?.Dispose();
            }
        }
    }
}
