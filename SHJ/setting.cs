using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;

namespace SHJ
{
    public partial class setting : Form
    {
        private const int SW_HIDE = 0;  //隐藏任务栏
        private const int SW_RESTORE = 9;//显示任务栏

        [DllImport("user32.dll")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        public setting()
        {
            InitializeComponent();
        }
        
        #region Feild

        private bool cankucunguanli;//是否打开库存管理
        private bool canyingshe = false;//是否编辑映射关系
        private bool needsave = false;//是否需要保存配置
        private int recordtype;//显示的记录类型
        private int stateOK;//货道状态是否正常

        public static string helpimgaddress;

        #endregion
        
        #region Load

        private void setting_Load(object sender, EventArgs e)
        {
            

            //config1.START((Control)this, System.Reflection.Assembly.GetExecutingAssembly().GetTypes(), null);

            if (Form1.myfunctionnode.Attributes.GetNamedItem("fenbianlv").Value == "0")
            {
                this.Width = 1920;
                this.Height = 1080;
                this.Location = new Point(0, 0);
            }
            updatecaidan();
        }

        #endregion

        #region Method
        
        /// <summary>
        /// 向设备发送货道号和开始运行指令
        /// </summary>
        /// <param name="huodaorecv">货道号</param>
        public static void SendTiHuoMa(int huodaorecv)
        {
            PCHMI.CONFIG.PLC_OFF[0] = false;
            switch (huodaorecv)
            {
                case 1:
                    new PCHMI.VAR().SEND_CTRL(0, "400208", "字写入", "257");
                    break;
                case 2:
                    new PCHMI.VAR().SEND_CTRL(0, "400208", "字写入", "513");
                    break;
                case 3:
                    new PCHMI.VAR().SEND_CTRL(0, "400208", "字写入", "769");
                    break;
            }
        }

        /// <summary>
        /// 将修改保存到XML
        /// </summary>
        private void updatexml()
        {
            if (checkBox3.Checked)
            {
                Form1.paytypes |= 0x01;
            }
            else
            {
                Form1.paytypes &= 0xFFFE;
            }

            if (checkBox4.Checked)
            {
                Form1.paytypes |= 0x02;
            }
            else
            {
                Form1.paytypes &= 0xFFFD;
            }
            if (checkBox11.Checked)
            {
                Form1.paytypes |= 0x04;
            }
            else
            {
                Form1.paytypes &= 0xFFFB;
            }
            if (checkBox12.Checked)
            {
                Form1.paytypes |= 0x08;
            }
            else
            {
                Form1.paytypes &= 0xFFF7;
            }
            if (checkBox14.Checked)
            {
                Form1.paytypes |= 0x10;
            }
            else
            {
                Form1.paytypes &= 0xFFEF;
            }
            Form1.mypayconfignode.Attributes.GetNamedItem("allpay").Value = Form1.paytypes.ToString();
            Form1.mypayconfignode.Attributes.GetNamedItem("zhekou").Value = textBox5.Text;
            Form1.mynetcofignode.Attributes.GetNamedItem("ipconfig").Value = 
                textBox6.Text + "." + textBox7.Text + "." + textBox8.Text + "." + textBox9.Text;

            Form1.mynetcofignode.Attributes.GetNamedItem("port").Value = textBox3.Text;
            Form1.mynetcofignode.Attributes.GetNamedItem("netdelay").Value = textBox4.Text;
            if (checkBox8.Checked)
            {
                Form1.myfunctionnode.Attributes.GetNamedItem("netlog").Value = "1";
            }
            else
            {
                Form1.myfunctionnode.Attributes.GetNamedItem("netlog").Value = "0";
            }

            if (checkBox9.Checked)
            {
                Form1.myfunctionnode.Attributes.GetNamedItem("kucunguanli").Value = "1";
            }
            else
            {
                Form1.myfunctionnode.Attributes.GetNamedItem("kucunguanli").Value = "0";
            }

            if (checkBox5.Checked)
            {
                Form1.myfunctionnode.Attributes.GetNamedItem("adupdate").Value = "0";
            }
            else
            {
                Form1.myfunctionnode.Attributes.GetNamedItem("adupdate").Value = "1";
            }

            int i;
            for (i = 0; i < dataGridView1.Rows.Count; i++)
            {
                Form1.mynodelistshangpin[i].Attributes.GetNamedItem("jiage").Value = dataGridView1.Rows[i].Cells[1].Value.ToString();
                Form1.mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value = dataGridView1.Rows[i].Cells[2].Value.ToString();
            }

            for (i = 0; i < dataGridView2.Rows.Count; i++)
            {
                Form1.mynodelisthuodao[i].Attributes.GetNamedItem("fenzu").Value = dataGridView2.Rows[i].Cells[1].Value.ToString();
                Form1.mynodelisthuodao[i].Attributes.GetNamedItem("kucun").Value = dataGridView2.Rows[i].Cells[2].Value.ToString();
                Form1.mynodelisthuodao[i].Attributes.GetNamedItem("volume").Value = dataGridView2.Rows[i].Cells[4].Value.ToString();
                Form1.mynodelisthuodao[i].Attributes.GetNamedItem("position").Value = dataGridView2.Rows[i].Cells[5].Value.ToString();
            }
            Form1.myfunctionnode.Attributes.GetNamedItem("temperature1").Value = hScrollBar1.Value.ToString();
            Form1.myfunctionnode.Attributes.GetNamedItem("temperature2").Value = hScrollBar2.Value.ToString();
            //Form1.myfunctionnode.Attributes.GetNamedItem("touch").Value = hScrollBar4.Value.ToString()+"+"+ hScrollBar5.Value.ToString();
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        private void updatecaidan()
        {
            label28.Text = "Version:" + Form1.versionstring;
            label15.Text = "device ID:" + Encoding.ASCII.GetString(Form1.IMEI);

            comboBox9.SelectedIndex = 1;
            comboBox1.SelectedIndex = 2;

            if ((Form1.paytypes & 0x01) != 0)
            {
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Checked = false;
            }
            if ((Form1.paytypes & 0x02) != 0)
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }
            if ((Form1.paytypes & 0x04) != 0)
            {
                checkBox11.Checked = true;
            }
            else
            {
                checkBox11.Checked = false;
            }
            if ((Form1.paytypes & 0x08) != 0)
            {
                checkBox12.Checked = true;
            }
            else
            {
                checkBox12.Checked = false;
            }
            if ((Form1.paytypes & 0x10) != 0)
            {
                checkBox14.Checked = true;
            }
            else
            {
                checkBox14.Checked = false;
            }
            textBox5.Text = Form1.mypayconfignode.Attributes.GetNamedItem("zhekou").Value;
            string[] ipstring = Form1.mynetcofignode.Attributes.GetNamedItem("ipconfig").Value.Split('.');
            if (ipstring.Length == 4)
            {
                textBox6.Text = ipstring[0];
                textBox7.Text = ipstring[1];
                textBox8.Text = ipstring[2];
                textBox9.Text = ipstring[3];
            }
            textBox3.Text = Form1.mynetcofignode.Attributes.GetNamedItem("port").Value;
            textBox4.Text = Form1.mynetcofignode.Attributes.GetNamedItem("netdelay").Value;
            if (Form1.myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                checkBox8.Checked = true;
            }
            else
            {
                checkBox8.Checked = false;
            }
            if (Form1.myfunctionnode.Attributes.GetNamedItem("kucunguanli").Value == "1")
            {
                checkBox9.Checked = true;
                cankucunguanli = true;
            }
            else
            {
                checkBox9.Checked = false;
                cankucunguanli = false;
            }

            if (Form1.myfunctionnode.Attributes.GetNamedItem("adupdate").Value == "1")
            {
                checkBox5.Checked = false;
            }
            else
            {
                checkBox5.Checked = true;
            }

            textBox11.Text = Form1.mynodelistshangpin.Count.ToString();//商品数量

            hScrollBar1.Value = int.Parse(Form1.myfunctionnode.Attributes.GetNamedItem("temperature1").Value);
            hScrollBar2.Value = int.Parse(Form1.myfunctionnode.Attributes.GetNamedItem("temperature2").Value);
            string tempvalue = Form1.myfunctionnode.Attributes.GetNamedItem("touch").Value;
            //if(tempvalue.Contains("+"))
            //{
            //    hScrollBar4.Value = int.Parse(tempvalue.Substring(0,tempvalue.IndexOf("+")));
            //    hScrollBar5.Value = int.Parse(tempvalue.Substring(tempvalue.IndexOf("+")+1));
            //}
            //else
            //{
            //    hScrollBar4.Value = int.Parse(tempvalue);
            //    hScrollBar5.Value = 0;
            //}

            dataGridView1.Columns.Add("c0", "Goods");
            dataGridView1.Columns.Add("c1", "Price");
            dataGridView1.Columns.Add("c2", "CargoLane");
            dataGridView1.Columns.Add("c3", "State");
            dataGridView1.Columns.Add("c4", "Sale");
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = true;
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;

            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
            foreach (XmlNode _node in Form1.mynodelistshangpin)
            {               
                dataGridView1.Rows.Add();
                dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[0].Value = _node.Attributes.GetNamedItem("shangpinnum").Value;
                dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[1].Value = _node.Attributes.GetNamedItem("jiage").Value;
                dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[2].Value = _node.Attributes.GetNamedItem("huodao").Value;
                if (_node.Attributes.GetNamedItem("state").Value == "0")
                {
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[3].Value = "normal";
                }
                else
                {
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[3].Value = "pause";
                }
                dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[4].Value = _node.Attributes.GetNamedItem("salesum").Value;
            }
            dataGridView1.ClearSelection();

            textBox12.Text = Form1.mynodelisthuodao.Count.ToString();//货道数量
            dataGridView2.Columns.Add("c0", "CargoLane");
            dataGridView2.Columns.Add("c1", "Group No");
            dataGridView2.Columns.Add("c2", "In stock");
            dataGridView2.Columns.Add("c3", "State");
            dataGridView2.Columns.Add("c4", "Capacity");
            dataGridView2.Columns.Add("c5", "Type");
            dataGridView2.Columns[0].ReadOnly = true;
            dataGridView2.Columns[3].ReadOnly = true;
            dataGridView2.Columns[1].ReadOnly = true;
            dataGridView2.Columns[4].ReadOnly = true;
            dataGridView2.Columns[5].ReadOnly = true;

            dataGridView2.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;
            stateOK = 0;
            foreach (XmlNode _node in Form1.mynodelisthuodao)
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[0].Value = _node.Attributes.GetNamedItem("huodaonum").Value;
                dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[1].Value = _node.Attributes.GetNamedItem("fenzu").Value;
                dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[2].Value = _node.Attributes.GetNamedItem("kucun").Value;
                if (_node.Attributes.GetNamedItem("state").Value == "0")
                {
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "Normal";
                    stateOK++;
                }
                else if (_node.Attributes.GetNamedItem("state").Value == "1")
                {
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "Exceed";
                }
				else if (_node.Attributes.GetNamedItem("state").Value == "2")
                {
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "Disconnect";
                }
				else
                {
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "Fault";
                }
                dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[4].Value = _node.Attributes.GetNamedItem("volume").Value;
                dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[5].Value = _node.Attributes.GetNamedItem("position").Value;
            }
            dataGridView2.ClearSelection();
            
            showsalerecord();
            needsave = false;//开始加载的数据变化不需要保存
        }

