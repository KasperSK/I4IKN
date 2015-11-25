namespace TransportLayer
{
    public interface ISequenceGenerator
    {
        byte Sequence { get; set; }

        void Reset();

        void Increment();
    }
}