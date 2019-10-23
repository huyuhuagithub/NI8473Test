using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace NI8473Test
{


    public unsafe class NI8473Class
    {

        //ReadMultiFrame(); 需要内存对齐方式为 2字节
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct NCTYPE_CAN_STRUCT
        {
            public ulong Timestamp;//0对应 C 类型: unsigned long
            public uint ArbitrationId;//8对应 C 类型: unsigned long
            public byte FrameType;//10 9对应 C 类型:unsigned char
            public byte DataLength;//10 11 对应 C 类型:unsigned char
            public fixed byte Data[8];//24对应 C 类型:unsigned char
        }
        internal struct NCTYPE_CAN_FRAME
        {
            public uint ArbitrationId;//0
            public byte IsRemote;//8
            public byte DataLength;//12
            public fixed byte Data[8];//20
        }
        static uint pObjHandlePtr = 0;
        static uint pActualDataSize = 0;

        #region DLL

        [DllImport("nican.dll")]
        static extern int ncConfig(string ObjName, uint NumAttrs, ref uint AttrIdList, ref uint AttrValueList);
        [DllImport("nican.dll")]
        static extern int ncOpenObject(string ObjName, ref uint ObjHandle);
        [DllImport("nican.dll")]
        static extern int ncAction(uint ObjHandle, ulong Opcode);
        [DllImport("nican.dll")]
        static extern int ncRead(uint ObjHandle, int DataSize, NCTYPE_CAN_STRUCT* DataPtr);
        [DllImport("nican.dll")]
        static extern int ncCloseObject(uint ObjHandle);
        [DllImport("nican.dll")]
        static extern int ncWrite(uint ObjHandle, int DataSize, NCTYPE_CAN_FRAME* DataPtr);
        [DllImport("nican.dll")]
        static extern int ncReadMult(uint ObjHandle, int SizeofData, NCTYPE_CAN_STRUCT* DataPtr, ref uint ActualDataSize);



        [DllImport("nican.dll")]
        static extern int ncWriteMult(ulong ObjHandle, ulong DataSize, NCTYPE_CAN_STRUCT* DataPtr);
        #endregion
        static uint[] attrIdList = new uint[]
        {
            0x80000006, /* Start On Open, NCTYPE_BOOL, Set, CAN Interface */
            0x80000007, /* Baud Rate, Set, CAN Interface */
            0x80000013, /* Read Queue Length, NCTYPE_UINT32, Set, CAN Interface/Object */
            0x80000014, /* Write Queue Length, NCTYPE_UINT32, Set, CAN Interface/Object */
            0x80010001, /* Standard Comparator, NCTYPE_CAN_ARBID, Set, CAN Interface */
            0x80010002, /* Standard Mask (11 bits), NCTYPE_UINT32, Set, CAN Interface */
            0x80010003, /* Extended Comparator (29 bits), NCTYPE_CAN_ARBID, Set, CAN Interface */
            0x80010004  /* Extended Mask (29 bits), NCTYPE_UINT32, Set, CAN Interface */
        };

        static uint[] attrValueList = new uint[] { 1, 500000, 150, 10, 0, 0, 0, 0 };

        public static int Init(string canName)
        {
            ncConfig(canName, 8, ref attrIdList[0], ref attrValueList[0]);
            return ncOpenObject(canName, ref pObjHandlePtr);
        }

        public static void Read()
        {
            NCTYPE_CAN_STRUCT _STRUCT = new NCTYPE_CAN_STRUCT();
            NCTYPE_CAN_STRUCT* p = &_STRUCT;
            int size = sizeof(NCTYPE_CAN_STRUCT);
            ncRead(pObjHandlePtr, size, p);
            DateTime date = DateTime.FromFileTime((long)p->Timestamp);
            Console.Write("Timestamp:{0}\t", date);
            Console.Write("Id:{0:X2}\t", p->ArbitrationId);
            Console.Write("FrameType:{0:X2}\t", p->FrameType);
            Console.Write("Length:{0:X2}\t", p->DataLength);
            byte* pdata = p->Data;
            for (int i = 0; i < 8; i++)
            {
                Console.Write("{0:X2}" + " ", *pdata);
                pdata++;
            }
            Console.WriteLine();
        }

        public static void ReadMult()
        {
            //读取多帧只是依次读取多帧，导致不能实时读到发送后的值，需在一段时间内读去查询值。
            var cancelTokenSource = new CancellationTokenSource(1000);
            while (!cancelTokenSource.IsCancellationRequested)//设置读取超时
            {
                NCTYPE_CAN_STRUCT[] _STRUCT1 = new NCTYPE_CAN_STRUCT[150];
                fixed (NCTYPE_CAN_STRUCT* p = _STRUCT1)
                {
                    NCTYPE_CAN_STRUCT* pp = p;
                    int szie = sizeof(NCTYPE_CAN_STRUCT) * _STRUCT1.Length;
                    ncReadMult(pObjHandlePtr, szie, pp, ref pActualDataSize);
                    pActualDataSize = pActualDataSize / (uint)sizeof(NCTYPE_CAN_STRUCT);
                    for (int i = 0; i < pActualDataSize; i++)
                    {
                        if (pp->ArbitrationId == 0x7ab)
                        {
                            DateTime date = DateTime.FromFileTime((long)pp->Timestamp);
                            Console.Write("Timestamp:{0}\t", date);
                            Console.Write("Id:{0:X2}\t", pp->ArbitrationId);
                            Console.Write("FrameType:{0:X2}\t", pp->FrameType);
                            Console.Write("Length:{0:X2}\t", pp->DataLength);
                            byte* pdata = pp->Data;
                            for (int t = 0; t < 8; t++)
                            {
                                Console.Write("{0:X2}" + " ", *pdata);
                                pdata++;
                            }
                            Console.WriteLine();
                        }
                        pp++;
                    }
                }
            }
        }
        public static void Write(string Data)
        {
            //用ref 的方式发送数据+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // [DllImport("nican.dll")]
            //static extern int ncWrite(uint ObjHandle, int DataSize, ref NCTYPE_CAN_FRAME DataPtr);

            //NCTYPE_CAN_FRAME sendobj = new NCTYPE_CAN_FRAME()
            //{
            //    ArbitrationId = 0x512,
            //    IsRemote = 0,
            //    DataLength = 8,
            //};
            //string strdata = Data;
            //int len = (strdata.Length + 1) / 3;
            //List<byte> bytelist = new List<byte>();
            //for (int t = 0; t < len; t++)
            //{
            //    bytelist.Add(System.Convert.ToByte("0x" + strdata.Substring(t * 3, 2), 16));
            //    sendobj.Data[t] = bytelist[t];

            //}
            //int size = sizeof(NCTYPE_CAN_FRAME);
            //int ii = ncWrite(pObjHandlePtr, size, ref sendobj);

            //指针的方式发送数据++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            NCTYPE_CAN_FRAME _STRUCT = new NCTYPE_CAN_FRAME();
            NCTYPE_CAN_FRAME* p = &_STRUCT;
            p->ArbitrationId = 0x72b;
            p->IsRemote = 0;
            p->DataLength = 8;
            string strdata = Data;
            int len = (strdata.Length + 1) / 3;
            List<byte> bytelist = new List<byte>();
            for (int t = 0; t < len; t++)
            {
                bytelist.Add(Convert.ToByte("0x" + strdata.Substring(t * 3, 2), 16));
                p->Data[t] = bytelist[t];

            }
            int size1 = sizeof(NCTYPE_CAN_FRAME);
            ncWrite(pObjHandlePtr, size1, p);

        }

        public static void NICANClose()
        {
            ncCloseObject(pObjHandlePtr);
        }
    }
}
