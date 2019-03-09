using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace MmPort
{
    public class MmSerialPort:MmPort.MmPortManager
    {
        public event UpdateData updateSerialData;


        static System.Timers.Timer timerTOA; //timerTimeOut A 总体观察（暂无数据时）

        byte[] buff; //临时数组，存放每次的返回结果

        int delayTime; //允许的超时次数

        int offset; //当前偏移量

        int bytesNum; //本次读取到的数量


        public MmSerialPort(MmConfiguration mmConfiguration)
        {
            if (mmConfiguration.isAllCustom)
            {
                mmPort.PortName = mmConfiguration.portName;
                mmPort.BaudRate = mmConfiguration.baudRate;
                mmPort.Parity = mmConfiguration.parity;
                mmPort.DataBits = mmConfiguration.dataBits;
                mmPort.StopBits = mmConfiguration.stopBits;
                mmPort.ReceivedBytesThreshold = mmConfiguration.ReceivedBytesThreshold;

            }
            else
            {
                mmPort.PortName = mmConfiguration.portName;
                mmPort.BaudRate = mmConfiguration.baudRate;
            }


            mmPort.DataReceived += MmPort_DataReceived1;
        }

        private void MmPort_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


    }
}
