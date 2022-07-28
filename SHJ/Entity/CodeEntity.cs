using System;

namespace SHJ
{
    class CodeEntity
    {
        public static ushort M119 { get; set; }
        public static short RunCode { get; set; }
        public static short FaultCode { get; set; }
        /// <summary>
        /// 托盘指令
        /// </summary>
        public static Int16 TrayState {get;set;}
        /// <summary>
        /// 货道盒子数量
        /// </summary>
        public static Int16 PrintFaceNum { get; set; }
    }
}
