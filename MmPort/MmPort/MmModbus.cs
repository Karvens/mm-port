using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace MmPort
{
    public class MmModbus : MmPort.MmPortManager
    {

        public event UpdateData updateModbusData;


        private int CurrentAddr;//定义当前通讯设备的地址
        private byte[] bData = new byte[1024];//最大接受的1024个字节
        public string strErrMsg;//错误信息
        public string strUpData, strDownData, strUpData1;//定义上行及下行数据字符串
        public byte[] bUpData = new byte[1024];
        private string mTempStr;
        public bool mRtuFlag = true;
        private byte mReceiveByte;
        private int mReceiveByteCount=0;
        public long iMWordStartAddr, iMBitStartAddr;//保持寄存器及输出线圈的起始地址
        public int iMWordLen, iMBitLen;//保持寄存器及输出线圈长度
        public UInt16[,] MWordVaue = new UInt16[16, 256];//定义最大保持寄存器二维数组 
        public bool[,] MBitVaue = new bool[16, 256];//定义输出线圈二维数组
        private bool bCommWell;
        public bool[] bCommFlag = new bool[16];//false 通讯正常  true 通讯异常 最大16个设备
        public bool comBusying;  //串口忙
        private byte ucCRCHi = 0xFF;
        private byte ucCRCLo = 0xFF;
        private bool IsReturn = false;//串口返回数据


        public MmModbus(MmConfiguration mmConfiguration)
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


            mmPort.DataReceived += MmPort_DataReceived;
        }



        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public bool closePort()
        {
            try
            {
                mmPort.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool openPort()
        {
           try
            {
                if (!mmPort.IsOpen)
                {
                 
                    mmPort.Open();
                   
                }
                return true;

            }
            catch (Exception )
            {
                return false;
            }
        }


        private  void MmPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                int i;
                //循环接收数据
                while (mmPort.BytesToRead > 0)
                {
                    mReceiveByte = (byte)mmPort.ReadByte();                  
                    bData[mReceiveByteCount] = mReceiveByte;
                 
                    //System.Diagnostics.Trace.WriteLine(bData[mReceiveByte].ToString()); 
                    mReceiveByteCount = mReceiveByteCount + 1;
                    //缓冲区溢出复位
                    if (mReceiveByteCount >= 1024)
                    {
                        mReceiveByteCount = 0;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                        return;
                    }
                }
                updateModbusData(bData);
                if (mRtuFlag == true)//RTU接受方式
                {
                    if (mReceiveByteCount >= (iMWordLen * 2 + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x10)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMWordLen * 2 + 10); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区

                    }
                    if (mReceiveByteCount >= (iMWordLen * 2 + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x06)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMWordLen * 2 + 10); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                        IsReturn = true;

                    }
                    if (mReceiveByteCount >= (iMWordLen * 2 + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x0f)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMWordLen * 2 + 10); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区

                    }
                    //输入寄存器 功能码0x04
                    if (mReceiveByteCount >= (iMWordLen * 2 + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x05)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMWordLen * 2 + 14); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区

                    }
                    //输入寄存器 功能码0x04
                    if (mReceiveByteCount >= (iMWordLen * 2 + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x04)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMWordLen * 2 + 5); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区

                    }
                    //保持寄存器 功能码0x03
                    if (mReceiveByteCount >= (iMWordLen * 2 + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x03)
                    {
                        // bUpData = bData;
                        mTempStr = "";
                        for (i = 0; i < (iMWordLen * 2 + 5); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");

                        }
                        strUpData = mTempStr;
                        //赋值给全局MW变量  从起始地址开始
                        int nDownPos;
                        nDownPos = 0;
                        for (i = 0; i < iMWordLen; i++)
                        {
                            MWordVaue[CurrentAddr, nDownPos] = (UInt16)(bData[3 + 2 * i] * 256 + bData[3 + 2 * i + 1]);
                            nDownPos = nDownPos + 1;
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                    }
                    //输出线圈 功能码0x01
                    if (mReceiveByteCount >= (iMBitLen + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x01)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMBitLen + 5); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        //赋值给全局变量
                        for (i = 0; i < bData[2]; i++)
                        {
                            MmUtils. ByteToBArray(bData[3 + i], CurrentAddr, i * 8);
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                    }
                    //输入线圈 功能码0x02

                    if (mReceiveByteCount >= (iMBitLen + 5) && bData[0] == CurrentAddr + 1 && bData[1] == 0x02)
                    {
                        mTempStr = "";
                        for (i = 0; i < (iMBitLen + 5); i++)
                        {
                            mTempStr = mTempStr + " " + bData[i].ToString("X2");
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                    }
                }
                else //ASCII协议解析
                {
                    //保持寄存器 功能码0x03
                    string strTmpAddr, strTmpFun;
                    strTmpAddr = System.Text.Encoding.ASCII.GetString(bData, 1, 2);
                    strTmpFun = System.Text.Encoding.ASCII.GetString(bData, 3, 2);
                    if (bData[0] == ':' && mReceiveByteCount >= (iMWordLen * 4 + 11) && strTmpAddr == (CurrentAddr + 1).ToString("X2") && strTmpFun == "03")
                    {
                        mTempStr = System.Text.Encoding.ASCII.GetString(bData, 0, iMWordLen * 4 + 11);
                        //赋值给全局MW变量  从起始地址开始
                        int nDownPos;
                        nDownPos = 0;
                        byte bTmpHi, bTmpLo;
                        string tmpData;
                        for (i = 0; i < iMWordLen; i++)
                        {

                            tmpData = System.Text.Encoding.ASCII.GetString(bData, 7 + i * 4, 2);
                            bTmpHi = Convert.ToByte(tmpData, 16);
                            tmpData = System.Text.Encoding.ASCII.GetString(bData, 9 + i * 4, 2);
                            bTmpLo = Convert.ToByte(tmpData, 16);

                            MWordVaue[CurrentAddr, nDownPos] = (UInt16)(bTmpHi * 256 + bTmpLo);
                            nDownPos = nDownPos + 1;
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区

                    }
                    //输入寄存器04
                    if (bData[0] == ':' && mReceiveByteCount >= (iMWordLen * 4 + 11) && strTmpAddr == (CurrentAddr + 1).ToString("X2") && strTmpFun == "04")
                    {
                        mTempStr = System.Text.Encoding.ASCII.GetString(bData, 0, iMWordLen * 4 + 11);
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区

                    }
                    //输出线圈 功能码0x01
                    if (bData[0] == ':' && mReceiveByteCount >= (iMBitLen * 2 + 11) && strTmpAddr == (CurrentAddr + 1).ToString("X2") && strTmpFun == "01")
                    {
                        byte bDataValue;
                        string tmpData;
                        mTempStr = "";
                        mTempStr = System.Text.Encoding.ASCII.GetString(bData, 0, iMBitLen * 2 + 11);
                        //赋值给全局变量
                        for (i = 0; i < iMBitLen; i++)
                        {
                            tmpData = System.Text.Encoding.ASCII.GetString(bData, 7 + i * 2, 2);
                            bDataValue = Convert.ToByte(tmpData, 16);
                           MmUtils. ByteToBArray(bDataValue, CurrentAddr, i * 8);
                        }
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                    }
                    //输入线圈 功能码0x02
                    if (bData[0] == ':' && mReceiveByteCount >= (iMBitLen * 2 + 11) && strTmpAddr == (CurrentAddr + 1).ToString("X2") && strTmpFun == "02")
                    {
                        //byte bDataValue;
                        //string tmpData;
                        mTempStr = "";
                        mTempStr = System.Text.Encoding.ASCII.GetString(bData, 0, iMBitLen * 2 + 11);
                        strUpData = mTempStr;
                        mmPort.DiscardInBuffer();//清输入缓冲区
                    }
                }

            //   updateModbusData( MmUtils.HexStringToByteArray(strUpData));
            }
            catch (Exception ex)
            {
                strErrMsg = ex.Message.ToString();
            }
        }

        #region 读保持寄存器  功能码03
        // MODBUS读保持寄存器 iAddress 开始地址(0开始),iLength 寄存器数量
        //主站请求：01 03 00 00 00 06 70 08
        //地址    1字节
        //功能码  1字节   0x03
        //起始寄存器地址  2字节   0x0000~0x0005
        //寄存器数量  2字节   0x01~0x06
        //CRC校验 2字节
        public byte[] ReadKeepReg(int iDevAdd, long iAddress, int iLength)
        {

            byte[] ResByte = null;
            iMWordStartAddr = iAddress;
            iMWordLen = iLength;
            if (comBusying == true) Thread.Sleep(30);
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd - 1;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x03;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = (byte)((iLength - iLength % 256) / 256);
            SendCommand[5] = (byte)(iLength % 256);
             MmUtils.Crc16(SendCommand, 6);
            SendCommand[6] = MmUtils.ucCRCLo;
            SendCommand[7] = MmUtils.ucCRCHi;
            try
            {
                //发送指令
                mmPort.Write(SendCommand, 0, 8);
            }
            catch
            {
                return ResByte;
            }
            mReceiveByteCount = 0;
            strUpData = "";
            Thread.Sleep(100);
            ResByte =MmUtils.HexStringToByteArray(this.strUpData);
            return ResByte;
        }
        #endregion



        #region 读输出状态  功能码01
        //MODBUS读输出状态 iAddress 开始地址(0开始),iLength 寄存器数量
        //主站请求：01 01 00 00 00 07 70 08
        //地址    1字节
        //功能码  1字节   0x01
        //起始寄存器地址  2字节   0x0000~0x0005
        //寄存器数量  2字节   0x01~0x06
        //CRC校验 2字节
        public byte[] ReadOutputStatus(int iDevAdd, long iAddress, int iLength)
        {

            byte[] ResByte = null;
            //一个字节代表8个位状态
            //一个字节代表8个位状态
            if (iLength % 8 == 0)
            {
                iMBitLen = iLength / 8;
            }
            else
            {
                iMBitLen = iLength / 8 + 1;
            }
            iMBitStartAddr = iAddress;
            if (comBusying == true) Thread.Sleep(250);
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd - 1;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x01;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = (byte)((iLength - iLength % 256) / 256);
            SendCommand[5] = (byte)(iLength % 256);
            MmUtils. Crc16(SendCommand, 6);
            SendCommand[6] = ucCRCLo;
            SendCommand[7] = ucCRCHi;
            try
            {
                //发送指令。
                mmPort.Write(SendCommand, 0, 8);
            }
            catch (Exception)
            {
                return ResByte;
            }

            mReceiveByteCount = 0;
            strUpData = "";
            //strDownData = "";
            //for (i = 0; i < 8; i++)
            //{
            //    strDownData = strDownData + " " + SendCommand[i].ToString("X2");
            //}
            Thread.Sleep(300);
            ResByte =MmUtils.HexStringToByteArray(this.strUpData);
            return ResByte;
        }
        #endregion



        #region 强制单线圈  功能码05
        // MODBUS强制单线圈 iAddress 开始地址(0开始)
        //主站请求：01 05 00 00 FF 00 70 08
        //地址    1字节
        //功能码  1字节   0x05
        //起始寄存器地址  2字节   0x0000~0x0005
        //寄存器数量  2字节   0x01~0x06
        //CRC校验 2字节
        public byte[] ForceOn(int iDevAdd, long iAddress)
        {
            // bool Success = true;
            byte[] ResByte = null;
            iMWordStartAddr = iAddress;
            //iMWordLen = 0;
            if (comBusying == true) Thread.Sleep(250);
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd - 1;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x05;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = 0xff;
            SendCommand[5] = 0x00;
            MmUtils. Crc16(SendCommand, 6);
            SendCommand[6] = ucCRCLo;
            SendCommand[7] = ucCRCHi;
            strUpData = "";
            try
            {
                //发送指令。
                mmPort.Write(SendCommand, 0, 8);
                Thread.Sleep(100);
            }
            catch
            {
                return ResByte;
            }
            mReceiveByteCount = 0;

            ResByte =MmUtils. HexStringToByteArray(this.strUpData);
            return ResByte;
            //  return Success;
        }
        #endregion


        #region 复位单线圈  功能码05
        //MODBUS复位单线圈 iAddress 开始地址(0开始)
        //主站请求：01 05 00 00 00 00 70 08
        //地址    1字节
        //功能码  1字节   0x05
        //起始寄存器地址  2字节   0x0000~0x0005
        //寄存器数量  2字节   0x01~0x06
        //CRC校验 2字节
        public byte[] ForceOff(int iDevAdd, long iAddress)
        {
            //    bool Success = true;
            byte[] ResByte = null;

            iMWordStartAddr = iAddress;
            //iMWordLen = 0;
            if (comBusying == true) Thread.Sleep(250);
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd - 1;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x05;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = 0x00;
            SendCommand[5] = 0x00;
             MmUtils. Crc16(SendCommand, 6);
            SendCommand[6] = ucCRCLo;
            SendCommand[7] = ucCRCHi;
            strUpData = "";
            try
            {
                //发送指令。
                mmPort.Write(SendCommand, 0, 8);
                Thread.Sleep(100);
            }
            catch (Exception)
            {
                return ResByte;
            }

            mReceiveByteCount = 0;
            strUpData = "";
            //strDownData = "";
            //for (i = 0; i < 8; i++)
            //{
            //    strDownData = strDownData + " " + SendCommand[i].ToString("X2");
            //}
            comBusying = true;//设置串口忙标志
            bCommWell = false;//设置本次通讯标志
            ResByte =MmUtils. HexStringToByteArray(this.strUpData);
            return ResByte;
        }
        #endregion



        #region 预置单字寄存器  功能码06
        //MODBUS预置单字寄存器 iAddress 开始地址(0开始),iHiValue 数据
        //主站请求：01 06 00 00 00 06 70 08
        //地址    1字节
        //功能码  1字节   0x06
        //起始寄存器地址  2字节   0x0000~0x0005
        //寄存器数量  2字节   0x01~0x06
        //CRC校验 2字节
        public byte[] PreSetKeepReg(int iDevAdd, long iAddress, UInt16 SetValue)
        {
            int i;
            iMWordStartAddr = iAddress;
            byte[] ResByte = null;
            //iMWordLen = 0;
            if (comBusying == true) Thread.Sleep(250);
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd - 1;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x06;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = (byte)((SetValue - SetValue % 256) / 256); ;
            SendCommand[5] = (byte)(SetValue % 256); ;
            MmUtils. Crc16(SendCommand, 6);
            SendCommand[6] = ucCRCLo;
            SendCommand[7] = ucCRCHi;
            strUpData1 = "";
            //发送指令。
            try
            {
                mmPort.Write(SendCommand, 0, 8);
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                return ResByte;
            }


            mReceiveByteCount = 0;

            strDownData = "";
            for (i = 0; i < 8; i++)
            {
                strDownData = strDownData + " " + SendCommand[i].ToString("X2");
            }
            comBusying = true;//设置串口忙标志
            bCommWell = false;//设置本次通讯标志
            ResByte =MmUtils. HexStringToByteArray(this.strUpData);
            return ResByte;

        }
        #endregion



        #region 预置双字寄存器  功能码10
        //MODBUS预置双字寄存器 iAddress 开始地址(0开始)
        //主站请求：01 06 00 00 00 10 70 08
        //地址    1字节
        //功能码  1字节   0x10
        //起始寄存器地址  2字节   0x0000~0x0005
        //寄存器数量  2字节   0x01~0x06
        //CRC校验 2字节
        public byte[] PreSetFloatKeepReg(int iDevAdd, long iAddress, float SetValue)
        {

            byte[] ResByte = null;
            int i;
            byte[] bSetValue = new byte[4];

            bSetValue = BitConverter.GetBytes(SetValue);


            //bSetValue = &SetValue;
            iMWordStartAddr = iAddress;
            //iMWordLen = 0;
            if (comBusying == true) Thread.Sleep(250);
            byte[] SendCommand = new byte[13];
            CurrentAddr = iDevAdd - 1;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x10;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = 0x00;
            SendCommand[5] = 0x02;
            SendCommand[6] = 0x04;
            SendCommand[7] = bSetValue[3];
            SendCommand[8] = bSetValue[2];
            SendCommand[9] = bSetValue[1];
            SendCommand[10] = bSetValue[0];
            MmUtils. Crc16(SendCommand, 11);
            SendCommand[11] = ucCRCLo;
            SendCommand[12] = ucCRCHi;
            strUpData = "";
            try
            {
                //发送指令。
                mmPort.Write(SendCommand, 0, 13);
                Thread.Sleep(200);
            }
            catch (Exception)
            {
                return ResByte;
            }
            mReceiveByteCount = 0;

            strDownData = "";
            for (i = 0; i < 13; i++)
            {
                strDownData = strDownData + " " + SendCommand[i].ToString("X2");
            }
            comBusying = true;//设置串口忙标志
            bCommWell = false;//设置本次通讯标志
            ResByte =MmUtils. HexStringToByteArray(this.strUpData);
            return ResByte;
        }
        #endregion

    }
}
