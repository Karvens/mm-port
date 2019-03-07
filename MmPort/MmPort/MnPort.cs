using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace MmPort
{
    public class MnPort
    {
        public SerialPort mmPort = null;

        public delegate void UpdateData(byte[] data);
        public event UpdateData updateData;

        public MnPort()
        {
            mmPort = new SerialPort();

            
        }

        public void start()
        {
            Thread t = new Thread(test);
            t.IsBackground = true;
            t.Start();
        }


        public MnPort(MmConfiguration mmConfiguration,
                            DataReceiveHander[] dataReceiveHanders)
        {
            if (mmConfiguration.isAllCustom)
            {
                mmPort = new SerialPort(mmConfiguration.portName,
                                                     mmConfiguration.baudRate,
                                                     mmConfiguration.parity,
                                                     mmConfiguration.dataBits,
                                                     mmConfiguration.stopBits
                    );
                mmPort.ReceivedBytesThreshold = mmConfiguration.ReceivedBytesThreshold;
                
            }
            else
            {
                mmPort = new SerialPort(mmConfiguration.portName,
                                     mmConfiguration.baudRate
            );

                foreach (DataReceiveHander d in dataReceiveHanders)
                {
                    this.updateData += d.test;
                }
               
                mmPort.DataReceived += MmPort_DataReceived;
            }

        }

        private void MmPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void test()
        {
            while (true)
            {
                byte[] data = new byte[1];
                data[0] = 255;
                updateData(data);
            }
           
        }
    }
}
