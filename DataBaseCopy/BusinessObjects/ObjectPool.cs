using System;
using System.Collections.Generic;

namespace DatabaseCopy.BusinessObjects
{
    public abstract class ObjectPool<T>
	{
		#region Class Variables
		//Hashtable of teh checked-out objects
		protected static Dictionary<T, long> locked;
		//Hasttable of available objects
		protected static Dictionary<T, long> unlocked;
		//Array List to Stored Objects Marked for Deletion
		private static List<T> MarkedForDeletion;
		//Clean-up interval
		internal static long GARBAGE_INTERVAL = 90 * 1000; //90 seconds
		#endregion
		static ObjectPool()
		{
			locked = new Dictionary<T, long>();
			unlocked = new Dictionary<T, long>();
			MarkedForDeletion = new List<T>();
		}
		internal ObjectPool()
		{
			//Create a Time to track the expired objects for cleanup.
			System.Timers.Timer aTimer = new System.Timers.Timer();
			aTimer.Enabled = true;
			aTimer.Interval = GARBAGE_INTERVAL;
			aTimer.Elapsed += new System.Timers.ElapsedEventHandler(CollectGarbage);
		}
		protected abstract T Create();
		protected abstract bool Validate(T o);
		protected abstract void Expire(T o);
		internal T GetObjectFromPool()
		{
			long now = DateTime.Now.Ticks;
			T o;
			lock(this)
			{
				foreach(KeyValuePair<T, long> myEntry in unlocked)
				{
					o = myEntry.Key;
					if(Validate(o))
					{
						unlocked.Remove(o);
						locked.Add(o,now);
						return o;
					} 
					else 
					{
						//Delete Objects out side the loop.
						MarkObjectForDeletetion(o);
						//Removed these lines due to issues with the following
						//Collection was modified; enumeration operation may not execute.
						//unlocked.Remove(o);
						//Expire(o);
					}
				}
				if(MarkedForDeletion.Count > 0)
				{
					DeleteMarkedObjects();
				}
				o = Create();
				locked.Add(o,now);
			}
			return o;
		}
		internal void ReturnObjectToPool(T o)
		{
			lock(this)
			{
				if(o != null)
				{
					locked.Remove(o);
					unlocked.Add(o, DateTime.Now.Ticks);
				}
			}
		}
		private void CollectGarbage(object sender, System.Timers.ElapsedEventArgs ea)
		{
			lock(this)
			{
				long v_now = DateTime.Now.Ticks;
				foreach (KeyValuePair<T, long> myEntry in unlocked)
				{
					long tlife = v_now - (Convert.ToInt64(myEntry.Value));
					if((unlocked.Count > 1) && (tlife > GARBAGE_INTERVAL))
					{
						MarkObjectForDeletetion(myEntry.Key);
					}
				}

				//Only Call DeleteMarkedObjects if there are items to Delete.
				if(MarkedForDeletion.Count > 0)
				{
					DeleteMarkedObjects();
				}
			}
		}
		private void MarkObjectForDeletetion(T o)
		{
			MarkedForDeletion.Add(o);
		}
		private void DeleteMarkedObjects()
		{
			lock(this)
			{
				foreach(T t in MarkedForDeletion)
				{
					unlocked.Remove(t);
					Expire(t);
				}
				MarkedForDeletion.Clear();
			}
		}
		internal void ExpireAllObjects()
		{
			List<T> tmplockObjects = new List<T>();
			foreach (KeyValuePair<T, long> myEntry in locked)
			{
				tmplockObjects.Add(myEntry.Key);
			}
			foreach(T t in tmplockObjects)
			{
				this.ReturnObjectToPool(t);
			}
			tmplockObjects.Clear();
			tmplockObjects = null;
			foreach (KeyValuePair<T, long> myEntry in unlocked)
			{
				MarkObjectForDeletetion(myEntry.Key);
			}
			if(MarkedForDeletion.Count > 0)
			{
				DeleteMarkedObjects();
			}
		}
	}
}
