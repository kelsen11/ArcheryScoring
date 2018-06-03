using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C_Sharp_Demo
{
    public partial class SelectBoard : Form
    {
        public SelectBoard()
        {
            InitializeComponent();
        }

        private void btnok_Click(object sender, EventArgs e)
        {
            Screen.LeftScore.ChangeBoardId(comboBox1.Text);
            Screen.Wind.ChangeBoardId(comboBox2.Text);
            Screen.RightScore.ChangeBoardId(comboBox3.Text);
            MessageBox.Show("板卡型号已经修改成功，重启软件后生效！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.Close();
        }

        private void btncancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SelectBoard_Load(object sender, EventArgs e)
        {
            comboBox1.Text = Screen.LeftScore.GetBoardNameStrFromCfg();
            comboBox2.Text = Screen.Wind.GetBoardNameStrFromCfg();
            comboBox3.Text = Screen.RightScore.GetBoardNameStrFromCfg();
        }
    }
}
