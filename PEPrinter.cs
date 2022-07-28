using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace SHJ
{
    class PEPrinter
    {
        [DllImport("TprnComDll.dll", EntryPoint = "_PE_Open", CharSet = CharSet.Auto)]
        public extern static int PE_Open(int dwlpAddr, Int16 wPort, ref IntPtr hDev);

        [DllImport("TprnComDll.dll", EntryPoint = "_PE_Close", CharSet = CharSet.Auto)]
        public extern static void PE_Close(IntPtr hDev);

        [DllImport("TprnComDll.dll", EntryPoint = "_PE_Send", CharSet = CharSet.Auto)]
        public extern static int PE_Send(IntPtr hDev, byte[] lpBuff, int dwLength);

        [DllImport("TprnComDll.dll", EntryPoint = "_PE_Receive", CharSet = CharSet.Auto)]
        public extern static int PE_Receive(IntPtr hDev, byte[] lpBuff, int dwLength, int dwTimout, ref Int16 lpCmdCode, ref Int16 lpDataSize);

        //public delegate void FUNC_TPRN_CB(Int16 wCmdCode, Int16 wResult, byte[] lpDetail, int dwDetailBuffLength);

        //[DllImport("TprnComDll.dll", EntryPoint = "_PE_RegisterEventCB", CharSet = CharSet.Auto)]
        //public extern static void PE_RegisterEventCB(FUNC_TPRN_CB fncbEventCB);

        //[DllImport("TprnComDll.dll", EntryPoint = "_PE_SetOption", CharSet = CharSet.Auto)]
        //public extern static void PE_SetOption(Int16 wOptionld, int dwOptionVal);

        public static IntPtr PEhandle;//打印机句柄
        public static bool isconnected;//打印机连接是否正常
        public static bool isPEPrinterReady;//打印机是否准备就绪
        public static UInt16 PEPrinterState=0xFFFF;//打印机状态字
        public static string PEPrinterStatedetail;//打印机状态详细描述
        public static byte MediaType;//打印机媒介类型
        public static byte TrayCondition;//打印机托盘状态
        public static UInt16 HeadTemperature;//打印头温度
        public static UInt16 MediaLength;//打印机媒介长度

        public static UInt16 ProductID;//打印机编号
        public static UInt16 Version;//打印机F/W版本号，两个字节16进制值表示，高字节Major，低字节Minor
        public static UInt16 PrintableWidth =1344;//可打印图像点宽度1344
        public static UInt16 PrintableHeight =2492;//可打印图像点高度2492
        public static double HeadResolution;//打印头分辨率（整数部分/小数部分各3位）。Integral＝600，Decimal＝456表示600.456dpi
        public static UInt16 HeadDots;//打印头的所有点数1344
        public static UInt16 ValidHeadDots =1344;//打印机中有效打印头的点数1344
        public static UInt16 HeadBlockDots =64;//每块打印头的点数64

        public static UInt16 SeepTransitionTime;//以分钟为单位设定进入SLEEP模式的时间
        public static UInt16 OutputNumber;//输出灰度值（2～256）,根据打印线速度的指定内容输出灰度值的最大值不同
        public static double LineSpeed1;//指定橡胶印刷线的速度.例子：16.9msec/line→Integral=16，Decimal=90
        public static double LineSpeed2;//指定标签行速
        public static UInt16 PreheatData;//指定执行预热操作时的输出数据值。（0～255）另外，如果指定了A5A5h，则不修改打印机的设定内容。
        public static UInt16 PreheatLines;//指定预热操作的执行行数目。在本字段为0的情况下，打印机预热动作不执行。（0～1000）另外，指定A5A5h时，不修改打印机的设置内容。
        public static UInt16 PreheatLineStrobeLow;//预热操作的STROBE信号低电平
        public static UInt16 LineSpeedValue1;//指定橡胶打印线速度的设定值。(0～300)
        public static UInt16 LineSpeedValue2;//指定标签打印线速度的设定值。(0～300)
        


        public static bool needReset;//需要复位
        public static bool needQueryState;//需要查询状态
        public static bool needGetStateData;//需要获取状态数据
        public static bool needQueryPrinterInfo;//需要查询打印机信息
        public static bool needGetPrinterInfoData;//需要获取打印机信息数据
        public static bool needPutPrintCondition;//需要发送打印配置命令
        public static bool needPutPrintConditionData;//需要发送打印配置数据
        public static bool needQueryPrintCondition;//需要查询打印配置
        public static bool needGetPrintConditionData;//需要获取打印配置数据
        public static int needMoveTray;//需要托盘操作1-5
        
        public static string PicPath;//打印图片地址
        public static bool needPrint;//需要打印
        public static bool needPutImage;//需要发送图片命令
        //public static bool needPutImageData;//需要发送图片数据
        public static bool needPushMedia;//需要推送媒介
        public static UInt16 MediaPosition;
        public static UInt16 MediaCoefficient;
        public static UInt16 TaryPosition;
        public static bool needMoveToOrigin;//需要托盘定位

        private byte[] PESendData = new byte[100];//发送命令字数组
        private byte[] PEReceiveData = new byte[100];//发送命令字数组

        public static int pxSealLeftRight_try = 58;
        public static int pxSealUpDown_try = 56;

        public PEPrinter()
        {

        }

        /// <summary>
        /// 打印机的初始化操作
        /// </summary>
        private bool ResetPrinter()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x00;
            PESendData[2] = 0x00;
            PESendData[3] = 0x00;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if(Funreturn ==0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 发送获取打印机的状态数据的命令
        /// </summary>
        private bool GetReady()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x01;
            PESendData[2] = 0x00;
            PESendData[3] = 0x00;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 获取打印机的状态数据
        /// </summary>
        private bool ReadReady()
        {
            short CmdCode=0;
            short CmdreturnLen=0;
            int Funreturn = PE_Receive(PEhandle, PEReceiveData, 8,200, ref CmdCode,ref CmdreturnLen);

            //teststring = "test"+Encoding.Default.GetString(PEReceiveData,0,8)+"+"+CmdCode.ToString()+"+"+CmdreturnLen.ToString();
            //if (Funreturn == 0)
            if ((Funreturn == 0)&&(CmdCode == 0x01) &&(CmdreturnLen == 8))
            {
                isconnected = true;
                PEPrinterState = (UInt16)((((int)PEReceiveData[0]) << 8) + PEReceiveData[1]);//打印机状态字
                TransDetail();
                MediaType = PEReceiveData[4];//打印机媒介类型
                TrayCondition = PEReceiveData[5];//打印机托盘状态
                HeadTemperature = (UInt16)((((int)PEReceiveData[6]) << 8) + PEReceiveData[7]);//打印头温度
                //MediaLength = (UInt16)((((int)PEReceiveData[8]) << 8) + PEReceiveData[9]);//打印机媒介长度
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 发送获取打印机的信息数据的命令
        /// </summary>
        private bool GetPrinterInfo()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x02;
            PESendData[2] = 0x00;
            PESendData[3] = 0x00;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x24;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 获取打印机的信息数据
        /// </summary>
        private bool ReadPrinterInfo()
        {
            short CmdCode = 0;
            short CmdreturnLen = 0;
            int Funreturn = PE_Receive(PEhandle, PEReceiveData, 36, 200, ref CmdCode, ref CmdreturnLen);
            //if (Funreturn == 0)
            if ((Funreturn == 0) && (CmdCode == 0x02) && (CmdreturnLen == 36))
            {
                isconnected = true;

                ProductID = (UInt16)((((int)PEReceiveData[2]) << 8) + PEReceiveData[3]);//打印机编号
                Version = (UInt16)((((int)PEReceiveData[8]) << 8) + PEReceiveData[9]);//打印机F/W版本号，两个字节16进制值表示，高字节Major，低字节Minor
                PrintableWidth = (UInt16)((((int)PEReceiveData[16]) << 8) + PEReceiveData[17]);//可打印图像点宽度1344
                PrintableHeight = (UInt16)((((int)PEReceiveData[18]) << 8) + PEReceiveData[19]);//可打印图像点高度2492
                HeadResolution = ((((int)PEReceiveData[24]) << 8) + PEReceiveData[25])
                    +((double)((((int)PEReceiveData[26]) << 8) + PEReceiveData[27]))/1000;//打印头分辨率（整数部分/小数部分各3位）。Integral＝600，Decimal＝456表示600.456dpi
                HeadDots = (UInt16)((((int)PEReceiveData[28]) << 8) + PEReceiveData[29]);//所有打印头的点数1344
                ValidHeadDots = (UInt16)((((int)PEReceiveData[30]) << 8) + PEReceiveData[31]);//打印机中有效打印头的点数1344
                HeadBlockDots = (UInt16)((((int)PEReceiveData[32]) << 8) + PEReceiveData[33]);//每块打印头的点数64
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 发送设置打印机打印条件
        /// </summary>
        private bool PutPrintCondition()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x03;
            PESendData[2] = 0x00;
            PESendData[3] = 0x00;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x20;

            PESendData[8] = 0x00;
            PESendData[9] = 0x20;
            PESendData[10] = (byte)(SeepTransitionTime >> 8);//以分钟为单位设定进入SLEEP模式的时间
            PESendData[11] = (byte)SeepTransitionTime;
            PESendData[12] = 0x00;
            PESendData[13] = 0x00;
            PESendData[14] = 0x00;
            PESendData[15] = 0x00;

            PESendData[16] = (byte)(OutputNumber >> 8);//输出灰度值（2～256）,根据打印线速度的指定内容输出灰度值的最大值不同
            PESendData[17] = (byte)OutputNumber;
            PESendData[18] = (byte)(Math.Floor(LineSpeed1));//指定橡胶印刷线的速度.例子：16.9msec/line→Integral=16，Decimal=90
            PESendData[19] = (byte)(Math.Floor((LineSpeed1 - Math.Floor(LineSpeed1)) * 100));
            PESendData[20] = (byte)(Math.Floor(LineSpeed2));//指定标签行速
            PESendData[21] = (byte)(Math.Floor((LineSpeed2 - Math.Floor(LineSpeed2)) * 100));
            PESendData[22] = 0x00;
            PESendData[23] = 0x00;
            PESendData[24] = (byte)(PreheatData >> 8);//指定执行预热操作时的输出数据值。（0～255）另外，如果指定了A5A5h，则不修改打印机的设定内容。
            PESendData[25] = (byte)PreheatData;
            PESendData[26] = (byte)(PreheatLines >> 8);//指定预热操作的执行行数目。在本字段为0的情况下，打印机预热动作不执行。（0～1000）另外，指定A5A5h时，不修改打印机的设置内容。
            PESendData[27] = (byte)PreheatLines;
            PESendData[28] = (byte)(PreheatLineStrobeLow >> 8);//预热操作的STROBE信号低电平
            PESendData[29] = (byte)PreheatLineStrobeLow;
            PESendData[30] = (byte)(LineSpeedValue1 >> 8);//指定橡胶打印线速度的设定值。(0～300)
            PESendData[31] = (byte)LineSpeedValue1;
            PESendData[32] = (byte)(LineSpeedValue2 >> 8);//指定标签打印线速度的设定值。(0～300)
            PESendData[33] = (byte)LineSpeedValue2;
            PESendData[34] = 0x00;
            PESendData[35] = 0x00;
            PESendData[36] = 0x00;
            PESendData[37] = 0x00;
            PESendData[38] = 0x00;
            PESendData[39] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 40);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 设置打印机打印条件数据
        /// </summary>
        private bool PutPrintConditionData()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x20;
            PESendData[2] = (byte)(SeepTransitionTime >> 8);//以分钟为单位设定进入SLEEP模式的时间
            PESendData[3] = (byte)SeepTransitionTime;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            PESendData[8] = (byte)(OutputNumber >> 8);//输出灰度值（2～256）,根据打印线速度的指定内容输出灰度值的最大值不同
            PESendData[9] = (byte)OutputNumber;
            PESendData[10] = (byte)(Math.Floor(LineSpeed1));//指定橡胶印刷线的速度.例子：16.9msec/line→Integral=16，Decimal=90
            PESendData[11] = (byte)(Math.Floor((LineSpeed1 - Math.Floor(LineSpeed1)) * 100));
            PESendData[12] = (byte)(Math.Floor(LineSpeed2));//指定标签行速
            PESendData[13] = (byte)(Math.Floor((LineSpeed2 - Math.Floor(LineSpeed2)) * 100));
            PESendData[14] = 0x00;
            PESendData[15] = 0x00;
            PESendData[16] = (byte)(PreheatData >> 8);//指定执行预热操作时的输出数据值。（0～255）另外，如果指定了A5A5h，则不修改打印机的设定内容。
            PESendData[17] = (byte)PreheatData;
            PESendData[18] = (byte)(PreheatLines >> 8);//指定预热操作的执行行数目。在本字段为0的情况下，打印机预热动作不执行。（0～1000）另外，指定A5A5h时，不修改打印机的设置内容。
            PESendData[19] = (byte)PreheatLines;
            PESendData[20] = (byte)(PreheatLineStrobeLow >> 8);//预热操作的STROBE信号低电平
            PESendData[21] = (byte)PreheatLineStrobeLow;
            PESendData[22] = (byte)(LineSpeedValue1 >> 8);//指定橡胶打印线速度的设定值。(0～300)
            PESendData[23] = (byte)LineSpeedValue1;
            PESendData[24] = (byte)(LineSpeedValue2 >> 8);//指定标签打印线速度的设定值。(0～300)
            PESendData[25] = (byte)LineSpeedValue2;
            PESendData[26] = 0x00;
            PESendData[27] = 0x00;
            PESendData[28] = 0x00;
            PESendData[29] = 0x00;
            PESendData[30] = 0x00;
            PESendData[31] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 32);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 发送读打印机打印条件数据指令
        /// </summary>
        private bool GetPrintCondition()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x04;
            PESendData[2] = 0x00;
            PESendData[3] = 0x00;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x20;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 获取打印机的打印条件数据
        /// </summary>
        private bool ReadPrintCondition()
        {
            short CmdCode = 0;
            short CmdreturnLen = 0;
            int Funreturn = PE_Receive(PEhandle, PEReceiveData, 32, 200, ref CmdCode, ref CmdreturnLen);
            //if (Funreturn == 0)
            if ((Funreturn == 0) && (CmdCode == 0x04) && (CmdreturnLen == 32))
            {
                isconnected = true;
                SeepTransitionTime = (UInt16)((((UInt16)PEReceiveData[2]) << 8) + PEReceiveData[3]);//以分钟为单位设定进入LEEP模式的时间
                OutputNumber = (UInt16)((((int)PEReceiveData[8]) << 8) + PEReceiveData[9]);//输出灰度值（2～256）,根据打印线速度的指定内容输出灰度值的最大值不同
                LineSpeed1 = PEReceiveData[10] + ((double)PEReceiveData[11]) / 100;//指定橡胶印刷线的速度.例子：16.9msec/line→Integral=16，Decimal=90
                LineSpeed2 = PEReceiveData[12] + ((double)PEReceiveData[13]) / 100;//指定标签行速
                PreheatData = (UInt16)((((int)PEReceiveData[16]) << 8) + PEReceiveData[17]);//指定执行预热操作时的输出数据值。（0～255）另外，如果指定了A5A5h，则不修改打印机的设定内容。
                PreheatLines = (UInt16)((((int)PEReceiveData[18]) << 8) + PEReceiveData[19]);//指定预热操作的执行行数目。在本字段为0的情况下，打印机预热动作不执行。（0～1000）另外，指定A5A5h时，不修改打印机的设置内容。
                PreheatLineStrobeLow = (UInt16)((((int)PEReceiveData[20]) << 8) + PEReceiveData[21]);//预热操作的STROBE信号低电平

                LineSpeedValue1 = (UInt16)((((int)PEReceiveData[22]) << 8) + PEReceiveData[23]);//指定橡胶打印线速度的设定值。(0～300)
                LineSpeedValue2 = (UInt16)((((int)PEReceiveData[24]) << 8) + PEReceiveData[25]);//指定标签打印线速度的设定值。(0～300)
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 打印机托盘操作
        /// </summary>
        /// <param name="movetype">托盘操作方式</param>
        private bool MoveTray(int movetype)
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x10;
            switch(movetype)
            {
                case 0://Load （通常動作：Eject→Home）
                    PESendData[2] = 0x00;
                    PESendData[3] = 0x01;
                    break;
                case 1://Eject （通常動作：Home→Eject）
                    PESendData[2] = 0x00;
                    PESendData[3] = 0x02;
                    break;
                case 2://Load ＋ Media / Adapter检查（印画时动作）
                    PESendData[2] = 0x01;
                    PESendData[3] = 0x01;
                    break;
                case 3://Eject （印画时动作）
                    PESendData[2] = 0x01;
                    PESendData[3] = 0x02;
                    break;
                default:
                //case 4://OSMO用 Load +Media / Adapter检查
                    PESendData[2] = 0x02;
                    PESendData[3] = 0x01;
                    break;
            }
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 开始打印
        /// </summary>
        private bool StartPrint()
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x11;
            PESendData[2] = 0x00;
            PESendData[3] = 0x00;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 发送图像命令
        /// </summary>
        private bool PutImage(byte[] Imagedata)
        {
            int ImageLen = Imagedata.Length;
            byte[] tempsend = new byte[ImageLen + 8];

            tempsend[0] = 0x00;
            tempsend[1] = 0x12;
            tempsend[2] = 0x00;
            tempsend[3] = 0x00;
            tempsend[4] = (byte)(ImageLen >> 24);
            tempsend[5] = (byte)(ImageLen >> 16);
            tempsend[6] = (byte)(ImageLen >> 8);
            tempsend[7] = (byte)ImageLen;
            for(int i=0;i<ImageLen;i++)
            {
                tempsend[i + 8] = Imagedata[i];
            }
            int Funreturn = PE_Send(PEhandle, tempsend, 8 + ImageLen);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 发送图像数据
        /// </summary>
        private bool PutImageData(byte[] Imagearray)
        {
            int Funreturn = PE_Send(PEhandle, Imagearray, Imagearray.Length);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 推送媒介
        /// </summary>
        /// <param name="Position">指定的位置距离,0～1500：(10/1mm)</param>
        /// <param name="Coefficient">头压修正数据(800～头最大加压克)。如果在设置范围以外，则应用头部最大加压。</param>
        private bool PushMedia(UInt16 Position,UInt16 Coefficient)
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x13;
            PESendData[2] = (byte)(Position >> 8);
            PESendData[3] = (byte)Position;
            PESendData[4] = (byte)(Coefficient >> 8);
            PESendData[5] = (byte)Coefficient;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 移动托盘到原点,若托盘不在HP则使用托盘Load实施
        /// </summary>
        /// <param name="Position">从托盘HP到头部下部位置的距离,0～1500：(10/1mm)</param>
        private bool MoveToOrigin(UInt16 Position)
        {
            PESendData[0] = 0x00;
            PESendData[1] = 0x14;
            PESendData[2] = (byte)(Position >> 8);
            PESendData[3] = (byte)Position;
            PESendData[4] = 0x00;
            PESendData[5] = 0x00;
            PESendData[6] = 0x00;
            PESendData[7] = 0x00;

            int Funreturn = PE_Send(PEhandle, PESendData, 8);
            if (Funreturn == 0)
            {
                isconnected = true;
                return true;
            }
            else
            {
                isconnected = false;
                return false;
            }
        }

        /// <summary>
        /// 更新打印机状态描述
        /// </summary>
        private void TransDetail()
        {
            if (PEPrinterState == 0x0000)
            {
                isPEPrinterReady = true;
            }
            else
            {
                isPEPrinterReady = false;
            }

            switch (PEPrinterState)
            {
                case 0x6000:
                    PEPrinterStatedetail = "打印机正在初始化";
                    break;
                case 0x0000:
                    PEPrinterStatedetail = "打印机准备就绪";
                    break;
                case 0x0001:
                    PEPrinterStatedetail = "打印机按键操作的测试动作中";
                    break;
                case 0x0002:
                    PEPrinterStatedetail = "正在进行功能检查的测试动作";
                    break;
                case 0x1000:
                    PEPrinterStatedetail = "打印机处于Sleep状态";
                    break;
                case 0x2000:
                    PEPrinterStatedetail = "正在设定数字电位计";
                    break;
                case 0x4000:
                    PEPrinterStatedetail = "打印头部冷却中";
                    break;
                case 0x4001:
                    PEPrinterStatedetail = "等待打印头部冷却";
                    break;
                case 0x6400:
                    PEPrinterStatedetail = "打印机托盘加载中";
                    break;
                case 0x6500:
                    PEPrinterStatedetail = "打印机托盘弹出中";
                    break;
                case 0x6800:
                    PEPrinterStatedetail = "正在打印中";
                    break;
                case 0x7FFD:
                    PEPrinterStatedetail = "打印机I/F命令执行中";
                    break;
                case 0x7FFE:
                    PEPrinterStatedetail = "打印机正在写入FlashROM";
                    break;
                case 0x7FFF:
                    PEPrinterStatedetail = "打印机正忙";
                    break;
                case 0x8100:
                    PEPrinterStatedetail = "打印机的顶盖打开了";
                    break;
                case 0x8800:
                    PEPrinterStatedetail = "配置了非法媒介适配器或者未设置媒介适配器";
                    break;
                case 0x8A00:
                    PEPrinterStatedetail = "媒介异常";
                    break;
                case 0x8AFF:
                    PEPrinterStatedetail = "印章未设置";
                    break;
                case 0x9100:
                    PEPrinterStatedetail = "打印头单元异常";
                    break;
                case 0x9101:
                    PEPrinterStatedetail = "在打印头初始化动作向Down方向移动时，不能检测HOME传感器的ON";
                    break;
                case 0x9102:
                    PEPrinterStatedetail = "在打印头初始化动作的Up方向移动时，不能检测HOME传感器的OFF";
                    break;
                case 0x9110:
                    PEPrinterStatedetail = "在打印头Up操作时，不能检测Home传感器的OFF";
                    break;
                case 0x9120:
                    PEPrinterStatedetail = "在打印头Down操作时，无法检测Home传感器的ON";
                    break;
                case 0x9200:
                    PEPrinterStatedetail = "打印机托盘组件异常";
                    break;
                case 0x9201:
                    PEPrinterStatedetail = "在托盘初始化动作的弹出方向驱动中，不能检测Home传感器的OFF";
                    break;
                case 0x9202:
                    PEPrinterStatedetail = "在托盘初始化动作的加载方向驱动中，不能检测Home传感器的ON";
                    break;
                case 0x9210:
                    PEPrinterStatedetail = "托盘归位(加载)操作时，不能检测Home传感器的ON";
                    break;
                case 0x9220:
                    PEPrinterStatedetail = "归位的托盘弹出操作时，不能检测Home传感器的OFF";
                    break;
                case 0x9230:
                    PEPrinterStatedetail = "打印时，无法检测移动到托盘的媒介适配器的检查位置的Eject传感器的OFF";
                    break;
                case 0x9281:
                    PEPrinterStatedetail = "在托盘初始化动作的Eject方向的驱动中，托盘编码器1的脉冲数为0（脱调）";
                    break;
                case 0x9283:
                    PEPrinterStatedetail = "在托盘初始化动作的Load方向驱动中，托盘编码器1的脉冲数为0（脱调）";
                    break;
                case 0x9291:
                    PEPrinterStatedetail = "托盘归位(加载)操作时，托盘编码器1的脉冲数为0";
                    break;
                case 0x92A1:
                    PEPrinterStatedetail = "托盘弹出操作时，托盘编码器1的脉冲数为0";
                    break;
                case 0x92B1:
                    PEPrinterStatedetail = "托盘移动到媒介适配器的检查位置时，托盘编码器1的脉冲数为0";
                    break;
                case 0x92C1:
                    PEPrinterStatedetail = "托盘移动到打印开始位置时，托盘编码器1的脉冲数为0";
                    break;
                case 0x92C3:
                    PEPrinterStatedetail = "在打印操作时，托盘编码器1的脉冲数为0";
                    break;
                case 0xA100:
                    PEPrinterStatedetail = "Hostによる印刷動作のアボート";
                    break;
                case 0xC100:
                    PEPrinterStatedetail = "ストローブオーバー";
                    break;
                case 0xC101:
                    PEPrinterStatedetail = "印刷動作時、ストローブオーバーを検出した";
                    break;
                case 0xD100:
                    PEPrinterStatedetail = "打印头错误";
                    break;
                case 0xD101:
                    PEPrinterStatedetail = "打印头过热";
                    break;
                case 0xD102:
                    PEPrinterStatedetail = "打印头低温故障，或未安装打印头";
                    break;
                case 0x8300:
                    PEPrinterStatedetail = "リボン切れを検知した";
                    break;
                case 0x8400:
                    PEPrinterStatedetail = "ボビンがセットされていない";
                    break;
                case 0x8700:
                    PEPrinterStatedetail = "打印机停止键被按下，进入紧急停止状态";
                    break;
                case 0x9400:
                    PEPrinterStatedetail = "推动机构故障";
                    break;
                case 0x9401:
                    PEPrinterStatedetail = "推动机构向DOWN方向运转中超时";
                    break;
                case 0x9402:
                    PEPrinterStatedetail = "推动机构向UP方向运转中超时";
                    break;
                case 0x9600:
                    PEPrinterStatedetail = "F/W更新故障";
                    break;
                case 0x9601:
                    PEPrinterStatedetail = "在向ROM传输F/W更新文件时，USB电缆被拔出";
                    break;
                case 0xE000:
                    PEPrinterStatedetail = "F/W更新时严重错误";
                    break;
                case 0xE001:
                    PEPrinterStatedetail = "内置ROM校验错误";
                    break;
                case 0xE100:
                    PEPrinterStatedetail = "上电时严重错误";
                    break;
                case 0xE101:
                    PEPrinterStatedetail = "内置ROM校验错误";
                    break;
                case 0xE102:
                    PEPrinterStatedetail = "内置Flash校验错误";
                    break;
                case 0xE103:
                    PEPrinterStatedetail = "内置SRAM校验错误";
                    break;
                case 0xE104:
                    PEPrinterStatedetail = "外部SDRAM校验错误";
                    break;
                case 0xE200:
                    PEPrinterStatedetail = "正常操作中的严重错误";
                    break;
                case 0xE201:
                    PEPrinterStatedetail = "在写入调节参数时,ROM不能正常写入";
                    break;
                case 0xE211:
                    PEPrinterStatedetail = "托盘马达驱动芯片过热";
                    break;
                case 0xFFFF:
                    PEPrinterStatedetail = "未连接到打印机";
                    break;
            }
        }

        public static byte[] myImagearray = null;//灰度图数组

        /// <summary>
        /// Stamp type
        /// </summary>
        public enum TYPE_STAMP
        {
            TYPE_1010 = 0,  // width:236px height:236px
            TYPE_2020,      // width:472px height:472px
            TYPE_2530,      // width:708px height:591px
        }

        /// <summary>
        /// 根据图片路径, 返回一张灰度加工图
        /// </summary>
        /// <param name="strPicPath">图片路径</param>
        /// <returns>灰度加工图对象</returns>
        public static Bitmap CreateProcessingData(string strPicPath, TYPE_STAMP type)
        {
            // Create a new bitmap.
            int newWidth;
            int newHeight;
            switch(type)
            {
                case TYPE_STAMP.TYPE_1010:
                    newWidth = 236;
                    newHeight = 236;
                    //newWidth = 354;
                    //newHeight = 354;
                    break;
                case TYPE_STAMP.TYPE_2020:
                    newWidth = 472;
                    newHeight = 472;
                    //newWidth = 590;
                    //newHeight = 472;
                    break;
                case TYPE_STAMP.TYPE_2530:
                    newWidth = 708;
                    newHeight = 591;
                    //newWidth = 708;
                    //newHeight = 591;
                    break;
                default:
                    newWidth = 708;
                    newHeight = 591;
                    break;
            }
            Bitmap bitmap = KiResizeImage(new Bitmap(strPicPath), newWidth,newHeight);
            // 左右反转
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            // Lock the bitmap's bits. 
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int iStride = bmpData.Stride; //图片一行象素所占用的字节  

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int iBytes = iStride * bitmap.Height;
            byte[] rgbValues = new byte[iBytes];
            byte[] image = new byte[bitmap.Width * bitmap.Height];

            // Copy RGB values into the Array
            Marshal.Copy(ptr, rgbValues, 0, iBytes);

            for (int y = 0; y < bmpData.Height; ++y)
            {
                for (int x = 0; x < bmpData.Width; ++x)
                {
                    int iThird = iStride * y + 3 * x;
                    byte avg = (byte)((rgbValues[iThird] * 0.299 + rgbValues[iThird + 1] * 0.587 + rgbValues[iThird + 2] * 0.114));//转化成灰度
                    //rgbValues[iThird] = avg;
                    //rgbValues[iThird + 1] = avg;
                    //rgbValues[iThird + 2] = avg;
                    image[bmpData.Width * y + x] = avg;
                }
            }

            int width = bmpData.Width;
            int height = bmpData.Height;

            // byte数据转位图
            bitmap.Dispose();
            bitmap = ConvertByteArrayToBitmap(image, width, height);

            // 边框部分添加
            image = AddSealPart(image, ref width, ref height, type);

            // 空白部分添加
            image = AddDummyPart(image, ref width, height);

            myImagearray = new byte[width * height];

            Array.Copy(image, myImagearray, myImagearray.Length);

            //// byte数据转位图
            //bitmap.Dispose();
            //bitmap = ConvertByteArrayToBitmap(image, width, height);

            return bitmap;
        }

        /// <summary>
        /// 边框部分添加
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="type"></param>
        private static byte[] AddSealPart(byte[] image, ref int width, ref int height, TYPE_STAMP type)
        {
            int pxSealLeftRight;
            int pxSealUpDown;

            switch (type)
            {
                case TYPE_STAMP.TYPE_1010:
                case TYPE_STAMP.TYPE_2020:
                    pxSealLeftRight = 59;
                    pxSealUpDown = 59;
                    break;
                case TYPE_STAMP.TYPE_2530:
                    pxSealLeftRight = 58;
                    pxSealUpDown = 65;
                    //pxSealLeftRight = pxSealLeftRight_try;
                    //pxSealUpDown = pxSealUpDown_try;
                    break;
                default:
                    pxSealLeftRight = 58;
                    pxSealUpDown = 65;
                    break;
            }

            byte[] imgSeal = Enumerable.Repeat<byte>(255, (width + (pxSealLeftRight * 2)) * (height + (pxSealUpDown * 2))).ToArray();
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    imgSeal[((width + (pxSealLeftRight * 2)) * (h + pxSealUpDown)) + (w + pxSealLeftRight)] = image[(width * h) + w];
                }
            }

            width = width + (pxSealLeftRight * 2);
            height = height + (pxSealUpDown * 2);

            return imgSeal;
        }

        /// <summary>
        /// 空白部分添加
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static byte[] AddDummyPart(byte[] image, ref int width, int height)
        {
            int pxDummy = (1344 - width) / 2;

            byte[] imgDummy = Enumerable.Repeat<byte>(0, (width + (pxDummy * 2)) * height).ToArray();
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    imgDummy[(width + (pxDummy * 2)) * h + (w + pxDummy)] = image[(width * h) + w];
                }
            }

            width = width + (pxDummy * 2);

            return imgDummy;
        }

        /// <summary>
        /// byte数据转位图
        /// </summary>
        /// <param name="array"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static Bitmap ConvertByteArrayToBitmap(byte[] array, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(array, 0, ptr, array.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }


        //private static Bitmap KiResizeImage(Bitmap bmp, int newW, int newH)
        //{
        //    try
        //    {
        //        Bitmap b = new Bitmap(newW, newH);
        //        Graphics g = Graphics.FromImage(b);

        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        //        g.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
        //        g.Dispose();

        //        return b;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        private static Bitmap KiResizeImage(Bitmap bmp, int TargetWidth, int TargetHeight)
        {
            int IntWidth; //新的图片宽
            int IntHeight; //新的图片高
            try
            {
                System.Drawing.Imaging.ImageFormat format = bmp.RawFormat;
                System.Drawing.Bitmap SaveImage = new System.Drawing.Bitmap(TargetWidth, TargetHeight);
                Graphics g = Graphics.FromImage(SaveImage);
                g.Clear(Color.White);


                if (bmp.Width > TargetWidth && bmp.Height <= TargetHeight)//宽度比目的图片宽度大，长度比目的图片长度小
                {
                    IntWidth = TargetWidth;
                    IntHeight = (IntWidth * bmp.Height) / bmp.Width;
                }
                else if (bmp.Width <= TargetWidth && bmp.Height > TargetHeight)//宽度比目的图片宽度小，长度比目的图片长度大
                {
                    IntHeight = TargetHeight;
                    IntWidth = (IntHeight * bmp.Width) / bmp.Height;
                }
                else if (bmp.Width <= TargetWidth && bmp.Height <= TargetHeight) //长宽比目的图片长宽都小
                {
                    IntHeight = bmp.Width;
                    IntWidth = bmp.Height;
                }
                else//长宽比目的图片的长宽都大
                {
                    IntWidth = TargetWidth;
                    IntHeight = (IntWidth * bmp.Height) / bmp.Width;
                    if (IntHeight > TargetHeight)//重新计算
                    {
                        IntHeight = TargetHeight;
                        IntWidth = (IntHeight * bmp.Width) / bmp.Height;
                    }
                }

                g.DrawImage(bmp, (TargetWidth - IntWidth) / 2, (TargetHeight - IntHeight) / 2, IntWidth, IntHeight);
                bmp.Dispose();

                return SaveImage;
            }
            catch (Exception ex)
            {
                return null;
            }           
        }

        public static string PEloopstate;//当前打印机执行任务步骤
        private int loopdelay;
        /// <summary>
        /// 处理打印机任务
        /// </summary>
        public void PEloop()
        {
            if (loopdelay>0)
            {
                loopdelay = 0;
            }
            else
            {
                loopdelay++;
            }
            if (isconnected)
            {
                if (needReset)//需要复位
                {
                    bool ret = ResetPrinter();
                    if (ret)
                    {
                        needReset = false;
                    }
                    PEloopstate = "复位";
                }
                else if (needQueryState)//需要查询状态
                {
                    bool ret = GetReady();
                    if (ret)
                    {
                        needGetStateData = true;
                        needQueryState = false;
                    }
                    PEloopstate = "查询状态";
                }
                else if (needGetStateData)//需要获取状态数据
                {
                    bool ret = ReadReady();
                    if (ret)
                    {
                        needGetStateData = false;
                    }
                    PEloopstate = "获取状态数据";
                }
                else if (needQueryPrinterInfo)//需要查询打印机信息
                {
                    bool ret = GetPrinterInfo();
                    if (ret)
                    {
                        needGetPrinterInfoData = true;
                        needQueryPrinterInfo = false;
                    }
                    PEloopstate = "查询打印机信息";
                }
                else if (needGetPrinterInfoData)//需要获取打印机信息数据
                {
                    bool ret = ReadPrinterInfo();
                    if (ret)
                    {
                        needGetPrinterInfoData = false;
                    }
                    PEloopstate = "获取打印机信息数据";
                }
                else if (needPutPrintCondition)//需要发送打印配置命令
                {
                    bool ret = PutPrintCondition();
                    if (ret)
                    {
                        //needPutPrintConditionData = true;
                        needPutPrintCondition = false;
                    }
                    PEloopstate = "发送打印配置";
                }
                else if (needPutPrintConditionData)//需要发送打印配置数据
                {
                    bool ret = PutPrintConditionData();
                    if (ret)
                    {
                        needPutPrintConditionData = false;
                    }
                    PEloopstate = "发送打印配置数据";
                }
                else if (needQueryPrintCondition)//需要查询打印配置
                {
                    bool ret = GetPrintCondition();
                    if (ret)
                    {
                        needGetPrintConditionData = true;
                        needQueryPrintCondition = false;
                    }
                    PEloopstate = "查询打印配置";
                }
                else if (needGetPrintConditionData)//需要获取打印配置数据
                {
                    bool ret = ReadPrintCondition();
                    if (ret)
                    {
                        needGetPrintConditionData = false;
                    }
                    PEloopstate = "获取打印配置数据";
                }
                else if (needMoveTray > 0)//需要托盘操作1-5
                {
                    bool ret = MoveTray(needMoveTray - 1);
                    if (ret)
                    {
                        needMoveTray = 0;
                    }
                    PEloopstate = "托盘操作";
                }
                else if (needPrint)//需要打印
                {
                    bool ret = StartPrint();
                    if (ret)
                    {
                        needPrint = false;
                    }
                    PEloopstate = "打印";
                }
                else if (needPutImage)//需要发送图片
                {
                    bool ret = PutImage(myImagearray);
                    if (ret)
                    {
                        needPrint = true;
                        needPutImage = false;
                    }
                    PEloopstate = "发送图片命令";
                }

                else if (needPushMedia)//需要推送媒介
                {
                    bool ret = PushMedia(MediaPosition, MediaCoefficient);
                    if (ret)
                    {
                        needPushMedia = false;
                    }
                    PEloopstate = "推送媒介";
                }
                else if (needMoveToOrigin)//需要托盘定位
                {
                    bool ret = MoveToOrigin(TaryPosition);
                    if (ret)
                    {
                        needMoveToOrigin = false;
                    }
                    PEloopstate = "托盘定位";
                }
                else
                {
                    needQueryState = true;//需要查询状态
                    needQueryPrinterInfo = true;//需要查询打印机信息
                    needQueryPrintCondition = true;//需要查询打印配置

                    PEloopstate = "添加查询指令";
                }
            }
            else
            {
                int Funreturn = PE_Open(0, 0, ref PEhandle);
                if (Funreturn == 0)
                {
                    isconnected = true;
                    needReset = true;//需要复位一下
                }
                else
                {
                    PE_Close(PEhandle);
                    isconnected = false;
                }
            }
        }
    }
}
