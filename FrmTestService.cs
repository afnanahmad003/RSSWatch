using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RESSWATCH
{
    public partial class FrmTestService : Form
    {
        public FrmTestService()
        {
            InitializeComponent();
        }

        private void FrmTestService_Load(object sender, EventArgs e)
        {
            RESSWATCHServer onj = new RESSWATCHServer();
            onj.StartServer();
        }
    }
}
