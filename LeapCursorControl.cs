using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Leap;

using System.Windows;
using System.Runtime.InteropServices;

namespace MouseForm
{
    public partial class LeapCursorControl : Form
    {
        Controller cntrl = new Controller();
        LeapListener listener = new LeapListener();

        public LeapCursorControl()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            cntrl.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);
            cntrl.AddListener(listener);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cntrl.RemoveListener(listener);
            cntrl.Dispose();
        }
        private void Form1_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }


    }
}
