using System.Data.Common;

namespace DatabaseCopy.BusinessObjects
{
    public class ConnectionManager : ObjectPool<PairedConnection>
    {
        private string _sourceprovider;
        private string _destinationprovider;
        private DbConnectionStringBuilder _sourcestring;
        private DbConnectionStringBuilder _destinationstring;

        public string SourceProvider
        {
            get
            {
                return _sourceprovider;
            }
            set
            {
                _sourceprovider = value;
            }
        }
        public string DestinationProvider
        {
            get
            {
                return _destinationprovider;
            }
            set
            {
                _destinationprovider = value;
            }
        }
        public DbConnectionStringBuilder SourceConnectionString
        {
            get
            {
                return _sourcestring;
            }
            set
            {
                _sourcestring = value;
            }
        }
        public DbConnectionStringBuilder DestinationConnectionString
        {
            get
            {
                return _destinationstring;
            }
            set
            {
                _destinationstring = value;
            }
        }

        #region Performance Counters
        private long iRequestedConnectionsCount = 0;
        private long iClosedConnectionsCount = 0;
        private long iNewConnectionCount = 0;
        private long iQueryExecutionCount = 0;
        private long iTotalExecutionTime = 0;
        private long iAvgExecutionTime = 0;

        public long PrefCounterRequestedConnections
        {
            get
            {
                return this.iRequestedConnectionsCount;
            }
        }
        public long PrefCounterClosedConnections
        {
            get
            {
                return this.iClosedConnectionsCount;
            }
        }
        public long PrefCounterNewConnections
        {
            get
            {
                return this.iNewConnectionCount;
            }
        }
        public long PrefCounterCurrentOpenConnections
        {
            get
            {
                return locked.Count;
            }
        }
        public long PrefCounterTotalConnectionPool
        {
            get
            {
                return locked.Count + unlocked.Count;
            }
        }
        public long PrefCounterTotalQueriesExecuted
        {
            get
            {
                return iQueryExecutionCount;
            }
        }
        public long PrefCounterTotalExecutionTime
        {
            get
            {
                return iTotalExecutionTime;
            }
        }
        public long PrefCounterAvgExecutionTime
        {
            get
            {
                return iAvgExecutionTime;
            }
        }
        #endregion

        public static readonly ConnectionManager Instance = new ConnectionManager();

        private ConnectionManager()
        { }

        protected override PairedConnection Create()
        {
            iNewConnectionCount++;
            PairedConnection _os = new PairedConnection(_sourcestring, _destinationstring);
            _os.OpenConnection(SourceProvider, DestinationProvider);
            return _os;
        }

        protected override bool Validate(PairedConnection o)
        {
            return (!(o.IsClosed));
        }

        protected override void Expire(PairedConnection o)
        {
            iClosedConnectionsCount++;
            o.CloseConnection();
            o.Dispose();
        }

        public PairedConnection Open(DbConnectionStringBuilder source, DbConnectionStringBuilder destination)
        {
            iRequestedConnectionsCount++;
            this._sourcestring = source;
            this._destinationstring = destination;
            return base.GetObjectFromPool();
        }
        public PairedConnection Open()
        {
            iRequestedConnectionsCount++;
            return base.GetObjectFromPool();
        }

        public void Close(PairedConnection o)
        {
            base.ReturnObjectToPool(o);
        }
        public void CloseAllConnections()
        {
            base.ExpireAllObjects();
        }
    }
}
