using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace water
{
    class datagrid2html : IDisposable
    {
        public datagrid2html()
        {
        }

        ~datagrid2html()
        {
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public StringBuilder gv2html(DataGridView dg)
        {
            StringBuilder strB = new StringBuilder();
            //create html & table
            strB.AppendLine("<center><" +
                          "table border='1' cellpadding='0' cellspacing='0'>");
            strB.AppendLine("<tr>");
            //cteate table header
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                if (dg.Columns[i].Visible==true && !dg.Columns[i].Name.Contains("notprn"))
                strB.AppendLine("<td align='center' valign='middle'>" +
                               dg.Columns[i].HeaderText + "</td>");
            }
            //create table body
            strB.AppendLine("<tr>");
            for (int i = 0; i < dg.Rows.Count; i++)
            {
                strB.AppendLine("<tr>");
                foreach (DataGridViewCell dgvc in dg.Rows[i].Cells)
                {
                    if (dg.Columns[dgvc.ColumnIndex].Visible == true && !dg.Columns[dgvc.ColumnIndex].Name.Contains("notprn"))
                    strB.AppendLine("<td align='center' valign='middle'>" +
                                    dgvc.Value.ToString() + "</td>");
                }
                strB.AppendLine("</tr>");

            }
            //table footer & end of html file
            strB.AppendLine("</table></center>");
            return strB;
        }
    }
}
