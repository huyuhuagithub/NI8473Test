using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
namespace NI8473Test
{
    public unsafe class ItakonClass
    {
        internal struct _VCI_INIT_CONFIG
        {
            public uint AccCode;  //unsigned long
            public uint AccMask;  //unsigned long
            public uint Reserved;
            public byte Filter;   //unsigned char
            public byte Timing0;
            public byte Timing1;
            public byte Mode;
        }

        internal struct _VCI_CAN_OBJ
        {
            public uint ID;         //unsigned int
            public uint TimeStamp;  //unsigned int
            public byte TimeFlag;   //unsigned char
            public byte SendType;
            public byte RemoteFlag;//是否是远程帧
            public byte ExternFlag;//是否是扩展帧
            public byte DataLen;
            public fixed byte Data[8];
            public fixed byte Reserved[3];    //Reserved[0] 第0位表示特殊的空行或者高亮帧
        }

        #region dll
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_OpenDevice(uint DeviceType, uint DeviceInd, uint Reserved);
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_InitCAN(uint DeviceType, uint DeviceInd, uint CANInd, _VCI_INIT_CONFIG* pInitConfig);
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_StartCAN(uint DevType, uint DevIndex, uint CANIndex);
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_GetReceiveNum(uint DevType, uint DevIndex, uint CANIndex);
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_Transmit(uint DeviceType, uint DeviceInd, uint CANInd, _VCI_CAN_OBJ* pSend, uint Len);
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_Receive(uint DeviceType, uint DeviceInd, uint CANInd, _VCI_CAN_OBJ* pReceive, uint Len, int WaitTime);
        [DllImport("ControlCAN.dll")]
        static extern uint VCI_CloseDevice(uint DeviceType, uint DeviceInd);
        #endregion
        //CAN盒类型：USBCAN II型号
        static uint DeviceType = 4;
        static uint DeviceInd = 0;
        static uint CANInd = 0;

        public static uint Init()
        {

            uint o = VCI_OpenDevice(DeviceType, DeviceInd, 0);
            _VCI_INIT_CONFIG InitConfig = new _VCI_INIT_CONFIG();
            _VCI_INIT_CONFIG* pInitConfig = &InitConfig;
            pInitConfig->AccCode = 0;
            pInitConfig->AccMask = 0xffffffff;
            pInitConfig->Reserved = 0;
            pInitConfig->Filter = 1;
            pInitConfig->Mode = 0;
            pInitConfig->Timing0 = 00;  //波特率
            pInitConfig->Timing1 = 0x1c;//波特率500k
            uint init = VCI_InitCAN(DeviceType, DeviceInd, CANInd, pInitConfig);
            uint s = VCI_StartCAN(DeviceType, DeviceInd, CANInd);

            return s;
        }
        //一次可以接收多条数据
        public static void Receive()
        {
            var cancelTokenSource = new CancellationTokenSource(500);
            while (!cancelTokenSource.IsCancellationRequested)//设置读取超时
            {
                uint dataNumber = VCI_GetReceiveNum(DeviceType, DeviceInd, CANInd);
                _VCI_CAN_OBJ[] canObj = new _VCI_CAN_OBJ[100];
                fixed (_VCI_CAN_OBJ* pcanObj = canObj)
                {
                    _VCI_CAN_OBJ* pNewcanObj = pcanObj;
                    int szie = sizeof(_VCI_CAN_OBJ) * canObj.Length;
                    uint FrameNumber = VCI_Receive(DeviceType, DeviceInd, CANInd, pcanObj, 100, 400);
                    for (int i = 0; i < FrameNumber; i++)
                    {
                        if (pNewcanObj->ID == 0x77b)
                        {
                            DateTime date = DateTime.FromFileTime(pNewcanObj->TimeStamp);
                            Console.Write("Timestamp:{0}\t", date);
                            Console.Write("Id:{0:X2}\t", pNewcanObj->ID);
                            Console.Write("FrameType:{0:X2}\t", pNewcanObj->RemoteFlag);
                            Console.Write("Length:{0:X2}\t", pNewcanObj->DataLen);
                            byte* pdata = pNewcanObj->Data;
                            for (int t = 0; t < 8; t++)
                            {
                                Console.Write("{0:X2}" + " ", *pdata);
                                pdata++;
                            }
                            pNewcanObj++;
                            Console.WriteLine();
                        }
                      
                    }
                    
                }
              
            }
        }

