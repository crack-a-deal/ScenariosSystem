namespace Networking
{
    public sealed class DispatchedResponse
    {
        public string Contract { get; }
        public string Payload { get; }

        public DispatchedResponse(string contract, string payload)
        {
            Contract = contract;
            Payload = payload;
        }
    }
}
