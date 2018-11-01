using System.ComponentModel;

namespace DatabaseCopy.BusinessObjects
{
    public enum TableStatus
    {
        Pending,
        Started,
        Completed,
        Error
    }

    public class Table : INotifyPropertyChanged
    {
        private bool _selected;
        private TableStatus _status;
        private long _rowCount;
        private long _rowscompleted;
        private double _percentcomplete;
        private string _logPath;

        public Table(string name, string destschema, string srcschema)
        {
            Name = name;
            DestinationSchema = destschema;
            SourceSchema = srcschema;
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                RaiseOnPropertyvalueChanged(nameof(Selected));
            }
        }
        public TableStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                RaiseOnPropertyvalueChanged(nameof(Status));
            }
        }
        public string Name { get; }
        [Bindable(false)]
        public string LogFile
        {
            get => _logPath;
            set
            {
                _logPath = value;
                RaiseOnPropertyvalueChanged(nameof(LogFile));
            }
        }
        [Bindable(false)]
        public string DestinationSchema { get; }
        [Bindable(false)]
        public string SourceSchema { get; }
        [Bindable(false)]
        public long RowsCompleted
        {
            get => _rowscompleted;
            set
            {
                if (value != _rowscompleted)
                {
                    _rowscompleted = value;
                    PercentComplete = ((_rowCount == 0) ? 0 : (((double)_rowscompleted / (double)_rowCount)));
                }
            }
        }
        public long RowCount
        {
            get => _rowCount;
            set
            {
                _rowCount = value;
                RaiseOnPropertyvalueChanged(nameof(RowCount));
            }
        }
        public double PercentComplete
        {
            get => _percentcomplete;
            set
            {
                _percentcomplete = value;
                RaiseOnPropertyvalueChanged(nameof(PercentComplete));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropertyvalueChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}