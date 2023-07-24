using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class JConfig
{
    private readonly string filePath;
    private object data;

    public JConfig(string filePath)
    {
        this.filePath = filePath;
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

    public void Load<T>()
    {
        if (!File.Exists(filePath))
        {
            data = CreateDefaultConfig<T>();
            SaveDefaultConfig();
            //throw new FileNotFoundException($"JSON file not found: {filePath}");
        }

        string jsonString = File.ReadAllText(filePath);
        data = JsonConvert.DeserializeObject<T>(jsonString);
    }

    public async Task LoadAsync<T>()
    {
        if (!File.Exists(filePath))
        {
            data = CreateDefaultConfig<T>();
            await SaveDefaultConfigAsync();
            //throw new FileNotFoundException($"JSON file not found: {filePath}");
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            string jsonString = await reader.ReadToEndAsync();
            data = JsonConvert.DeserializeObject<T>(jsonString);
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
    private T CreateDefaultConfig<T>()
    {
        // Создаем экземпляр дефолтного конфига со всеми параметрами, равными null
        return Activator.CreateInstance<T>();
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
