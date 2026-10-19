using System;
using System.IO;
using UnityEngine;

public static class NetworkingConfigLoader
{
    private const string CONFIG_PATH = "Config/networking.json";

    public static NetworkingConfig Load()
    {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            CONFIG_PATH);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Networking config not found: {path}");
        }

        string json = File.ReadAllText(path);
        NetworkingConfig config = JsonUtility.FromJson<NetworkingConfig>(json);

        Validate(config);

        return config;
    }

    private static void Validate(NetworkingConfig config)
    {
        if (config == null)
            throw new InvalidOperationException("Networking config is empty.");

        if (config.tcp == null || config.tcp.port < 1 || config.tcp.port > 65535)
            throw new InvalidOperationException("Invalid TCP port.");

        if (config.udpDiscovery == null ||
            config.udpDiscovery.port < 1 ||
            config.udpDiscovery.port > 65535)
        {
            throw new InvalidOperationException("Invalid UDP discovery port.");
        }
    }
}

public static class TestsConfigLoader
{
    public static TestsConfig Load()
    {
        string configPath = Path.Combine(
            Application.streamingAssetsPath,
            "Config/tests.json");

        if (!File.Exists(configPath))
            throw new FileNotFoundException(
                $"Tests config not found: {configPath}");

        string json = File.ReadAllText(configPath);
        TestsConfig config = JsonUtility.FromJson<TestsConfig>(json);

        if (config == null || string.IsNullOrWhiteSpace(config.testBankFile))
            throw new InvalidDataException(
                "Tests config does not contain testBankFile.");

        return config;
    }
}