using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SHJ
{
    public partial class keyboard : Form
    {
        public keyboard()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)//1
        {
            Form1.keyboardstring += "1";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button2_Click(object sender, EventArgs e)//2
        {
            Form1.keyboardstring += "2";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button3_Click(object sender, EventArgs e)//3
        {
            Form1.keyboardstring += "3";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button8_Click(object sender, EventArgs e)//4
        {
            Form1.keyboardstring += "4";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button7_Click(object sender, EventArgs e)//5
        {
            Form1.keyboardstring += "5";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button6_Click(object sender, EventArgs e)//6
        {
            Form1.keyboardstring += "6";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button12_Click(object sender, EventArgs e)//7
        {
            Form1.keyboardstring += "7";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button11_Click(object sender, EventArgs e)//8
        {
            Form1.keyboardstring += "8";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button10_Click(object sender, EventArgs e)//9
        {
            Form1.keyboardstring += "9";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "1";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
        }

        private void button5_Click(object sender, EventArgs e)//0
        {
            Form1.keyboardstring += "0";
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardnum == 30)
            {
                Form1.keyboardstring = "0";
            }
            textBox1.Text = Form1.keyboardstring.TrimStart('0');
            if (textBox1.Text.Length==0)
            {
                textBox1.Text = "0";
            }
            
        }

        private void button9_Click(object sender, EventArgs e)//确定
        {
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零            
            //Form1.Modbusdata = textBox1.Text;
            //Form1.setModbusdata = true;
            this.Visible=false;
        }

        private void button4_Click(object sender, EventArgs e)//退出
        {
            Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            if (Form1.keyboardstring.Length > 0)
            {
                Form1.keyboardstring = Form1.keyboardstring.Substring(0, Form1.keyboardstring.Length - 1);//去除一个字符
                textBox1.Text = Form1.keyboardstring.TrimStart('0');
                if (textBox1.Text.Length == 0)
                {
                    textBox1.Text = "0";
                }
            }
            else if((Form1.keyboardnum == 30)||(Form1.keyboardnum == 31))
            {
                textBox1.Text = "0";
            }
            else
            {
                this.Visible = false;
            }

        }

        private void keyboard_Deactivate(object sender, EventArgs e)
        {
            Form1.keyboardnum = 0;
            textBox1.Text = Form1.keyboardstring;
            this.Visible = false;
        }

        private void keyboard_Activated(object sender, EventArgs e)
        {
            if ((Form1.keyboardnum == 30) || (Form1.keyboardnum == 31))
            {
                textBox1.Visible = true;
                textBox1.Text = Form1.keyboardstring;
            }
            else
            {
                textBox1.Visible = false;
            }
        }
    }
}
