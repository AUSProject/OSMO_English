using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Ibms.Net.TcpCSFramework;
using ThoughtWorks.QRCode.Codec;    
using System.Threading.Tasks;

namespace SHJ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public extern static void ShowCursor(int status);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern bool GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 写入ini文件
        /// </summary>
        /// <param name="section">名称</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="path">路径</param>
        private void IniWriteValue(string section, string key, string value, string path)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">项目名称</param>
        /// <param name="key">键</param>
        /// <param name="path">路径</param>
        private string IniReadValue(string section, string key, string path)
        {
            StringBuilder temp = new StringBuilder(6000);
            GetPrivateProfileString(section, key, "error", temp, 6000, path);
            return temp.ToString();
        }

        #region Form1MethodCallBack

        private static Form1 nowform1;
        /// <summary>
        /// 模拟运行
        /// </summary>
        /// <param name="num">货道号</param>
        /// <param name="path">图片地址</param>
        public static void CallWorkingTest(int num, string path)
        {
            if (nowform1 != null)
            {
                nowform1.WorkingTest(num, path);
            }
        }
        /// <summary>
        /// 检测打印机是否连接
        /// None为连接,ok为未连接
        /// </summary>
        /// <returns></returns>
        public static bool CallError()
        {
            if (nowform1 != null)
            {
                return nowform1.PrintErrorInspect();
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 设备故障代码检测
        /// </summary>
        public static bool CallMachineError()
        {
            if (nowform1 != null)
                return nowform1.MachineErrorInspect();
            else
                return false;
        }

        public static bool CallGoodsInspect()
        {
            if (nowform1 != null)
                return nowform1.GoodsInspect();
            else
                return true;
        }

        #endregion

        #region Feild

        //private static int shouyao=0;//是否是售药机0tongyong1shouyao2shuangkaishouyao

        private string imageUrlFile;//图片下载地址文件夹

        public static bool needcloseform = false;//是否需要关闭窗体
        public static int HMIstep;//界面页面：0广告 1触摸选择商品 2支付页面
        private int BUYstep;//购买步骤0等待输入商品编号，1货道故障，2库存不足，3编号不正确
        private const int RXTXBUFLEN = 512;
        private const int GSMRXTXBUFLEN = 1500;
        private setting mysetting;//设置窗口

        private int guanggaoindex = 0;//广告文件夹中图片索引号
        public static string adimagesaddress;//广告图片路径
        public static string bkimagesaddress;//背景图片路径
        public static string cmimagesaddress;//商品图片路径
        public static string bcmimagesaddress;//商品图片路径
        public static string usedbcmimagesaddress;//已经提货的打印图片
        public static string dataaddress;
        public static FileStream netdatastream;
        public static string[] adimagefiles;//广告图片名
        public static bool needupdatePlaylist;//是否需要更新播放列表
        public static string[] cmimagefiles;//商品图片名
        public static string[] bcmimagefiles;//商品图片名
        private int cmnumsinpage = 12;//每页商品列表的个数
        private int cmliststotal, cmlistnum;//商品图片列表总页数和当前页数
        public static string configxmlfile;//配置文件名
        public static string salexmlfile;//销售记录文件名
        public static string configxmlfilecopy;//配置文件名
        public static string salexmlfilecopy;//销售记录文件名
        public static string PLCxmlfile;//PLC配置文件名
        private string regxmlfile;//注册文件名
        public static XmlDocument myxmldoc = new XmlDocument();//配置文件XML
        public static XmlNodeList mynodelistshangpin;//商品列表
        public static XmlNodeList mynodelisthuodao;//货道列表
        public static XmlNode mynetcofignode;//网络配置
        public static XmlNode myfunctionnode;//功能配置
        public static XmlNode mypayconfignode;//支付配置
        public static XmlDocument mysalexmldoc = new XmlDocument();//销售记录配置文件XML
        public static XmlNodeList mynodelistchuhuo;//销售记录
        public static XmlNodeList mynodelistpay;//支付记录
        //public static XmlDocument PLCxmldoc = new XmlDocument();//PLC配置文件XML
        //public static XmlNodeList PLCnodelistbitdata;//PLC数据列表
        //public static XmlNodeList PLCnodelistworddata;//PLC数据列表
        //public static bool PLCdataUpdated=false;//已经获取PLC数据
        //public static int Modbusdataaddr;//需要更新的PLC数据地址
        //public static string Modbusdata="";//需要更新的PLC数据
        //public static bool setModbusdata=false;//需要更新PLC数据

        public static string localsalerID = "";//本机商家号
        public static string vendortype = "0";//机器类型
        //public static string PLCPORT = "COM1";
        //public static string VMPORT = "COM2";
        private XmlDocument myregxmldoc = new XmlDocument();//注册配置文件XML
        private bool isregedit = false;//是否已经注册
        public static int guanggaoreturntime;//返回广告页面计时。3分钟不操作，则返回广告页面
        private int MAXreturntime = 120;
        private QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
        private PEPrinter myprint;

        private double netpaymoney;//需要网络支付的金额
        private string paystring;//支付信息
        public static bool renewpaystate = false;//重新开始支付状态，计时清零，二维码清除
        private int nowpaytype;//支付方式1现金2支付宝3微信4一码付5银联闪付6会员卡7提货码
        public static int paytypes;//第一位为支付宝、第二位为微信、第三位为一码付、第四位为银联闪付、第五位为会员卡
        private int defaultpaytype;//默认支付方式 

        //GPRS变量
        private byte[] GSMRxBuffer;  //GSM发送缓冲区
        private byte[] GSMTxBuffer = new byte[GSMRXTXBUFLEN];  //GSM接收缓冲区
        private byte[,] timerecord = new byte[4, 6];//二维码时间戳+下发印章图片时间戳
        private byte[,] netsendrecord = new byte[210, 34];//缓存200条
        private int netsendindex, netsendrecordindex, needsendrecordnum;//发送和缓存序号已经需要发送的条数
        private int netstep;//网络状态 0表示空闲 1等待返回2表示发送状态数据 3表示发送交易数据 4表示发送支付宝二维码请求数据5表示发送微信二维码请求数据
        private int lastnetstep;//上次网络状态
        private TcpCli myTcpCli = new TcpCli(new Coder(Coder.EncodingMothord.Unicode));//定义网络客户端
        public static bool isICYOK = false;//与服务器握手成功

        private string netstring;//网络二维码信息
        private string ipAddress;//服务器IP地址 
        private int netport;//服务器网络端口号
        private string myMAC;//MAC地址
        public static byte[] IMEI = new byte[15];//设备唯一号
        public static string versionstring = "ADH816AZV3.2.09";
        private string[] qrcodestring = new string[2];//支付宝和微信二维码字符串
        private int[] liushui = new int[2];
        private int liushuirecv;//接收到的流水号
        private int huodaorecv;//接收到的商品编号
        public static string myTihuomastr = "";//输入的7位提货码
        public static bool checktihuoma;//需要验证提货码
        public static string showprintstate;//制作过程状态显示
        public static string showprinttime;//制作过程倒计时显示
        public static string pictureaddr;//打印图片地址
        public static int OSMOtype;


        public static string keyboardstring = "";//键盘输入值
        public static int keyboardnum;//键盘输入对应的文本框编号
        public static keyboard mykeyborad = new keyboard();

        private int huohao;//商品货号
        public static int wulihuodao;//物理货道号
        private double shangpinjiage;
        private double maxprice = 0;//商品最高价格
        private int zhifutype;//0现金1支付宝2微信3一码付4提货码
        private int totalshangpinnum = 16;//显示的商品总数
        private int totalhuodaonum = 16;//显示的货道总数
        private int isextbusy;//扩展板是否正忙0表示空闲，1正在出货，2出货完成需要确认


        private int Aisleoutcount;//电机输出超时计时
        public static int tempAisleNUM;//商品货号选择
        public static bool istestmode;//测试出货模式

        //private byte[] STM32TXBUF = new byte[RXTXBUFLEN];//扩展板发送缓冲区
        //private byte[] STM32RXBUF = new byte[RXTXBUFLEN];//扩展板接收缓冲区

        //private byte[] VMTXBUF = new byte[RXTXBUFLEN];//扩展板发送缓冲区
        //private byte[] VMRXBUF = new byte[RXTXBUFLEN];//扩展板接收缓冲区
        //private ushort VMRXcount;//接收数量

        #endregion

        #region Crc
        private UInt16[] CrcTbl =
        {
            0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
            0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
            0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
            0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
            0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
            0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
            0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
            0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
            0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
            0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
            0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
            0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
            0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
            0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
            0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
            0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
            0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
            0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
            0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
            0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
            0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
            0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
            0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
            0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
            0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
            0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
            0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
            0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
            0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
            0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
            0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
            0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
        };

        //查表算法CRC-16
        UInt16 crcVal(byte[] pcMess, UInt16 wLen)
        {
            int i = 0;
            UInt16 nCRCData, Index = 0;
            nCRCData = 0xffff;
            while (wLen > 0)
            {
                wLen--;
                Index = (UInt16)(nCRCData >> 8);
                Index = (UInt16)(Index ^ (pcMess[i++] & 0x00ff));
                nCRCData = (UInt16)((nCRCData ^ CrcTbl[Index]) & 0x00ff);
                nCRCData = (UInt16)((nCRCData << 8) | (CrcTbl[Index] >> 8));
            }
            return (UInt16)(nCRCData >> 8 | nCRCData << 8);
        }

        #endregion

        #region  Load

        private void Form1_Load(object sender, EventArgs e)
        {
            nowform1 = this;

            config1.START((Control)this, System.Reflection.Assembly.GetExecutingAssembly(), null);

            this.panel1.Dock = DockStyle.Fill;
            this.panel2.Dock = DockStyle.Fill;
            this.panel3.Dock = DockStyle.Fill;
            this.panel4.Dock = DockStyle.Fill;

            imageUrlFile = Directory.GetCurrentDirectory() + "imageUrl.ini";

            adimagesaddress = System.IO.Directory.GetCurrentDirectory() + "\\adimages";
            bkimagesaddress = System.IO.Directory.GetCurrentDirectory() + "\\bkimages";
            cmimagesaddress = System.IO.Directory.GetCurrentDirectory() + "\\cmimages";
            bcmimagesaddress = System.IO.Directory.GetCurrentDirectory() + "\\bcmimages";
            usedbcmimagesaddress = bcmimagesaddress + "\\used";
            configxmlfile = System.IO.Directory.GetCurrentDirectory() + "\\app.dat";
            salexmlfile = System.IO.Directory.GetCurrentDirectory() + "\\sale.dat";
            configxmlfilecopy = System.IO.Directory.GetCurrentDirectory() + "\\app.xml";
            salexmlfilecopy = System.IO.Directory.GetCurrentDirectory() + "\\sale.xml";
            PLCxmlfile = System.IO.Directory.GetCurrentDirectory() + "\\PLCdata.xml";
            dataaddress = System.IO.Directory.GetCurrentDirectory() + "\\netdata";
            regxmlfile = "C:\\flexlm\\regEPTON.dll";
            if (System.IO.Directory.Exists(adimagesaddress) == false)//广告文件夹不存在
            {
                System.IO.Directory.CreateDirectory(adimagesaddress);
            }
            if (System.IO.Directory.Exists(bkimagesaddress) == false)//背景文件夹不存在
            {
                System.IO.Directory.CreateDirectory(bkimagesaddress);
            }
            if (System.IO.Directory.Exists(cmimagesaddress) == false)//商品文件夹不存在
            {
                System.IO.Directory.CreateDirectory(cmimagesaddress);
            }
            if (System.IO.Directory.Exists(bcmimagesaddress) == false)//商品文件夹不存在
            {
                System.IO.Directory.CreateDirectory(bcmimagesaddress);
            }
            if (System.IO.Directory.Exists(usedbcmimagesaddress) == false)//广告文件夹不存在
            {
                System.IO.Directory.CreateDirectory(usedbcmimagesaddress);
            }
            if (System.IO.Directory.Exists("C:\\flexlm") == false)//注册文件夹不存在
            {
                System.IO.Directory.CreateDirectory("C:\\flexlm");
            }
            if (System.IO.Directory.Exists(dataaddress) == false)//netdata文件夹不存在
            {
                System.IO.Directory.CreateDirectory(dataaddress);
            }
            if (!File.Exists(imageUrlFile))//图片路径文件
            {
                File.Create(imageUrlFile);
            }

            if (System.IO.File.Exists(configxmlfile))
            {
                try
                {
                    myxmldoc.Load(configxmlfile);
                    myxmldoc.Save(configxmlfilecopy);
                }
                catch
                {
                    if (System.IO.File.Exists(configxmlfilecopy))
                    {
                        try
                        {
                            myxmldoc.Load(configxmlfilecopy);
                            myxmldoc.Save(configxmlfile);
                        }
                        catch
                        {
                            initconfigxml();
                            myxmldoc.Save(configxmlfile);
                            myxmldoc.Save(configxmlfilecopy);
                        }
                    }
                }
            }
            else if (System.IO.File.Exists(configxmlfilecopy))
            {
                try
                {
                    myxmldoc.Load(configxmlfilecopy);
                    myxmldoc.Save(configxmlfile);
                }
                catch
                {
                    initconfigxml();
                    myxmldoc.Save(configxmlfile);
                    myxmldoc.Save(configxmlfilecopy);
                }
            }
            else
            {
                initconfigxml();
                myxmldoc.Save(configxmlfile);
                myxmldoc.Save(configxmlfilecopy);

            }

            if (System.IO.File.Exists(salexmlfile))
            {
                try
                {
                    mysalexmldoc.Load(salexmlfile);
                    mysalexmldoc.Save(salexmlfilecopy);
                }
                catch
                {
                    if (System.IO.File.Exists(salexmlfilecopy))
                    {
                        try
                        {
                            mysalexmldoc.Load(salexmlfilecopy);
                            mysalexmldoc.Save(salexmlfile);
                        }
                        catch
                        {
                            initsalexml();
                            mysalexmldoc.Save(salexmlfile);
                            mysalexmldoc.Save(salexmlfilecopy);
                        }
                    }
                }
            }
            else if (System.IO.File.Exists(salexmlfilecopy))
            {
                try
                {
                    mysalexmldoc.Load(salexmlfilecopy);
                    mysalexmldoc.Save(salexmlfile);
                }
                catch
                {
                    initsalexml();
                    mysalexmldoc.Save(salexmlfile);
                    mysalexmldoc.Save(salexmlfilecopy);
                }
            }
            else
            {
                initsalexml();
                mysalexmldoc.Save(salexmlfile);
                mysalexmldoc.Save(salexmlfilecopy);
            }
            
            if (System.IO.File.Exists(regxmlfile))//加载注册文件
            {
                myregxmldoc.Load(regxmlfile);
            }
            else
            {
                initregxml();
                myregxmldoc.Save(regxmlfile);
            }
            try
            {
                updatenodeaddress();
                InitFormsize();
                //ShowCursor(0);//关闭鼠标
                HMIstep = 0;//触摸选货界面

                adimagefiles = System.IO.Directory.GetFiles(adimagesaddress);//广告页面图片文件路径列表
                if (adimagefiles != null)
                {
                    if (guanggaoindex >= adimagefiles.Length)
                    {
                        guanggaoindex = 0;
                    }

                    bool ispicture = adimagefiles[guanggaoindex].EndsWith(".bmp") || adimagefiles[guanggaoindex].EndsWith(".jpg")
                                    || adimagefiles[guanggaoindex].EndsWith(".png") || adimagefiles[guanggaoindex].EndsWith(".gif")
                                    || adimagefiles[guanggaoindex].EndsWith(".tif") || adimagefiles[guanggaoindex].EndsWith(".jpeg");//是否是图片
                    if (ispicture)//是图片
                    {
                        this.pictureBox1.Load(adimagefiles[guanggaoindex]);
                    }
                }

                cmimagefiles = System.IO.Directory.GetFiles(cmimagesaddress);//选择商品图片文件路径列表
                bcmimagefiles = System.IO.Directory.GetFiles(bcmimagesaddress);//选择商品图片文件路径列表
                string str1;
                for (int i = 0; i < cmimagefiles.Length; i++)//文件名称排序
                {
                    for (int j = cmimagefiles.Length - 1; j > i; j--)
                    {
                        if (cmimagefiles[j].CompareTo(cmimagefiles[j - 1]) < 0)
                        {
                            str1 = cmimagefiles[j];
                            cmimagefiles[j] = cmimagefiles[j - 1];
                            cmimagefiles[j - 1] = str1;
                        }
                    }
                }

                try
                {
                    PictureBox mtemppicturebox = new PictureBox();
                    mtemppicturebox.Load(bkimagesaddress + "\\select.jpg");//选择商品背景
                    panel2.BackgroundImage = mtemppicturebox.Image;//为了能在程序中更新背景图片，必须释放图片文件
                    panel3.BackgroundImage = mtemppicturebox.Image;//为了能在程序中更新背景图片，必须释放图片文件
                }
                catch
                {
                }

                for (int i = 0; i < cmimagefiles.Length; i++)//商品触摸列表
                {

                    int mystartindex = cmimagefiles[i].LastIndexOf('\\');
                    int myendindex = cmimagefiles[i].LastIndexOf('.');
                    bool mycontainpic = cmimagefiles[i].EndsWith(".bmp") || cmimagefiles[i].EndsWith(".jpg")
                        || cmimagefiles[i].EndsWith(".png") || cmimagefiles[i].EndsWith(".gif")
                        || cmimagefiles[i].EndsWith(".tif") || cmimagefiles[i].EndsWith(".jpeg");
                    string mycmname = cmimagefiles[i].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                    bool hasshangpinnum = false;
                    for (int j = 0; j < mynodelistshangpin.Count; j++)//查找是否有配置数据
                    {
                        if (mynodelistshangpin[j].Attributes.GetNamedItem("shangpinnum").Value == mycmname)
                        {
                            hasshangpinnum = true;
                            break;
                        }
                    }
                    if ((mystartindex >= 0) && (myendindex >= 0) && (mycontainpic == true) && (hasshangpinnum == true))//文件名正确
                    {
                        totallistnums++;
                    }
                }
                if (totallistnums > 0)
                {
                    cmliststotal = (totallistnums - 1) / cmnumsinpage + 1;//总共页数
                }
                updateshangpinlist(0);//显示第一页

                if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
                {
                    dataaddress += "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
                    netdatastream = System.IO.File.Create(dataaddress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)//查找有线网
            {
                if ((!ni.Description.Contains("Wireless")) && (ni.Description.Contains("Realtek PCIe")) && (!ni.GetPhysicalAddress().ToString().Equals("")))
                {
                    myMAC = ni.GetPhysicalAddress().ToString().ToUpper();
                }
            }
            if (myMAC == null)
            {
                foreach (NetworkInterface ni in interfaces)//查找无线网络
                {
                    if ((ni.Description.Contains("Wireless")) && (!ni.GetPhysicalAddress().ToString().Equals("")))
                    {
                        myMAC = ni.GetPhysicalAddress().ToString().ToUpper();
                    }
                }
            }
            if (myMAC != null)
            {
                byte[] mbyteMAC = Encoding.ASCII.GetBytes(myMAC);
                byte adddata = 0;
                for (int i = 0; i < 12; i++)
                {
                    adddata += mbyteMAC[i];
                    IMEI[i] = mbyteMAC[i];
                }
                byte[] mbyteadddata = Encoding.ASCII.GetBytes(adddata.ToString("000"));
                IMEI[12] = mbyteadddata[0];
                IMEI[13] = mbyteadddata[1];
                IMEI[14] = mbyteadddata[2];
            }
            //判断是否注册
            UInt64 mregdata = UInt64.Parse(myregxmldoc.SelectSingleNode("reg").Attributes.GetNamedItem("regid").Value);
            UInt64 mimeidata = 0;
            for (int i = 0; i < 15; i++)
            {
                mimeidata = (mimeidata << 8) + (byte)(IMEI[i] & 0x77);
            }
            if (mimeidata != mregdata)
            {
                isregedit = false;
            }
            else
            {
                isregedit = true;
            }

            myprint = new PEPrinter();

            //setextenddata = 0x00;//复位PLC
            //needsetextend = true;

            myTcpCli.ReceivedDatagram += new NetEvent(myTcpCli_ReceivedDatagram);
            myTcpCli.DisConnectedServer += new NetEvent(myTcpCli_DisConnectedServer);
            myTcpCli.ConnectedServer += new NetEvent(myTcpCli_ConnectedServer);

            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeScale = 5;
            qrCodeEncoder.QRCodeVersion = 8;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;

        }

        #endregion

        #region Timer

        #region Timer1

        private int netcount;//网络状态循环计时
        private int netreturncount;//网络等待计时
        private int myminute;//分钟计时
        private void timer1_Tick(object sender, EventArgs e)//1s
        {

            if (myminute < 59)//最长1分钟，循环时间
            {
                myminute++;
            }
            else
            {
                myminute = 0;
            }
            if (guanggaoreturntime < 600)//最长计时10分钟
            {
                guanggaoreturntime++;
            }

            switch (HMIstep)
            {
                case 0:
                    //刷新广告页面
                    if (needupdatePlaylist)
                    {
                        needupdatePlaylist = false;
                        axWindowsMediaPlayer1.currentPlaylist.clear();
                        //检测播放列表是否更新
                        for (int i = 0; i < adimagefiles.Length; i++)//广告文件名列表
                        {

                            int mystartindex = adimagefiles[i].LastIndexOf('\\');
                            int myendindex = adimagefiles[i].LastIndexOf('.');
                            bool myisvideo = adimagefiles[i].EndsWith(".wav") || adimagefiles[i].EndsWith(".mid")
                                || adimagefiles[i].EndsWith(".mp4") || adimagefiles[i].EndsWith(".mp3")
                                || adimagefiles[i].EndsWith(".mpg") || adimagefiles[i].EndsWith(".avi")
                                || adimagefiles[i].EndsWith(".asf") || adimagefiles[i].EndsWith(".wmv")
                                || adimagefiles[i].EndsWith(".rm") || adimagefiles[i].EndsWith(".rmvb");
                            if ((mystartindex >= 0) && (myendindex >= 0) && (myisvideo == true))//文件名正确
                            {
                                axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(adimagefiles[i]));
                            }
                        }
                        if (axWindowsMediaPlayer1.currentPlaylist.count > 0)//有播放文件
                        {
                            axWindowsMediaPlayer1.Visible = true;
                            axWindowsMediaPlayer1.Ctlcontrols.play();
                        }
                        else
                        {
                            axWindowsMediaPlayer1.Visible = false;
                            axWindowsMediaPlayer1.Ctlcontrols.stop();
                        }
                    }
                    if (guanggaoreturntime >= 3)
                    {
                        guanggaoreturntime = 0;

                        try
                        {
                            //播放图片
                            if (adimagefiles != null)
                            {
                                if (guanggaoindex >= adimagefiles.Length)
                                {
                                    guanggaoindex = 0;
                                }
                                bool ispicture = adimagefiles[guanggaoindex].EndsWith(".bmp") || adimagefiles[guanggaoindex].EndsWith(".jpg")
                                    || adimagefiles[guanggaoindex].EndsWith(".png") || adimagefiles[guanggaoindex].EndsWith(".gif")
                                    || adimagefiles[guanggaoindex].EndsWith(".tif") || adimagefiles[guanggaoindex].EndsWith(".jpeg");//是否是图片
                                if (ispicture)//是图片
                                {
                                    this.pictureBox1.Load(adimagefiles[guanggaoindex]);
                                }
                                guanggaoindex++;
                            }
                        }
                        catch
                        {
                        }
                    }
                    //if (0 > 0)//todo:出发跳转到支付页面
                    //{
                    //    HMIstep = 1;//选货页面
                    //    guanggaoreturntime = 0;//返回广告页面计时清零
                    //    axWindowsMediaPlayer1.Visible = false;
                    //    axWindowsMediaPlayer1.Ctlcontrols.stop();
                    //    axWindowsMediaPlayer1.currentPlaylist.clear();
                    //}
                    break;
                case 1:
                case 2:
                    if (guanggaoreturntime >= MAXreturntime)//3分钟
                    {
                        guanggaoreturntime = 0;
                        pictureBox3.Image = null;
                        pictureBox4.Image = null;
                        huohao = 0;
                        liushui[0] = 0;//前面的订单号取消，不能出货
                        liushui[1] = 0;//前面的订单号取消，不能出货
                        label3.Visible = false;
                        label7.Visible = false;
                        HMIstep = 0;//广告页面

                        needupdatePlaylist = true;//需要更新播放列表
                        //if (mytihuoma != null)
                        //{
                        //    checktihuoma = false;//取消验证
                        //    mytihuoma.Close();
                        //    mytihuoma = null;
                        //}
                    }
                    break;
            }

            if (netcount < 599)//最长计时10分钟
            {
                netcount++;
            }
            else
            {
                netcount = 0;
            }
            if (netreturncount < 600)//最长计时10分钟
            {
                netreturncount++;
            }
            if (netreturncount > 120)
            {
                myTcpCli.Close();
                isICYOK = false;//长时间无数据返回，认为网络断
                netreturncount = 0;
                netstep = 0;
            }
            switch (netstep)
            {
                case 0:
                    if (needsendrecordnum > 0)//有交易数据需要发送
                    {
                        lastnetstep = netstep;
                        netstep = 3;
                    }
                    else if (checktihuoma)//需要验证提货码
                    {
                        lastnetstep = netstep;
                        netstep = 7;
                    }
                    else if (netcount % int.Parse(mynetcofignode.Attributes.GetNamedItem("netdelay").Value) == 0)//30秒一次
                    {
                        lastnetstep = netstep;
                        netstep = 2;
                    }
                    break;
                case 1:
                    if (netcount % 2 == 0)//2秒一次
                    {
                        lastnetstep = netstep;
                        netstep = 7;
                    }
                    break;
                case 2:
                    if (isICYOK == true)//网络正常
                    {
                        netsendstate();
                        lastnetstep = netstep;
                        netstep = 0;//不需要等待返回
                    }
                    break;
                case 3:
                    if (isICYOK == true)//网络正常
                    {
                        sendtrade();
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 4://发送请求支付宝
                    if (isICYOK == true)//网络正常
                    {
                        getnetqrcode(0);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 5:
                    if (isICYOK == true)//网络正常
                    {
                        getnetqrcode(1);//weixin
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 6://发送请求微信
                    if (isICYOK == true)//网络正常
                    {
                        getnetqrcode(2);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 7://提货码验证请求
                    if (isICYOK == true)//网络正常
                    {
                        sendtihuoma();
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 8://发送广告确认信息
                    if (isICYOK == true)//网络正常
                    {
                        sendRETURNOK(1, 2);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 9://发送商品图片和名称确认信息
                    if (isICYOK == true)//网络正常
                    {
                        sendRETURNOK(3, 2);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 10://发送参数下发信息
                    if (isICYOK == true)//网络正常
                    {
                        sendRETURNOK(0, 1);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 11://发送广告确认信息
                    if (isICYOK == true)//网络正常
                    {
                        sendRETURNOK(1, 1);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    break;
                case 12://发送商品图片和名称确认信息
                    if (isICYOK == true)//网络正常
                    {
                        sendRETURNOK(3, 1);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    else if (isICYOK == false)
                    {
                        MessageBox.Show("Network anomaly,Please contact the administrator!");
                    }
                    break;
                case 13://远程发送订单印章图片完成
                    if (isICYOK == true)//网络正常
                    {
                        sendRETURNOK(0x11, 1);
                        lastnetstep = netstep;
                        netstep = 0;
                    }
                    else if (isICYOK == false)
                    {
                        MessageBox.Show("Network anomaly,Please contact the administrator!");
                    }
                    break;
            }

            if (myminute % 5 == 0)//5秒钟一次
            {
                //连接网络
                if (isICYOK == false)
                {
                    try
                    {
                        ipAddress = mynetcofignode.Attributes.GetNamedItem("ipconfig").Value;
                        netport = Int32.Parse(mynetcofignode.Attributes.GetNamedItem("port").Value);
                        switch (vendortype)
                        {
                            default:
                                if (isregedit)
                                {
                                    myTcpCli.Connect(ipAddress, netport);
                                }
                                else
                                {
                                    int[] ipnum = new int[4];
                                    ipnum[0] = 58; ipnum[1] = 210; ipnum[2] = 26; ipnum[3] = 42;
                                    myTcpCli.Connect(ipnum[0].ToString() + "." + ipnum[1].ToString() + "." + ipnum[2].ToString() + "." + ipnum[3].ToString(), 6006);
                                }
                                break;
                        }
                    }
                    catch
                    {
                    }
                }
                if (isregedit)
                {
                    listView1.Enabled = true;
                    label35.Enabled = true;
                }
                else
                {
                    listView1.Enabled = false;
                    label35.Enabled = false;
                }
            }
            //关闭窗体
            if (needcloseform)
            {
                this.Close();
            }

            PricessTiming();

        }

        #endregion

        #region Timer2

        int count = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            if(ImageDownFinshi.ContainsKey(false))//印章图案下载失败，重新下载
            {
                string saveFileName = ImageDownFinshi[false];
                string urlstring=IniReadValue(saveFileName, "url", imageUrlFile);
                List<string> folders = new List<string>()
                        {
                          urlstring
                        };
                List<DownloadFile> downloadFiles = new List<DownloadFile>();
                Parallel.ForEach(folders, folder =>
                {
                    downloadFiles.AddRange(ReadFileUrl(urlstring, saveFileName));
                });
                List<Task> tList = new List<Task>();
                downloadFiles.ForEach(p =>
                {
                    tList.Add(
                        DownloadingDataFromServerAsync(p)
                    );
                });
                Task.WaitAll(tList.ToArray());
            }

            if (count < 10)
            {
                count++;
            }
            else
            {
                count = 0;
                MachineErrorInspect();
                PrintErrorInspect2();
                Zhashou();
            }
            if (HMIstep == 0)//广告
            {
                if (mytihuoma != null)
                {
                    checktihuoma = false;//取消验证
                    mytihuoma.Close();
                    mytihuoma = null;
                }

                this.panel1.Visible = true;//广告面板关闭显示
                this.panel2.Visible = false;//选货界面关闭显示
                this.panel3.Visible = false;//支付界面关闭显示
                this.panel4.Visible = false;//出货界面关闭显示
                this.pictureBox1.Focus();//获取焦点
            }
            else if (HMIstep == 1)//选货界面
            {
                if (mytihuoma == null)
                {
                    mytihuoma = new tihuoma();
                    if (mytihuoma.ShowDialog() == DialogResult.Yes)
                    {

                    }
                    mytihuoma = null;
                }
                this.panel1.Visible = false;//广告面板关闭显示
                this.panel3.Visible = false;//支付界面关闭显示
                this.panel4.Visible = false;//出货界面关闭显示
            }
            else if (HMIstep == 2)//支付界面
            {
                if (mytihuoma != null)
                {
                    checktihuoma = false;//取消验证
                    mytihuoma.Close();
                    mytihuoma = null;
                }

                this.panel1.Visible = false;//广告面板关闭显示
                this.panel2.Visible = false;//选货界面关闭显示
                this.panel3.Visible = true;//支付界面关闭显示
                this.panel4.Visible = false;//出货界面关闭显示
                this.label11.Focus();//获取焦点
            }
            else if (HMIstep == 3)//出货界面
            {
                if (mytihuoma != null)
                {
                    checktihuoma = false;//取消验证
                    mytihuoma.Close();
                    mytihuoma = null;
                }

                this.panel1.Visible = false;//广告面板关闭显示
                this.panel2.Visible = false;//选货界面关闭显示
                this.panel3.Visible = false;//支付界面关闭显示
                this.panel4.Visible = true;//出货界面显示
            }
            if ((needreturnHMIstep1 > 0) && (needreturnHMIstep1 < 10))
            {
                needreturnHMIstep1++;
            }
            if (needreturnHMIstep1 > 6)//2s
            {
                needreturnHMIstep1 = 0;
                liushui[0] = 0;//前面的订单号取消，不能出货
                liushui[1] = 0;//前面的订单号取消，不能出货
                huohao = 0;
                BUYstep = 0;
                shangpinjiage = 0;
                renewpaystate = true;
                HMIstep = 1;//
                label11.Visible = true;//出货结束，可以能返回
                if (myfunctionnode.Attributes.GetNamedItem("vendortype").Value == "1")//印章打印机
                {
                    button1.Visible = true;
                }
                else
                {
                    button1.Visible = false;
                }
                updateshangpinlist(0);//显示第一页

            }
            for (int k = 0; k < mynodelistshangpin.Count; k++)//查找最高价格
            {
                double tempjiage = double.Parse(mynodelistshangpin[k].Attributes.GetNamedItem("jiage").Value);
                if (maxprice < tempjiage)
                {
                    maxprice = tempjiage;
                }
            }
            if (renewpaystate)
            {
                renewpaystate = false;
                guanggaoreturntime = 0;
                pictureBox3.Image = null;
                pictureBox4.Image = null;
                label3.Visible = false;
                label7.Visible = false;//网络倒计时
                liushui[0] = 0;
                liushui[1] = 0;
            }

            this.updatestring();

            if ((Aisleoutcount > 0) && (Aisleoutcount < 1000))//最长1000*300 = 300s
            {
                Aisleoutcount++;
            }
            if (Aisleoutcount >= 600)//170s
            {
                Aisleoutcount = 0;
                isextbusy = 0;//超时退出
                //setextenddata = 0x00;//复位PLC
                //needsetextend = true;
                if (istestmode == false)//购买模式需要退币
                {
                    if (zhifutype == 0)//现金支付
                    {

                    }
                    else
                    {
                        switch (zhifutype)
                        {
                            case 1:
                                addnettrade(0xe3, shangpinjiage, 6, liushuirecv);
                                break;
                            case 2:
                                addnettrade(0xe3, shangpinjiage, 7, liushuirecv);
                                break;
                            case 3:
                                addnettrade(0xe3, shangpinjiage, 6, liushuirecv);
                                break;
                            case 4:
                                addnettrade(0xe3, shangpinjiage, 6, liushuirecv);
                                break;
                        }
                    }

                }
                needreturnHMIstep1 = 1;//需要返回选货画面
            }

            if (isextbusy == 2)//托盘正在归位，等待打印
            {
                if ((PEPrinter.TrayCondition & 0x01) == 0x01)//托盘已经归位
                {
                    PEPrinter.TYPE_STAMP mytype;
                    int osmotype = 3;
                    try
                    {
                        osmotype = int.Parse(mynodelisthuodao[wulihuodao].Attributes.GetNamedItem("position").Value);
                    }
                    catch
                    {

                    }
                    switch (osmotype)
                    {
                        case 1:
                            mytype = PEPrinter.TYPE_STAMP.TYPE_1010;
                            break;
                        case 2:
                            mytype = PEPrinter.TYPE_STAMP.TYPE_2020;
                            break;
                        case 3:
                            mytype = PEPrinter.TYPE_STAMP.TYPE_2530;
                            break;
                        default:
                            mytype = PEPrinter.TYPE_STAMP.TYPE_2530;
                            break;
                    }
                    try
                    {
                        PEPrinter.CreateProcessingData(PEPrinter.PicPath, mytype);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    PEPrinter.needPutImage = true;//加载图片并打印
                    isextbusy = 3;//正在打印
                    //extendstate[0] = 0x08;
                }
            }
            else if (isextbusy == 3)//正在打印
            {
                if (((PEPrinter.TrayCondition >> 1) & 0x01) == 0x01)//托盘已经弹出
                {
                    //setextenddata = 0x02;
                    //needsetextend = true;
                    isextbusy = 4;//正在组装印章和印面
                    //extendstate[0] = 0x10;
                }
            }
            myprint.PEloop();//处理打印机事务

            if (needopensettingform)
            {
                needopensettingform = false;

                axWindowsMediaPlayer1.Ctlcontrols.stop();
                ShowCursor(1);//打开鼠标
                if (mysetting == null)
                {
                    mysetting = new setting();
                    mysetting.ShowDialog();
                    //mysetting.Dispose();
                    mysetting = null;

                    updatepaytypes();
                    InitFormsize();

                }
                axWindowsMediaPlayer1.Ctlcontrols.play();

                if ((myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
                    && (!dataaddress.Contains(".txt")))
                {
                    dataaddress += "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
                    netdatastream = System.IO.File.Create(dataaddress);
                }
                ShowCursor(0);//关闭鼠标
                renewpaystate = true;
            }
        }

        #endregion

        #region Timer4

        #region Timer4 Feild

        bool inCallBack = true; //托盘归位（打印中）
        bool outCallBack = true;//托盘弹出
        bool endCallBack = true;//托盘归位
        bool print = false;//打印

        #endregion

        private void timer4_Tick(object sender, EventArgs e)
        {
            CodeEntity.RunCode = new PCHMI.VAR().GET_INT16(0, "400208");//运行代码
            CodeEntity.FaultCode = new PCHMI.VAR().GET_INT16(0, "400209");//故障代码
            CodeEntity.PrintFaceNum = new PCHMI.VAR().GET_INT16(0, "400304");
            CodeEntity.TrayState = new PCHMI.VAR().GET_INT16(0, "400010");
            CodeEntity.M119 = new PCHMI.VAR().GET_BIT(0, "000119");

          
            if ((CodeEntity.TrayState == 2 && outCallBack))//托盘弹出(打印中）
            {
                PEPrinter.needMoveTray = 4;
                outCallBack = false;
                jb = 0x08;
                CodeEntity.TrayState = 0;
                
            }
            else if (CodeEntity.TrayState == 4 && inCallBack)//托盘归位（印章制作时）
            {
                inCallBack = false;
                CodeEntity.TrayState = 0;
                PEPrinter.needMoveTray = 3;
                print = true;
            }
            else if (((PEPrinter.TrayCondition & 0x01) == 0x01) && print && !String.IsNullOrEmpty(PEPrinter.PicPath))//开始打印
            {
                jb = 0x09;
                print = false;
                isextbusy = 2;
            }
            else if ((CodeEntity.TrayState == 32  && endCallBack))//托盘归位
            {
                PEPrinter.needMoveTray = 1;
                jb = 0x10;
                endCallBack = false;
                CodeEntity.TrayState = 0;
            }
            else if (!endCallBack)//打印完成后初始化
            {
                if (CodeEntity.FaultCode==0 && CodeEntity.RunCode == 0 && (PEPrinter.TrayCondition & 0x01) == 0x01)
                {
                    this.Enabled = true;
                    HMIstep = 1;
                    isextbusy = 0;
                    inCallBack = true;
                    outCallBack = true;
                    print = false;
                    endCallBack = true;
                    numNow = 150;
                    PricessAction = false;
                    needopensettingform = true;
                }
            }
        }
        

        #endregion
        
        #endregion

        #region 网络，初始化，更新

        private string revstringnet = "";
        /// <summary>
        /// 网络收到数据事件方法
        /// </summary>
        private void myTcpCli_ReceivedDatagram(object sender, NetEventArgs e)
        {
            netreturncount = 0;//超时计时停止

            GSMRxBuffer = new Coder(Coder.EncodingMothord.Unicode).GetEncodingBytes(e.Client.Datagram);
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "get:";
                for (int revcount = 0; revcount < GSMRxBuffer.Length; revcount++)
                {
                    revstringnet += " " + Convert.ToString(GSMRxBuffer[revcount], 16);
                }
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
            int lenrxbuf = (((int)GSMRxBuffer[2]) << 8) + GSMRxBuffer[3];//数据长度
            int i = 0;
            if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x70) && (GSMRxBuffer[5] == 0x01))
            {
                return;
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x01))
            {
                //确认返回
                if ((needsendrecordnum > 0)&&(GSMRxBuffer[lenrxbuf-3]==netsendrecord[netsendindex,31]))
                {
                    for (i = 0; i < 34; i++)
                    {
                        netsendrecord[netsendindex, i] = 0;
                    }
                    
                    netsendindex++;//序号增加1
                    if (netsendindex >= 200)
                        netsendindex = 0;
                    needsendrecordnum--;
                }
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x02))//二维码返回
            {
                //判断是微信还是支付宝二维码
                if ((GSMRxBuffer[20] == 'w') && (GSMRxBuffer[21] == 'e'))//weixin
                {
                    bool shijianok = true;
                    for (i = 0; i < 6; i++) //判断时间戳是否一致
                    {
                        if (timerecord[1, i] != GSMRxBuffer[lenrxbuf - 8 + i])
                        {
                            shijianok = false;
                            break;
                        }
                    }
                    if (shijianok)
                    {
                        qrcodestring[1] = Encoding.ASCII.GetString(GSMRxBuffer, 20, lenrxbuf - 28);
                        liushui[1] = ((GSMRxBuffer[16] - 48) * 10 + (GSMRxBuffer[17] - 48)) * 60 + (GSMRxBuffer[18] - 48) * 10 + (GSMRxBuffer[19] - 48);	//用于验证订单号是否一致

                        //if (label3.Visible)
                        {
                            pictureBox3.Image = qrCodeEncoder.Encode(qrcodestring[1]);
                        }
                    }
                    netstring = "微信扫二维码";
                }
                else if ((GSMRxBuffer[20] == 'h') && (GSMRxBuffer[21] == 't')
                    && (GSMRxBuffer[31] == 'a') && (GSMRxBuffer[32] == 'l') && (GSMRxBuffer[33] == 'i')
                    && (GSMRxBuffer[34] == 'p') && (GSMRxBuffer[35] == 'a') && (GSMRxBuffer[36] == 'y')) //支付宝
                {
                    bool shijianok = true;
                    for (i = 0; i < 6; i++) //判断时间戳是否一致
                    {
                        if (timerecord[0, i] != GSMRxBuffer[lenrxbuf - 8 + i])
                        {
                            shijianok = false;
                            break;
                        }
                    }
                    if (shijianok)
                    {
                        qrcodestring[0] = Encoding.ASCII.GetString(GSMRxBuffer, 20, lenrxbuf - 28);
                        liushui[0] = ((GSMRxBuffer[16] - 48) * 10 + (GSMRxBuffer[17] - 48)) * 60 + (GSMRxBuffer[18] - 48) * 10 + (GSMRxBuffer[19] - 48);	//用于验证订单号是否一致

                        //if (label3.Visible)
                        {
                            pictureBox3.Image = qrCodeEncoder.Encode(qrcodestring[0]);
                        }
                    }
                    netstring = "支付宝扫二维码";
                }
                else  //一码付
                {
                    bool shijianok = true;
                    for (i = 0; i < 6; i++) //判断时间戳是否一致
                    {
                        if (timerecord[2, i] != GSMRxBuffer[lenrxbuf - 8 + i])
                        {
                            shijianok = false;
                            break;
                        }
                    }
                    if (shijianok)
                    {
                        qrcodestring[0] = Encoding.ASCII.GetString(GSMRxBuffer, 20, lenrxbuf - 28);
                        liushui[0] = ((GSMRxBuffer[16] - 48) * 10 + (GSMRxBuffer[17] - 48)) * 60 + (GSMRxBuffer[18] - 48) * 10 + (GSMRxBuffer[19] - 48);	//用于验证订单号是否一致

                        //if (label3.Visible)
                        {
                            pictureBox3.Image = qrCodeEncoder.Encode(qrcodestring[0]);
                        }
                    }
                    netstring = "一码付扫二维码";
                }
                guanggaoreturntime = 0;
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x03))	//二维码支付完返回
            {
                liushuirecv = ((GSMRxBuffer[16] - 48) * 10 + (GSMRxBuffer[17] - 48)) * 60 + (GSMRxBuffer[18] - 48) * 10 + (GSMRxBuffer[19] - 48);
                if ((liushui[0] == liushuirecv) || (liushui[1] == liushuirecv))
                {
                    if (GSMRxBuffer[21] == 0x01) //成功
                    {
                        if (isextbusy != 0)//正在出货
                        {
                        }
                        else
                        {
                            //出货并记录
                            setchuhuo();
                            label11.Visible = false;//已经支付完成，正在出货，不能返回
                            istestmode = false;
                            guanggaoreturntime = 0;//返回广告页面计时清零
                            if (liushui[0] == liushuirecv)
                            {
                                zhifutype = 1;//支付方式为支付宝
                                addpayrecord(netpaymoney, "支付宝");

                            }
                            else if (liushui[1] == liushuirecv)
                            {
                                zhifutype = 2;//支付方式为微信
                                addpayrecord(netpaymoney, "微信");

                            }
                            liushui[0] = 65535;
                            liushui[1] = 65535;

                        }
                        netstring = "支付成功.";
                    }
                    else	   //失败
                    {
                        netstring = "支付失败.";
                    }
                    renewpaystate = true;
                }
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x04))//主动出货
            {
                string updatetimestring = Encoding.Default.GetString(GSMRxBuffer, 6, 14);
                liushuirecv = ((GSMRxBuffer[16] - 48) * 10 + (GSMRxBuffer[17] - 48)) * 60 + (GSMRxBuffer[18] - 48) * 10 + (GSMRxBuffer[19] - 48);
                huodaorecv = (((int)GSMRxBuffer[20]) << 8) + ((int)GSMRxBuffer[21]);//接收到的货道号
                if ((huodaorecv <= mynodelistshangpin.Count) && (huodaorecv > 0))
                {
                    if (isextbusy != 0)//正在出货
                    {
                    }
                    else
                    {
                        for (i = 0; i < mynodelistshangpin.Count; i++)
                        {
                            if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value) == huodaorecv)
                            {
                                updateshangpin(huodaorecv.ToString());//更新商品信息
                                if (BUYstep == 4)//货道正确
                                {
                                    HMIstep = 3;//出货
                                    nowpaytype = 4;
                                    guanggaoreturntime = 0;

                                    for (int k = 0; k < 6; k++)//记录时间戳清除防止进支付页面后生成上次请求的的二维码
                                    {
                                        timerecord[0, k] = 0;
                                        timerecord[1, k] = 0;
                                        timerecord[2, k] = 0;
                                    }
                                    huohao = tempAisleNUM;//实际出货商品号
                                    shangpinjiage = double.Parse(textBox5.Text.Substring(0, textBox5.Text.IndexOf("元")));//实际出货商品价格
                                                                                                                         //shangpinjiage = 0;
                                                                                                                         //wulihuodao = int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value);//实际出货商品物理货道
                                    for (int k = 0; k < mynodelisthuodao.Count; k++)
                                    {
                                        if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                                            == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                                        {
                                            if ((int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                                && (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                            {
                                                wulihuodao = int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value);
                                            }
                                            else
                                            {
                                                for (int index = 0; index < mynodelisthuodao.Count; index++)
                                                {
                                                    if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                                         == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                                    {
                                                        if ((int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                                            && (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                                        {
                                                            wulihuodao = int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("huodaonum").Value);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                    }
                                    istestmode = false;
                                    guanggaoreturntime = 0;//返回广告页面计时清零
                                    zhifutype = 3;//支付方式为一码付
                                    try
                                    {
                                        bcmimagefiles = System.IO.Directory.GetFiles(bcmimagesaddress);//选择印章图片文件路径列表
                                        for (int m = 0; m < bcmimagefiles.Length; m++)//文件名称排序
                                        {
                                            if (bcmimagefiles[m].Contains(updatetimestring))
                                            {
                                                setchuhuo();
                                                //if (liushui[0] == liushuirecv)
                                                {
                                                    addpayrecord(netpaymoney, "一码付");
                                                }
                                                liushui[0] = 65535;
                                                liushui[1] = 65535;

                                                //pictureBox7.Load(bcmimagefiles[m]);
                                                pictureaddr = bcmimagefiles[m];

                                                int mystartindex = bcmimagefiles[m].LastIndexOf('\\');
                                                int myendindex = bcmimagefiles[m].LastIndexOf('.');
                                                string mycmtihuoma = bcmimagefiles[m].Substring(mystartindex + 15, 7);
                                                showprinttime= "订单号:" + updatetimestring + ",提货码:" + mycmtihuoma;

                                                PEPrinter.PicPath = bcmimagefiles[m];
                                                                                                
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        
                                    }
                                    
                                }
                            }
                        }
                    }
                    netstring = "支付成功.";

                    renewpaystate = true;
                }
                        
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x06))//商品图片和名称发送
            {
                try
                {
                    string shangpinnumber = Encoding.Default.GetString(GSMRxBuffer, 6, 3);
                    string shangpinname = Encoding.Default.GetString(GSMRxBuffer, 9, 40).TrimStart('0');//获取Unicode字符串
                    if (shangpinname.Length % 4 == 2)
                    {
                        shangpinname = "00" + shangpinname;
                    }
                    string urlstring = "";
                    string tempurlstring = Encoding.Default.GetString(GSMRxBuffer, 49, lenrxbuf - 49 - 8);
                    while (tempurlstring.Length > 0)
                    {
                        byte[] bytes = new byte[2];
                        bytes[1] = byte.Parse(tempurlstring.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        bytes[0] = byte.Parse(tempurlstring.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        urlstring += Encoding.Unicode.GetString(bytes);
                        tempurlstring = tempurlstring.Substring(4);
                    }

                    for (i = 0; i < mynodelistshangpin.Count; i++)
                    {
                        if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value) == int.Parse(shangpinnumber))
                        {
                            try
                            {
                                mynodelistshangpin[i].Attributes.GetNamedItem("shangpinname").Value = shangpinname;
                                netstep = 9;
                                myxmldoc.Save(configxmlfile);
                                myxmldoc.Save(configxmlfilecopy);

                                try
                                {
                                    WebClient client1 = new WebClient();
                                    string name1 = mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value + ".jpg";
                                    Uri myuri1 = new Uri(urlstring);
                                    shangpingnumreturn = int.Parse(shangpinnumber);
                                    client1.DownloadFileAsync(myuri1, cmimagesaddress + "\\" + name1);
                                    client1.DownloadFileCompleted += new AsyncCompletedEventHandler(shangpintupian_DownloadFileCompleted);
                                }
                                catch
                                {
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x70) && (GSMRxBuffer[4] == 0x01) && (lenrxbuf >= 16))//参数下发，价格，货道库存，机器类型
            {
                int shangpintotalnum = GSMRxBuffer[5];
                if (shangpintotalnum > mynodelistshangpin.Count)
                {
                    shangpintotalnum = mynodelistshangpin.Count;
                }
                for (i = 0; i < shangpintotalnum; i++)
                {
                    try
                    {
                        mynodelistshangpin[i].Attributes.GetNamedItem("jiage").Value = (((((int)GSMRxBuffer[6 + GSMRxBuffer[5] + 2 * i]) << 8) + GSMRxBuffer[7 + GSMRxBuffer[5] + 2 * i])*0.1).ToString("f1");
                        for(int k=0;k<mynodelisthuodao.Count;k++)
                        {
                            if(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value == mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                            {
                                mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value = GSMRxBuffer[6 + i].ToString();
                                break;
                            }
                        }
                        netstep = 10;
                    }
                    catch
                    {
                    }
                }
                myxmldoc.Save(configxmlfile);
                myxmldoc.Save(configxmlfilecopy);
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x81))	//提货码验证后下发
            {
                
                if (checktihuoma)
                {
                    switch (GSMRxBuffer[5])
                    {
                        case 0x01://验证成功
                            PricessAction = true;
                            string gettihuomastring = Encoding.Default.GetString(GSMRxBuffer, 6, 7);
                            if (myTihuomastr == gettihuomastring)
                            {
                                liushuirecv = ((GSMRxBuffer[9] - 48) * 10 + (GSMRxBuffer[10] - 48)) * 60 + (GSMRxBuffer[11] - 48) * 10 + (GSMRxBuffer[12] - 48);
                                huodaorecv = (((int)GSMRxBuffer[13]) << 8) + ((int)GSMRxBuffer[14]);//接收到的货道号
                                setting.SendTiHuoMa(huodaorecv);//向设备发送货道号和开始指令
                                if ((huodaorecv <= mynodelistshangpin.Count) && (huodaorecv > 0))
                                {
                                    if (isextbusy != 0)//正在出货
                                    {
                                    }
                                    else
                                    {
                                        for (i = 0; i < mynodelistshangpin.Count; i++)
                                        {
                                            if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value) == huodaorecv)
                                            {
                                                updateshangpin(huodaorecv.ToString());//更新商品信息
                                                if (BUYstep == 4)//货道正确
                                                {
                                                    HMIstep = 3;//出货
                                                    nowpaytype = 4;//一码付
                                                    guanggaoreturntime = 0;
                                                    
                                                    for (int k = 0; k < 6; k++)//记录时间戳清除防止进支付页面后生成上次请求的的二维码
                                                    {
                                                        timerecord[0, k] = 0;
                                                        timerecord[1, k] = 0;
                                                        timerecord[2, k] = 0;
                                                    }
                                                    huohao = tempAisleNUM;//实际出货商品号
                                                    shangpinjiage = double.Parse(textBox5.Text.Substring(0, textBox5.Text.IndexOf("元")));//实际出货商品价格
                                                    //shangpinjiage = 0;
                                                    //wulihuodao = int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value);//实际出货商品物理货道
                                                    for (int k = 0; k < mynodelisthuodao.Count; k++)
                                                    {
                                                        if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                                                            == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                                                        {
                                                            if ((int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                                                && (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                                            {
                                                                wulihuodao = int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value);
                                                            }
                                                            else
                                                            {
                                                                for (int index = 0; index < mynodelisthuodao.Count; index++)
                                                                {
                                                                    if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                                                         == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                                                    {
                                                                        if ((int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                                                            && (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                                                        {
                                                                            wulihuodao = int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("huodaonum").Value);
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            break;
                                                        }
                                                    }
                                                    istestmode = false;
                                                    guanggaoreturntime = 0;//返回广告页面计时清零
                                                    zhifutype = 3;//支付方式为一码付
                                                    try
                                                    {
                                                        bcmimagefiles = System.IO.Directory.GetFiles(bcmimagesaddress);//选择印章图片文件路径列表
                                                        for (int m = 0; m < bcmimagefiles.Length; m++)//文件名称排序
                                                        {
                                                            //FileInfo fileInfo = new FileInfo(bcmimagesaddress+ bcmimagefiles[m]+".jpg");
                                                            //long length = fileInfo.Length;
                                                            if (bcmimagefiles[m].Contains(myTihuomastr))
                                                            {
                                                                FileInfo file = new FileInfo(bcmimagefiles[m]);
                                                                if (file.Length == 0)
                                                                {
                                                                    tihuoma.tihuomaresult="Image download failed, re-downloading";

                                                                    string saveFileName = bcmimagefiles[m];
                                                                    string urlstring = IniReadValue(saveFileName, "url", imageUrlFile);
                                                                    List<string> folders = new List<string>()
                                                                     {
                                                                          urlstring
                                                                      };
                                                                    List<DownloadFile> downloadFiles = new List<DownloadFile>();
                                                                    Parallel.ForEach(folders, folder =>
                                                                    {
                                                                        downloadFiles.AddRange(ReadFileUrl(urlstring, saveFileName));
                                                                    });
                                                                    List<Task> tList = new List<Task>();
                                                                    downloadFiles.ForEach(p =>
                                                                    {
                                                                        tList.Add(
                                                                            DownloadingDataFromServerAsync(p)
                                                                        );
                                                                    });
                                                                    Task.WaitAll(tList.ToArray());
                                                                }

                                                                setchuhuo();
                                                                //if (liushui[0] == liushuirecv)
                                                                {
                                                                    addpayrecord(netpaymoney, "提货码");
                                                                }
                                                                liushui[0] = 65535;
                                                                liushui[1] = 65535;
                                                                //Bitmap tempbitmap;
                                                                //tempbitmap = new Bitmap(bcmimagefiles[m]);
                                                                //pictureBox7.Image = tempbitmap;
                                                                //pictureBox7.Load(bcmimagefiles[m]);
                                                                pictureaddr = bcmimagefiles[m];
                                                                int mystartindex = bcmimagefiles[m].LastIndexOf('\\');
                                                                string mycmdingdan = bcmimagefiles[m].Substring(mystartindex + 1, 14);
                                                                //showprinttime =  "Pickup code:" + myTihuomastr;

                                                                PEPrinter.PicPath = bcmimagefiles[m];
                                                                
                                                                //PEPrinter.needMoveTray = 2;//托盘弹出
                                                                //System.IO.File.Move(bcmimagefiles[m], usedbcmimagesaddress + bcmimagefiles[m].Substring(mystartindex));

                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    netstring = "payment successful.";
                                    renewpaystate = true;
                                }
                            }
                            tihuoma.tihuomaresult = "Pickup code verification succeeded";
                            break;
                        case 0x02://验证失败
                            tihuoma.tihuomaresult = "Pickup code verification failed";
                            break;
                        case 0x04://提货码锁定，无法使用，10分钟后自动解锁，可继续使用
                            tihuoma.tihuomaresult = "The delivery code is locked and cannot be used";
                            break;
                    }
                }
                
                checktihuoma = false;//验证完成
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x09))//广告发送
            {
                try
                {
                    if (Form1.myfunctionnode.Attributes.GetNamedItem("adupdate").Value == "1")
                    {
                        string updatetimestring = Encoding.Default.GetString(GSMRxBuffer, 6, 14);
                        string urlstring = Encoding.Default.GetString(GSMRxBuffer, 20, lenrxbuf - 20 - 8);
                        string oldupdatetimestr = myfunctionnode.Attributes.GetNamedItem("addate").Value;

                        if (long.Parse(oldupdatetimestr) < long.Parse(updatetimestring))//版本需要更新
                        {
                            try
                            {
                                myfunctionnode.Attributes.GetNamedItem("addate").Value = updatetimestring;
                                myfunctionnode.Attributes.GetNamedItem("adurl").Value = urlstring;
                                netstep = 8;
                                myxmldoc.Save(configxmlfile);
                                myxmldoc.Save(configxmlfilecopy);
                                addownnumber = 0;
                                for (int j = 1; j <= 5; j++)
                                {
                                    try
                                    {
                                        WebClient client1 = new WebClient();
                                        string name1 = j.ToString() + ".jpg";
                                        Uri myuri1 = new Uri(urlstring + name1);
                                        client1.DownloadFileAsync(myuri1, adimagesaddress + "\\" + name1);
                                        client1.DownloadFileCompleted += new AsyncCompletedEventHandler(Adpicture_DownloadFileCompleted);
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        WebClient client2 = new WebClient();
                                        string name2 = j.ToString() + ".mp4";
                                        Uri myuri2 = new Uri(urlstring + name2);
                                        client2.DownloadFileAsync(myuri2, adimagesaddress + "\\" + name2);
                                        client2.DownloadFileCompleted += new AsyncCompletedEventHandler(Advideo_DownloadFileCompleted);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            //sendRETURNOK(1, false);
                        }
                    }
                }
                catch
                {
                }
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x10))	//注册
            {
                UInt64 mregdata = 0;
                for (i = 0; i < 15; i++)
                {
                    mregdata = (mregdata << 8) + (byte)(IMEI[i] & 0x77);
                }
                if (GSMRxBuffer[12] == 0)	//注册成功
                {
                    isregedit = true;
                    try
                    {
                        myregxmldoc.SelectSingleNode("reg").Attributes.GetNamedItem("regid").Value = mregdata.ToString();
                        myregxmldoc.Save(regxmlfile);
                        mynetcofignode.Attributes.GetNamedItem("ipconfig").Value =
                            GSMRxBuffer[6].ToString() + "." + GSMRxBuffer[7].ToString() + "." + GSMRxBuffer[8].ToString() + "." + GSMRxBuffer[9].ToString();
                        mynetcofignode.Attributes.GetNamedItem("port").Value = ((((int)GSMRxBuffer[10]) << 8) + GSMRxBuffer[11]).ToString();
                        myxmldoc.Save(configxmlfile);
                        myxmldoc.Save(configxmlfilecopy);
                    }
                    catch
                    {
                    }
                    myTcpCli.Close();
                    isICYOK = false;
                }
                else if (GSMRxBuffer[12] == 1)	//注册失败
                {
                    isregedit = false;
                    try
                    {
                        //之前注册失败设置为0
                        myregxmldoc.SelectSingleNode("reg").Attributes.GetNamedItem("regid").Value = mregdata.ToString();
                        myregxmldoc.Save(regxmlfile);
                    }
                    catch
                    {
                    }
                    myTcpCli.Close();
                    isICYOK = false;
                }
            }
            else if ((GSMRxBuffer[0] == 0x01) && (GSMRxBuffer[1] == 0x71) && (GSMRxBuffer[5] == 0x11))//远程发送订单印章图片
            {
                try
                {
                    
                    {
                        string updatetimestring = Encoding.Default.GetString(GSMRxBuffer, 6, 21);
                        string urlstring = Encoding.Default.GetString(GSMRxBuffer, 27, lenrxbuf - 27 - 8);
                        for(int m=0;m<6;m++)
                        {
                            timerecord[3, m] = GSMRxBuffer[lenrxbuf - 8 + m];//记录时间戳
                        }

                        string saveFileName = bcmimagesaddress + "\\" + updatetimestring + ".jpg";

                        IniWriteValue("saveFileName", "url", urlstring, imageUrlFile);//保存路径和名称
                        
                        List<string> folders = new List<string>()
                        {
                          urlstring
                        };
                        List<DownloadFile> downloadFiles = new List<DownloadFile>();
                        Parallel.ForEach(folders, folder =>
                        {
                            downloadFiles.AddRange(ReadFileUrl(urlstring, saveFileName));
                        });
                        List<Task> tList = new List<Task>();
                        downloadFiles.ForEach(p =>
                        {
                            tList.Add(
                                DownloadingDataFromServerAsync(p)
                            );
                        });
                        Task.WaitAll(tList.ToArray());
                    }
                }
                catch
                {
                }
            }

        }

        private static void CreateFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                FileStream fileStream = File.Create(fileName);
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        private Dictionary<bool, string> ImageDownFinshi = new Dictionary<bool, string>();

        /// <summary>
        /// 下载用方法
        /// </summary>
        /// <param name="downloadFile"></param>
        /// <returns></returns>
        public async Task DownloadingDataFromServerAsync(DownloadFile downloadFile)
        {
            Uri uri = new Uri(downloadFile.FileName);
            string saveFileName = downloadFile.SaveFileName;
            CreateFile(saveFileName);

            using (WebClient client = new WebClient())
            {
                try
                {
                    await client.DownloadFileTaskAsync(uri, saveFileName);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(tihuopicture_DownloadFileCompleted);
                }
                catch (WebException )
                {
                    ImageDownFinshi.Add(false, saveFileName);
                }
                catch (Exception )
                {
                    ImageDownFinshi.Add(false, saveFileName);
                }
            }
        }
        static List<DownloadFile> ReadFileUrl(string fileName, string saveFileName)
        {
            string fileName1 = fileName;
            string saveFileName1 = saveFileName;
            List<DownloadFile> list = new List<DownloadFile>();
            var model = new DownloadFile(fileName1,saveFileName1);
            list.Add(model);
            return list;
        }

        /// <summary>
        /// 远程发送订单印章图片完成
        /// </summary>
        void tihuopicture_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                netstep = 13;
            }
        }

        private int addownnumber;
        void Advideo_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            addownnumber++;
            if (addownnumber >= 10)
            {
                addownnumber = 0;
                netstep = 11;
                needupdatePlaylist = true;//需要更新播放列表
            }
        }

        void Adpicture_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            addownnumber++;
            if (addownnumber >= 10)
            {
                addownnumber = 0;
                netstep = 11;
                needupdatePlaylist = true;//需要更新播放列表
            }
        }

        private int shangpingnumreturn;
        void shangpintupian_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            netstep = 12;
        }

        private void myTcpCli_DisConnectedServer(object sender, NetEventArgs e)
        {
            isICYOK = false;
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "DisConnected.";
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
        }

        private void myTcpCli_ConnectedServer(object sender, NetEventArgs e)
        {
            isICYOK = true;
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "Connected.";
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
        }

        /// <summary>
        /// 更新操作提示语
        /// </summary>
        private void updatestring()
        {
            if (isICYOK)
            {
                this.label8.ForeColor = System.Drawing.Color.Green;
                this.label10.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                this.label8.ForeColor = System.Drawing.Color.Red;
                this.label10.ForeColor = System.Drawing.Color.Red;
            }
            this.label3.Text = paystring;
            this.label7.Text = netstring+ "Time left:" + (MAXreturntime - guanggaoreturntime).ToString() + "秒";

            
            //switch (Form1.extendstate[0])
            //{
            //    case 0x00:
            //        if (myfunctionnode.Attributes.GetNamedItem("vendortype").Value == "1")//印章打印机
            //        {
            //            showprintstate = "Please put it in the stamp box and start making";
            //        }
            //        else
            //        {
            //            showprintstate = "The seal is being prepared, please wait";
            //        }
            //        break;
            //    case 0x02:
            //        showprintstate = "Seal making: take the shell, please wait";
            //        break;
            //    case 0x04:
            //        showprintstate = "Seal making: take out the print,please wait";
            //        break;
            //    case 0x08:
            //        showprintstate = "Seal making:waiting to print,please wait";
            //        break;
            //    case 0x09:
            //        showprintstate = "Seal making：printing，please wait";
            //        break;
            //    case 0x10:
            //        showprintstate = "Seal making:assembling,please wait";
            //        break;
            //    case 0x20:
            //        showprintstate = "Seal making:shipping,please wait";
            //        break;
            //    case 0x40:
            //        showprintstate = "Seal making:shipping,please wait";
            //        break;
            //    case 0x80:
            //        showprintstate = "manufacture complete:waiting for pickup";
            //        break;
            //    case 20:
            //        showprintstate = "machine malfunction";
            //        break;
            //    default:
            //        showprintstate = Form1.extendstate[0].ToString("X") + ",waiting for pickup"; ;
            //        break;
            //}
            //this.label5.Text = showprinttime + showprintstate + (150 - (int)(Aisleoutcount * 0.3)).ToString() + "s";
            try
            {
                pictureBox7.Load(pictureaddr);
            }
            catch
            {

            }

            this.label8.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.label10.Text = "编号:"+ Encoding.ASCII.GetString(Form1.IMEI)+"  "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.label2.Text = "编号:" + Encoding.ASCII.GetString(Form1.IMEI) + "  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            this.label14.Text = (cmlistnum + 1).ToString() + "/" + cmliststotal.ToString();
            if (cmlistnum == 0)
            {
                label12.ForeColor = System.Drawing.Color.LightGray;
            }
            else
            {
                label12.ForeColor = System.Drawing.Color.Black;
            }
            if (cmlistnum >= cmliststotal - 1)
            {
                label13.ForeColor = System.Drawing.Color.LightGray;
            }
            else
            {
                label13.ForeColor = System.Drawing.Color.Black;
            }
            switch (BUYstep)
            {
                case 0:
                    this.label6.Text = "请输入要购买的商品编号";
                    this.label9.Text = "请选择要购买的商品";
                    this.label1.Text = "";
                    break;
                case 1:
                    this.label6.Text = "商品对应货道不可用";
                    this.label9.Text = "商品对应货道不可用";
                    this.label1.Text = "";
                    break;
                case 2:
                    this.label6.Text = "商品库存不足";
                    this.label9.Text = "商品库存不足";
                    this.label1.Text = "";
                    break;
                case 3:
                    if (tempAisleNUM == 0)
                    {
                        this.label6.Text = "请选择要购买的商品";
                        this.label9.Text = "请选择要购买的商品";
                        this.label1.Text = "";
                    }
                    else
                    {

                        this.label6.Text = "商品编号不正确";
                        this.label9.Text = "商品编号不正确";
                        this.label1.Text = "";
                    }
                    break;
                case 4:
                    this.label9.Text = "请选择要购买的商品";
                    if (isextbusy == 0)
                    {
                        double mtempjiage = double.Parse(textBox5.Text.Substring(0, textBox5.Text.IndexOf("元")));
                        if (huohao > 0)
                        {
                            switch (nowpaytype)
                            {
                                case 1:
                                    this.label6.Text = "请用手机扫屏幕上二维码";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                                case 2:
                                    this.label6.Text = "请打开支付宝扫屏幕上的二维码";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                                case 3:
                                    this.label6.Text = "请打开微信扫屏幕上的二维码";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                                case 4:
                                    this.label6.Text = "请用各手机钱包扫屏幕上二维码";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                                case 5:
                                    this.label6.Text = "请刷银联闪付卡或手机NFC支付";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                                case 6:
                                    this.label6.Text = "请刷会员卡支付";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                                default:
                                    this.label6.Text = "请用手机扫屏幕上二维码";
                                    this.label1.Text = "或选择其他支付方式出货";
                                    break;
                            }
                        }
                        else
                        {
                            this.label6.Text = "出货完成.";
                            this.label1.Text = "";
                        }
                    }
                    else if (isextbusy == 1)
                    {
                        this.label6.Text = "正在出货,请稍后...";
                        this.label1.Text = "";
                    }
                    else if (isextbusy == 2)
                    {
                        this.label6.Text = "出货结束.";
                        this.label1.Text = "";
                    }
                    break;
            }
            System.Windows.Forms.Application.DoEvents();
        }
        /// <summary>
        /// 更新商品信息
        /// </summary>
        private void updateshangpin(string tempshangpinnum)
        {
            int i;
            try
            {
                tempAisleNUM = Convert.ToInt32(tempshangpinnum, 10);
            }
            catch//货道文本非数字
            {
                tempAisleNUM = 0;
            }
            
            for (i = 0; i < mynodelistshangpin.Count; i++)
            {
                if (tempAisleNUM == int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value))
                {
                    this.textBox5.Text = mynodelistshangpin[i].Attributes.GetNamedItem("jiage").Value+"元";
                    int j;
                    for (j = 0; j < cmimagefiles.Length; j++)
                    {
                        int mystartindex = cmimagefiles[j].LastIndexOf('\\');
                        int myendindex = cmimagefiles[j].LastIndexOf('.');
                        string mycmname = cmimagefiles[j].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                        if ((mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value == mycmname))//文件名正确
                        {
                            try
                            {
                                pictureBox2.Load(cmimagefiles[j]);
                                pictureBox8.Load(cmimagefiles[j]);
                            }
                            catch
                            {
                            }
                            break;
                        }
                    }
                    if (j == cmimagefiles.Length)//未找到图片
                    {
                        try
                        {
                            pictureBox2.Image = global::SHJ.Properties.Resources.shangpin;
                            pictureBox8.Image = global::SHJ.Properties.Resources.shangpin;
                        }
                        catch
                        {
                        }
                    }
                    if (myfunctionnode.Attributes.GetNamedItem("fenbianlv").Value == "0")//1920x1080
                    {
                        for (j = 0; j < bcmimagefiles.Length; j++)
                        {
                            int mystartindex = bcmimagefiles[j].LastIndexOf('\\');
                            int myendindex = bcmimagefiles[j].LastIndexOf('.');
                            string mycmname = bcmimagefiles[j].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                            if ((mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value == mycmname))//文件名正确
                            {
                                try
                                {
                                    this.pictureBox2.Location = new Point(40, 220);
                                    this.pictureBox5.Size = new Size(400, 600);
                                    this.pictureBox5.Location = new Point(640, 220);
                                    pictureBox5.Load(bcmimagefiles[j]);
                                }
                                catch
                                {
                                }
                                break;
                            }
                        }
                        if (j == bcmimagefiles.Length)//未找到图片
                        {
                            try
                            {
                                pictureBox5.Image = null;
                                this.pictureBox2.Location = new Point(250, 220);
                                this.pictureBox5.Size = new Size(0, 0);
                            }
                            catch
                            {
                            }
                        }
                    }
                    //查找库存和货道状态
                    if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("state").Value) != 0)//货道停售（状态）
                    {
                        BUYstep = 1;
                    }
                    for (int k = 0; k < mynodelisthuodao.Count; k++)
                    {
                        if(int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                            ==int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                        {
                            int totalkuncun = 0;//计算总库存
                            if (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态） 
                            {
                                totalkuncun += int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value);//对应货道库存
                            }
                            for (int index = 0; index < mynodelisthuodao.Count; index++)
                            {
                                if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                     == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                {
                                    if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                    {
                                        totalkuncun += int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value);
                                    }
                                }
                            }                         
                            
                            if (totalkuncun == 0)
                            {
                                BUYstep = 2;
                                return;
                            }
                        }
                    }
                    
                    {
                        BUYstep = 4;
                    }                  
                    break;
                }
            }
            if (i == mynodelistshangpin.Count)//未找到商品编号,编号不正确
            {
                BUYstep = 3;
                this.textBox5.Text = "0元";
                pictureBox2.Image = null;
                pictureBox5.Image = null;
                return;
            }
        }
        
        /// <summary> 
        /// 读取机器类型 
        private void Getvendortype()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode xNode;
            try
            {
                xDoc.Load("conf.config");

                xNode = xDoc.SelectSingleNode("//appSettings");
                vendortype = xNode.SelectSingleNode("setting3").Attributes.GetNamedItem("value").Value;
                localsalerID = xNode.SelectSingleNode("setting4").Attributes.GetNamedItem("value").Value;
                //PLCPORT = xNode.SelectSingleNode("setting5").Attributes.GetNamedItem("PLCPORT").Value;
                //VMPORT = xNode.SelectSingleNode("setting5").Attributes.GetNamedItem("VMPORT").Value;
            }
            catch
            {
            }
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        private void initconfigxml()
        {
            myxmldoc.RemoveAll();//去除所有节点
            myxmldoc.CreateXmlDeclaration("1.0", "utf-8","yes");
            //创建根节点1
            XmlNode rootNode = myxmldoc.CreateElement("config");//配置定义

            XmlNode NetNode = myxmldoc.CreateElement("netconfig");//网络定义
            XmlAttribute ipconfigAttribute = myxmldoc.CreateAttribute("ipconfig");//IP地址
            ipconfigAttribute.Value = "120.77.110.254";
            NetNode.Attributes.Append(ipconfigAttribute);//xml节点附件属性
            XmlAttribute portAttribute = myxmldoc.CreateAttribute("port");//端口号
            portAttribute.Value = "5006";
            NetNode.Attributes.Append(portAttribute);//xml节点附件属性
            XmlAttribute netdelayAttribute = myxmldoc.CreateAttribute("netdelay");//网络发送延时（间隔）
            netdelayAttribute.Value = "60";
            NetNode.Attributes.Append(netdelayAttribute);//xml节点附件属性
            rootNode.AppendChild(NetNode);

            XmlNode functionNode = myxmldoc.CreateElement("function");//功能定义
            XmlAttribute netlogAttribute = myxmldoc.CreateAttribute("netlog");//网络日志
            netlogAttribute.Value = "0";
            functionNode.Attributes.Append(netlogAttribute);//xml节点附件属性
            XmlAttribute kucunguanliAttribute = myxmldoc.CreateAttribute("kucunguanli");//库存管理
            kucunguanliAttribute.Value = "0";
            functionNode.Attributes.Append(kucunguanliAttribute);//xml节点附件属性
            XmlAttribute mimaAttribute = myxmldoc.CreateAttribute("mima");//密码
            mimaAttribute.Value = "1314";
            functionNode.Attributes.Append(mimaAttribute);//xml节点附件属性
            XmlAttribute touchAttribute = myxmldoc.CreateAttribute("touch");//是否支持?
            touchAttribute.Value = "1";
            functionNode.Attributes.Append(touchAttribute);//xml节点附件属性
            XmlAttribute temp1Attribute = myxmldoc.CreateAttribute("temperature1");//温区1温度
            temp1Attribute.Value = "25";
            functionNode.Attributes.Append(temp1Attribute);//xml节点附件属性
            XmlAttribute temp2Attribute = myxmldoc.CreateAttribute("temperature2");//温区2温度
            temp2Attribute.Value = "25";
            functionNode.Attributes.Append(temp2Attribute);//xml节点附件属性
            XmlAttribute fenbianlvAttribute = myxmldoc.CreateAttribute("fenbianlv");//分辨率
            fenbianlvAttribute.Value = "0";
            functionNode.Attributes.Append(fenbianlvAttribute);//xml节点附件属性
            XmlAttribute addateAttribute = myxmldoc.CreateAttribute("addate");//广告更新时间
            addateAttribute.Value = "20160101010101";
            functionNode.Attributes.Append(addateAttribute);//xml节点附件属性
            XmlAttribute adurlAttribute = myxmldoc.CreateAttribute("adurl");//广告地址
            adurlAttribute.Value = "";
            functionNode.Attributes.Append(adurlAttribute);//xml节点附件属性
            XmlAttribute adupdateAttribute = myxmldoc.CreateAttribute("adupdate");//广告推送是否打开
            adupdateAttribute.Value = "0";
            functionNode.Attributes.Append(adupdateAttribute);//xml节点附件属性

            XmlAttribute vendortypeAttribute = myxmldoc.CreateAttribute("vendortype");//机器类型0印章机
            vendortypeAttribute.Value = "0";
            functionNode.Attributes.Append(vendortypeAttribute);//xml节点附件属性

            rootNode.AppendChild(functionNode);

            XmlNode config1Node = myxmldoc.CreateElement("payconfig");//支付定义

            XmlAttribute allpayAttribute = myxmldoc.CreateAttribute("allpay");//第一位为支付宝、第二位为微信、第三位为一码付、第四位为银联闪付、第五位为提货码、第六位为微信刷脸、第七位为支付宝刷脸
            allpayAttribute.Value = "0";
            config1Node.Attributes.Append(allpayAttribute);//xml节点附件属性
            XmlAttribute zhekouAttribute = myxmldoc.CreateAttribute("zhekou");//折扣
            zhekouAttribute.Value = "100";
            config1Node.Attributes.Append(zhekouAttribute);//xml节点附件属性
            rootNode.AppendChild(config1Node);

            //创建根节点2
            XmlNode config2Node = myxmldoc.CreateElement("shangpin");//商品定义
            for (int i = 1; i <= totalshangpinnum; i++)
            {
                //创建货道节点
                XmlNode shangpinNode = myxmldoc.CreateElement("shangpin"+(i-1).ToString());//商品定义
                XmlAttribute shangpinnumAttribute = myxmldoc.CreateAttribute("shangpinnum");//商品编号
                shangpinnumAttribute.Value = i.ToString("000");
                shangpinNode.Attributes.Append(shangpinnumAttribute);//xml节点附件属性
                XmlAttribute shangpinnameAttribute = myxmldoc.CreateAttribute("shangpinname");//对应商品名称
                shangpinnameAttribute.Value = "";
                shangpinNode.Attributes.Append(shangpinnameAttribute);//xml节点附件属性
                XmlAttribute jiageAttribute = myxmldoc.CreateAttribute("jiage");//商品价格
                jiageAttribute.Value = "0.1";
                shangpinNode.Attributes.Append(jiageAttribute);//xml节点附件属性
                XmlAttribute huodaoAttribute = myxmldoc.CreateAttribute("huodao");//货道定义
                huodaoAttribute.Value = i.ToString();
                shangpinNode.Attributes.Append(huodaoAttribute);//xml节点附件属性

                XmlAttribute stateAttribute = myxmldoc.CreateAttribute("state");//商品状态
                stateAttribute.Value = "0";//默认正常
                shangpinNode.Attributes.Append(stateAttribute);//xml节点附件属性
                XmlAttribute salesumAttribute = myxmldoc.CreateAttribute("salesum");//商品销售统计
                salesumAttribute.Value = "0";//默认正常
                shangpinNode.Attributes.Append(salesumAttribute);//xml节点附件属性
                config2Node.AppendChild(shangpinNode);
            }
            rootNode.AppendChild(config2Node);


            //创建根节点3
            XmlNode config3Node = myxmldoc.CreateElement("huodao");//商品定义
            for (int i = 1; i <= totalhuodaonum; i++)
            {
                //创建货道节点
                XmlNode huodaoNode = myxmldoc.CreateElement("huodao" + (i - 1).ToString());//货道定义
                XmlAttribute huodaonumAttribute = myxmldoc.CreateAttribute("huodaonum");//货道编号
                huodaonumAttribute.Value = i.ToString();
                huodaoNode.Attributes.Append(huodaonumAttribute);//xml节点附件属性
                XmlAttribute fenzuAttribute = myxmldoc.CreateAttribute("fenzu");//货道分组定义默认不分组
                fenzuAttribute.Value = i.ToString();
                huodaoNode.Attributes.Append(fenzuAttribute);//xml节点附件属性
                XmlAttribute kucunAttribute = myxmldoc.CreateAttribute("kucun");//货道库存
                kucunAttribute.Value = "255";
                huodaoNode.Attributes.Append(kucunAttribute);//xml节点附件属性
                XmlAttribute stateAttribute = myxmldoc.CreateAttribute("state");//货道状态
                stateAttribute.Value = "0";//默认正常
                huodaoNode.Attributes.Append(stateAttribute);//xml节点附件属性
                XmlAttribute typeAttribute = myxmldoc.CreateAttribute("volume");//货道容量
                typeAttribute.Value = "8";//默认正常
                huodaoNode.Attributes.Append(typeAttribute);//xml节点附件属性
                XmlAttribute positionAttribute = myxmldoc.CreateAttribute("position");//印章类型1：1010，2：2020，3：2530，其他2530
                positionAttribute.Value = "3";//默认2530
                huodaoNode.Attributes.Append(positionAttribute);//xml节点附件属性
                XmlAttribute fangxiangAttribute = myxmldoc.CreateAttribute("fangxiang");//货道坐标
                fangxiangAttribute.Value = ((i - 1) / 10 + 1).ToString();
                huodaoNode.Attributes.Append(fangxiangAttribute);//xml节点附件属性

                config3Node.AppendChild(huodaoNode);
            }
            rootNode.AppendChild(config3Node);

            myxmldoc.AppendChild(rootNode);
        }

        /// <summary>
        /// 初始化销售记录文件
        /// </summary>
        public static void initsalexml()
        {
            mysalexmldoc.RemoveAll();//去除所有节点
            mysalexmldoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            //创建根节点
            XmlNode rootNode = mysalexmldoc.CreateElement("sale");//配置定义

            //创建销售数据节点1
            XmlNode sale1Node = mysalexmldoc.CreateElement("chuhuo");//出货定义
            for (int i = 0; i < 500; i++)
            {
                //创建货道节点
                XmlNode chuhuoNode = mysalexmldoc.CreateElement("chuhuo" + i.ToString());//出货定义
                XmlAttribute timeAttribute = mysalexmldoc.CreateAttribute("time");//时间戳
                timeAttribute.Value = "";
                chuhuoNode.Attributes.Append(timeAttribute);//xml节点附件属性
                XmlAttribute shangpinnumAttribute = mysalexmldoc.CreateAttribute("shangpinnum");//对应商品编号
                shangpinnumAttribute.Value = "";
                chuhuoNode.Attributes.Append(shangpinnumAttribute);//xml节点附件属性
                XmlAttribute jiageAttribute = mysalexmldoc.CreateAttribute("jiage");//商品价格
                jiageAttribute.Value = "";
                chuhuoNode.Attributes.Append(jiageAttribute);//xml节点附件属性
                XmlAttribute typeAttribute = mysalexmldoc.CreateAttribute("type");//支付方式
                typeAttribute.Value = "";
                chuhuoNode.Attributes.Append(typeAttribute);//xml节点附件属性

                XmlAttribute startAttribute = mysalexmldoc.CreateAttribute("start");//是否是最新记录
                startAttribute.Value = "";
                chuhuoNode.Attributes.Append(startAttribute);//xml节点附件属性

                sale1Node.AppendChild(chuhuoNode);
            }
            rootNode.AppendChild(sale1Node);

            //创建销售数据节点1
            XmlNode sale2Node = mysalexmldoc.CreateElement("pay");//支付定义
            for (int i = 0; i < 500; i++)
            {
                //创建货道节点
                XmlNode payNode = mysalexmldoc.CreateElement("pay" + i.ToString());//支付定义
                XmlAttribute timeAttribute = mysalexmldoc.CreateAttribute("time");//时间戳
                timeAttribute.Value = "";
                payNode.Attributes.Append(timeAttribute);//xml节点附件属性
                XmlAttribute moneyAttribute = mysalexmldoc.CreateAttribute("money");//支付金额
                moneyAttribute.Value = "";
                payNode.Attributes.Append(moneyAttribute);//xml节点附件属性
                XmlAttribute typeAttribute = mysalexmldoc.CreateAttribute("type");//支付方式
                typeAttribute.Value = "";
                payNode.Attributes.Append(typeAttribute);//xml节点附件属性

                XmlAttribute startAttribute = mysalexmldoc.CreateAttribute("start");//是否是最新记录
                startAttribute.Value = "";
                payNode.Attributes.Append(startAttribute);//xml节点附件属性

                sale2Node.AppendChild(payNode);
            }
            rootNode.AppendChild(sale2Node);

            mysalexmldoc.AppendChild(rootNode);
        }

        /// <summary>
        /// 初始化注册文件
        /// </summary>
        public void initregxml()
        {
            myregxmldoc.RemoveAll();//去除所有节点
            myregxmldoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            //创建根节点
            XmlNode rootNode = myregxmldoc.CreateElement("reg");//配置定义
            XmlAttribute regAttribute0 = myregxmldoc.CreateAttribute("regid");//注册号
            regAttribute0.Value = "0";
            rootNode.Attributes.Append(regAttribute0);//xml节点附件属性

            myregxmldoc.AppendChild(rootNode);
        }

        //public void initPLCxml()
        //{
        //    PLCxmldoc.RemoveAll();//去除所有节点
        //    PLCxmldoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
        //    XmlNode rootNode = PLCxmldoc.CreateElement("config");//配置定义

        //    XmlNode COMNode = PLCxmldoc.CreateElement("comconfig");//串口定义
        //    XmlAttribute baudrateAttribute = PLCxmldoc.CreateAttribute("baudrate");//波特率
        //    baudrateAttribute.Value = "38400";
        //    COMNode.Attributes.Append(baudrateAttribute);//xml节点附件属性
        //    XmlAttribute databitsAttribute = PLCxmldoc.CreateAttribute("databits");//数据位
        //    databitsAttribute.Value = "8";
        //    COMNode.Attributes.Append(databitsAttribute);//xml节点附件属性
        //    XmlAttribute parityAttribute = PLCxmldoc.CreateAttribute("parity");//校验位
        //    parityAttribute.Value = "None";
        //    COMNode.Attributes.Append(parityAttribute);//xml节点附件属性
        //    XmlAttribute stopbitsAttribute = PLCxmldoc.CreateAttribute("stopbits");//停止位
        //    stopbitsAttribute.Value = "One";
        //    COMNode.Attributes.Append(stopbitsAttribute);//xml节点附件属性
        //    rootNode.AppendChild(COMNode);

        //    //创建根节点2
        //    XmlNode config2Node = PLCxmldoc.CreateElement("bitdata");//商品定义
        //    for (int i = 0; i < Modbus.GetAddressValueLength(Modbus.D_BitM); i++)
        //    {
        //        //创建货道节点
        //        XmlNode dataNode = PLCxmldoc.CreateElement("dataM" + i.ToString());//数据定义
        //        XmlAttribute datanumAttribute = PLCxmldoc.CreateAttribute("datanum");//数据编号
        //        datanumAttribute.Value = "M"+i.ToString();
        //        dataNode.Attributes.Append(datanumAttribute);//xml节点附件属性

        //        XmlAttribute datanameAttribute = PLCxmldoc.CreateAttribute("dataname");//数据名称
        //        datanameAttribute.Value = "数据"+i.ToString("000");
        //        dataNode.Attributes.Append(datanameAttribute);//xml节点附件属性

        //        XmlAttribute dataaddrAttribute = PLCxmldoc.CreateAttribute("dataaddr");//数据地址
        //        dataaddrAttribute.Value = "0x"+(Modbus.D_BitM+i).ToString("X4");
        //        dataNode.Attributes.Append(dataaddrAttribute);//xml节点附件属性

        //        XmlAttribute datavalueAttribute = PLCxmldoc.CreateAttribute("datavalue");//数据值
        //        datavalueAttribute.Value = "0";
        //        dataNode.Attributes.Append(datavalueAttribute);//xml节点附件属性

        //        XmlAttribute iswriteAttribute = PLCxmldoc.CreateAttribute("iswrite");//数据是否可以写入
        //        iswriteAttribute.Value = "0";
        //        dataNode.Attributes.Append(iswriteAttribute);//xml节点附件属性

        //        config2Node.AppendChild(dataNode);
        //    }

        //    for (int i = 0; i < Modbus.GetAddressValueLength(Modbus.D_BitX); i++)
        //    {
        //        //创建货道节点
        //        XmlNode dataNode = PLCxmldoc.CreateElement("dataX" + i.ToString());//数据定义
        //        XmlAttribute datanumAttribute = PLCxmldoc.CreateAttribute("datanum");//数据编号
        //        datanumAttribute.Value = "X" + i.ToString();
        //        dataNode.Attributes.Append(datanumAttribute);//xml节点附件属性

        //        XmlAttribute datanameAttribute = PLCxmldoc.CreateAttribute("dataname");//数据名称
        //        datanameAttribute.Value = "数据" + i.ToString("000");
        //        dataNode.Attributes.Append(datanameAttribute);//xml节点附件属性

        //        XmlAttribute dataaddrAttribute = PLCxmldoc.CreateAttribute("dataaddr");//数据地址
        //        dataaddrAttribute.Value = "0x" + (Modbus.D_BitX+i).ToString("X4");
        //        dataNode.Attributes.Append(dataaddrAttribute);//xml节点附件属性

        //        XmlAttribute datavalueAttribute = PLCxmldoc.CreateAttribute("datavalue");//数据值
        //        datavalueAttribute.Value = "0";
        //        dataNode.Attributes.Append(datavalueAttribute);//xml节点附件属性

        //        XmlAttribute iswriteAttribute = PLCxmldoc.CreateAttribute("iswrite");//数据是否可以写入
        //        iswriteAttribute.Value = "0";
        //        dataNode.Attributes.Append(iswriteAttribute);//xml节点附件属性

        //        config2Node.AppendChild(dataNode);
        //    }
        //    rootNode.AppendChild(config2Node);


        //    //创建根节点3
        //    XmlNode config3Node = PLCxmldoc.CreateElement("worddata");//商品定义
        //    for (int i = 0; i < Modbus.GetAddressValueLength(Modbus.D_WordD0); i++)
        //    {
        //        //创建货道节点
        //        XmlNode dataNode = PLCxmldoc.CreateElement("dataD" + i.ToString());//数据定义
        //        XmlAttribute datanumAttribute = PLCxmldoc.CreateAttribute("datanum");//数据编号
        //        datanumAttribute.Value = "D" + i.ToString();
        //        dataNode.Attributes.Append(datanumAttribute);//xml节点附件属性

        //        XmlAttribute datanameAttribute = PLCxmldoc.CreateAttribute("dataname");//数据名称
        //        datanameAttribute.Value = "数据" + i.ToString("000");
        //        dataNode.Attributes.Append(datanameAttribute);//xml节点附件属性

        //        XmlAttribute dataaddrAttribute = PLCxmldoc.CreateAttribute("dataaddr");//数据地址
        //        dataaddrAttribute.Value = "0x" + (Modbus.D_WordD0+i).ToString("X4");
        //        dataNode.Attributes.Append(dataaddrAttribute);//xml节点附件属性

        //        XmlAttribute datavalueAttribute = PLCxmldoc.CreateAttribute("datavalue");//数据值
        //        datavalueAttribute.Value = "0";
        //        dataNode.Attributes.Append(datavalueAttribute);//xml节点附件属性

        //        XmlAttribute iswriteAttribute = PLCxmldoc.CreateAttribute("iswrite");//数据是否可以写入
        //        iswriteAttribute.Value = "0";
        //        dataNode.Attributes.Append(iswriteAttribute);//xml节点附件属性

        //        config3Node.AppendChild(dataNode);
        //    }

        //    for (int i = 0; i < Modbus.GetAddressValueLength(Modbus.D_WordD8000); i++)
        //    {
        //        //创建货道节点
        //        XmlNode dataNode = PLCxmldoc.CreateElement("dataD" + (i+8000).ToString());//数据定义
        //        XmlAttribute datanumAttribute = PLCxmldoc.CreateAttribute("datanum");//数据编号
        //        datanumAttribute.Value = "D" + (i + 8000).ToString();
        //        dataNode.Attributes.Append(datanumAttribute);//xml节点附件属性

        //        XmlAttribute datanameAttribute = PLCxmldoc.CreateAttribute("dataname");//数据名称
        //        datanameAttribute.Value = "数据" + i.ToString("000");
        //        dataNode.Attributes.Append(datanameAttribute);//xml节点附件属性

        //        XmlAttribute dataaddrAttribute = PLCxmldoc.CreateAttribute("dataaddr");//数据地址
        //        dataaddrAttribute.Value = "0x" + (Modbus.D_WordD8000+i).ToString("X4");
        //        dataNode.Attributes.Append(dataaddrAttribute);//xml节点附件属性

        //        XmlAttribute datavalueAttribute = PLCxmldoc.CreateAttribute("datavalue");//数据值
        //        datavalueAttribute.Value = "0";
        //        dataNode.Attributes.Append(datavalueAttribute);//xml节点附件属性

        //        XmlAttribute iswriteAttribute = PLCxmldoc.CreateAttribute("iswrite");//数据是否可以写入
        //        iswriteAttribute.Value = "0";
        //        dataNode.Attributes.Append(iswriteAttribute);//xml节点附件属性

        //        config3Node.AppendChild(dataNode);
        //    }
        //    rootNode.AppendChild(config3Node);

        //    PLCxmldoc.AppendChild(rootNode);
        //}
        /// <summary>
        /// 更新各个配置节点路径
        /// </summary>
        private void updatenodeaddress()
        {
            mynetcofignode = myxmldoc.SelectSingleNode("config").SelectSingleNode("netconfig");
            myfunctionnode = myxmldoc.SelectSingleNode("config").SelectSingleNode("function");
            mypayconfignode = myxmldoc.SelectSingleNode("config").SelectSingleNode("payconfig");
            mynodelistshangpin = myxmldoc.SelectSingleNode("config").SelectSingleNode("shangpin").ChildNodes;
            mynodelisthuodao = myxmldoc.SelectSingleNode("config").SelectSingleNode("huodao").ChildNodes;
            mynodelistchuhuo = mysalexmldoc.SelectSingleNode("sale").SelectSingleNode("chuhuo").ChildNodes;
            mynodelistpay = mysalexmldoc.SelectSingleNode("sale").SelectSingleNode("pay").ChildNodes;
            //PLCnodelistbitdata = PLCxmldoc.SelectSingleNode("config").SelectSingleNode("bitdata").ChildNodes;
            //PLCnodelistworddata = PLCxmldoc.SelectSingleNode("config").SelectSingleNode("worddata").ChildNodes;
            try
            {
                paytypes = int.Parse(mypayconfignode.Attributes.GetNamedItem("allpay").Value);
            }
            catch
            {
            }
            updatepaytypes();
            try
            {
                Getvendortype();
                if (vendortype != myfunctionnode.Attributes.GetNamedItem("vendortype").Value)
                {
                    myfunctionnode.Attributes.GetNamedItem("vendortype").Value = vendortype;
                    mysalexmldoc.Save(salexmlfile);
                    mysalexmldoc.Save(salexmlfilecopy);
                }
                if (myfunctionnode.Attributes.GetNamedItem("vendortype").Value == "1")//印章打印机
                {
                    button1.Visible = true;
                }
                else
                {
                    button1.Visible = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// 添加销售记录
        /// </summary>
        private void addsalerecord()
        {
            int i;
            for (i = 0; i < mynodelistchuhuo.Count; i++)
            {
                if (mynodelistchuhuo[i].Attributes.GetNamedItem("start").Value == "1")
                {
                    mynodelistchuhuo[i].Attributes.GetNamedItem("time").Value = DateTime.Now.ToString("MM-dd HH:mm:ss");
                    mynodelistchuhuo[i].Attributes.GetNamedItem("shangpinnum").Value = huohao.ToString();
                    mynodelistchuhuo[i].Attributes.GetNamedItem("jiage").Value = shangpinjiage.ToString();
                    mynodelistchuhuo[i].Attributes.GetNamedItem("type").Value = zhifutype.ToString();
                    mynodelistchuhuo[i].Attributes.GetNamedItem("start").Value = "";
                    if (i == mynodelistchuhuo.Count - 1)
                    {
                        mynodelistchuhuo[0].Attributes.GetNamedItem("start").Value = "1";
                    }
                    else
                    {
                        mynodelistchuhuo[i+1].Attributes.GetNamedItem("start").Value = "1";
                    }
                    break;
                }
            }
            if (i == mynodelistchuhuo.Count)//未找到起始位置
            {
                mynodelistchuhuo[0].Attributes.GetNamedItem("time").Value = DateTime.Now.ToString("MM-dd HH:mm:ss");
                mynodelistchuhuo[0].Attributes.GetNamedItem("shangpinnum").Value = huohao.ToString();
                mynodelistchuhuo[0].Attributes.GetNamedItem("jiage").Value = shangpinjiage.ToString();
                mynodelistchuhuo[0].Attributes.GetNamedItem("type").Value = zhifutype.ToString();
                mynodelistchuhuo[0].Attributes.GetNamedItem("start").Value = "";
                mynodelistchuhuo[1].Attributes.GetNamedItem("start").Value = "1";
            }
            mysalexmldoc.Save(salexmlfile);
            mysalexmldoc.Save(salexmlfilecopy);
        }

        /// <summary>
        /// 添加支付记录
        /// </summary>
        /// <param name="money">支付金额</param>
        /// <param name="type">支付方式</param>
        private void addpayrecord(double money,string type)
        {
            int i;
            for (i = 0; i < mynodelistpay.Count; i++)
            {
                if (mynodelistpay[i].Attributes.GetNamedItem("start").Value == "1")
                {
                    mynodelistpay[i].Attributes.GetNamedItem("time").Value = DateTime.Now.ToString("MM-dd HH:mm:ss");
                    mynodelistpay[i].Attributes.GetNamedItem("money").Value = money.ToString();
                    mynodelistpay[i].Attributes.GetNamedItem("type").Value = type;
                    mynodelistpay[i].Attributes.GetNamedItem("start").Value = "";
                    if (i == mynodelistpay.Count - 1)
                    {
                        mynodelistpay[0].Attributes.GetNamedItem("start").Value = "1";
                    }
                    else
                    {
                        mynodelistpay[i + 1].Attributes.GetNamedItem("start").Value = "1";
                    }
                    break;
                }
            }
            if (i == mynodelistpay.Count)//未找到起始位置
            {
                mynodelistpay[0].Attributes.GetNamedItem("time").Value = DateTime.Now.ToString("MM-dd HH:mm:ss");
                mynodelistpay[0].Attributes.GetNamedItem("money").Value = money.ToString();
                mynodelistpay[0].Attributes.GetNamedItem("type").Value = type;
                mynodelistpay[0].Attributes.GetNamedItem("start").Value = "";
                mynodelistpay[1].Attributes.GetNamedItem("start").Value = "1";
            }
            mysalexmldoc.Save(salexmlfile);
            mysalexmldoc.Save(salexmlfilecopy);
        }

        /// <summary>
        /// 设置出货
        /// </summary>
        private void setchuhuo()
        {
            //出货设置
            //setextenddata = 0x01;
            //needsetextend = true;
            Aisleoutcount = 1;//超时计时开始
            isextbusy = 1;//正在出货

        }
        /// <summary>
        /// 更新商品列表
        /// </summary>
        /// <param name="page">页面号</param>
        private void updateshangpinlist(int page)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            imageList1.Images.Clear();
            int imageindex = 0;
            for (int i = 0; i < cmimagefiles.Length; i++)//商品触摸列表
            {
                try
                {
                    int mystartindex = cmimagefiles[i].LastIndexOf('\\');
                    int myendindex = cmimagefiles[i].LastIndexOf('.');
                    bool mycontainpic = cmimagefiles[i].EndsWith(".bmp") || cmimagefiles[i].EndsWith(".jpg")
                        || cmimagefiles[i].EndsWith(".png") || cmimagefiles[i].EndsWith(".gif")
                        || cmimagefiles[i].EndsWith(".tif") || cmimagefiles[i].EndsWith(".jpeg");
                    string mycmname = cmimagefiles[i].Substring(mystartindex + 1, myendindex - mystartindex - 1);
                    double jiageshow = 0;
                    int totalkuncun = 0;//计算总库存
                    string shangpinnameshow = "";
                    for (int j = 0; j < mynodelistshangpin.Count; j++)//查找是否有配置数据
                    {
                        if (mynodelistshangpin[j].Attributes.GetNamedItem("shangpinnum").Value == mycmname)
                        {
                            jiageshow = double.Parse(mynodelistshangpin[j].Attributes.GetNamedItem("jiage").Value);
							//查找库存状态
                            for (int k = 0; k < mynodelisthuodao.Count; k++)
                            {
                                if (int.Parse(mynodelistshangpin[j].Attributes.GetNamedItem("huodao").Value)
                                    == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                                {
                                    if (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态） 
                                    {
                                        totalkuncun += int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value);//对应货道库存
                                    }
                                    for (int index = 0; index < mynodelisthuodao.Count; index++)
                                    {
                                        if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                             == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                        {
                                            if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                            {
                                                totalkuncun += int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }

                            if ((mystartindex >= 0) && (myendindex >= 0) && (mycontainpic == true))//文件名正确
                            {
                                imageindex++;
                                if ((imageindex > page * cmnumsinpage) && (imageindex <= (page + 1) * cmnumsinpage))//本页的图片
                                {
                                    try
                                    {
                                        string tempnameunicode = mynodelistshangpin[j].Attributes.GetNamedItem("shangpinname").Value;
                                        while (tempnameunicode.Length > 0)
                                        {
                                            byte[] bytes = new byte[2];
                                            bytes[1] = byte.Parse(tempnameunicode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                                            bytes[0] = byte.Parse(tempnameunicode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                                            shangpinnameshow += Encoding.Unicode.GetString(bytes);
                                            tempnameunicode = tempnameunicode.Substring(4);
                                        }
                                        imageList1.Images.Add(Image.FromFile(cmimagefiles[i]));
                                    }
                                    catch
                                    {
                                        shangpinnameshow = "编号:" + mycmname;
                                        imageList1.Images.Add(global::SHJ.Properties.Resources.shangpin);
                                    }
                                    ListViewItem lvi = new ListViewItem();
                                    lvi.ImageIndex = imageindex - 1 - page * cmnumsinpage;
                                    lvi.Name = mycmname;
                                    string kucunstr = "";
                                    if (totalkuncun == 0)
                                    {
                                        kucunstr = " 已售空";
                                    }
                                    lvi.Text = jiageshow.ToString("C") + kucunstr + "\r\n" + shangpinnameshow;
                                    listView1.Items.Add(lvi);
                                }
                            }
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
            listView1.EndUpdate();
            BUYstep=0;
        }
       
        private int totallistnums = 0;//计算商品页数

        private void InitFormsize()
        {
            //if (myfunctionnode.Attributes.GetNamedItem("fenbianlv").Value == "0")//1920x1080
            {
                this.Width = 1920;
                this.Height = 1080;
                this.pictureBox1.Dock = DockStyle.None;
                this.pictureBox1.Width = 0;
                this.pictureBox1.Height = 0;
                this.axWindowsMediaPlayer1.Width = 1920;//视频1000x525
                this.axWindowsMediaPlayer1.Height = 1080;
                this.axWindowsMediaPlayer1.uiMode = "None";
                this.axWindowsMediaPlayer1.stretchToFit = true;
                this.axWindowsMediaPlayer1.Location = new Point(0, 0);
                this.axWindowsMediaPlayer1.settings.autoStart = true;
                this.axWindowsMediaPlayer1.settings.setMode("loop", true);
                this.label10.Location = new Point(1300, 110);
                this.label9.Location = new Point(593, 130);
                this.imageList1.ImageSize = new Size(215, 215);
                this.listView1.Height = 775;

                cmnumsinpage = 21;
                this.listView1.Width = 1840;
                this.listView1.Location = new Point(40, 200);
                this.label12.Location = new Point(1040, 990);
                this.label13.Location = new Point(1200, 990);
                this.label14.Location = new Point(1120, 990);
                this.label35.Location = new Point(1800, 990);

                this.label12.Visible = true;
                this.label13.Visible = true;
                this.label14.Visible = true;

                this.label8.Location = new Point(1575, 110);

                this.label6.Location = new Point(1150, 275);
                this.label1.Location = new Point(1150, 340);
                this.label4.Location = new Point(1350, 630);
                this.textBox5.Location = new Point(1520, 623);
                this.pictureBox2.Size = new Size(600, 600);
                this.pictureBox2.Location = new Point(250, 220);
                this.pictureBox5.Size = new Size(0, 0);
                this.pictureBox3.Size = new Size(200, 200);
                this.pictureBox4.Size = new Size(250, 53);
                this.pictureBox3.Location = new Point(1405, 580);
                this.pictureBox4.Location = new Point(1380, 785);
                this.label7.Location = new Point(1110, 870);
                this.label3.Location = new Point(190, 870);
                this.label11.Location = new Point(920, 930);

                this.label2.Location = new Point(1400, 10);
                this.label5.Location = new Point(210, 740);
                this.label18.Location = new Point(210, 100);
                this.label19.Location = new Point(210, 200);
                this.label15.Location = new Point(668, 700);
                this.label16.Location = new Point(1133, 700);
                this.pictureBox6.Location = new Point(800, 800);
                this.pictureBox7.Location = new Point(550, 400);
                this.pictureBox8.Location = new Point(1020, 400);
                this.button1.Location = new Point(800,800);
            }

            needupdatePlaylist = true;
            if (totallistnums > 0)
            {
                cmliststotal = (totallistnums - 1) / cmnumsinpage + 1;//总共页数
            }
        }

        #endregion

        #region 服务器操作

        /// <summary>
        /// 向服务器发送交易数据
        /// </summary>
        private void sendtrade()
        {
            if (netsendrecord[netsendindex, 0] != 0)
            {
                for (int i = 0; i < 34; i++)
                {
                    GSMTxBuffer[i] = netsendrecord[netsendindex, i];
                }
                myTcpCli.Sendbytes(GSMTxBuffer, 34);
                if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
                {
                    revstringnet = DateTime.Now.ToString() + "Send:";
                    for (int revcount = 0; revcount < 34; revcount++)
                    {
                        revstringnet += " " + Convert.ToString(GSMTxBuffer[revcount], 16);
                    }
                    netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                    netdatastream.WriteByte(0x0d);
                    netdatastream.WriteByte(0x0a);
                    netdatastream.Flush();
                }

                //netreturncount = 1;//超时计时开始
                netcount = 0;//状态数据发送间隔重新开始

                //如果是出货失败的不需要返回确认
                if (netsendrecord[netsendindex, 19] == 0xE3)
                {
                    for (int i = 0; i < 34; i++)
                    {
                        netsendrecord[netsendindex, i] = 0;
                    }
                    
                    netsendindex++;//序号增加1
                    if (netsendindex >= 200)
                        netsendindex = 0;
                    needsendrecordnum--;
                }
            }
        }

        /// <summary>
        /// 向服务器发送确认信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="state">状态</param>
        private void sendRETURNOK(int type, int state)
        {
            int i;
            GSMTxBuffer[0] = 0x01;
            GSMTxBuffer[1] = 0x71;
            GSMTxBuffer[2] = 0x00;
            GSMTxBuffer[3] = 0x22;
            for (i = 0; i < 15; i++)
            {
                GSMTxBuffer[4 + i] = IMEI[i];
            }
            GSMTxBuffer[19] = 0xff;
	        GSMTxBuffer[20] = 0x00;
            GSMTxBuffer[21] = (byte)state;
            GSMTxBuffer[22] = (byte)type;//类型为广告更新,图片确认等

            if (type == 3)
            {
                GSMTxBuffer[23] = (byte)shangpingnumreturn;
            }
            else
            {
                GSMTxBuffer[23] = 0x00;
            }
            GSMTxBuffer[24] = 0x00;
            GSMTxBuffer[25] = 0x00;

            if(netstep == 13)
            {
                GSMTxBuffer[26] = timerecord[3,0] ;
                GSMTxBuffer[27] = timerecord[3,1];
                GSMTxBuffer[28] = timerecord[3,2];
                GSMTxBuffer[29] = timerecord[3,3];
                GSMTxBuffer[30] = timerecord[3,4];
                GSMTxBuffer[31] = timerecord[3,5];
            }
            else
            {
                GSMTxBuffer[26] = (byte)(System.DateTime.Now.Year - 2000);
                GSMTxBuffer[27] = (byte)(System.DateTime.Now.Month);
                GSMTxBuffer[28] = (byte)(System.DateTime.Now.Day);
                GSMTxBuffer[29] = (byte)(System.DateTime.Now.Hour);
                GSMTxBuffer[30] = (byte)(System.DateTime.Now.Minute);
                GSMTxBuffer[31] = (byte)(System.DateTime.Now.Second);
            }
            

            GSMTxBuffer[32] = 0x0d;
            GSMTxBuffer[33] = 0x0a;
            myTcpCli.Sendbytes(GSMTxBuffer, 34);
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "Send:";
                for (int revcount = 0; revcount < 34; revcount++)
                {
                    revstringnet += " " + Convert.ToString(GSMTxBuffer[revcount], 16);
                }
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
            //netreturncount = 1;//超时计时开始
            netcount = 0;//状态数据发送间隔重新开始
        }

        /// <summary>
        /// 向服务器发送提货码
        /// </summary>
        private void sendtihuoma()
        {
            int i;
            GSMTxBuffer[0] = 0x01;
            GSMTxBuffer[1] = 0x81;
            GSMTxBuffer[2] = 0x00;
            GSMTxBuffer[3] = 0x2d;
            for (i = 0; i < 15; i++)
            {
                GSMTxBuffer[4 + i] = IMEI[i];
            }
            GSMTxBuffer[19] = 0x01;
            byte[] tihuomatemp=Encoding.ASCII.GetBytes(myTihuomastr);
            for (i = 0; i < 7; i++)
            {
                GSMTxBuffer[20 + i] = tihuomatemp[i];
            }
            for (i = 0; i < 10; i++)
            {
                GSMTxBuffer[27 + i] = 0x00;
            }

            GSMTxBuffer[37] = (byte)(System.DateTime.Now.Year - 2000);
            GSMTxBuffer[38] = (byte)(System.DateTime.Now.Month);
            GSMTxBuffer[39] = (byte)(System.DateTime.Now.Day);
            GSMTxBuffer[40] = (byte)(System.DateTime.Now.Hour);
            GSMTxBuffer[41] = (byte)(System.DateTime.Now.Minute);
            GSMTxBuffer[42] = (byte)(System.DateTime.Now.Second);

            GSMTxBuffer[43] = 0x0d;
            GSMTxBuffer[44] = 0x0a;
            myTcpCli.Sendbytes(GSMTxBuffer, 45);
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "Send:";
                for (int revcount = 0; revcount < 45; revcount++)
                {
                    revstringnet += " " + Convert.ToString(GSMTxBuffer[revcount], 16);
                }
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
            //netreturncount = 1;//超时计时开始
            netcount = 0;//状态数据发送间隔重新开始
        }

        private int suijinshu;
        /// <summary>
        ///  添加向服务器发送的数据
        /// </summary>
        /// <param name="tradetype">交易类型：收款，退款，出货</param>
        /// <param name="jine">金额</param>
        /// <param name="paytype">支付方式</param>
        /// <param name="netliushui">网络流水号</param>
        private void addnettrade(byte tradetype,double jine,byte paytype, int netliushui)
        {
            if (needsendrecordnum < 200)
            {
                netsendrecord[netsendrecordindex, 0] = 0x01;
                netsendrecord[netsendrecordindex, 1] = 0x71;
                netsendrecord[netsendrecordindex, 2] = 0x00;
                netsendrecord[netsendrecordindex, 3] = 0x22;
                for (int i = 0; i < 15; i++)
                {
                    netsendrecord[netsendrecordindex, 4 + i] = IMEI[i];
                }
                netsendrecord[netsendrecordindex, 19] = tradetype;
                netsendrecord[netsendrecordindex, 20] = (byte)(((int)(jine * 10)) >> 8);
                netsendrecord[netsendrecordindex, 21] = (byte)((int)(jine * 10));
                if ((tradetype == 0x03)|| (tradetype == 0xE3))//出货
                {
                    netsendrecord[netsendrecordindex, 22] = 0x00;
                    netsendrecord[netsendrecordindex, 23] = (byte)huohao;
                }
                else
                {
                    netsendrecord[netsendrecordindex, 22] = paytype;
                    netsendrecord[netsendrecordindex, 23] = 0x00;
                   
                }
                if ((paytype == 0x06) || (paytype == 0x07))//支付宝或微信
                {
                    netsendrecord[netsendrecordindex, 24] = (byte)(((int)netliushui) >> 8);
                    netsendrecord[netsendrecordindex, 25] = (byte)((int)netliushui);
                }
                else
                {
                    netsendrecord[netsendrecordindex, 24] = 0x00;
                    netsendrecord[netsendrecordindex, 25] = 0x00;
                }
                netsendrecord[netsendrecordindex, 26] = (byte)(System.DateTime.Now.Year - 2000);
                netsendrecord[netsendrecordindex, 27] = (byte)(System.DateTime.Now.Month);
                netsendrecord[netsendrecordindex, 28] = (byte)(System.DateTime.Now.Day);
                netsendrecord[netsendrecordindex, 29] = (byte)(System.DateTime.Now.Hour);
                netsendrecord[netsendrecordindex, 30] = (byte)(System.DateTime.Now.Minute);
                suijinshu = suijinshu % 3;
                netsendrecord[netsendrecordindex, 31] = (byte)(System.DateTime.Now.Second + suijinshu * 60);
                suijinshu++;

                netsendrecord[netsendrecordindex, 32] = 0x0d;
                netsendrecord[netsendrecordindex, 33] = 0x0a;

                netsendrecordindex++;//序号增加1
                if (netsendrecordindex >= 200)
                    netsendrecordindex = 0;
                needsendrecordnum++;//需要发送的记录数量
                if (needsendrecordnum > 200)
                    needsendrecordnum = 200;
            }
        }

        /// <summary>
        /// 向服务器请求二维码
        /// </summary>
        private void getnetqrcode(int qrcodetype)
        {
            int i;
            GSMTxBuffer[0] = 0x01;
            GSMTxBuffer[1] = 0x71;
            GSMTxBuffer[2] = 0x00;
            GSMTxBuffer[3] = 0x22;
            for (i = 0; i < 15; i++)
            {
                GSMTxBuffer[4 + i] = IMEI[i];
            }
            GSMTxBuffer[19] = 0x04;
            GSMTxBuffer[20] = (byte)(((int)(netpaymoney*10)) >> 8);
            GSMTxBuffer[21] = (byte)((int)(netpaymoney*10));
            if (qrcodetype == 0)//支付宝
            {
                GSMTxBuffer[22] = 0x06;
            }
            else if (qrcodetype == 1)// 微信
            {
                GSMTxBuffer[22] = 0x07;
            }
            else if (qrcodetype == 2)// 一码多付
            {
                GSMTxBuffer[22] = 0x08;
            }

            GSMTxBuffer[23] = (byte)huohao;
            GSMTxBuffer[24] = (byte)(liushui[0] >> 8);
            GSMTxBuffer[25] = (byte)liushui[0];
            GSMTxBuffer[26] = (byte)(System.DateTime.Now.Year - 2000);
            GSMTxBuffer[27] = (byte)(System.DateTime.Now.Month);
            GSMTxBuffer[28] = (byte)(System.DateTime.Now.Day);
            GSMTxBuffer[29] = (byte)(System.DateTime.Now.Hour);
            GSMTxBuffer[30] = (byte)(System.DateTime.Now.Minute);
            if (qrcodetype == 0)//支付宝
            {
                GSMTxBuffer[31] = (byte)(System.DateTime.Now.Second);

                timerecord[0, 0] = GSMTxBuffer[26];//记录时间戳
                timerecord[0, 1] = GSMTxBuffer[27];
                timerecord[0, 2] = GSMTxBuffer[28];
                timerecord[0, 3] = GSMTxBuffer[29];
                timerecord[0, 4] = GSMTxBuffer[30];
                timerecord[0, 5] = GSMTxBuffer[31];
            }
            else if (qrcodetype == 1)// 微信
            {
                GSMTxBuffer[31] = (byte)(System.DateTime.Now.Second+60);

                timerecord[1, 0] = GSMTxBuffer[26];//记录时间戳
                timerecord[1, 1] = GSMTxBuffer[27];
                timerecord[1, 2] = GSMTxBuffer[28];
                timerecord[1, 3] = GSMTxBuffer[29];
                timerecord[1, 4] = GSMTxBuffer[30];
                timerecord[1, 5] = GSMTxBuffer[31];
            }
            else if (qrcodetype == 2)// 一码多付
            {
                GSMTxBuffer[31] = (byte)(System.DateTime.Now.Second + 120);

                timerecord[2, 0] = GSMTxBuffer[26];//记录时间戳
                timerecord[2, 1] = GSMTxBuffer[27];
                timerecord[2, 2] = GSMTxBuffer[28];
                timerecord[2, 3] = GSMTxBuffer[29];
                timerecord[2, 4] = GSMTxBuffer[30];
                timerecord[2, 5] = GSMTxBuffer[31];
            }
            
            GSMTxBuffer[32] = 0x0d;
            GSMTxBuffer[33] = 0x0a;
            myTcpCli.Sendbytes(GSMTxBuffer, 34);
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "Send:";
                for (int revcount = 0; revcount < 34; revcount++)
                {
                    revstringnet += " " + Convert.ToString(GSMTxBuffer[revcount], 16);
                }
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
            //netreturncount = 1;//超时计时开始
            netcount = 0;//状态数据发送间隔重新开始
        }

        /// <summary>
        /// 向服务器发送状态数据
        /// </summary>
        private void netsendstate()
        {
            int i;
            totalshangpinnum = mynodelistshangpin.Count;//商品数量

            GSMTxBuffer[0] = 0x01;
            GSMTxBuffer[1] = 0x70;
            GSMTxBuffer[2] = (byte)((56 + totalshangpinnum * 4) >> 8);
            GSMTxBuffer[3] = (byte)((56 + totalshangpinnum * 4) & 0xff);

            for (i = 0; i < 15; i++)
            {
                GSMTxBuffer[4 + i] = IMEI[i];
            }
            byte[] softversion = new Coder(Coder.EncodingMothord.ASCII).GetEncodingBytes(versionstring);
            if (softversion.Length >= 15)
            {
                for (i = 0; i < 15; i++)
                    GSMTxBuffer[19 + i] = softversion[i];
            }
            else
            {
                for (i = 0; i < softversion.Length; i++)
                    GSMTxBuffer[19 + i] = softversion[i];
            }
            GSMTxBuffer[34] = (byte)mynodelistshangpin.Count;
            //查找库存和货道状态
            for (i = 0; i < mynodelistshangpin.Count; i++)
            {
                GSMTxBuffer[35 + i] = byte.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("state").Value);//Aislestate[i];
                //查找库存状态
                int totalkuncun = 0;//计算总库存
                for (int k = 0; k < mynodelisthuodao.Count; k++)
                {
                    if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                        == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                    {
                        if (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态） 
                        {
                            totalkuncun += int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value);//对应货道库存
                        }
                        for (int index = 0; index < mynodelisthuodao.Count; index++)
                        {
                            if ((int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                 == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                 &&(index != k))
                            {
                                if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                {
                                    totalkuncun += int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value);
                                }
                            }
                        }
                    }
                }
                if (totalkuncun > 255)
                {
                    GSMTxBuffer[35 + totalshangpinnum + i] = 0xff;//库存大于255，发送255
                }
                else
                {
                    GSMTxBuffer[35 + totalshangpinnum + i] = (byte)totalkuncun;//库存大于255，发送255
                }
                int shangpinprices = (int)(double.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("jiage").Value) * 10);
                GSMTxBuffer[35 + 2 * totalshangpinnum + 2 * i] = (byte)(shangpinprices >> 8);
                GSMTxBuffer[35 + 2 * totalshangpinnum + 2 * i + 1] = (byte)shangpinprices;

            }

            GSMTxBuffer[35 + 4 * totalshangpinnum] = 0;//zhibijistate
            GSMTxBuffer[36 + 4 * totalshangpinnum] = 0;//yingbiqistate
            GSMTxBuffer[37 + 4 * totalshangpinnum] = 0;//EXboardstate
            GSMTxBuffer[38 + 4 * totalshangpinnum] = 0;//POS机
            GSMTxBuffer[39 + 4 * totalshangpinnum] = 1;//GPRS状态GPRSstate
            GSMTxBuffer[40 + 4 * totalshangpinnum] = 0;//numinpayout
            GSMTxBuffer[41 + 4 * totalshangpinnum] = 0;

            GSMTxBuffer[42 + 4 * totalshangpinnum] = 32;//NET_dBm
            GSMTxBuffer[43 + 4 * totalshangpinnum] = 0;//备用信息
            GSMTxBuffer[44 + 4 * totalshangpinnum] = 0;
            GSMTxBuffer[45 + 4 * totalshangpinnum] = 0;
            GSMTxBuffer[46 + 4 * totalshangpinnum] = 0;
            GSMTxBuffer[47 + 4 * totalshangpinnum] = 0;
            GSMTxBuffer[48 + 4 * totalshangpinnum] = (byte)(System.DateTime.Now.Year - 2000);
            GSMTxBuffer[49 + 4 * totalshangpinnum] = (byte)(System.DateTime.Now.Month);
            GSMTxBuffer[50 + 4 * totalshangpinnum] = (byte)(System.DateTime.Now.Day);
            GSMTxBuffer[51 + 4 * totalshangpinnum] = (byte)(System.DateTime.Now.Hour);
            GSMTxBuffer[52 + 4 * totalshangpinnum] = (byte)(System.DateTime.Now.Minute);
            GSMTxBuffer[53 + 4 * totalshangpinnum] = (byte)(System.DateTime.Now.Second);
            GSMTxBuffer[54 + 4 * totalshangpinnum] = 0x0d;
            GSMTxBuffer[55 + 4 * totalshangpinnum] = 0x0a;
            myTcpCli.Sendbytes(GSMTxBuffer, 56 + 4 * totalshangpinnum);
            if (myfunctionnode.Attributes.GetNamedItem("netlog").Value == "1")
            {
                revstringnet = DateTime.Now.ToString() + "Send:";
                for (int revcount = 0; revcount < 56 + 4 * totalshangpinnum; revcount++)
                {
                    revstringnet += " " + Convert.ToString(GSMTxBuffer[revcount], 16);
                }
                netdatastream.Write(Encoding.ASCII.GetBytes(revstringnet), 0, revstringnet.Length);
                netdatastream.WriteByte(0x0d);
                netdatastream.WriteByte(0x0a);
                netdatastream.Flush();
            }
            //netreturncount = 1;//超时计时开始
            netcount = 0;//状态数据发送间隔重新开始
        }

        /// <summary>
        /// 选择支付方式
        /// </summary>
        private void slectpaytype()
        {
            double zhekouxishu = double.Parse(mypayconfignode.Attributes.GetNamedItem("zhekou").Value) / 100;
            string shangpinnum = "";
            shangpinnum = huohao.ToString();
            switch (nowpaytype)
            {
                case 2:
                    renewpaystate = false;//不从新开始
                    pictureBox3.Image = null;
                    pictureBox4.Image = imageList4.Images[1];
                    label3.Visible = true;
                    label7.Visible = true;//网络倒计时
                    label4.Visible = false;
                    textBox5.Visible = false;
                    netpaymoney = (shangpinjiage - 0) * zhekouxishu;//实际网络支付的金额
                    paystring = "商品编号:" + shangpinnum + ". 价格:" + shangpinjiage.ToString() + "元. 非现金支付:" + netpaymoney.ToString() + "元.";

                    if (netpaymoney > 0)//目前网络只支持整数金额
                    {
                        netstep = 4;//请求支付宝二维码
                        netstring = "正在请求二维码,请稍后...";
                    }
                    break;
                case 3:
                    renewpaystate = false;//不从新开始
                    pictureBox3.Image = null;
                    pictureBox4.Image = imageList4.Images[2];
                    label3.Visible = true;
                    label7.Visible = true;//网络倒计时
                    label4.Visible = false;
                    textBox5.Visible = false;
                    netpaymoney = (shangpinjiage - 0) * zhekouxishu;//实际网络支付的金额
                    paystring = "商品编号:" + shangpinnum + ". 价格:" + shangpinjiage.ToString() + "元. 非现金支付:" + netpaymoney.ToString() + "元.";

                    if (netpaymoney > 0)//目前网络只支持整数金额
                    {
                        netstep = 5;//请求支付宝二维码
                        netstring = "正在请求二维码,请稍后...";
                    }
                    break;
                case 4:
                    renewpaystate = false;//不从新开始
                    pictureBox3.Image = null;
                    pictureBox4.Image = imageList4.Images[3];
                    label3.Visible = true;
                    label7.Visible = true;//网络倒计时
                    label4.Visible = false;
                    textBox5.Visible = false;
                    netpaymoney = (shangpinjiage - 0) * zhekouxishu;//实际网络支付的金额
                    paystring = "商品编号:" + shangpinnum + ". 价格:" + shangpinjiage.ToString() + "元. 非现金支付:" + netpaymoney.ToString() + "元.";
                    if (netpaymoney > 0)//目前网络只支持整数金额
                    {
                        netstep = 6;//请求支付宝二维码
                        netstring = "正在请求二维码,请稍后...";
                    }
                    break;
                case 5:
                    renewpaystate = false;//不从新开始
                    pictureBox3.Image = imageList5.Images[0];
                    pictureBox4.Image = imageList4.Images[4];
                    label3.Visible = true;
                    label7.Visible = true;//网络倒计时
                    label4.Visible = false;
                    textBox5.Visible = false;
                    netpaymoney = (shangpinjiage - 0) * zhekouxishu;//实际网络支付的金额
                    paystring = "商品编号:" + shangpinnum + ". 价格:" + shangpinjiage.ToString() + "元. 非现金支付:" + netpaymoney.ToString() + "元.";
                    if (netpaymoney > 0)//目前网络只支持整数金额
                    {
                        //TODO:OPEN PAY 
                        netstring = "银联闪付卡或手机NFC支付";
                    }
                    break;
                case 6:

                    break;
            }
        }

        private tihuoma mytihuoma;//提货码页面

        #endregion

        #region 下位机操作


        private int needreturnHMIstep1 = 0;//返回到选货界面1计时

        

        private void button2_Click(object sender, EventArgs e)
        {
            needopensettingform = true;
        }

        public static bool needopensettingform;

        
        #endregion

        #region 控件

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //如果已播放完毕就播放下一个文件
            if ((WMPLib.WMPPlayState)e.newState == WMPLib.WMPPlayState.wmppsReady) axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)//选货
        {
            if (e.IsSelected)
            {
                for (int i = 0; i < mynodelistshangpin.Count; i++)
                {
                    if (mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value == e.Item.Name)
                    {
                        updateshangpin(e.Item.Name);//更新商品信息
                        if (BUYstep == 4)//货道正确
                        {
                            Form1.HMIstep = 2;//支付
                            
                            guanggaoreturntime = 0;

                            liushui[0] = 0;//前面的订单号取消，不能出货
                            liushui[1] = 0;//前面的订单号取消，不能出货
                            for (int k = 0; k < 6; k++)//记录时间戳清除防止进支付页面后生成上次请求的的二维码
                            {
                                timerecord[0, k] = 0;
                                timerecord[1, k] = 0;
                                timerecord[2, k] = 0;
                            }
                            huohao = tempAisleNUM;//实际出货商品号
                            shangpinjiage = double.Parse(textBox5.Text.Substring(0, textBox5.Text.IndexOf("元")));//实际出货商品价格

                            //wulihuodao = int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value);//实际出货商品物理货道
                            for (int k = 0; k < mynodelisthuodao.Count; k++)
                            {
                                if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                                    == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                                {
                                    if ((int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                        && (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                    {
                                        wulihuodao = int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value);
                                    }
                                    else
                                    {
                                        for (int index = 0; index < mynodelisthuodao.Count; index++)
                                        {
                                            if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                                 == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                            {
                                                if ((int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                                    && (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                                {
                                                    wulihuodao = int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("huodaonum").Value);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    
                                    break;
                                }
                            }
                            nowpaytype = defaultpaytype;
                            slectpaytype();
                        }
                    }
                }

                e.Item.Selected = false;//为下一次可以选择

            }

        }
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {
                HMIstep = 1;//触摸选货界面
            
            BUYstep = 0;
            renewpaystate = true;
            axWindowsMediaPlayer1.Visible = false;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            axWindowsMediaPlayer1.currentPlaylist.clear();
            updateshangpinlist(0);//显示第一页

            if (mytihuoma == null)
            {
                mytihuoma = new tihuoma();
                if (mytihuoma.ShowDialog() == DialogResult.Yes)
                {

                }
                //mytihuoma.Dispose();
                mytihuoma = null;
            }
        }

        private void axWindowsMediaPlayer1_ClickEvent(object sender, AxWMPLib._WMPOCXEvents_ClickEvent e)
        {
            pictureBox1_Click(null, null);
        }

        private void label11_Click(object sender, EventArgs e)
        {
            liushui[0] = 0;//前面的订单号取消，不能出货
            liushui[1] = 0;//前面的订单号取消，不能出货
            huohao = 0;
            BUYstep = 0;
            shangpinjiage = 0;
            renewpaystate = true;
            HMIstep = 1;//触摸选货界面
        }

        private void label12_Click(object sender, EventArgs e)
        {
            if (cmlistnum > 0)
            {
                cmlistnum = cmlistnum - 1;
                updateshangpinlist(cmlistnum);
            }
            
        }

        private void label13_Click(object sender, EventArgs e)
        {
            if (cmlistnum < cmliststotal - 1)
            {
                cmlistnum = cmlistnum + 1;
                updateshangpinlist(cmlistnum);
            }
            
        }
        
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            pictureBox1_Click(null, null);
        }

        private void label11_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back:
                    this.label11_Click(null, null);
                    break;
                case Keys.Enter:
                    break;
            }
        }

        private void label31_Click(object sender, EventArgs e)//支付宝按钮
        {
            nowpaytype = 2;
            guanggaoreturntime = 0;
            slectpaytype();
        }

        private void label30_Click(object sender, EventArgs e)//微信按钮
        {
            nowpaytype = 3;
            guanggaoreturntime = 0;
            slectpaytype();
        }

        private void label29_Click(object sender, EventArgs e)//一码付按钮
        {
            nowpaytype = 4;
            guanggaoreturntime = 0;
            slectpaytype();

            
        }

        private void label28_Click(object sender, EventArgs e)//银联闪付按钮
        {
            nowpaytype = 5;
            guanggaoreturntime = 0;
            slectpaytype();
        }

        private void updatepaytypes()
        {
            defaultpaytype = 0;

            if ((paytypes & 0x01) != 0)
            {
                label31.Visible = true;
                if (defaultpaytype == 0)
                {
                    defaultpaytype = 2;
                }
            }
            else
            {
                label31.Visible = false;
            }
            if ((paytypes & 0x02) != 0)
            {
                label30.Visible = true;
                if (defaultpaytype == 0)
                {
                    defaultpaytype = 3;
                }
            }
            else
            {
                label30.Visible = false;
            }
            if ((paytypes & 0x04) != 0)
            {
                label29.Visible = true;
                if (defaultpaytype == 0)
                {
                    defaultpaytype = 4;
                }
            }
            else
            {
                label29.Visible = false;
            }
            if ((paytypes & 0x08) != 0)
            {
                label28.Visible = true;
                if (defaultpaytype == 0)
                {
                    defaultpaytype = 5;
                }
            }
            else
            {
                label28.Visible = false;
            }
            if ((paytypes & 0x10) != 0)
            {
                label35.Visible = true;
                if (defaultpaytype == 0)
                {
                    defaultpaytype = 6;
                }
            }
            else
            {
                label35.Visible = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            PEPrinter.PE_Close(PEPrinter.PEhandle);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if(needsetextend==false)
            //{
            //    setextenddata = 0x04;
            //    needsetextend = true;
            //    button1.Visible = false;
            //}
        }
        
        private void label35_Click(object sender, EventArgs e)
        {
            if (mytihuoma == null)
            {
                mytihuoma = new tihuoma();
                if (mytihuoma.ShowDialog() == DialogResult.Yes)
                {
                    //tihuoma.tihuomastring;
                }
                else
                {
                    
                }
                //mytihuoma.Dispose();
                mytihuoma = null;
            }
        }

        #endregion

        #region WorkingTest

        private static byte jb;
        int numNow = 150;
        
        public void WorkingTest(int huodaoNum,string PicPath)
        {
            PricessAction = true;
            pictureaddr = PicPath;
            myprint = new PEPrinter();
            netreturncount = 0;//超时计时停止
            int i = 0;
            huodaorecv = huodaoNum;
            if ((huodaorecv <= mynodelistshangpin.Count) && (huodaorecv > 0))
            {
                if (isextbusy != 0)//正在出货
                {
                }
                else
                {
                    for (i = 0; i < mynodelistshangpin.Count; i++)
                    {
                        if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("shangpinnum").Value) == huodaorecv)
                        {
                            updateshangpin(huodaorecv.ToString());//更新商品信息
                            jb = 0x00;
                            if (BUYstep == 4)//货道正确
                            {
                                HMIstep = 3;//出货
                                nowpaytype = 4;
                                guanggaoreturntime = 0;
                                isextbusy = 2;
                                huohao = tempAisleNUM;//实际出货商品号
                                shangpinjiage = double.Parse(textBox5.Text.Substring(0, textBox5.Text.IndexOf("元")));//实际出货商品价格
                                for (int k = 0; k < mynodelisthuodao.Count; k++)
                                {
                                    if (int.Parse(mynodelistshangpin[i].Attributes.GetNamedItem("huodao").Value)
                                        == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value))
                                    {
                                        if ((int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                            && (int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                        {
                                            wulihuodao = int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("huodaonum").Value);
                                        }
                                        else
                                        {
                                            for (int index = 0; index < mynodelisthuodao.Count; index++)
                                            {
                                                if (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("fenzu").Value)
                                                     == int.Parse(mynodelisthuodao[k].Attributes.GetNamedItem("fenzu").Value))
                                                {
                                                    if ((int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("state").Value) == 0)//货道反馈正常（状态）
                                                        && (int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("kucun").Value) > 0))//对应货道库存不为0
                                                    {
                                                        wulihuodao = int.Parse(mynodelisthuodao[index].Attributes.GetNamedItem("huodaonum").Value);
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }
                                istestmode = false;
                                guanggaoreturntime = 0;//返回广告页面计时清零
                                zhifutype = 3;//支付方式为一码付
                                try
                                {
                                    setchuhuo();
                                    liushui[0] = 65535; 
                                    liushui[1] = 65535;
                                    PEPrinter.PicPath = PicPath;
                                }
                                catch
                                {

                                }

                            }
                        }
                    }
                }
                netstring = "payment successful";
                renewpaystate = true;
                tihuoma.tihuomaresult = "Pickup code verification succeeded";
            }
        }

        private void ChoseNow()
        {
            switch (jb)
            {
                case 0x00:
                    if (myfunctionnode.Attributes.GetNamedItem("vendortype").Value == "1")//印章打印机
                    {
                        showprintstate = "Please put it in the stamp box and start making";
                    }
                    else
                    {
                        showprintstate = "The seal is being prepared, please wait";
                    }
                    break;
                case 0x02:
                    showprintstate = "Seal making: take the shell, please wait";
                    break;
                case 0x04:
                    showprintstate = "Seal making: take out the print,please wait";
                    break;
                case 0x08:
                    showprintstate = "Seal making:waiting to print,please wait";
                    break;
                case 0x09:
                    showprintstate = "Seal making：printing，please wait";
                    break;
                case 0x10:
                    showprintstate = "Seal making:assembling,please wait";
                    break;
                case 0x20:
                    showprintstate = "Seal making:shipping,please wait";
                    break;
                case 0x40:
                    showprintstate = "Seal making:shipping,please wait";
                    break;
                case 0x80:
                    showprintstate = "manufacture complete:waiting for pickup";
                    break;
                case 20:
                    showprintstate = "machine malfunction";
                    break;
                default:
                    break;
            }
            label5.Text = showprinttime + showprintstate+"...  " + (numNow--).ToString() + "s";
        }

        #endregion

        #region ErrorDetect

        bool printcallback = true;
        private void PrintErrorInspect2()
        {
            if (PEPrinter.PEPrinterState == 65535)
            {
            }
           else if(PEPrinter.PEPrinterState>0x8000 && printcallback)
            {
                printcallback = false;
                if(MessageBox.Show($"{PEPrinter.PEPrinterStatedetail}", "Error", MessageBoxButtons.OK)==DialogResult.OK)
                {
                    printcallback = true;
                    count = 0;
                }
            }
        }

        /// <summary>
        /// 打印机错误检测
        /// </summary>
        bool btnPrintCallback = true;
        private bool PrintErrorInspect()
        {
            if (PEPrinter.PEPrinterState > 0x8000 && btnPrintCallback)
            {
                if (PEPrinter.PEPrinterState == 65535)
                {
                    if (MessageBox.Show($"Printer not connected", "Error", MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        btnPrintCallback = true;
                        return true;
                    }
                    else
                        return true;
                }
                else if (MessageBox.Show($"{PEPrinter.PEPrinterStatedetail}", "Error", MessageBoxButtons.OK) == DialogResult.OK)
                {
                    btnPrintCallback = true;
                    return true;
                }
                else
                    return true;
            }
            else
                return false;
        }

        private void Zhashou()
        {
            short error = new PCHMI.VAR().GET_INT16(0, "1001010");
            switch (error)
            {
                case 0:
                    break;
                case 1:
                    MessageBox.Show("通用错误");
                    break;
                case 2:
                    MessageBox.Show("电机错误");
                    break;
                case 3:
                    MessageBox.Show("位置超差");
                    break;
                case 4:
                    MessageBox.Show("速度超差");
                    break;
                case 5:
                    MessageBox.Show("电机堵转");
                    break;
                case 6:
                    MessageBox.Show("初相励错误");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设备错误检测
        /// </summary>
        /// <returns></returns>
        string errorCode;
        bool btnCallBack = true;
        private string[] errorList = 
        {
            "Printer tray error",
             "no print",
             "The delivery position is wrong",
             "wrong assembly position",
             "wrong cover position",
             "Gripper failure",
             "Failed to pick up the printed surface at the chute position",
             "Failed to place lid storage location",
             "Timeout of box fetching" ,
             "Pickup failed at lid storage location",
             "Assembly location finished product pickup failed",
             "The finished product discharge chute failed to place the finished product"
        };

        private  bool MachineErrorInspect()
        {
            errorCode = Convert.ToString(CodeEntity.FaultCode, 2);
            if (btnCallBack && errorCode!="0")
            {
                btnCallBack = false;
                HMIstep = 1;
                if (mysetting != null)
                {
                    string errorPrint = "";
                    int listLenth = errorList.Length;
                    for(int i=errorCode.Length-1;i>=0;i--)
                    {
                        if (listLenth < 0)
                            break;
                        else
                        {
                            if (errorCode[i] == '1')
                                errorPrint += errorList[i];
                            if (i > 0 && errorCode[i - 1] == '1')
                                errorPrint += ",";
                        }
                        listLenth--;
                    }
                    if (MessageBox.Show($"{errorPrint}", "Fault", MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        btnCallBack = true;
                        count = 0;
                    }
                }
                else
                {
                    if (MessageBox.Show("Setup failure！Enter the background program to view the details", "Fault", MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        btnCallBack = true;
                        count = 0;
                    }
                }
                return true;
            }
            else
            {
                if (count > 30)
                    btnCallBack = true;
                return false;
            }
        }

       private bool GoodsInspect()
        {
            if (CodeEntity.PrintFaceNum == 0)
                return true;
            else
                return false;

        }

        #endregion

        #region 过程显示

        bool PricessAction = false;
        /// <summary>
        /// 印章机打印过程
        /// </summary>
        private void PricessTiming()
        {
            
            if (PricessAction)
            {
                if (numNow == 147)
                {
                    jb = 0x02;
                }
                else if (numNow == 140)
                {
                    jb = 0x04;
                }
                ChoseNow();
            }
        }

        #endregion
        
    }
    public class DownloadFile
    {
        public DownloadFile()
        {
        }
        public DownloadFile(string fileName, string saveFileName)
        {
            FileName = fileName;
            SaveFileName = saveFileName;
        }
        public string FileName;
        public string SaveFileName;
    }
}
