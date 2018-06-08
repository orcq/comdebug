using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;

namespace comdebug
{
    public partial class Form1 : Form
    {
        SerialPort sp = null;//声明一个串口类
        bool isOpen = false;//打开串口标志位
        bool isSetProperty = false;//属性设标志位
        bool isHex = false;//十六进制标志位

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            for (int i = 0; i < 10; i++)//支持的串口数量
            {
                cbxComPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxComPort.SelectedIndex = 0;
            //常用的波特率
            cbxBaudBate.Items.Add("1200");
            cbxBaudBate.Items.Add("2400");
            cbxBaudBate.Items.Add("4800");
            cbxBaudBate.Items.Add("1200");
            cbxBaudBate.Items.Add("9600");
            cbxBaudBate.Items.Add("19200");
            cbxBaudBate.Items.Add("38400");
            cbxBaudBate.Items.Add("43000");
            cbxBaudBate.Items.Add("56000");
            cbxBaudBate.Items.Add("57600");
            cbxBaudBate.Items.Add("115200");
            cbxBaudBate.SelectedIndex = 5;
            //列出停止位
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;
            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;
            //列出奇偶校验位
            cbxParitv.Items.Add("无");
            cbxParitv.Items.Add("奇校验");
            cbxParitv.Items.Add("偶校验");
            cbxParitv.SelectedIndex = 0;
            //默认为Char显示
            rbnChar.Checked = true;
        }

        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;//有可用的串口标志位
            cbxComPort.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxComPort.Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (comExistence)
            {
                cbxComPort.SelectedIndex = 0;//使串口号显示第一个添加的索引
            }
            else
            {
                MessageBox.Show("没有找到可用的串口！错误提示");
            }
        }

        private bool CheckPortSetting()//检查串口是否设置
        {
            if (cbxComPort.Text.Trim() == "") return false;
            if (cbxBaudBate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxParitv.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            return true;
        }

        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "") return false;
            return true;
        }

        private void SetPortProperty()//设置串口属性
        {
            sp = new SerialPort();
            sp.PortName = cbxComPort.Text.Trim();//设置串口名
            sp.BaudRate = Convert.ToInt32(cbxBaudBate.Text.Trim());//设置串口波特率
            float f = Convert.ToSingle(cbxStopBits.Text.Trim());//设置停止位
            if (f == 0)
            {
                sp.StopBits = StopBits.None;
            }
            else if(f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
               else if(f == 1)
            {
                sp.StopBits = StopBits.One;
            }
               else if(f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
               else 
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());//设置数据位

            string s = cbxParitv.Text.Trim();//设置奇偶校验位
            if(s.CompareTo("无")==0)
            {
                sp.Parity = Parity.None;
            }
            else if(s.CompareTo("奇校验")==0)
            
            {
                sp.Parity = Parity.Odd;
            }
            else if(s.CompareTo("偶校验")==0)
            
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            sp.ReadTimeout = -1;//设置超时读取时间
            sp.RtsEnable = true;

            //定义DATArecived事件，当串口收到数据后触发
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            if(rbnHex.Checked)
                {
                    isHex = true;
                }
                else
                {
                    isHex = false;
                }
            }

      
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (isOpen)//写串口数据
            {
                try
                {
                    sp.WriteLine(tbxSendData.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("发送数据时发生错误");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开");
                return;
            }
            if (!CheckSendData())//检测要发送的数据
            {
                MessageBox.Show("请输入要发送的数据！");
                return;
            }
        }

        private void btnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
            tbxSendData.Text = "";
        }

        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())//检测串口设置
                {
                    MessageBox.Show("串口未设置");
                    return;
                }
                if (!isSetProperty)//串口未设置重新设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;
                }
                try//打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";
                    //串口打开后则相关按钮失效
                    cbxComPort.Enabled = false;
                    cbxBaudBate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParitv.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;
                }
                catch (Exception)
                {
                    //打开串口失败后，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或被占用");
                }
            }
            else
            {
                try//关闭串口
                {
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCom.Text = "打开串口";
                    //串口关闭后则相关按钮可用
                    cbxComPort.Enabled = true;
                    cbxBaudBate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParitv.Enabled = true;
                    cbxStopBits.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHex.Enabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("关闭串口发生错误");
                }
            }
        }
        StringBuilder m_TestMachineBuff = new StringBuilder();//测试仪缓冲区
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
           System.Threading.Thread.Sleep(100);//等待接收完数据
            this.Invoke((EventHandler)(delegate//跨线程访问ui的方法
            {
                if (isHex == false)
                {
                    tbxRecvData.Text += sp.ReadLine();
                }
                else
                {
                    Byte[] ReceivedData = new Byte[sp.BytesToRead];//创建接收字节数组
                    sp.Read(ReceivedData, 0, ReceivedData.Length);//读取接收到的数组
                     string str = System.Text.Encoding.Default.GetString(ReceivedData);
                     String RecvDataText = null;
                     for (int i = 0; i < ReceivedData.Length - 1; i++)
                     {
                         RecvDataText += ("0X" + ReceivedData[i].ToString("X2") + "");
                     }
                     tbxRecvData.Text += RecvDataText;
                }
                sp.DiscardInBuffer();//丢弃接收的缓冲区数据
            }));
        }

    }
    }

