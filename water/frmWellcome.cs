using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace water
{
    public partial class frmWellcome : Form
    {

        public frmWellcome()
        {
            InitializeComponent();
        }

        private void frmWellcome_Shown(object sender, EventArgs e)
        {
            //string Encod = "CP-1251";
            wb.Navigate(@"http://10.1.1.228:8080");
            //while (wb != null && wb.ReadyState != WebBrowserReadyState.Complete)
            //{
            //    Application.DoEvents();
            //}
            //wb.Document.Encoding = Encod;
        }
    }
}
