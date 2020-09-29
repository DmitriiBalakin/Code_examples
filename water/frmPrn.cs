using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;


namespace water
{
    public partial class frmPrn : Form
    {
        string CurUrl = "";
        public frmPrn()
        {
            InitializeComponent();
            CurUrl = frmMain.UrlPrn;
            //frmMain.UrlPrn = "";
        }



        private void frmPrn_Shown(object sender, EventArgs e)
        {
            CurUrl = frmMain.UrlPrn;
            try
            {
                CurUrl = frmMain.UrlPrn;
                frmMain.UrlPrn = "";
                string Encod = "UTF-8";
                prn.Navigate(@CurUrl);
                while (prn.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                }
                prn.Document.Encoding = Encod;
                prn.Refresh();
            }
            catch
            {
                ///
            }
            finally
            {
                ///
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //PrinterSettings printer = new PrinterSettings;
            //printer.DefaultPageSettings.Landscape = true;
            //prn.ShowPrintDialog();

            prn.ShowPrintPreviewDialog();
        }
    }
}
