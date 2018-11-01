using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace DatabaseCopy.BusinessObjects
{
    public class TableCopier
    {
        private readonly Table _table;
        private readonly CopyLogger _logger;
        public TableCopier(Table table)
        {
            _table = table;
            _logger = new CopyLogger(table.Name);
        }

        public void Copy()
        {
            _logger?.WriteLine("Copying Table [" + _table.Name + "]");
            var cm = ConnectionManager.Instance;
            var _conn = cm.Open();
            try
            {
                _table.Status = TableStatus.Started;
                var sw = Stopwatch.StartNew();
                var fetchQuery = CreateQuery(_conn);
                _logger.WriteLine("Fecthing Data [" + _table.Name + "]");
                _table.RowCount = this.GetRowCount(_conn);
                _logger.WriteLine($"Fetching {_table.RowCount} Row(s)");
                using (var reader = _conn.dbExecuteReader(_conn.SourceConnection, fetchQuery))
                {
                    TruncateTable(_conn);
                    var options = SqlBulkCopyOptions.KeepNulls 
                        | SqlBulkCopyOptions.UseInternalTransaction
                        | SqlBulkCopyOptions.KeepIdentity 
                        | SqlBulkCopyOptions.TableLock;
                    using (var bc = new SqlBulkCopy(((SqlConnection)_conn.DestinationConnection), options, null))
                    {
                        bc.SqlRowsCopied += BulkCopy_SqlRowsCopied;
                        bc.DestinationTableName = _table.Name;
                        bc.BulkCopyTimeout = 90 * 1000;
                        bc.BatchSize = 1000;
                        bc.NotifyAfter = (_table.RowCount < 1000) ? (int)_table.RowCount : 100;
                        bc.WriteToServer(reader);
                        bc.Close();
                    }
                    if (_table.RowCount != _table.RowsCompleted)
                        _table.RowsCompleted = _table.RowCount;
                }
                sw.Stop();
                var elapsed = sw.Elapsed;
                _logger.WriteLine(String.Format("Completed Copy Table [" + _table.Name + "] Elapsed Time [{0}:{1}:{2}.{3}]", sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
                _table.Status = TableStatus.Completed;
            }
            catch (Exception e)
            {
                _table.Status = TableStatus.Error;
                _logger.WriteLine("---------  EXCEPTION  ---------");
                _logger.WriteLine(e.Source);
                _logger.WriteLine(e.Message);
                _logger.Write(e.StackTrace);
                throw;
            }
        }

        private void BulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            _logger?.WriteLine($"Rows Copied {e.RowsCopied}");
            _table.RowsCompleted = e.RowsCopied;
        }

        private void TruncateTable(PairedConnection conn)
        {
            var query = $"TRUNCATE TABLE [{_table.SourceSchema}].[{_table.Name}]";
            conn.dbExecureNonQuery(conn.DestinationConnection, query);
        }

        private long GetRowCount(PairedConnection conn)
        {
            long result = 0;
            var query = $"SELECT COUNT(*) FROM [{_table.SourceSchema}].[{_table.Name}]";
            using (var reader = conn.dbExecuteReader(conn.SourceConnection, query))
            {
                if (reader.Read())
                {
                    result = reader.GetInt64(0);
                }
            }
            return result;
        }

        private string CreateQuery(PairedConnection conn)
        {
            var tsql = new StringBuilder();
            var clms = new StringBuilder();
            var sb = new StringBuilder();
            tsql.AppendFormat("SELECT column_name, data_type FROM information_schema.columns WHERE table_schema='{0}' AND table_name='{1}'", _table.DestinationSchema, _table.Name);
            var reader = conn.dbExecuteReader(conn.DestinationConnection, tsql.ToString());
            while(reader.Read())
            {
                var clmName = reader.GetString(0);
                var clmType = reader.GetString(1);
                clms.AppendFormat("{0},", clmName);
            }
            clms.Remove(clms.Length - 1, 1);
            sb.AppendFormat("Select {0} From {1}.{2}", clms, _table.SourceSchema, _table.Name);
            return sb.ToString();
        }
    }
}
