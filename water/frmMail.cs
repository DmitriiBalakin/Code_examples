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
    public partial class frmMail : Form
    {
        public frmMail()
        {
            InitializeComponent();
        }

        private void frmMail_Shown(object sender, EventArgs e)
        {
            Email M = new Email("asu_ovk@list.ru", "water45", "pop.list.ru", 995);
            if (M.status())
            {
                label1.Text = M.m_newmess().ToString();
            }
            richTextBox1.Text =  M.Read(Convert.ToInt32(label1.Text));
            M.disconnect();

        }
    }
}
