using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace iTrainBilderEinfügen
{
    public partial class logWin : Form
    {
        public logWin()
        {
            InitializeComponent();
        }

        private void logWin_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing) {
                Hide();
                e.Cancel = true;
            }
        }

        public void clear()
        {
            logTB.Clear();
        }

        public void write(String text)
        {
            logTB.AppendText(text);
        }

        private void OKBtn_Click(Object sender, EventArgs e)
        {
            Hide();
        }

        private void saveBtn_Click(Object sender, EventArgs e)
        {
            using(SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.DefaultExt = "txt";
                sfd.AddExtension = true;
                sfd.Filter = "Log- und Text-Dateien |.txt";
                if(sfd.ShowDialog() == DialogResult.OK) {
                    using(StreamWriter sw = new StreamWriter(sfd.FileName)) {
                        sw.Write(logTB.Text);
                    }
                }
            }
        }
    }
}
