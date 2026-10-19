namespace Networking
{
    public interface IMessageSerializer
    {
        T Deserialize<T>(string json);
        string Serialize<T>(T value);
    }
}