using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class JConfig
{
    private readonly string filePath;
    private readonly Type dataType;
    private object data;

    public JConfig(string filePath, Type dataType, ELoadType loadType = ELoadType.DEFAULT)
    {
        this.filePath = filePath;
        this.dataType = dataType;
        if (loadType == ELoadType.DEFAULT)
        {
            Load();
        }
        if (loadType == ELoadType.ASYNC)
        {
            _ = LoadAsync();
        }
    }

    public void Update()
    {
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, jsonString);
    }

    public async Task UpdateAsync()
    {
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            await writer.WriteAsync(jsonString);
        }
    }

    public void Load()
    {
        if (!File.Exists(filePath))
        {
            data = CreateDefaultConfig();
            SaveDefaultConfig();
            //throw new FileNotFoundException($"JSON file not found: {filePath}");
        }

        string jsonString = File.ReadAllText(filePath);
        data = JsonConvert.DeserializeObject(jsonString, dataType);
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(filePath))
        {
            data = CreateDefaultConfig();
            await SaveDefaultConfigAsync();
            //throw new FileNotFoundException($"JSON file not found: {filePath}");
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            string jsonString = await reader.ReadToEndAsync();
            data = JsonConvert.DeserializeObject(jsonString, dataType);
        }
    }

    public T GetData<T>()
    {
        if (data == null)
        {
            throw new InvalidOperationException("JSON data has not been loaded.");
        }

        return (T)data;
    }

    public object GetData()
    {
        if (data == null)
        {
            throw new InvalidOperationException("JSON data has not been loaded.");
        }

        return data;
    }

    private object CreateDefaultConfig()
    {
        // Создаем экземпляр дефолтного конфига со всеми параметрами, равными null
        return Activator.CreateInstance(dataType);
    }

    private void SaveDefaultConfig()
    {
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, jsonString);
    }

    private async Task SaveDefaultConfigAsync()
    {
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            await writer.WriteAsync(jsonString);
        }
    }
}
