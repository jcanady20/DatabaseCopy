using System;
using System.Windows.Forms;

namespace DatabaseCopy
{
    public partial class TransferDialog : Form
    {
        #region Private Fields
        private int MaxThreads = 20;
        private int MinThreads = 1;
        private bool _sourceverified = false;
        private bool _destinverified = false;
        private BusinessObjects.ConnectionManager cm = BusinessObjects.ConnectionManager.Instance;
        private PerformanceMonitoring pm;
        private BusinessObjects.CopyTableProcessor ctp;
        #endregion

        public TransferDialog()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            pm = new PerformanceMonitoring();
            pm.OnMonitorValueChanged += Pm_OnMonitorValueChanged;
            tstxbxMaxThreads.Text = Environment.ProcessorCount.ToString();
#if DEBUG
            LoadDefaults();
#endif
        }

        private void Pm_OnMonitorValueChanged(object sender, MonitorEventArgs e)
        {
            
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void LoadDefaults()
        {
            this.txtbxDstServerName.Text = "localhost";
            this.txtbxDstDatabase.Text = "db2";
            this.txtbxDstUserName.Text = "sa";
            this.txtbxDstPassword.Text = "sa";
            this.txtbxDstSchema.Text = "dbo";

            this.txtbxSrcServerName.Text = "localhost";
            this.txtbxSrcDatabase.Text = "db1";
            this.txtbxSrcUserName.Text = "sa";
            this.txtbxSrcPassword.Text = "sa";
            this.txtbxSrcSchema.Text = "dbo";
        }

        #region Form Event Handlers
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGetTables_Click(object sender, EventArgs e)
        {
            if (_sourceverified && _destinverified)
            {
                InitConnectionManager();
                PopulateGrid();
            }
            else
            {
                MessageBox.Show("Please verify connections prior to gathering database information.");
            }
        }

        private void TransferDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pm != null)
                pm.Dispose();
            BusinessObjects.ConnectionManager.Instance.CloseAllConnections();
            if (ctp != null)
                ctp.Dispose();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex == -1)
                cntxColumnHeader.Show(dataGridView1, e.X, e.Y);

            if (e.Button == MouseButtons.Left && e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                DataGridViewCheckBoxCell chkbx = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
                if (chkbx == null)
                    return;
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = (!((bool)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value));
            }
        }

        private void clearCheckBoxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ToggleSelection(false);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ToggleSelection(true);
        }

        private void btnStartProcessing_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource != null)
            {
                PopulatePerformancedata();
                Queuetables();
            }
        }

        private void TransferDialog_Shown(object sender, EventArgs e)
        {
            PopulateProviders();
            PopulatePerformanceInsatncenames();

            // Add EventHandlers for Combo boxes after they are populated
            cmbxIOInstance.SelectedIndexChanged += new EventHandler(cmbxInstance_SelectedIndexChanged);
            cmbxNetworkInstance.SelectedIndexChanged += new EventHandler(cmbxInstance_SelectedIndexChanged);
        }

        private void cmbxSourceProvider_DisplayMemberChanged(object sender, EventArgs e)
        {

        }

        private void cmbxDestinationProvider_DisplayMemberChanged(object sender, EventArgs e)
        {

        }

        private void txtbx_TextChanged(object sender, EventArgs e)
        {
            TextBox tbx = sender as TextBox;
            if (tbx == null)
                return;
            foreach (Control c in tbx.Parent.Controls)
            {
                Button btn = c as Button;
                if (btn == null)
                    continue;
                btn.Enabled = true;
            }
            ChangeVerifyImage(false, tbx.Parent.Tag.ToString());
        }

        private void cmbxInstance_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePerformancedata();
        }

        private void btnVerifySource_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;
            string _provider = GetConnectionProvider(btn.Parent);
            System.Data.Common.DbConnectionStringBuilder cb = GetConntionStringBuilder(btn.Parent);

            System.Diagnostics.Debug.WriteLine(cb.ConnectionString);
            if (BusinessObjects.PairedConnection.VerifyConnection(cb, _provider))
            {
                MessageBox.Show("Successfully Connectioned to the server.");
                btn.Enabled = false;
                ChangeVerifyImage(true, btn.Parent.Tag.ToString());
            }
            else
            {
                MessageBox.Show("Unable to Connect to the specified Server.\r\nPlease Verify the settings and try again.");
                ChangeVerifyImage(false, btn.Parent.Tag.ToString());
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
            {
                BusinessObjects.Table t = dataGridView1.Rows[e.RowIndex].DataBoundItem as BusinessObjects.Table;
                if (t == null)
                    return;
                System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo("notepad.exe");
                si.Arguments = t.LogFile;
                System.Diagnostics.Process.Start(si);
            }
        }
        private void tstxbxMaxThreads_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void tstxbxMaxThreads_Leave(object sender, EventArgs e)
        {
            int i;
            if (Int32.TryParse(tstxbxMaxThreads.Text, out i))
            {
                if (i > MaxThreads)
                    tstxbxMaxThreads.Text = MaxThreads.ToString();
                if (i < MinThreads)
                    tstxbxMaxThreads.Text = MinThreads.ToString();
            }
        }
        #endregion

        #region UI Worker Methods
        private void PopulateProviders()
        {
            System.Data.DataTable dt1 = System.Data.Common.DbProviderFactories.GetFactoryClasses();
            // Remove Unsupported Providers
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                if (dt1.Rows[i]["InvariantName"].ToString().Contains("SqlServerCe"))
                    dt1.Rows[i].Delete();
            }
            dt1.AcceptChanges();

            this.cmbxSourceProvider.DataSource = dt1;
            this.cmbxSourceProvider.DisplayMember = "Name";
            this.cmbxSourceProvider.ValueMember = "InvariantName";

            System.Data.DataTable dt2 = dt1.Copy();
            this.cmbxDestinationProvider.DataSource = dt2;
            this.cmbxDestinationProvider.DisplayMember = "Name";
            this.cmbxDestinationProvider.ValueMember = "InvariantName";
        }

        private void PopulateGrid()
        {
            dataGridView1.SuspendLayout();
            dataGridView1.DataSource = BusinessObjects.TableCollection.GetTables(this.txtbxDstSchema.Text);
            dataGridView1.ResumeLayout();
        }

        private void InitConnectionManager()
        {
            cm.SourceConnectionString = GetConntionStringBuilder(pnlSource);
            cm.SourceProvider = GetConnectionProvider(pnlSource);

            cm.DestinationConnectionString = GetConntionStringBuilder(pnlDestination);
            cm.DestinationProvider = GetConnectionProvider(pnlDestination);
        }

        private void ToggleSelection(bool bt)
        {
            dataGridView1.SuspendLayout();
            BusinessObjects.TableCollection tc = dataGridView1.DataSource as BusinessObjects.TableCollection;
            if (tc == null)
                return;
            foreach (BusinessObjects.Table t in tc)
            {
                t.Selected = bt;
            }
            dataGridView1.ResumeLayout();
            dataGridView1.Refresh();
        }

        private void Queuetables()
        {
            BusinessObjects.TableCollection tc = dataGridView1.DataSource as BusinessObjects.TableCollection;
            if (tc == null)
                return;

            int _threads;
            if (!Int32.TryParse(tstxbxMaxThreads.Text, out _threads))
                _threads = Environment.ProcessorCount;

            if (ctp == null)
                ctp = new DatabaseCopy.BusinessObjects.CopyTableProcessor(_threads);
            ctp.QueueWorkItems(tc);
        }

        private void PopulatePerformancedata()
        {
            if (pm == null)
                pm = new PerformanceMonitoring();
            pm.StopMonitor();
            this.iousagechart.Reset();
            this.netusagechart.Reset();
            pm.IOInstanceName = this.cmbxIOInstance.Text;
            pm.NetworkInstanceName = this.cmbxNetworkInstance.Text;
            pm.StartMonitor();
        }

        private void PopulatePerformanceInsatncenames()
        {
            this.cmbxIOInstance.DataSource = PerformanceMonitoring.GetInstanceNames("PhysicalDisk");
            this.cmbxNetworkInstance.DataSource = PerformanceMonitoring.GetInstanceNames("NetWork Interface");
        }

        private System.Data.Common.DbConnectionStringBuilder GetConntionStringBuilder(Control ctl)
        {
            System.Data.Common.DbConnectionStringBuilder csb = new System.Data.Common.DbConnectionStringBuilder();
            foreach (Control c in ctl.Controls)
            {
                TextBox tb = c as TextBox;
                if (tb == null || tb.Tag == null)
                    continue;
                else
                    csb.Add(tb.Tag.ToString(), tb.Text);
            }
            return csb;
        }

        private string GetConnectionProvider(Control ctl)
        {
            string _provider = null;
            foreach (Control c in ctl.Controls)
            {
                ComboBox cbx = c as ComboBox;
                if (cbx == null || cbx.Tag == null)
                    continue;
                _provider = cbx.SelectedValue.ToString();
            }
            return _provider;
        }

        private void ChangeVerifyImage(bool verified, string SrcDest)
        {
            switch (SrcDest)
            {
                case "Source":
                    picbxSource.Image = (verified) ? Properties.Resources.SqlServer_Verified : Properties.Resources.SqlServer_NotVerified;
                    _sourceverified = verified;
                    break;
                case "Destination":
                    picbxDestination.Image = (verified) ? Properties.Resources.SqlServer_Verified : Properties.Resources.SqlServer_NotVerified;
                    _destinverified = verified;
                    break;
            }
        }
        #endregion
    }
}