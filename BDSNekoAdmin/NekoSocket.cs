using ConfigDatas;
using System.Net;
using System.Net.Sockets;
using System.Text;

class MessageReceivedEventArgs
{
    public MessageReceivedEventArgs(string message) { Message = message; }
    public string Message { get; }
}

class NekoSocket
{
    bool connect;
    TcpClient? client;
    NetworkStream? stream;
    readonly NetPack netPack = new();
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public event MessageReceivedEventHandler MessageReceived;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    NetworkStream? GetNetworkStream()
    {
        return stream;
    }

    TcpClient? GetTcpClient()
    {
        return client;
    }

    public void StartServer(string IPAddressAndPort)
    {
        Thread server = new(() => { 
            try
            {
                IPAddress iPAddress = IPAddress.Parse(IPAddressAndPort.Split(":")[0]);
                int port = int.Parse(IPAddressAndPort.Split(":")[1]);
                TcpListener tcpListener = new(iPAddress, port);
                tcpListener.Start();

                DiaLog.Log($"NekoSocket服务器已启动： {IPAddressAndPort} 正在等待插件连接... ");

                Thread heartbeat = new(() => {
                    while (true)
                    {
                        while(connect)
                        {
                            SendMessage(NetPackTool.GetSerialize(new Heartbeat()));
                            HeartbeatData.SendHeartbeatTime = DateTime.Now;
                            Thread.Sleep(5000);
                        }
                    }
                });
                heartbeat.Start();

                Thread heartbeatCheck = new(() => {
                    while (true)
                    {
                        while (connect)
                        {
                            int second = (HeartbeatData.SendHeartbeatTime.Minute * 60 + HeartbeatData.SendHeartbeatTime.Second) - (HeartbeatData.ReceiveHeartbeatTime.Minute * 60 + HeartbeatData.ReceiveHeartbeatTime.Second);

                            if (second > 60)
                            {
                                GetTcpClient()?.Close();
                                connect = false;
                                DiaLog.Warning("插件未能及时发送心跳包，已断开NekoSocket连接!");
                            }

                            Thread.Sleep(5000);
                        }
                    }
                });
                heartbeatCheck.Start();

                var bytes = new byte[1024];
                string data = string.Empty;

                while (true)
                {
                    client = tcpListener.AcceptTcpClient();
                    DiaLog.Log($"插件已连接: {client.Client.RemoteEndPoint}");
                    connect = true;
                    stream = client.GetStream();
                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        MessageReceived?.Invoke(this, new(Encoding.UTF8.GetString(bytes, 0, i)));
                    }
                    DiaLog.Log($"插件已断开连接: {client.Client.RemoteEndPoint}");
                    client.Close();
                    connect = false;
                }
            } catch (Exception ex) {
                GetTcpClient()?.Close();
                connect = false;
                DiaLog.Error($"NekoSocket服务器出现错误! 已断开连接: {ex}");
            }
        });
        server.Start();
    }
    
    public void SendMessage(string msg)
    {
        try
        {
            if (msg.IndexOf(netPack.Heartbeat.Name) == 0)
                DiaLog.Log($"发送消息JSON: {msg}");
            if (connect)
                GetNetworkStream()?.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
        }
        catch (Exception ex)
        {
            DiaLog.Error($"NekoSocket服务器发送消息出错: {ex}");
        }
    }
}