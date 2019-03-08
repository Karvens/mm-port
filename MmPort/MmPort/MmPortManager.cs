using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace MmPort
{
    public class MmPortManager
    {
        public SerialPort mmPort = null;

        public delegate void UpdateData(byte[] data);
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public MmPortManager()
        {
            mmPort = new SerialPort();
        }

        /// <summary>
        /// 获取串口对象构造函数
        /// </summary>
        /// <param name="tYPE">串口类型</param>
        /// <param name="hander">自定义hander</param>
        /// <returns></returns>
        public MmPort.MmPortManager DefaultMmPortManager(COMMUNICATION_TYPE tYPE, MmConfiguration mmConfiguration, MmDataReceiveHander hander)
        {

          

            mmPort.DataReceived += MmPort_DataReceived;

            switch (tYPE)
            {
                case COMMUNICATION_TYPE.SerialPort:
                    {
                        MmSerialPort mm = new MmSerialPort(mmConfiguration);
                        mm.updateSerialData += hander.test;
                        return mm;
                    }
                   
                case COMMUNICATION_TYPE.Modbus:
                    {
                        MmModbus mm = new MmModbus(mmConfiguration);
                        mm.updateModbusData += hander.test;
                        return mm;
                    }
                default:
                    return null;
            }

        }

        public virtual void MmPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


    }
}
