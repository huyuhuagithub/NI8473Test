using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NI8473Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
          int Initok= NI8473Class.Init("CAN0");
          if (Initok>0)
          {
              Console.WriteLine("InitFail!");
          }
          Console.WriteLine("InitOK!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NI8473Class.Read();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            NI8473Class.NICANClose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NI8473Class.Write("00 74 12 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 13 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 14 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 15 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 16 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 17 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 18 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 19 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 1a 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 1b 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 1c 00 00 00 00 00");
            Thread.Sleep(150);
            NI8473Class.Write("00 74 1d 00 00 00 00 00");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            NI8473Class.ReadMult();
        }

        private void button6_Click(object sender, EventArgs e)
        {
           uint i= ZLGCANClass.Init();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ZLGCANClass.Receive();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ZLGCANClass.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ZLGCANClass.Transmit();
            //Thread.Sleep(50);
            ZLGCANClass.Receive();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ItakonClass.Init();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ItakonClass.Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ItakonClass.Transmit();
            ItakonClass.Receive();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ItakonClass.Receive();
        }
    }
}
