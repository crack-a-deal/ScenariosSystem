using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class AdminSeedDataLoader
{
    public AdminSeedData Load()
    {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            "Admin",
            "admin.json");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Файл admin.json не найден: {path}");
        }

        string json = File.ReadAllText(path);

        AdminSeedData data =
            JsonConvert.DeserializeObject<AdminSeedData>(json);

        if (data == null)
        {
            throw new JsonSerializationException(
                "Не удалось десериализовать admin.json.");
        }

        return data;
    }
}