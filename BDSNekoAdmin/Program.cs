using ConfigDatas;
using ConfigManage;
using EleCho.Json;
using IlyfairyLib.GoCqHttpSdk;
using IlyfairyLib.GoCqHttpSdk.Api;

try
{
    Data? config;

    config = Config.ReadConfig();

    if (config == null)
    {
        Console.Write("配置文件读取失败，请删除配置文件或者打开文本编辑器尝试修复! 请按任意键退出程序. . .");
        Console.ReadKey();
        Environment.Exit(-1);
    }

    Session session = new(config.Robot.WebSocketAddress, config.Robot.HttpAddress);
    int groupId = config.Robot.GroupId;
    //const int groupId = 295322097;
    List<uint> admin = config.Robot.Admin;
    NekoSocket nekoSocket = new();

#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行

    //当WebSocket的连接状态发生改变时
    session.UseWebSocketConnect(async (isConnect) =>
    {
        if (isConnect)
        {
            DiaLog.Info("WebSocket已连接");
            DiaLog.Info("\n==============================\nBDSNekoAdmin v1.0.0 Made By ArsiIksait\nProvide By ilyfairy's C# GoCqHttp Sdk&GoCqHttp QQ Robot\n发送 #help 命令查看帮助!\n==============================");
        }
        else
        {
            DiaLog.Error("ws连接断开, 正在尝试重连");
        }
        return true;
    });

    session.UseLifecycle(async (v) =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Robot连接成功");
        nekoSocket.StartServer(config.Robot.PluginSocketAddress);
        return true;
    });

    //程序退出事件
    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
    {
        DiaLog.Log("程序退出 保存配置文件");
        Config.SaveConfig(config);
    };

    //接收所有群消息
    session.UseGroupMessage(async v =>
    {
        if (v.GroupId == groupId)
        {
            string json = NetPackTool.GetSerialize(new Message(v.Sender?.CardName ?? v.Sender?.Name!, v.QQ, v.Message.Text));
            DiaLog.Log($"发送JSON消息: {json}");
            nekoSocket.SendMessage(json);
        }
        return true;
    });

    nekoSocket.MessageReceived += (async (sender, e) => {
        DiaLog.Log($"收到客户端消息JSON: {e.Message}");
        JsonAny jAny = (JsonObject)JsonReader.Read(e.Message);
        switch (jAny["Name"].StringValue)
        {
            case "Heartbeat":
                HeartbeatData.ReceiveHeartbeatTime = DateTime.Now;
                break;
            case "Message":
                #region
                var message = JsonSerializer.Deserialize<Message>(e.Message);
                if (message != null)
                {
                    await session.SendGroupMessageAsync(groupId, $"[{message.SenderName}]: {message.Content}");
                }
                #endregion
                //await session.SendGroupMessageAsync(groupId, $"[{jAny["SenderName"].StringValue}]: {jAny["Content"].StringValue}");
                break;
        }
    });

    session.Build();
    Thread.Sleep(-1);
}
catch (Exception ex)
{
    DiaLog.Error($"机器人出错: {ex}");
}