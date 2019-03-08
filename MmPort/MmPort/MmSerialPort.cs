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
