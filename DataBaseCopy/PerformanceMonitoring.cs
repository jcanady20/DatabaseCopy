using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace DatabaseCopy
{
	public class PerformanceMonitoring : IDisposable
	{
		private string _ioinstance;
		private string _netinstance;
		private List<Counter> _counters;
		private bool _monitor = true;
		private PerformanceCounter dataSentCounter;
		private PerformanceCounter bandwidthCounter;
		private PerformanceCounter dataReceivedCounter;
		
		public string NetworkInstanceName
		{
			get
			{
				return _netinstance;
			}
			set
			{
				if (value != _netinstance)
				{
					_netinstance = value;
					RaiseOnInstanceNameChanged();
					AddCounters();
				}
			}
		}
		
		public string IOInstanceName
		{
			get
			{
				return _ioinstance;
			}
			set
			{
				if (value != _ioinstance)
				{
					_ioinstance = value;
					RaiseOnInstanceNameChanged();
					AddCounters();
				}
			}
		}

		public PerformanceMonitoring()
		{
			_counters = new List<Counter>();
		}
		
		public PerformanceMonitoring(string _net, string _disk) : this()
		{
			_ioinstance = _disk;
			_netinstance = _net;
			AddCounters();
		}

		private void AddCounters()
		{
			this.ClearCounters();
			_counters.Add(new Counter(new PerformanceCounter("Processor", "% Processor Time", "_Total")));
			_counters.Add(new Counter(new PerformanceCounter("PhysicalDisk", "Disk Writes/sec", _ioinstance)));
			_counters.Add(new Counter(new PerformanceCounter("NetWork Interface", "Bytes Total/sec", _netinstance)));
		}

		private void ClearCounters()
		{
			foreach (Counter c in _counters)
			{
				c.Dispose();
			}
			_counters.Clear();
		}

		public static List<string> GetInstanceNames(string strCategory)
		{
			List<string> instances = new List<string>();
			PerformanceCounterCategory pcCat = new PerformanceCounterCategory(strCategory);
			string[]  _ins = pcCat.GetInstanceNames();
			for (int i = 0; i < _ins.Length; i++)
				instances.Add(_ins[i]);

			return instances;
		}
		
		public void StartMonitor()
		{
			_monitor = true;
			AddCounters();
			Thread t = new Thread(new ThreadStart(Start));
			t.Start();
		}

		private void Start()
		{
			while (_monitor)
			{
				foreach (Counter pc in _counters)
				{
					if(pc.PrefCounter.CategoryName.ToLower() == "network interface")
						RaiseOnMonitorValueChanged(pc.PrefCounter.CategoryName, (int)GetNetworkUtilization(pc.PrefCounter.InstanceName));
					else
						RaiseOnMonitorValueChanged(pc.PrefCounter.CategoryName, pc.Current);
				}
				Thread.Sleep(1000);
			}
		}

		public void StopMonitor()
		{
			_monitor = false;
			this.ClearCounters();
		}

		public double GetNetworkUtilization(string networkCard)
		{
			const int numberOfIterations = 10;
			if(bandwidthCounter == null)
				bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
			if (dataSentCounter == null)
				dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);
			if (dataReceivedCounter == null)
				dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);

			float bandwidth = bandwidthCounter.NextValue();
			float sendSum = 0;
			float receiveSum = 0;
			for (int index = 0; index < numberOfIterations; index++)
			{
				sendSum += dataSentCounter.NextValue();
				receiveSum += dataReceivedCounter.NextValue();
			}

			return (8 * (sendSum + receiveSum)) / (bandwidth * numberOfIterations) * 100;
		}

		public event EventHandler<EventArgs> OnInstanceNameChanged;
		public void RaiseOnInstanceNameChanged()
		{
			if (OnInstanceNameChanged != null)
				OnInstanceNameChanged(this, EventArgs.Empty);
		}

		public event EventHandler<MonitorEventArgs> OnMonitorValueChanged;
		public void RaiseOnMonitorValueChanged(string name, int current)
		{
			if (OnMonitorValueChanged != null)
			{
				OnMonitorValueChanged(this, new MonitorEventArgs(name, current));
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			this.ClearCounters();
			if(bandwidthCounter != null)
				bandwidthCounter.Dispose();
			if (dataSentCounter != null)
				dataSentCounter.Dispose();
			if (dataReceivedCounter != null)
				dataReceivedCounter.Dispose();
		}

		#endregion
	}

	public class Counter : IDisposable
	{
		public int Current
		{
			get
			{
				if (PrefCounter == null)
					return 0;
				return (int)PrefCounter.NextValue();
			}
		}
		public PerformanceCounter PrefCounter;
		public Counter(PerformanceCounter c)
		{
			PrefCounter = c;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (PrefCounter != null)
				PrefCounter.Dispose();
		}

		#endregion
	}

	public class MonitorEventArgs : EventArgs
	{
		public MonitorEventArgs()
		{
		}
		public MonitorEventArgs(string name, int c)
		{
			CounterName = name;
			CurrentValue = c;
		}
		public string CounterName;
		public int CurrentValue;
	}
}