using System;
using System.Text;
using System.Data;
using System.Data.Common;

namespace DatabaseCopy.BusinessObjects
{
    public class PairedConnection : IDisposable
    {
        DbProviderFactory _sourceFactory;
        DbProviderFactory _destinationFactory;

        private DbConnection _sourceConnection;
        private DbConnection _destinationConnection;

        private DbConnectionStringBuilder _sourceconnectionstring;
        private DbConnectionStringBuilder _destinationconnectionstring;


        public PairedConnection(DbConnectionStringBuilder source, DbConnectionStringBuilder destination)
        {
            this._sourceconnectionstring = source;
            this._destinationconnectionstring = destination;
        }

        public bool IsClosed
        {
            get
            {
                if (_destinationConnection == null || _destinationConnection.State == System.Data.ConnectionState.Closed)
                    return true;
                if (_sourceConnection == null || _sourceConnection.State == System.Data.ConnectionState.Closed)
                    return true;
                return false;
            }
        }

        public DbConnectionStringBuilder SourcseConnectionString
        {
            get
            {
                return _sourceconnectionstring;
            }
            set
            {
                _sourceconnectionstring = value;
            }
        }
        public DbConnectionStringBuilder DestinationConnectionString
        {
            get
            {
                return _destinationconnectionstring;
            }
            set
            {
                _destinationconnectionstring = value;
            }
        }


        public DbProviderFactory SourceFactory
        {
            get
            {
                return _sourceFactory;
            }
        }
        public DbProviderFactory DestinationFactory
        {
            get
            {
                return _destinationFactory;
            }
        }

        public DbConnection SourceConnection
        {
            get
            {
                return _sourceConnection;
            }
        }
        public DbConnection DestinationConnection
        {
            get
            {
                return _destinationConnection;
            }
        }

        public void OpenConnection(string sourceprovider, string destinationprovider)
        {
            _sourceFactory = DbProviderFactories.GetFactory(sourceprovider);
            _sourceConnection = _sourceFactory.CreateConnection();

            _destinationFactory = DbProviderFactories.GetFactory(destinationprovider);
            _destinationConnection = _destinationFactory.CreateConnection();

            _sourceConnection.ConnectionString = _sourceconnectionstring.ConnectionString;
            _destinationConnection.ConnectionString = _destinationconnectionstring.ConnectionString;

            _sourceConnection.Open();
            _destinationConnection.Open();
        }
        public void CloseConnection()
        {
            if (_sourceConnection != null && _sourceConnection.State != System.Data.ConnectionState.Closed)
                _sourceConnection.Close();
            if (_destinationConnection != null && _destinationConnection.State != System.Data.ConnectionState.Closed)
                _destinationConnection.Close();
        }

        public IDbCommand dbCommand(IDbConnection _conn, string tsql)
        {
            IDbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = tsql;
            return cmd;
        }
        public void dbExecureNonQuery(IDbConnection _conn, string tsql)
        {
            using (IDbCommand dbCmd = this.dbCommand(_conn, tsql))
            {
                dbCmd.ExecuteNonQuery();
            }
        }
        public object dbExecuteScalar(IDbConnection _conn, string tsql)
        {
            object o = null;
            using (IDbCommand dbCmd = this.dbCommand(_conn, tsql))
            {
                o = dbCmd.ExecuteScalar();
            }
            return o;
        }
        public DataSet dbExecuteDataSet(DbProviderFactory f, IDbConnection _conn, string tsql)
        {
            IDbDataAdapter da = null;
            DataSet ds = new DataSet();
            using (IDbCommand dbCmd = this.dbCommand(_conn, tsql))
            {
                da = f.CreateDataAdapter();
                da.SelectCommand = dbCmd;
                da.Fill(ds);
            }
            return ds;
        }
        public DataTable dbExecuteDataTable(DbProviderFactory f, IDbConnection _conn, string tsql)
        {
            DataSet ds = dbExecuteDataSet(f, _conn, tsql);
            DataTable dt = ds.Tables[0].Copy();

            ds.Clear();
            ds.Dispose();

            return dt;
        }
        public IDataReader dbExecuteReader(IDbConnection _conn, string tsql)
        {
            IDataReader ir = null;
            using (IDbCommand dbCmd = this.dbCommand(_conn, tsql))
            {
                ir = dbCmd.ExecuteReader();
            }
            return ir;
        }
        public DataTable dbExecuteGetSchema(IDbConnection _conn)
        {
            return ((DbConnection)_conn).GetSchema("Tables");
        }
        public void dbExecuteScriptFile(IDbConnection _conn, string filename)
        {
            StringBuilder sb = new StringBuilder();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
            {
                string line = String.Empty;
                while ((line = sr.ReadLine()) != String.Empty)
                {
                    if (line.ToUpper() != "GO")
                        sb.AppendFormat("{0}\r\n", line);
                    else
                    {
                        dbExecureNonQuery(_conn, sb.ToString());
                        sb.Length = 0;
                    }
                }
            }
        }

        public static bool VerifyConnection(DbConnectionStringBuilder dbConnstring, string dbProvider)
        {
            bool r = false;
            try
            {
                var factory = DbProviderFactories.GetFactory(dbProvider);
                using (var cn = factory.CreateConnection())
                {
                    cn.ConnectionString = dbConnstring.ToString();
                    cn.Open();
                    r = true;
                    cn.Close();
                }
            }
            catch (Exception)
            { }
            return r;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            { }
        }

    }
}