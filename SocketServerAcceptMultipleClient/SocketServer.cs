using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SocketServerAcceptMultipleClient
{
    public class SocketServer
    {
        // 创建一个和客户端通信的套接字
        static Socket socketwatch = null;
        //定义一个集合，存储客户端信息
        static Dictionary<string, Socket> clientConnectionItems = new Dictionary<string, Socket> { };

        public static void Main(string[] args)
        {
            //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议）  
            socketwatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //服务端发送信息需要一个IP地址和端口号  
            IPAddress address = IPAddress.Parse("127.0.0.1");
            //将IP地址和端口号绑定到网络节点point上  
            IPEndPoint point = new IPEndPoint(address, 8098);
            //此端口专门用来监听的  

            //监听绑定的网络节点  
            socketwatch.Bind(point);

            //将套接字的监听队列长度限制为20  
            socketwatch.Listen(20);

            //负责监听客户端的线程:创建一个监听线程  
            Thread threadwatch = new Thread(watchconnecting);

            //将窗体线程设置为与后台同步，随着主线程结束而结束  
            threadwatch.IsBackground = true;

            //启动线程     
            threadwatch.Start();

            Console.WriteLine("开启监听。。。");
            Console.WriteLine("点击输入任意数据回车退出程序。。。");
            Console.ReadKey();
            Console.WriteLine("退出监听，并关闭程序。");
        }

        //监听客户端发来的请求  
        static void watchconnecting()
        {
            Socket connection = null;

            //持续不断监听客户端发来的请求     
            while (true)
            {
                try
                {
                    connection = socketwatch.Accept();
                }
                catch (Exception ex)
                {
                    //提示套接字监听异常     
                    Console.WriteLine(ex.Message);
                    break;
                }

                //客户端网络IP和port
                string remoteEndPoint = connection.RemoteEndPoint.ToString();
                //添加客户端信息  
                clientConnectionItems.Add(remoteEndPoint, connection);

                Console.WriteLine("【" + remoteEndPoint + "】已上线，在线人数" + clientConnectionItems.Count.ToString());

                byte[] sendByte = new byte[1024 * 1024];
                //通知各个客户端，有新客户端上线
                foreach (var item in clientConnectionItems)
                {
                    //排除掉上线的那个客户端
                    if (!String.Equals(remoteEndPoint, item.Key.ToString()))
                    {
                        sendByte = Encoding.UTF8.GetBytes("【" + remoteEndPoint + "】已上线");
                        item.Value.Send(sendByte);
                    }
                }

                //创建一个通信线程      
                ParameterizedThreadStart pts = new ParameterizedThreadStart(recv);
                Thread thread = new Thread(pts);
                //设置为后台线程，随着主线程退出而退出 
                thread.IsBackground = true;
                //启动线程     
                thread.Start(connection);
            }
        }

        /// <summary>
        /// 接收客户端发来的信息，客户端套接字对象
        /// </summary>
        /// <param name="socketclientpara"></param>    
        static void recv(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;

            //获取客户端的IP和Port
            string clientIPE = socketServer.RemoteEndPoint.ToString();

            //创建一个内存缓冲区，其大小为1024*1024字节  即1M     
            byte[] arrServerRecMsg = new byte[1024 * 1024];

            while (true)
            {

                
                try
                {
                    sendToClient(socketServer, clientIPE, arrServerRecMsg);
                }
                catch (Exception e)
                {
                    exceptionHandling(socketServer, e);
                    break;
                }



            }
        }
        //将接收到的信息存入到内存缓冲区
        static void sendToClient(Socket server, string clientIP, byte[] arrMsg)
        {
            int length = server.Receive(arrMsg);

            //将机器接受到的字节数组转换为人可以读懂的字符串     
            string strSRecMsg = Encoding.UTF8.GetString(arrMsg, 0, length);

            //将发送的字符串信息附加到文本框txtMsg上     
            Console.WriteLine("客户端:" + server.RemoteEndPoint + ",time:" + GetCurrentTime() + "\r\n" + strSRecMsg + "\r\n\n");

            foreach (var item in clientConnectionItems)
            {
                //将上线的那个客户端的IP和Port改为“我”
                byte[] strSend = new byte[1024 * 1024];
                if (!String.Equals(clientIP, item.Key.ToString()))
                {
                    strSend = Encoding.UTF8.GetBytes("【" + clientIP + "】：  " + strSRecMsg);
                }
                else
                {
                    //strSend = Encoding.UTF8.GetBytes("【 我 】：  " + strSRecMsg);
                }
                item.Value.Send(strSend);
            }
        }


        static void exceptionHandling(Socket server, Exception ex)
        {
            clientConnectionItems.Remove(server.RemoteEndPoint.ToString());

            Console.WriteLine("Client Count:" + clientConnectionItems.Count);

            //提示套接字监听异常  
            Console.WriteLine("客户端" + server.RemoteEndPoint + "已经中断连接" + "\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n");
            //关闭之前accept出来的和客户端进行通信的套接字 
            server.Close();
        }
        ///      
        /// 获取当前系统时间的方法    
        /// 当前时间     
        static DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }
    }
}
