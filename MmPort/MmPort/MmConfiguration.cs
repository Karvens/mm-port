using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace MmPort
{
     public  class  MmConfiguration
    {

        /// <summary>
        /// 串口号
        /// </summary>
        public string portName { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        public int baudRate { get; set; }

        /// <summary>
        /// 校验
        /// </summary>
        public Parity parity { get; set; }

       /// <summary>
       /// 数据位
       /// </summary>
        public int dataBits { get; set; }
         
      /// <summary>
      /// 停止位
      /// </summary>
        public StopBits stopBits { get; set; }

        /// <summary>
        /// 是否全部自定义
        /// </summary>
        public bool isAllCustom { get; set; }

        /// <summary>
        /// 触发串口接受事件的最低字节数
        /// </summary>
        public int ReceivedBytesThreshold { get; set; }

        /// <summary>
        /// 是否开启超时处理
        /// </summary>
        public bool openTimeOut { get; set; }

        /// <summary>
        /// 超时时间 ms
        /// </summary>
        public int timeOutTime { get; set; }

        /// <summary>
        /// 通信方式
        /// </summary>
        public COMMUNICATION_TYPE communicationType { get; set; }

        public MmConfiguration()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="receivedBytesThreshold">触发串口接受事件的最低字节数</param>
        public MmConfiguration(string portName,
                                               int baudRate,
                                               Parity parity,
                                                int dataBits,
                                                 StopBits stopBits,
                                                 int receivedBytesThreshold)
        {

            this.portName = portName;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.ReceivedBytesThreshold = receivedBytesThreshold;
            this.isAllCustom = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        public MmConfiguration(string portName,
                                       int baudRate)
        {

            this.portName = portName;
            this.baudRate = baudRate;
            this.isAllCustom = false;
        }

    }
}
