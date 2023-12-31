﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class JConfig : IDisposable
{
    private readonly string filePath;
    private readonly Type dataType;
    private object data;
    private Dictionary<int, string> comments; // Словарь для хранения строк-комментариев
    private int totalLoadedLenght;
    
    public string Name => dataType.Name;
    public int Comments => comments.Count;
    public int LoadedLenght => totalLoadedLenght;

    public JConfig(string filePath, Type dataType, ELoadType loadType = ELoadType.DEFAULT)
    {
        this.filePath = filePath;
        this.dataType = dataType;
        this.comments = new Dictionary<int, string>(); // Инициализация словаря комментариев
        this.totalLoadedLenght = 0;

        if (loadType == ELoadType.DEFAULT) Load();
        if (loadType == ELoadType.ASYNC) Task.Run(() => LoadAsync());
    }

    // Добавляем метод для обновления словаря комментариев
    private void UpdateComments(string jsonString)
    {
        string[] lines = jsonString.Split('\n');
        comments.Clear();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("//"))
            {
                comments[i] = line;
                lines[i] = string.Empty; // Удаляем комментарий из JSON строки
            }
        }

        jsonString = string.Join("\n", lines);
        jsonString = Regex.Replace(jsonString, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline); // Удаляем пустые строки
        data = JsonConvert.DeserializeObject(jsonString, dataType);
        totalLoadedLenght = jsonString.Length;

        JConfigEvents.Invoke_OnLoaded(this);
    }

    public void Update()
    {
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

        // Добавляем комментарии к строке JSON перед сохранением
        foreach (var kvp in comments)
        {
            jsonString = InsertComment(jsonString, kvp.Key, kvp.Value);
        }

        File.WriteAllText(filePath, jsonString);
        
        JConfigEvents.Invoke_OnUpdated(this);
    }

    public async Task UpdateAsync()
    {
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

        // Добавляем комментарии к строке JSON перед сохранением
        foreach (var kvp in comments)
        {
            jsonString = InsertComment(jsonString, kvp.Key, kvp.Value);
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            await writer.WriteAsync(jsonString);
        }
        
        JConfigEvents.Invoke_OnUpdated(this);
    }

    public void Load()
    {
        if (!File.Exists(filePath))
        {
            data = CreateDefaultConfig();
            SaveDefaultConfig();

            JConfigEvents.Invoke_OnError(
                this, new FileNotFoundException($"JSON file not found: {filePath}"));
        }

        string jsonString = File.ReadAllText(filePath);
        UpdateComments(jsonString); // Обновляем словарь комментариев при загрузке
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(filePath))
        {
            data = CreateDefaultConfig();
            await SaveDefaultConfigAsync();

            JConfigEvents.Invoke_OnError(
                this, new FileNotFoundException($"JSON file not found: {filePath}"));
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            string jsonString = await reader.ReadToEndAsync();
            UpdateComments(jsonString); // Обновляем словарь комментариев при асинхронной загрузке
        }
    }

    public T GetData<T>()
    {
        if (data == null)
        {
            JConfigEvents.Invoke_OnError(
                this, new InvalidOperationException("JSON data has not been loaded."));
        }

        return (T)data;
    }

    public object GetData()
    {
        if (data == null)
        {
            JConfigEvents.Invoke_OnError(
                this, new InvalidOperationException("JSON data has not been loaded."));
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

    // Метод для вставки комментария в JSON строку
    private string InsertComment(string jsonString, int lineIndex, string comment)
    {
        string[] lines = jsonString.Split('\n');

        if (lineIndex >= lines.Length)
        {
            // Если индекс больше или равен длине массива строк, добавляем комментарий в конец
            jsonString += $"\n{comment}";
        }
        else
        {
            lines[lineIndex] = comment + "\n" + lines[lineIndex];
            jsonString = string.Join("\n", lines);
        }

        return jsonString;
    }

    public void Dispose()
    {
        data = null;
        comments = null;
    }
}
