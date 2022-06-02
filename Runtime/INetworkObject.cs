namespace FunkySheep.NetWind
{
    public interface INetworkObject
    {
        ulong NetId { get; }
        bool IsOwn { get; }
    }
}
