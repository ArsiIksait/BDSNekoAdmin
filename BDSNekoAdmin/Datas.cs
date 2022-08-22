using EleCho.Json;

namespace ConfigDatas;

class Data
{
    public Robot Robot = new();
}

public class Robot
{
    public string PluginSocketAddress = "127.0.0.1:8000";
    public string WebSocketAddress = "ws://127.0.0.1:6700";
    public string HttpAddress = "http://127.0.0.1:5700";
    public int GroupId = 687864919;
    public List<uint> Admin = new()
    {
        3251242073
    };
}

class NetPack
{
    public Heartbeat Heartbeat = new();
}

class NetPackTool
{
    public static string GetSerialize(object classObject)
    {
        return JsonSerializer.Serialize(classObject);
    }

    /*public static T? GetDeserialize<T>(string jsonText) where T : class
    {
        return JsonSerializer.Deserialize<T>(jsonText);
    }*/
}

class HeartbeatData
{
    public static DateTime SendHeartbeatTime;
    public static DateTime ReceiveHeartbeatTime;
}

public class Heartbeat
{
    public string Name = "Heartbeat";
    public int Id = new Random().Next(int.MinValue, int.MaxValue);
}

public class Message
{
    public string Name = "Message";
    public string SenderName = string.Empty;
    public long SenderQQ;
    public string Content = string.Empty;

    public Message() { }
    public Message(string senderName, long senderQQ, string content)
    {
        SenderName = senderName;
        SenderQQ = senderQQ;
        Content = content;
    }
}