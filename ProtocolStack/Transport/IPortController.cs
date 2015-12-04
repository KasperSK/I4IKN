namespace Transport
{
    public interface IPortController
    {
        IPort GetPort(int comPort);
    }
}