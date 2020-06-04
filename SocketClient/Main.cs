using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace SocketClient
{
    public partial class Main : Form
    {
        //创建 1个客户端套接字 和1个负责监听服务端请求的线程  
        Thread threadclient = null;
        Socket socketclient = null;

        public Main()
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;
            //关闭对文本框的非法线程操作检查  
            TextBox.CheckForIllegalCrossThreadCalls = false;

            this.btnSendMessage.Enabled = false;
            this.btnSendMessage.Visible = false;

            this.txtMessage.Visible = false;
        }

        private void btnConnection_Click(object sender, EventArgs e)
        {
            this.btnConnection.Enabled = false;
            //定义一个套接字监听  
            socketclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //获取文本框中的IP地址  
            IPAddress address = IPAddress.Parse("127.0.0.1");

            //将获取的IP地址和端口号绑定在网络节点上  
            IPEndPoint point = new IPEndPoint(address, 8098);

            try
            {
                //客户端套接字连接到网络节点上，用的是Connect  
                socketclient.Connect(point);
                this.btnSendMessage.Enabled = true;
                this.btnSendMessage.Visible = true;
                this.txtMessage.Visible = true;
            }
            catch (Exception)
            {
                Debug.WriteLine("连接失败\r\n");

                this.txtDebugInfo.AppendText("连接失败\r\n");
                return;
            }

            threadclient = new Thread(recv);
            threadclient.IsBackground = true;
            threadclient.Start();
        }

        // 接收服务端发来信息的方法    
        void recv()
        {
            //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
            byte[] arrRecvmsg = new byte[1024 * 1024];

            int x = 0;
            //持续监听服务端发来的消息 

            while (true)
            {
                try
                {
                    //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                    int length = socketclient.Receive(arrRecvmsg);

                    //将套接字获取到的字符数组转换为人可以看懂的字符串  
                    string strRevMsg = Encoding.UTF8.GetString(arrRecvmsg, 0, length);
                    if (x == 1)
                    {
                        this.txtDebugInfo.AppendText("服务器:" + GetCurrentTime() + "\r\n" + strRevMsg + "\r\n\n");
                        Debug.WriteLine("服务器:" + GetCurrentTime() + "\r\n" + strRevMsg + "\r\n\n");
                    }
                    else
                    {
                        this.txtDebugInfo.AppendText(strRevMsg + "\r\n\n");
                        Debug.WriteLine(strRevMsg + "\r\n\n");
                        x = 1;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("远程服务器已经中断连接" + "\r\n\n");
                    Debug.WriteLine("远程服务器已经中断连接" + "\r\n");
                    break;
                }
            }
        }

        //获取当前系统时间  
        DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        //发送字符信息到服务端的方法  
        void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组     
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组     
            socketclient.Send(arrClientSendMsg);
            //将发送的信息追加到聊天内容文本框中     
            Debug.WriteLine("hello...." + ": " + GetCurrentTime() + "\r\n" + sendMsg + "\r\n\n");
            this.txtDebugInfo.AppendText("hello...." + ": " + GetCurrentTime() + "\r\n" + sendMsg + "\r\n\n");
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            //调用ClientSendMsg方法 将文本框中输入的信息发送给服务端     
            ClientSendMsg(this.txtMessage.Text.Trim());
            this.txtMessage.Clear();
        }

        private void txtDebugInfo_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
