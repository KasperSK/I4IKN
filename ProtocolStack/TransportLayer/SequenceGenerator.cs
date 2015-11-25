namespace TransportLayer
{
    public class SequenceGenerator : ISequenceGenerator
    {
        public SequenceGenerator()
        {
            Sequence = 0;
        }

        public byte Sequence { get; set; }

        public virtual void Reset()
        {
            Sequence = 0;
        }

        public void Increment()
        {
            ++Sequence;
        }
    }
}