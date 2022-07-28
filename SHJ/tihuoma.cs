using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SHJ
{
    public partial class tihuoma : Form
    {
        public tihuoma()
        {
            InitializeComponent();
            Form1.CallGoodsInspect();
        }
        
        public static string tihuomaresult= "Please enter the delivery code";//验证结果提示语

        private void label11_Click(object sender, EventArgs e)
        {
            Form1.checktihuoma = false;//取消验证
            Form1.HMIstep = 0;//广告页面
            Form1.needupdatePlaylist = true;//需要更新播放列表
            this.DialogResult = DialogResult.No;
            this.Close();
        }     

        public static string tihuomastring;

        private void updateshow()
        {
            if(Form1.myfunctionnode.Attributes.GetNamedItem("vendortype").Value == "1")//印章打印机
            {
                tihuomaresult = tihuomaresult.Replace("Pick up", "Print");
            }
            label2.Text = tihuomaresult;
            if(Form1.isICYOK)
            {
                this.label10.ForeColor = System.Drawing.SystemColors.HighlightText;
            }
            else
            {
                this.label10.ForeColor = System.Drawing.Color.Red;
            }
            this.label10.Text = "Number:" + Encoding.ASCII.GetString(Form1.IMEI) + "  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (Form1.checktihuoma)//需要验证提货码
            {
                pictureBox1.Visible = true;
            }
            else
            {
                pictureBox1.Visible = false;
            }
            System.Windows.Forms.Application.DoEvents();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            updateshow();
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.TextLength;
        }

        #region Keyboard

        private void button13_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if(textBox1.Text.Length<7)//提货码七位
            {
                textBox1.Text += "1";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "2";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "3";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "4";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "5";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "6";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "7";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "8";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "9";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length < 7)//提货码七位
            {
                textBox1.Text += "0";
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button4_Click(object sender, EventArgs e)//清除
        {
            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length>0)
            {
                textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);//去除一个字符
            }
            tihuomaresult = "Please enter the delivery code";
            Form1.guanggaoreturntime = 0;
        }

        private void button9_Click(object sender, EventArgs e)//确认提货
        {
            if (SummaryCheck())
            {
                textBox1.Text = "";
                return;
            }

            this.label2.Focus();//获取焦点
            if (textBox1.Text.Length == 7)//提货码小于七位
            {
                Form1.myTihuomastr = textBox1.Text;
                Form1.checktihuoma = true;//需要验证提货码
                tihuomaresult = "Verifying pickup code";
            }
            else
            {
                tihuomaresult = "Wrong pickup code length";
            }
            Form1.guanggaoreturntime = 0;
        }

        #endregion

        private void tihuoma_Load(object sender, EventArgs e)
        {
            panelTest.Visible = false;
            tihuoma.tihuomaresult = "Please enter the delivery code";
        }

        private void label10_DoubleClick(object sender, EventArgs e)
        {
            Form1.needopensettingform = true;
        }

        #region Test Run

        private string imageFilePath;
        private int cargoWayNum;
        private void btnTry_Click(object sender, EventArgs e)
        {
            panelTest.BackColor = Color.FromArgb(98, Color.White);
            panelTest.Visible = true;
        }

        private void FileChose()
        {
            string filePath = Application.StartupPath + "\\TestImages";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = filePath;
            openFileDialog.Filter = "jpg|*.JPG";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageFilePath = openFileDialog.FileName;
                ShowIamge();
            }

        }

        private void ShowIamge()
        {
            picPrintImage.Image = Image.FromFile(imageFilePath);
            picPrintImage.SizeMode = PictureBoxSizeMode.Zoom;
            picPrintImage.BorderStyle = BorderStyle.None;

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (SummaryCheck())
                return;
            if (String.IsNullOrEmpty(cmbCargoWay.Text))
            {
                MessageBox.Show("Please select a cargo channel","Remind");
            }
            else if (String.IsNullOrEmpty(imageFilePath))
            {
                MessageBox.Show("Please select the pattern","Remind");
            }
            else
            {
                cargoWayNum = cmbCargoWay.SelectedIndex + 1;
                setting.SendTiHuoMa(cargoWayNum);
                Form1.CallWorkingTest(cargoWayNum, imageFilePath);
                this.Dispose();
                this.Close();
            }
        }

        private void btnPicChoice_Click(object sender, EventArgs e)
        {
            FileChose();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            panelTest.Visible = false;
        }

        /// <summary>
        /// 检查设备是否连接
        /// </summary>
        /// <returns>true or false</returns>
       private bool CheckPortConnect()
        {
            bool callback = false;
            string[] gcom = System.IO.Ports.SerialPort.GetPortNames();
            if(gcom.Length>0)
            {
                callback = true;
            }
            return callback;
        }
        /// <summary>
        /// 对设备和打印机进行检查
        /// </summary>
        /// <returns>true：设备或打印机故障</returns>
        private bool SummaryCheck()
        {
            if (Form1.CallError())//打印机检查 
            {
                textBox1.Text = "";
                return true;
            }
            if (!CheckPortConnect())//设备连接检查
            {
                textBox1.Text = "";
                MessageBox.Show("The machine is not connected, please check the connection or restart the device", "Error");
                return true;
            }
            if (Form1.CallMachineError())//机器故障检查
            {
                MessageBox.Show("Setup failure！Enter the background program to view the details", "Fault", MessageBoxButtons.OK);
                return true;
            }
            if (Form1.CallGoodsInspect())//印面数量检查
            {
                textBox1.Text = "";
                MessageBox.Show("Out of stock in the cargo lane", "Remind");
                return true;
            }
            else
                return false;
        }
        
        #endregion
        
    }
}