        /// <summary>
        /// 添加销售记录
        /// </summary>
        private void showsalerecord()
        {
            double saletoday = 0;
            double salelastday = 0;
            listBox1.Items.Clear();
            int i;
            for (i = 0; i < Form1.mynodelistchuhuo.Count; i++)
            {
                if (Form1.mynodelistchuhuo[i].Attributes.GetNamedItem("start").Value == "1")
                {
                    for (int k = 1; k <= Form1.mynodelistchuhuo.Count; k++)
                    {
                        if (i - k >= 0)//未到第一条
                        {
                            string temptime = Form1.mynodelistchuhuo[i - k].Attributes.GetNamedItem("time").Value;
                            if (temptime.Length > 0)//有记录数据
                            {
                                string salerecord = temptime + " 出货:" + Form1.mynodelistchuhuo[i - k].Attributes.GetNamedItem("shangpinnum").Value
                                    + " 价格:" + Form1.mynodelistchuhuo[i - k].Attributes.GetNamedItem("jiage").Value;
                                switch (Form1.mynodelistchuhuo[i - k].Attributes.GetNamedItem("type").Value)
                                {
                                    case "0":
                                        salerecord += "元 现金";
                                        break;
                                    case "1":
                                        salerecord += "元 支付宝";
                                        break;
                                    case "2":
                                        salerecord += "元 微信";
                                        break;
                                    case "3":
                                        salerecord += "元 一码付";
                                        break;
                                    case "4":
                                        salerecord += "元 会员卡";
                                        break;
                                }
                                listBox1.Items.Add(salerecord);
                                try
                                {
                                    string tempjiage = Form1.mynodelistchuhuo[i - k].Attributes.GetNamedItem("jiage").Value;
                                    int tempmonth = int.Parse(temptime.Substring(0,2));
                                    int tempday = int.Parse(temptime.Substring(3,2));
                                    if(tempmonth==DateTime.Now.Month)//记录的是当前月
                                    {
                                        if(tempday == DateTime.Now.Day -1)//昨天
                                        {
                                            salelastday += double.Parse(tempjiage);
                                        }
                                        if (tempday == DateTime.Now.Day)//今天
                                        {
                                            saletoday += double.Parse(tempjiage);
                                        }
                                    }
                                    else if((DateTime.Now.Month==1)&&(tempmonth==12)) //记录的是上月,去年
                                    {
                                        if (tempday == DateTime.DaysInMonth(DateTime.Now.Year-1,tempmonth))//昨天
                                        {
                                            salelastday += double.Parse(tempjiage);
                                        }
                                    }
                                    else if (tempmonth == DateTime.Now.Month - 1)  //记录的是上月，今年
                                    {
                                        if (tempday == DateTime.DaysInMonth(DateTime.Now.Year, tempmonth))//昨天
                                        {
                                            salelastday += double.Parse(tempjiage);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            string temptime = Form1.mynodelistchuhuo[Form1.mynodelistchuhuo.Count + i - k].Attributes.GetNamedItem("time").Value;
                            if (temptime.Length > 0)//有记录数据
                            {
                                string salerecord = temptime + " 出货:" + Form1.mynodelistchuhuo[Form1.mynodelistchuhuo.Count + i - k].Attributes.GetNamedItem("shangpinnum").Value
                                    + " 价格:" + Form1.mynodelistchuhuo[Form1.mynodelistchuhuo.Count + i - k].Attributes.GetNamedItem("jiage").Value;
                                switch (Form1.mynodelistchuhuo[Form1.mynodelistchuhuo.Count + i - k].Attributes.GetNamedItem("type").Value)
                                {
                                    case "0":
                                        salerecord += "元 现金";
                                        break;
                                    case "1":
                                        salerecord += "元 支付宝";
                                        break;
                                    case "2":
                                        salerecord += "元 微信";
                                        break;
                                    case "3":
                                        salerecord += "元 一码付";
                                        break;
                                    case "4":
                                        salerecord += "元 会员卡";
                                        break;
                                }
                                listBox1.Items.Add(salerecord);
                                try
                                {
                                    string tempjiage = Form1.mynodelistchuhuo[Form1.mynodelistchuhuo.Count + i - k].Attributes.GetNamedItem("jiage").Value;
                                    int tempmonth = int.Parse(temptime.Substring(0, 2));
                                    int tempday = int.Parse(temptime.Substring(3, 2));
                                    if (tempmonth == DateTime.Now.Month)//记录的是当前月
                                    {
                                        if (tempday == DateTime.Now.Day - 1)//昨天
                                        {
                                            salelastday += double.Parse(tempjiage);
                                        }
                                        if (tempday == DateTime.Now.Day)//今天
                                        {
                                            saletoday += double.Parse(tempjiage);
                                        }
                                    }
                                    else if ((DateTime.Now.Month == 1) && (tempmonth == 12)) //记录的是上月,去年
                                    {
                                        if (tempday == DateTime.DaysInMonth(DateTime.Now.Year - 1, tempmonth))//昨天
                                        {
                                            salelastday += double.Parse(tempjiage);
                                        }
                                    }
                                    else if (tempmonth == DateTime.Now.Month - 1)  //记录的是上月，今年
                                    {
                                        if (tempday == DateTime.DaysInMonth(DateTime.Now.Year, tempmonth))//昨天
                                        {
                                            salelastday += double.Parse(tempjiage);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    break;
                }
            }

        }

        /// <summary>
        /// 添加支付记录
        /// </summary>
        private void showpayrecord()
        {
            listBox1.Items.Clear();
            int i;
            for (i = 0; i < Form1.mynodelistpay.Count; i++)
            {
                if (Form1.mynodelistpay[i].Attributes.GetNamedItem("start").Value == "1")
                {
                    for (int k = 1; k <= Form1.mynodelistpay.Count; k++)
                    {
                        if (i - k >= 0)//未到第一条
                        {
                            if (Form1.mynodelistpay[i - k].Attributes.GetNamedItem("time").Value.Length > 0)//有记录数据
                            {
                                string payrecord = Form1.mynodelistpay[i - k].Attributes.GetNamedItem("time").Value
                                     + " " + Form1.mynodelistpay[i - k].Attributes.GetNamedItem("type").Value;
                                if (Form1.mynodelistpay[i - k].Attributes.GetNamedItem("money").Value.StartsWith("-"))
                                {
                                    payrecord += " 退款:" + Form1.mynodelistpay[i - k].Attributes.GetNamedItem("money").Value.TrimStart('-') + "元";
                                }
                                else
                                {
                                    payrecord += " 收款:" + Form1.mynodelistpay[i - k].Attributes.GetNamedItem("money").Value.TrimStart('-') + "元";
                                }

                                listBox1.Items.Add(payrecord);
                            }
                        }
                        else
                        {
                            if (Form1.mynodelistpay[Form1.mynodelistpay.Count + i - k].Attributes.GetNamedItem("time").Value.Length > 0)//有记录数据
                            {
                                string payrecord = Form1.mynodelistpay[Form1.mynodelistpay.Count + i - k].Attributes.GetNamedItem("time").Value
                                     + " " + Form1.mynodelistpay[Form1.mynodelistpay.Count + i - k].Attributes.GetNamedItem("type").Value;
                                if (Form1.mynodelistpay[Form1.mynodelistpay.Count + i - k].Attributes.GetNamedItem("money").Value.StartsWith("-"))
                                {
                                    payrecord += " 退款:" + Form1.mynodelistpay[Form1.mynodelistpay.Count + i - k].Attributes.GetNamedItem("money").Value.TrimStart('-') + "元";
                                }
                                else
                                {
                                    payrecord += " 收款:" + Form1.mynodelistpay[Form1.mynodelistpay.Count + i - k].Attributes.GetNamedItem("money").Value.TrimStart('-') + "元";
                                }

                                listBox1.Items.Add(payrecord);
                            }
                        }
                        
                    }
                    break;
                }
            }
        }
        
        private int count1000;//1000ms计数
        /// <summary>
        /// 更新菜单状态
        /// </summary>
        private void updatemenu()
        {
            
            if (count1000 < 9)
            {
                count1000++;
            }
            else
            {
                count1000 = 0;
                Form1.guanggaoreturntime = 0;//返回广告页面计时清零
            }
            if(PEPrinterupdatedelay>0)//需要更新打印机橡胶打印线速度
            {
                if(PEPrinterupdatedelay<30)//3秒内不重复更新
                {
                    PEPrinterupdatedelay++;
                }
                else
                {
                    PEPrinterupdatedelay = 0;
                    if (hScrollBar1.Value != PEPrinter.LineSpeedValue1)
                    {
                        PEPrinter.LineSpeedValue1= (UInt16)hScrollBar1.Value;
                        PEPrinter.needPutPrintCondition = true;
                    }
                    if (hScrollBar2.Value != PEPrinter.OutputNumber)
                    {
                        PEPrinter.OutputNumber = (UInt16)hScrollBar2.Value;
                        PEPrinter.needPutPrintCondition = true;
                    }
                    //if (hScrollBar4.Value != PEPrinter.PreheatData)
                    //{
                    //    PEPrinter.PreheatData = (UInt16)hScrollBar4.Value;
                    //    PEPrinter.needPutPrintCondition = true;
                    //}
                    //if (hScrollBar5.Value != PEPrinter.PreheatLines)
                    //{
                    //    PEPrinter.PreheatLines = (UInt16)hScrollBar5.Value;
                    //    PEPrinter.needPutPrintCondition = true;
                    //}
                }
            }
            else 
            {
                //if ((hScrollBar1.Value != PEPrinter.LineSpeedValue1)&&(PEPrinter.LineSpeedValue1>= hScrollBar1.Minimum))
                //{
                //    hScrollBar1.Value = PEPrinter.LineSpeedValue1;
                //}
                //if ((hScrollBar2.Value != PEPrinter.OutputNumber) && (PEPrinter.OutputNumber >= hScrollBar2.Minimum))
                //{
                //    hScrollBar2.Value = PEPrinter.OutputNumber;
                //}
                //if ((hScrollBar4.Value != PEPrinter.PreheatData) && (PEPrinter.PreheatData >= hScrollBar4.Minimum))
                //{
                //    hScrollBar4.Value = PEPrinter.PreheatData;
                //}
                //if ((hScrollBar5.Value != PEPrinter.PreheatLines) && (PEPrinter.PreheatLines >= hScrollBar5.Minimum))
                //{
                //    hScrollBar5.Value = PEPrinter.PreheatLines;
                //}

            }

            if (Form1.istestmode)
            {
                stateOK = 0;
                for (int i = 0; i < Form1.mynodelisthuodao.Count; i++)
                {
                    if (Form1.mynodelisthuodao[i].Attributes.GetNamedItem("state").Value == "0")
                    {
                        dataGridView2.Rows[i].Cells[3].Value = "normal";
                        stateOK++;
                    }
                    else if (Form1.mynodelisthuodao[i].Attributes.GetNamedItem("state").Value == "1")
                    {
                        dataGridView2.Rows[i].Cells[3].Value = "过流";
                    }
                    else if (Form1.mynodelisthuodao[i].Attributes.GetNamedItem("state").Value == "2")
                    {
                        dataGridView2.Rows[i].Cells[3].Value = "disconnection";
                    }
                    else
                    {
                        dataGridView2.Rows[i].Cells[3].Value = "Fault";
                    }
                }
            }
            
            {
                //switch (Form1.extendstate[0]&0xfc)
                //{
                //    default:
                //        //label29.Text = "Running state:" + Form1.extendstate[0].ToString("X");
                //        break;
                //}
            }
                
            //label33.Text = "Machine state:" + Form1.extendstate[1].ToString("X");
            //label3.Text = "Restock button:" + (Form1.extendstate[1]&0x01).ToString();
            //label40.Text = "Settings button:" + ((Form1.extendstate[1] >> 1) & 0x01).ToString();
            //label35.Text = "A axis is not at zero:" + ((Form1.extendstate[1] >> 4) & 0x01).ToString();
            //label37.Text = "B axis is not at zero:" + ((Form1.extendstate[1] >> 5) & 0x01).ToString();
            //label39.Text = "Z1 axis is not at zero:" + ((Form1.extendstate[1] >> 6) & 0x01).ToString();
            //label43.Text = "PLC send frame:" + Form1.STM32Sendstr;
            //label46.Text = "PLC receive frame:" + Form1.STM32Recestr;
            //label22.Text = "Driver board sends frame:" + Form1.VMSendstr;
            //label25.Text = "Driver board receive frame:" + Form1.VMRecestr;

            switch (Form1.keyboardnum)
            {
                case 11://textBox1
                    //textBox1.Text = Form1.keyboardstring;
                    break;
                case 12://textBox2
                    //textBox2.Text = Form1.keyboardstring;
                    break;
                case 13://textBox5
                    textBox5.Text = Form1.keyboardstring;
                    break;
                case 14://textBox6
                    textBox6.Text = Form1.keyboardstring;
                    break;
                case 15://textBox7
                    textBox7.Text = Form1.keyboardstring;
                    break;
                case 16://textBox8
                    textBox8.Text = Form1.keyboardstring;
                    break;
                case 17://textBox9
                    textBox9.Text = Form1.keyboardstring;
                    break;
                case 18://textBox3
                    textBox3.Text = Form1.keyboardstring;
                    break;
                case 19://textBox4
                    textBox4.Text = Form1.keyboardstring;
                    break;
                case 20://textBox10
                    textBox10.Text = Form1.keyboardstring;
                    break;
                case 21://textBox11
                    textBox11.Text = Form1.keyboardstring;
                    break;
                case 22://dataGridView1
                    try
                    {
                        if (this.dataGridView1.CurrentCell.ColumnIndex == 1)//价格
                        {
                            this.dataGridView1.CurrentCell.Value = (double.Parse(Form1.keyboardstring) / 10).ToString("f1");
                        }
                        else
                        {
                            this.dataGridView1.CurrentCell.Value = Form1.keyboardstring;
                        }
                    }
                    catch
                    {
                        this.dataGridView1.CurrentCell.Value = "0";
                    }
                    break;
                case 23://dataGridView2
                    this.dataGridView2.CurrentCell.Value = Form1.keyboardstring;
                    break;
                case 24://textBox12
                    textBox12.Text = Form1.keyboardstring;
                    break;
                case 25://textBox13
                    textBox13.Text = Form1.keyboardstring;
                    break;
                case 26://textBox14
                    textBox14.Text = Form1.keyboardstring;
                    break;
                case 28://textBox16
                    textBox16.Text = Form1.keyboardstring;
                    break;
            }
            if (cankucunguanli)
            {
                dataGridView2.Columns[2].Visible = true;
            }
            else
            {
                dataGridView2.Columns[2].Visible = false;
            }

            if (canyingshe)
            {
                dataGridView2.Columns[1].ReadOnly = false;
                dataGridView2.Columns[4].ReadOnly = false;
                dataGridView2.Columns[5].ReadOnly = false;
                dataGridView1.Columns[2].ReadOnly = false;
            }
            else
            {
                dataGridView2.Columns[1].ReadOnly = true;
                dataGridView2.Columns[4].ReadOnly = true;
                dataGridView2.Columns[5].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
            }

            //打印机状态
            if(PEPrinter.isconnected)
            {
                label70.Text = "Connection Status:connected";
            }
            else
            {
                label70.Text = "Connection Status:not connected";
            }

            if (PEPrinter.isPEPrinterReady)
            {
                label2.Text = "Machine state:Ready";
            }
            else
            {
                label2.Text = "Machine state:not ready";
            }
            label16.Text = "Task step:" + PEPrinter.PEloopstate;
            label69.Text = "Status word:" + PEPrinter.PEPrinterState.ToString("X4");
            label61.Text = "Status description:" + PEPrinter.PEPrinterStatedetail;
            label68.Text = "Media type:" + PEPrinter.MediaType.ToString();
            label66.Text = "Print head temperature:" + (PEPrinter.HeadTemperature / 2.0).ToString("f1") + "℃";
            switch(PEPrinter.TrayCondition)
            {
                case 0x00:
                    label67.Text = "tray status:in motion";
                    label65.Text = "media state:not exist";
                    break;
                case 0x01:
                    label67.Text = "tray status:reset";
                    label65.Text = "media state:not exist";
                    break;
                case 0x02:
                    label67.Text = "tray status:pop up";
                    label65.Text = "media state:unknown";
                    break;
                case 0x04:
                    label67.Text = "tray status:in motion";
                    label65.Text = "media state:exist";
                    break;
                case 0x05:
                    label67.Text = "tray status:reset";
                    label65.Text = "media state:exist";
                    break;
                default:
                    label67.Text = "tray status:unknown" + PEPrinter.TrayCondition.ToString("X2");
                    label65.Text = "media state:unknown";
                    break;
            }
            label75.Text = "Printer number:" + PEPrinter.ProductID.ToString("X4");
            label72.Text = "F/W version:V" + (PEPrinter.Version>>8).ToString("X")+"."+ (PEPrinter.Version&0xff).ToString("X2");
            label74.Text = "Widest pixel that can be printed:" + PEPrinter.PrintableWidth.ToString();
            label73.Text = "Printable highest pixel:" + PEPrinter.PrintableHeight.ToString();
            label64.Text = "Print head resolution:" + PEPrinter.HeadResolution.ToString("f3")+ "dpi";
            label71.Text = "All printhead dots:" + PEPrinter.HeadDots.ToString();
            label63.Text = "Effective print head dots:" + PEPrinter.ValidHeadDots.ToString();
            label62.Text = "Dots per print head:" + PEPrinter.HeadBlockDots.ToString();
            label77.Text = "Rubber speed:" + PEPrinter.LineSpeed1.ToString("f1") + "ms/row";
            label76.Text = "Label speed:" + PEPrinter.LineSpeed2.ToString("f1") + "ms/row";
            label54.Text = "Rubber printing line speed setting value:" + PEPrinter.LineSpeedValue1.ToString();
            label56.Text = "Label printing speed setting:" + PEPrinter.LineSpeedValue2.ToString();
            label60.Text = "Sleep mode entry time:" + PEPrinter.SeepTransitionTime.ToString()+ "minute";
            label59.Text = "Output gray value:" + PEPrinter.OutputNumber.ToString();
            label78.Text = "Preheat output value:" + PEPrinter.PreheatData.ToString();
            label58.Text = "Warm-up execution lines:" + PEPrinter.PreheatLines.ToString();
            label57.Text = "Warm up operation STROBE signal low level:" + PEPrinter.PreheatLineStrobeLow.ToString();

            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        ///  更新广告图片和商品图片
        /// </summary>
        private void updatepic()
        {
            try
            {
                string mymima = textBox10.Text;
                DriveInfo[] s = DriveInfo.GetDrives();
                foreach (DriveInfo drive in s)
                {
                    if (drive.DriveType == DriveType.Removable)
                    {
                        if (System.IO.Directory.Exists(drive.Name + "EPTON") == false)//更新文件夹不存在
                        {
                            System.IO.Directory.CreateDirectory(drive.Name + "EPTON");
                        }
                        string[] upanfiles;
                        switch (mymima)
                        {
                            case "":
                                //更新广告
                                if (System.IO.Directory.Exists(drive.Name + "EPTON\\adimages") == false)//更新文件夹不存在
                                {
                                    System.IO.Directory.CreateDirectory(drive.Name + "EPTON\\adimages");
                                }
                                upanfiles = System.IO.Directory.GetFiles(drive.Name + "EPTON\\adimages");
                                for (int i = 0; i < upanfiles.Length; i++)//复制文件到系统
                                {
                                    int mystartindex = upanfiles[i].LastIndexOf('\\');
                                    int myendindex = upanfiles[i].LastIndexOf('.');
                                    string tempupanfile = upanfiles[i].ToLower();
                                    bool mycontainpic = tempupanfile.EndsWith(".bmp") || tempupanfile.EndsWith(".jpg")
                                        || tempupanfile.EndsWith(".png") || tempupanfile.EndsWith(".gif")
                                        || tempupanfile.EndsWith(".tif") || tempupanfile.EndsWith(".jpeg");
                                    if ((mystartindex >= 0) && (myendindex >= 0) && (mycontainpic == true))//文件名正确
                                    {
                                        string mycmname = upanfiles[i].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                                        try
                                        {
                                            int adnum = int.Parse(mycmname);
                                            if ((adnum > 0) && (adnum <= 5))//支持5个广告图片
                                            {
                                                File.Copy(upanfiles[i], Form1.adimagesaddress + "\\" + mycmname + ".jpg", true);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                    mycontainpic = tempupanfile.EndsWith(".wav") || tempupanfile.EndsWith(".mid")
                                        || tempupanfile.EndsWith(".mp4") || tempupanfile.EndsWith(".mp3")
                                        || tempupanfile.EndsWith(".mpg") || tempupanfile.EndsWith(".avi")
                                        || tempupanfile.EndsWith(".asf") || tempupanfile.EndsWith(".wmv")
                                        || tempupanfile.EndsWith(".rm") || tempupanfile.EndsWith(".rmvb");
                                    if ((mystartindex >= 0) && (myendindex >= 0) && (mycontainpic == true))//文件名正确
                                    {
                                        string mycmname = upanfiles[i].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                                        try
                                        {
                                            int adnum = int.Parse(mycmname);
                                            if ((adnum > 0) && (adnum <= 5))//支持5个广告视频
                                            {
                                                File.Copy(upanfiles[i], Form1.adimagesaddress + "\\" + mycmname + ".mp4", true);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                                Form1.adimagefiles = System.IO.Directory.GetFiles(Form1.adimagesaddress);

                                for (int i = 0; i < Form1.adimagefiles.Length; i++)//复制文件到U盘
                                {
                                    int mystartindex = Form1.adimagefiles[i].LastIndexOf('\\');
                                    string mycmname = Form1.adimagefiles[i].Substring(mystartindex);
                                    File.Copy(Form1.adimagefiles[i], drive.Name + "EPTON\\adimages" + mycmname, true);
                                }
                                //更新商品图片
                                if (System.IO.Directory.Exists(drive.Name + "EPTON\\cmimages") == false)//更新文件夹不存在
                                {
                                    System.IO.Directory.CreateDirectory(drive.Name + "EPTON\\cmimages");
                                }
                                upanfiles = System.IO.Directory.GetFiles(drive.Name + "EPTON\\cmimages");
                                for (int i = 0; i < upanfiles.Length; i++)//复制文件到系统
                                {
                                    int mystartindex = upanfiles[i].LastIndexOf('\\');
                                    int myendindex = upanfiles[i].LastIndexOf('.');
                                    string tempupanfile = upanfiles[i].ToLower();
                                    bool mycontainpic = tempupanfile.EndsWith(".bmp") || tempupanfile.EndsWith(".jpg")
                                        || tempupanfile.EndsWith(".png") || tempupanfile.EndsWith(".gif")
                                        || tempupanfile.EndsWith(".tif") || tempupanfile.EndsWith(".jpeg");
                                    if ((mystartindex >= 0) && (myendindex >= 0) && (mycontainpic == true))//文件名正确
                                    {
                                        string mycmname = upanfiles[i].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                                        try
                                        {
                                            int adnum = int.Parse(mycmname);
                                            if ((adnum > 0) && (adnum < 1000) && (mycmname.Length == 3))//支持1-999个商品图片,编码从001-999
                                            {
                                                File.Copy(upanfiles[i], Form1.cmimagesaddress + "\\" + mycmname + ".jpg", true);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                                Form1.cmimagefiles = System.IO.Directory.GetFiles(Form1.cmimagesaddress);
                                for (int i = 0; i < Form1.cmimagefiles.Length; i++)//复制文件到U盘
                                {
                                    int mystartindex = Form1.cmimagefiles[i].LastIndexOf('\\');
                                    string mycmname = Form1.cmimagefiles[i].Substring(mystartindex);
                                    File.Copy(Form1.cmimagefiles[i], drive.Name + "EPTON\\cmimages" + mycmname, true);
                                }

                                break;
                            case "999"://更新系统
                                if (System.IO.Directory.Exists(drive.Name + "EPTON\\bkimages") == false)//更新文件夹不存在
                                {
                                    System.IO.Directory.CreateDirectory(drive.Name + "EPTON\\bkimages");
                                }
                                string tempfilestr = drive.Name + "EPTON\\bkimages\\select.jpg";
                                try
                                {
                                    File.Copy(tempfilestr, Form1.bkimagesaddress + tempfilestr.Substring(tempfilestr.LastIndexOf('\\')), true);
                                }
                                catch { }
                                tempfilestr = drive.Name + "EPTON\\reg.dll";
                                try
                                {
                                    //判断注册是否正确
                                    XmlDocument mxmldoc = new XmlDocument();
                                    mxmldoc.Load(tempfilestr);
                                    UInt64 mregdata = UInt64.Parse(mxmldoc.SelectSingleNode("reg").Attributes.GetNamedItem("regid").Value);
                                    UInt64 mimeidata = 0;
                                    for (int i = 0; i < 15; i++)
                                    {
                                        mimeidata = (mimeidata << 8) + (byte)(Form1.IMEI[i] & 0x77);
                                    }
                                    if (mimeidata == mregdata)//正确复制文件
                                    {
                                        File.Copy(tempfilestr, Form1.bkimagesaddress.Remove(Form1.bkimagesaddress.LastIndexOf('\\')) + "\\reg.dll", true);
                                    }
                                }
                                catch { }
                                tempfilestr = drive.Name + "EPTON\\SHJ.exe";
                                try
                                {
                                    File.Copy(tempfilestr, Form1.bkimagesaddress.Remove(Form1.bkimagesaddress.LastIndexOf('\\')) + "\\new.exe", true);
                                }
                                catch { }
                                tempfilestr = drive.Name + "EPTON\\ADHStart.exe";
                                try
                                {
                                    File.Copy(tempfilestr, Form1.bkimagesaddress.Remove(Form1.bkimagesaddress.LastIndexOf('\\')) + "\\ADHStart.exe", true);
                                }
                                catch { }
                                break;
                        }
                    }
                }
            }
            catch (Exception myexp)
            {
                MessageBox.Show(myexp.Message);
            }
        }

        /// <summary>
        /// 检查键盘值是否正确
        /// </summary>
        private void checkkeyboardstring(int mymaxnum)
        {
            int tempkeyv;
            try
            {
                tempkeyv = Convert.ToInt32(Form1.keyboardstring, 10);
            }
            catch//货道文本非数字
            {
                Form1.keyboardstring = "0";
                tempkeyv = 0;
            }
            if (Form1.keyboardstring.Length > 1)
            {
                if (tempkeyv > mymaxnum)
                {
                    Form1.keyboardstring = Form1.keyboardstring.Substring(1).TrimStart('0');
                }
                else if (Form1.keyboardstring.Length > 2)
                {
                    Form1.keyboardstring = Form1.keyboardstring.TrimStart('0');
                }
                else if (tempkeyv == 0)
                {
                    Form1.keyboardstring = "0";
                }
            }
            else if (Form1.keyboardstring.Length == 0)
            {
                Form1.keyboardstring = "0";
            }
            needsave = true;//需要保存
        }

        #endregion

        #region Timer

        bool printTry = false;
        public bool checkPrint = false;
        private void timer1_Tick(object sender, EventArgs e)//500ms
        {
            updatemenu();
            if (checkPrint)
            {
                if (CodeEntity.M119 == 1 && !printTry)//弹出
                {
                    printTry = true;
                    PEPrinter.needMoveTray = 2;
                }
                else if (printTry && CodeEntity.M119 == 0)//归位
                {
                    printTry = false;
                    checkPrint = false;
                    PEPrinter.needMoveTray = 1;
                }
            }
        }

        #endregion

        #region FormClosing

        private void setting_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult myresult;
            if (needsave)
            {
                needsave = false;
                myresult = MessageBox.Show("The configuration has been modified, do you need to save and exit the background management?", "Hint", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (myresult == DialogResult.Yes)
                {
                    updatexml();
                    Form1.myxmldoc.Save(Form1.configxmlfile);
                    Form1.myxmldoc.Save(Form1.configxmlfilecopy);
                }
                else if (myresult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            else
            {
                myresult = MessageBox.Show("Whether to exit the background management", "Hint", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (myresult == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region TextBox
        
        private void textBox5_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox5.Text;
            Form1.keyboardnum = 13;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox6_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox6.Text;
            Form1.keyboardnum = 14;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox7_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox7.Text;
            Form1.keyboardnum = 15;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox8_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox8.Text;
            Form1.keyboardnum = 16;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox9_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox9.Text;
            Form1.keyboardnum = 17;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox3_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox3.Text;
            Form1.keyboardnum = 18;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox4_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox4.Text;
            Form1.keyboardnum = 19;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox10_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox10.Text;
            Form1.keyboardnum = 20;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox11_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox11.Text;
            Form1.keyboardnum = 21;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }
        
        private void textBox12_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox12.Text;
            Form1.keyboardnum = 24;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(508, 78);
        }

        private void textBox13_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox13.Text;
            Form1.keyboardnum = 25;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(995, 600);
        }

        private void textBox14_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox14.Text;
            Form1.keyboardnum = 26;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(995, 600);
        }

        private void textBox16_Click(object sender, EventArgs e)
        {
            Form1.keyboardstring = this.textBox16.Text;
            Form1.keyboardnum = 28;
            Form1.mykeyborad.Show();
            Form1.mykeyborad.Location = new Point(995, 600);
        }


        private void textBox5_TextChanged(object sender, EventArgs e)//非现金折扣 0-1.0
        {
            checkkeyboardstring(100);
            if (textBox5.Text.Length == 0)
            {
                textBox5.Text = "0";
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)//ip1
        {
            checkkeyboardstring(255);
            if (textBox6.Text.Length == 0)
            {
                textBox6.Text = "0";
            }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)//ip2
        {
            checkkeyboardstring(255);
            if (textBox7.Text.Length == 0)
            {
                textBox7.Text = "0";
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)//ip3
        {
            checkkeyboardstring(255);
            if (textBox8.Text.Length == 0)
            {
                textBox8.Text = "0";
            }
        }

        private void textBox9_TextChanged(object sender, EventArgs e)//ip4
        {
            checkkeyboardstring(255);
            if (textBox9.Text.Length == 0)
            {
                textBox9.Text = "0";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//port
        {
            checkkeyboardstring(65535);
            if (textBox3.Text.Length == 0)
            {
                textBox3.Text = "0";
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)//网络上报间隔
        {
            checkkeyboardstring(180);
            if (textBox4.Text.Length == 0)
            {
                textBox4.Text = "0";
            }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)//输入密码
        {
            if (Form1.keyboardstring.Length > 5)
            {
                Form1.keyboardstring = Form1.keyboardstring.Substring(1);
            }
        }

        private void textBox11_TextChanged(object sender, EventArgs e)//商品数量
        {
            checkkeyboardstring(1000);
            if (textBox11.Text.Length == 0)
            {
                textBox11.Text = "0";
            }
        }
        
        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            checkkeyboardstring(1000);
            if (textBox12.Text.Length == 0)
            {
                textBox12.Text = "0";
            }
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            checkkeyboardstring(150);
            if (textBox13.Text.Length == 0)
            {
                textBox13.Text = "0";
            }
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            checkkeyboardstring(10000);
            if (textBox14.Text.Length == 0)
            {
                textBox14.Text = "0";
            }
        }

        private void textBox16_TextChanged(object sender, EventArgs e)
        {
            checkkeyboardstring(150);
            if (textBox16.Text.Length == 0)
            {
                textBox16.Text = "0";
            }
        }

        #endregion
        
        #region CheckBox

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
            if (checkBox9.Checked)//打开库存管理
            {
                cankucunguanli = true;
            }
            else
            {
                cankucunguanli = false;
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;
        }
        
        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            needsave = true;//需要保存
        }

        #endregion

        #region DataGridView

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)//商品表格编辑一次完成
        {
            if (e.RowIndex > 0)
            {
                if (dataGridView1.Rows[e.RowIndex - 1].Cells[e.ColumnIndex].Value.ToString() == "")
                {
                    dataGridView1.Rows[e.RowIndex - 1].Cells[e.ColumnIndex].Value = "0";
                }
            }
        }

        private void dataGridView2_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                if (dataGridView2.Rows[e.RowIndex - 1].Cells[e.ColumnIndex].Value.ToString() == "")
                {
                    dataGridView2.Rows[e.RowIndex - 1].Cells[e.ColumnIndex].Value = "0";
                }
            }
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridView2.CurrentCell.ColumnIndex == 1)//分组
            {
                checkkeyboardstring(999);
            }
            else if (this.dataGridView2.CurrentCell.ColumnIndex == 2)//库存
            {
                checkkeyboardstring(255);
            }
            else if (this.dataGridView2.CurrentCell.ColumnIndex == 4)//容量
            {
                checkkeyboardstring(255);
            }
            else if (this.dataGridView2.CurrentCell.ColumnIndex == 5)//印章类型
            {
                checkkeyboardstring(3);
            }

            if (this.dataGridView2.CurrentCell.Value.ToString().Length == 0)
            {
                this.dataGridView2.CurrentCell.Value = "0";
            }
        }

        private void dataGridView2_Click(object sender, EventArgs e)
        {
            if (this.dataGridView2.CurrentCell.ReadOnly == false)
            {
                Form1.keyboardstring = this.dataGridView2.CurrentCell.Value.ToString();
                Form1.keyboardnum = 23;
                Form1.mykeyborad.Show();
                Form1.mykeyborad.Location = new Point(508, 318);
            }
        }
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentCell.ReadOnly == false)
            {
                string tempstr = this.dataGridView1.CurrentCell.Value.ToString();
                try
                {
                    if (this.dataGridView1.CurrentCell.ColumnIndex == 1)//价格
                    {
                        Form1.keyboardstring = ((int)(double.Parse(tempstr) * 10)).ToString();
                    }
                    else
                    {
                        Form1.keyboardstring = tempstr;
                    }
                }
                catch
                {
                    Form1.keyboardstring = "0";
                }
                Form1.keyboardnum = 22;
                Form1.mykeyborad.Show();
                Form1.mykeyborad.Location = new Point(508, 78);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridView1.CurrentCell.ColumnIndex == 1)//价格
            {
                checkkeyboardstring(10000);
            }
            else if (this.dataGridView1.CurrentCell.ColumnIndex == 2)//库存
            {
                checkkeyboardstring(255);
            }
            else if (this.dataGridView1.CurrentCell.ColumnIndex == 3)//映射
            {
                checkkeyboardstring(999);
            }

            if (this.dataGridView1.CurrentCell.Value.ToString().Length == 0)
            {
                this.dataGridView1.CurrentCell.Value = "0";
            }
        }

        #endregion

        #region Button

        private void button10_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                dataGridView2.Rows[i].Cells[2].Value = dataGridView2.Rows[i].Cells[4].Value;
            }
            needsave = true;
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    Form1.setextenddata = 0x01;//放印章印面
        //    //Form1.wulihuodao = int.Parse(textBox1.Text);
        //    Form1.needsetextend = true;
        //    Form1.istestmode = true;
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            //Form1.setextenddata = 0x09;//软件补货
            //Form1.needsetextend = true;
            Form1.istestmode = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Form1.setextenddata = 0x03;//停止机器
            //Form1.needsetextend = true;
            Form1.istestmode = true;
        }

        private void button3_Click(object sender, EventArgs e)//登录工程师
        {
            if (button3.Text == "Login")
            {
                if (textBox10.Text == Form1.myfunctionnode.Attributes.GetNamedItem("mima").Value)//密码正确
                {
                    canyingshe = true;
                    button4.Enabled = true;
                    button8.Enabled = true;
                    button3.Text = "Quit";
                }
                else
                {
                    canyingshe = false;
                }
            }
            else
            {
                button3.Text = "Login";
                textBox10.Text = "";
                button4.Enabled = false;
                button8.Enabled = false;
                canyingshe = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int tempnum = Convert.ToInt32(this.textBox11.Text, 10);
            XmlNode shangpinconfignode = Form1.mynodelistshangpin[0].ParentNode;
            if (tempnum > Form1.mynodelistshangpin.Count)//需要增加商品数量
            {
                for (int i = Form1.mynodelistshangpin.Count + 1; i <= tempnum; i++)
                {
                    //创建货道节点
                    XmlNode shangpinNode = Form1.myxmldoc.CreateElement("shangpin" + (i - 1).ToString());//商品定义
                    XmlAttribute shangpinnumAttribute = Form1.myxmldoc.CreateAttribute("shangpinnum");//商品编号
                    shangpinnumAttribute.Value = i.ToString("000");
                    shangpinNode.Attributes.Append(shangpinnumAttribute);//xml节点附件属性
                    XmlAttribute shangpinnameAttribute = Form1.myxmldoc.CreateAttribute("shangpinname");//对应商品名称
                    shangpinnameAttribute.Value = "";
                    shangpinNode.Attributes.Append(shangpinnameAttribute);//xml节点附件属性
                    XmlAttribute jiageAttribute = Form1.myxmldoc.CreateAttribute("jiage");//商品价格
                    jiageAttribute.Value = "1";
                    shangpinNode.Attributes.Append(jiageAttribute);//xml节点附件属性
                    XmlAttribute huodaoAttribute = Form1.myxmldoc.CreateAttribute("huodao");//货道定义
                    huodaoAttribute.Value = i.ToString();
                    shangpinNode.Attributes.Append(huodaoAttribute);//xml节点附件属性

                    XmlAttribute stateAttribute = Form1.myxmldoc.CreateAttribute("state");//货道状态
                    stateAttribute.Value = "0";//默认正常
                    shangpinNode.Attributes.Append(stateAttribute);//xml节点附件属性
                    XmlAttribute salesumAttribute = Form1.myxmldoc.CreateAttribute("salesum");//商品销售统计
                    salesumAttribute.Value = "0";//默认正常
                    shangpinNode.Attributes.Append(salesumAttribute);//xml节点附件属性

                    shangpinconfignode.AppendChild(shangpinNode);

                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[0].Value = shangpinNode.Attributes.GetNamedItem("shangpinnum").Value;
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[1].Value = shangpinNode.Attributes.GetNamedItem("jiage").Value;
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[2].Value = shangpinNode.Attributes.GetNamedItem("huodao").Value;
                    if (shangpinNode.Attributes.GetNamedItem("state").Value == "0")
                    {
                        dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[3].Value = "正常";
                    }
                    else// if (_node.Attributes.GetNamedItem("state").Value == "1")
                    {
                        dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[3].Value = "暂停";
                    }
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[4].Value = shangpinNode.Attributes.GetNamedItem("salesum").Value;
                }
            }
            else if (tempnum == 0)//商品数量为0
            {
                this.textBox11.Text = Form1.mynodelistshangpin.Count.ToString();
            }
            else if (tempnum < Form1.mynodelistshangpin.Count)//需要减少商品数量
            {
                for (int i = Form1.mynodelistshangpin.Count; i > tempnum; i--)
                {
                    shangpinconfignode.RemoveChild(Form1.mynodelistshangpin[i - 1]);
                    dataGridView1.Rows.RemoveAt(i - 1);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //updatepic();//更新图片
            try
            {
                System.Diagnostics.Process[] MyProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process MyProcess in MyProcesses)
                {
                    if (MyProcess.ProcessName.CompareTo("ADHStart") == 0)
                    {
                        MyProcess.Kill();
                    }
                }
            }
            catch { }
            try
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\upd.exe");
            }
            catch
            {

            }
            Form1.needcloseform = true;
            this.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int tempnum = Convert.ToInt32(this.textBox12.Text, 10);
            XmlNode huodaoconfignode = Form1.mynodelisthuodao[0].ParentNode;
            if (tempnum > Form1.mynodelisthuodao.Count)//需要增加货道数量
            {
                for (int i = Form1.mynodelisthuodao.Count + 1; i <= tempnum; i++)
                {
                    //创建货道节点
                    XmlNode huodaoNode = Form1.myxmldoc.CreateElement("huodao" + (i - 1).ToString());//货道定义
                    XmlAttribute huodaonumAttribute = Form1.myxmldoc.CreateAttribute("huodaonum");//货道编号
                    huodaonumAttribute.Value = i.ToString();
                    huodaoNode.Attributes.Append(huodaonumAttribute);//xml节点附件属性
                    XmlAttribute fenzuAttribute = Form1.myxmldoc.CreateAttribute("fenzu");//货道分组定义默认不分组
                    fenzuAttribute.Value = i.ToString();
                    huodaoNode.Attributes.Append(fenzuAttribute);//xml节点附件属性
                    XmlAttribute kucunAttribute = Form1.myxmldoc.CreateAttribute("kucun");//货道库存
                    kucunAttribute.Value = "255";
                    huodaoNode.Attributes.Append(kucunAttribute);//xml节点附件属性
                    XmlAttribute stateAttribute = Form1.myxmldoc.CreateAttribute("state");//货道状态
                    stateAttribute.Value = "0";//默认正常
                    huodaoNode.Attributes.Append(stateAttribute);//xml节点附件属性
                    XmlAttribute typeAttribute = Form1.myxmldoc.CreateAttribute("volume");//货道容量
                    typeAttribute.Value = "8";//默认正常
                    huodaoNode.Attributes.Append(typeAttribute);//xml节点附件属性
                    XmlAttribute positionAttribute = Form1.myxmldoc.CreateAttribute("position");//印章类型1：1010，2：2020，3：2530，其他2530
                    positionAttribute.Value = "0";//默认正常
                    huodaoNode.Attributes.Append(positionAttribute);//xml节点附件属性

                    huodaoconfignode.AppendChild(huodaoNode);

                    dataGridView2.Rows.Add();
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[0].Value = huodaoNode.Attributes.GetNamedItem("huodaonum").Value;
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[1].Value = huodaoNode.Attributes.GetNamedItem("fenzu").Value;
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[2].Value = huodaoNode.Attributes.GetNamedItem("kucun").Value;

                    if (huodaoNode.Attributes.GetNamedItem("state").Value == "0")
                    {
                        dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "正常";
                        stateOK++;
                    }
                    else if (huodaoNode.Attributes.GetNamedItem("state").Value == "1")
                    {
                        dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "过流";
                    }
                    else if (huodaoNode.Attributes.GetNamedItem("state").Value == "2")
                    {
                        dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "断线";
                    }
                    else// if (_node.Attributes.GetNamedItem("state").Value == "2")
                    {
                        dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[3].Value = "故障";
                    }
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[4].Value = huodaoNode.Attributes.GetNamedItem("volume").Value;
                    dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[5].Value = huodaoNode.Attributes.GetNamedItem("position").Value;
                }
            }
            else if (tempnum == 0)//货道数量为0
            {
                this.textBox12.Text = Form1.mynodelisthuodao.Count.ToString();
            }
            else if (tempnum < Form1.mynodelisthuodao.Count)//需要减少商品数量
            {
                for (int i = Form1.mynodelisthuodao.Count; i > tempnum; i--)
                {
                    huodaoconfignode.RemoveChild(Form1.mynodelisthuodao[i - 1]);
                    dataGridView2.Rows.RemoveAt(i - 1);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)//退出设置
        {
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Form1.setextenddata = 0x02;//取印章印面
            //Form1.needsetextend = true;
            Form1.istestmode = true;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Form1.initsalexml();
            Form1.mysalexmldoc.Save(Form1.salexmlfile);
            Form1.mysalexmldoc.Save(Form1.salexmlfilecopy);
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[4].Value = "0";
            }
            for (int i = 0; i < Form1.mynodelistshangpin.Count; i++)
            {
                Form1.mynodelistshangpin[i].Attributes.GetNamedItem("salesum").Value = "0";
            }
            needsave = true;
        }


        private void button20_Click(object sender, EventArgs e)
        {
            PEPrinter.pxSealLeftRight_try = Convert.ToInt16(textBox15.Text);
            PEPrinter.pxSealUpDown_try = Convert.ToInt16(textBox15.Text);
        }

        /// <summary>
        /// 复位打印机
        /// </summary>
        private void button28_Click(object sender, EventArgs e)
        {
            PEPrinter.needReset = true;
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        private void button25_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open the stamp to make a picture";
            openFileDialog1.InitialDirectory = Application.StartupPath+ "\\TestImages";
            openFileDialog1.ShowDialog();
            try
            {
                pictureBox1.Load(openFileDialog1.FileName);
                pictureBox1.Height = (125 * pictureBox1.Image.Height) / pictureBox1.Image.Width;
                PEPrinter.PicPath = openFileDialog1.FileName;
                PEPrinter.TYPE_STAMP mytype;
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        mytype = PEPrinter.TYPE_STAMP.TYPE_1010;
                        pictureBox2.Width = 125;
                        pictureBox2.Height = 125;
                        break;
                    case 1:
                        mytype = PEPrinter.TYPE_STAMP.TYPE_2020;
                        pictureBox2.Width = 125;
                        pictureBox2.Height = 125;
                        break;
                    case 2:
                        mytype = PEPrinter.TYPE_STAMP.TYPE_2530;
                        pictureBox2.Width = 125;
                        pictureBox2.Height = 104;
                        break;
                    default:
                        mytype = PEPrinter.TYPE_STAMP.TYPE_2530;
                        pictureBox2.Width = 125;
                        pictureBox2.Height = 104;
                        break;
                }
                Bitmap tempbitmap = PEPrinter.CreateProcessingData(PEPrinter.PicPath, mytype);
                pictureBox2.Image = tempbitmap;
                
            }
            catch
            {

            }
            
        }

        /// <summary>
        /// 打印
        /// </summary>
        private void button26_Click(object sender, EventArgs e)
        {
            PEPrinter.needPutImage = true;
        }

        /// <summary>
        /// 托盘操作
        /// </summary>
        private void button27_Click(object sender, EventArgs e)
        {
            PEPrinter.needMoveTray = comboBox9.SelectedIndex+1;
        }

        /// <summary>
        /// 托盘定位
        /// </summary>
        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                PEPrinter.TaryPosition = UInt16.Parse(textBox13.Text);
                PEPrinter.needMoveToOrigin = true;
            }
            catch
            {
                MessageBox.Show("Pallet parameter range is incorrect");
            }
        }

        /// <summary>
        /// 推送媒介
        /// </summary>
        private void button29_Click(object sender, EventArgs e)
        {
            try
            {
                PEPrinter.MediaPosition = UInt16.Parse(textBox16.Text);
                PEPrinter.MediaCoefficient = UInt16.Parse(textBox14.Text);
                PEPrinter.needPushMedia = true;
            }
            catch
            {
                MessageBox.Show("Incorrect parameter range");
            }
            
        }

        //private void button14_Click(object sender, EventArgs e)
        //{
        //    //Form1.setextenddata = 0x10;//A轴归零点
        //    //Form1.needsetextend = true;
        //    Form1.istestmode = true;
        //}

        //private void button12_Click(object sender, EventArgs e)
        //{
        //    //Form1.setextenddata = 0x11;//B轴归零点
        //    //Form1.needsetextend = true;
        //    Form1.istestmode = true;
        //}

        private void button17_Click(object sender, EventArgs e)
        {
            PCHMI.CONFIG.PLC_OFF[0] = false;
            panelBackGroud.Visible = true;
        }

        //private void button19_Click(object sender, EventArgs e)
        //{
        //    Form1.setextenddata = 0x12;//Z1轴归零点
        //    Form1.needsetextend = true;
        //    Form1.istestmode = true;
        //}

        //private void button11_Click(object sender, EventArgs e)
        //{
        //    Form1.setextenddata = 0x00;//复位机器
        //    Form1.needsetextend = true;
        //    Form1.istestmode = true;
        //}

        //private void button15_Click(object sender, EventArgs e)
        //{
        //    //测试弹簧货道
        //    //Form1.wulihuodao = int.Parse(textBox1.Text);
        //    Form1.needsetVM = true;
        //    Form1.istestmode = true;
        //}

        //private void button16_Click(object sender, EventArgs e)
        //{
        //    Form1.setextenddata = 0x04;
        //    Form1.needsetextend = true;
        //    Form1.istestmode = true;
        //}

        private void buttonGoBack_Click(object sender, EventArgs e)
        {
            PCHMI.CONFIG.PLC_OFF[0] = true;
            panelBackGroud.Visible = false;

        }

        #endregion

        #region hScrollBar

        private int PEPrinterupdatedelay;
        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            label30.Text = "Set the rubber print speed:" + hScrollBar1.Value.ToString();
            PEPrinterupdatedelay = 1;
            needsave = true;
        }
        
        private void hScrollBar2_ValueChanged(object sender, EventArgs e)
        {
            label31.Text = "Set the output gray value:" + hScrollBar2.Value.ToString();
            PEPrinterupdatedelay = 1;
            needsave = true;
        }

        //private void hScrollBar4_ValueChanged(object sender, EventArgs e)
        //{
        //    label34.Text = "Print Left and Right Correction:" + hScrollBar4.Value.ToString();
        //    PEPrinterupdatedelay = 1;
        //    needsave = true;
        //}

        //private void hScrollBar5_ValueChanged(object sender, EventArgs e)
        //{
        //    label36.Text = "print top and bottom correction:" + hScrollBar5.Value.ToString();
        //    PEPrinterupdatedelay = 1;
        //    needsave = true;
        //}

        #endregion

        #region HideButton

        private void label运行代码_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process[] MyProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process MyProcess in MyProcesses)
                {
                    if (MyProcess.ProcessName.CompareTo("ADHStart") == 0)
                    {
                        MyProcess.Kill();
                    }
                }
            }
            catch { }
            Form1.needcloseform = true;
            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_RESTORE);
            ShowWindow(FindWindow("Button", null), SW_RESTORE);
        }

        #endregion

        #region PanelLine

        private void paneld100_108_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, paneld100_108.ClientRectangle,
                    Color.White, 0, ButtonBorderStyle.Solid,
                    Color.White, 3, ButtonBorderStyle.Solid,
                    Color.White, 0, ButtonBorderStyle.Solid,
                    Color.White, 0, ButtonBorderStyle.Solid
                    );
        }

        private void panelAAndB_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelAAndB.ClientRectangle,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid
                );
        }

        private void panel抓手_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel抓手.ClientRectangle,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid
                );
        }

        private void panelZ1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelZ1.ClientRectangle,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid
                );
        }

        private void panelX_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelX.ClientRectangle,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid
                );
        }

        private void panelbtnM420_M416_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelbtnM420_M416.ClientRectangle,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid
                );
        }

        private void panelRightUp_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelRightUp.ClientRectangle,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid
                );
        }

        private void panelRightDown_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelRightDown.ClientRectangle,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid,
                Color.White, 3, ButtonBorderStyle.Solid
                );
        }

        private void panelB_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelB.ClientRectangle,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid
                );
        }

        private void panel抓手数据读取_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel抓手数据读取.ClientRectangle,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid
                );
        }

        private void panelM10Btn_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelM10Btn.ClientRectangle,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid,
                Color.White, 0, ButtonBorderStyle.Solid
                );
        }

        #endregion

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            needsave = true;
        }
        
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (recordtype == 0)
            {
                recordtype = 1;
                showpayrecord();
            }
            else
            {
                recordtype = 0;
                showsalerecord();
            }
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SendTiHuoMa(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendTiHuoMa(2);
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            SendTiHuoMa(3);
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            new PCHMI.VAR().SEND_CTRL(0, "000119", "反转", "");
            checkPrint = true;
        }

        private void btn_PortCheck_Click(object sender, EventArgs e)
        {
            new PCHMI.UpConfig().ShowDialog();//调用通信配置窗口
        }
    }
}