        public static void Close()
        {
            VCI_CloseDevice(DeviceType, DeviceInd);
        }

        //一次可以发送多条数据
        public static void Transmit()
        {
            //EC A0 45 2D 43 61 72 78
            byte[] dataValue1 = new byte[8] { 0xEC, 0xA0, 0x45, 0x2D, 0x43, 0x61, 0x72, 0x78 };
            //byte[] dataValue1 = new byte[8] { 0x11, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //byte[] dataValue3 = new byte[8] { 0x00, 0x74, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //List<byte[]> MultFrameValue = new List<byte[]>();
            //MultFrameValue.Add(dataValue1);
            ////MultFrameValue.Add(dataValue2);
            ////MultFrameValue.Add(dataValue3);
            //List<_VCI_CAN_OBJ> nCTYPE_CAN_FRAMEs = NewMethod(MultFrameValue);
            //_VCI_CAN_OBJ[] canObj = new _VCI_CAN_OBJ[1];
            //fixed (_VCI_CAN_OBJ* pcanObj = nCTYPE_CAN_FRAMEs.ToArray())
            //{
            //    _VCI_CAN_OBJ* pNewcanObj = pcanObj;
            //    int szie = sizeof(_VCI_CAN_OBJ) * canObj.Length;
            //    uint FrameNumber = VCI_Transmit(DeviceType, DeviceInd, CANInd, pNewcanObj, 1);
            //}


            _VCI_CAN_OBJ nCTYPE_CAN_FRAME1 = new _VCI_CAN_OBJ();
            nCTYPE_CAN_FRAME1.ID = 0x77A;//帧ID
            nCTYPE_CAN_FRAME1.SendType = 0;
            nCTYPE_CAN_FRAME1.RemoteFlag = 0;
            nCTYPE_CAN_FRAME1.ExternFlag = 0;
            nCTYPE_CAN_FRAME1.DataLen = 8;
            for (int d = 0; d < 8; d++)
            {
                nCTYPE_CAN_FRAME1.Data[d] = dataValue1[d];
            }

            _VCI_CAN_OBJ* pcanObj = &nCTYPE_CAN_FRAME1;
            int szie = sizeof(_VCI_CAN_OBJ);
            uint FrameNumber = VCI_Transmit(DeviceType, DeviceInd, CANInd, pcanObj, 1);

        }

        //拼装多条数据
        private static List<_VCI_CAN_OBJ> NewMethod(List<byte[]> data)
        {
            List<_VCI_CAN_OBJ> nCTYPE_CAN_FRAMEs = new List<_VCI_CAN_OBJ>();
            _VCI_CAN_OBJ nCTYPE_CAN_FRAME1 = new _VCI_CAN_OBJ();
            for (int i = 0; i < data.Count; i++)
            {
                nCTYPE_CAN_FRAME1.ID = 0x77A;//帧ID
                nCTYPE_CAN_FRAME1.SendType = 0;
                nCTYPE_CAN_FRAME1.RemoteFlag = 0;
                nCTYPE_CAN_FRAME1.ExternFlag = 0;
                nCTYPE_CAN_FRAME1.DataLen = 8;
                fixed (byte* pdata = data[i])
                {
                    byte* pnewdata = pdata;
                    for (int d = 0; d < 8; d++)
                    {
                        nCTYPE_CAN_FRAME1.Data[d] = *pnewdata;
                        pnewdata++;
                    }
                }
                nCTYPE_CAN_FRAMEs.Add(nCTYPE_CAN_FRAME1);
            }
            return nCTYPE_CAN_FRAMEs;
        }

    }
}
