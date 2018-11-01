namespace DatabaseCopy.BusinessObjects
{
    public class TableCollection : SortableBindingList<Table>
    {
        public static TableCollection GetTables(string oraschema)
        {
            ConnectionManager cm = ConnectionManager.Instance;
            TableCollection tc = new TableCollection();
            PairedConnection _conn = cm.Open();
            var dt = _conn.dbExecuteGetSchema(_conn.DestinationConnection);
            for (int r = 0; r < dt.Rows.Count; r++)
            {
                if (dt.Rows[r]["Table_Type"].ToString().ToLower() == "view")
                    continue;

                Table t = new Table(dt.Rows[r]["Table_Name"].ToString(), dt.Rows[r]["Table_Schema"].ToString(), oraschema);
                t.RowCount = GetRowCount(t, _conn);
                tc.Add(t);
            }
            return tc;
        }

        private static long GetRowCount(Table table, PairedConnection conn)
        {
            long result = 0;
            var query = $"SELECT COUNT(*) FROM [{table.SourceSchema}].[{table.Name}]";
            using (var reader = conn.dbExecuteReader(conn.SourceConnection, query))
            {
                if (reader.Read())
                {
                    result = reader.GetInt64(0);
                }
            }
            return result;
        }
    }
}