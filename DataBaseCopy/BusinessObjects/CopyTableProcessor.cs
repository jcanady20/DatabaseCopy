using System;
using System.Threading;

namespace DatabaseCopy.BusinessObjects
{
    public class CopyTableProcessor : IDisposable
    {
        public CopyTableProcessor()
        { }
        public CopyTableProcessor(int maxThreads)
        {
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }
        public void QueueWorkItems(TableCollection tc)
        {
            foreach (Table t in tc)
            {
                if (t.Selected)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(TaskCallBack), t);
            }
        }

        private static void TaskCallBack(object table)
        {
            var tbl = table as BusinessObjects.Table;
            if (tbl == null) return;
            var copier = new TableCopier(tbl);
            copier.Copy();
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