using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class JsonTestParser
{
    public JsonTestDTO Parse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON файл не найден: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);

        JsonTestDTO test = JsonConvert.DeserializeObject<JsonTestDTO>(json);

        if (test == null)
        {
            Debug.LogError("Не удалось распарсить JSON.");
            return null;
        }

        return test;
    }
}