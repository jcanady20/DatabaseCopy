using System;
using System.Windows.Forms;
using System.Drawing;

namespace DatabaseCopy
{
    public class DataGridViewPercentageColumn : DataGridViewColumn
	{
		public DataGridViewPercentageColumn()
		{
			this.CellTemplate = new DataGridViewPercentageCell();
		}
	}

	public class DataGridViewPercentageCell : DataGridViewTextBoxCell
	{
		public DataGridViewPercentageCell()
		{
			this.Style.Format = "0%";
		}
		public override Type EditType
		{
			get
			{
				return null;
			}
		}

		protected override void Paint(System.Drawing.Graphics graphics, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

			if (cellBounds.Height > 0 && cellBounds.Width > 0)
			{
				System.Drawing.Rectangle rec = new Rectangle();
				rec.X = cellBounds.X + 5;
				rec.Y = cellBounds.Y + 5;
				rec.Width = cellBounds.Width - 10;
				rec.Height = cellBounds.Height - 10;
				
				if (rec.Height > 0 && rec.Width > 0)
				{
					using (System.Drawing.Drawing2D.LinearGradientBrush lgb = new System.Drawing.Drawing2D.LinearGradientBrush(rec, System.Drawing.Color.White, System.Drawing.Color.DarkGray, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
					{
						graphics.FillRectangle(lgb, rec);
					}
					graphics.DrawRectangle(System.Drawing.Pens.DimGray, rec);

					double dbl = 0;
					if (Double.TryParse(value.ToString(), out dbl))
					{
						if (dbl > 0)
						{
							System.Drawing.Rectangle r = new Rectangle();
							r.X = cellBounds.X + 5;
							r.Y = cellBounds.Y + 5;
							r.Width = (int)((cellBounds.Width - 10) * dbl);
							r.Height = cellBounds.Height - 10;

							if (rec.Width > 0)
							{
								using (System.Drawing.Drawing2D.LinearGradientBrush lgb = new System.Drawing.Drawing2D.LinearGradientBrush(r, System.Drawing.Color.DarkGreen, System.Drawing.Color.LightGreen, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
								{
									graphics.FillRectangle(lgb, r);
								}
								graphics.DrawRectangle(System.Drawing.Pens.DimGray, r);
							}
						}
					}
				}
			}
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, DataGridViewPaintParts.None | DataGridViewPaintParts.ContentForeground);			
		}
	}
}