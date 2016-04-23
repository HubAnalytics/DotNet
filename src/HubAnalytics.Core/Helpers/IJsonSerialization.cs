namespace HubAnalytics.Core.Helpers
{
    public interface IJsonSerialization
    {
        T Deserialize<T>(string serializedJson) where T : class;
        object Deserialize(string serialziedJson);
        string Serialize(object obj);
    }
}
