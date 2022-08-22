using ConfigDatas;
using EleCho.Json;

namespace ConfigManage;

class Config
{
    static readonly string configFile = "config.json";

    public static Data? ReadConfig()
    {
        try
        {
            DiaLog.Log("正在读取配置文件");

            if (!File.Exists(configFile))
            {
                SaveConfig(null);
                DiaLog.Log("未发现配置文件，已自动创建");
            }

            string jsonText = File.ReadAllText(configFile, System.Text.Encoding.UTF8);
            return JsonSerializer.Deserialize<Data>(jsonText);
        }
        catch (Exception ex)
        {
            DiaLog.Error($"读取配置文件时出错: {ex}");
            return null;
        }
    }

    public static bool SaveConfig(Data? data)
    {
        try
        {
            if (data == null)
            {
                Data defaultData = new()
                {
                    Robot = new()
                };
                File.WriteAllText(configFile, JsonSerializer.Serialize(defaultData), System.Text.Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(configFile, JsonSerializer.Serialize(data), System.Text.Encoding.UTF8);
                DiaLog.Log("已保存配置文件");
            }

            return true;
        }
        catch (Exception ex)
        {
            DiaLog.Error($"保存配置文件时出错: {ex}");
            return false;
        }
    }
}