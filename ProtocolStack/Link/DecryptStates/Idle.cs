namespace LinkLayer.DecryptStates
{
    public class Idle : DecryptState
    {
        public override void NewMessage(DecryptStm context, byte[] buffer)
        {
            context.Buffer = buffer;
            context.BufferSize = 0;
            context.SetState(new ReadyToParse());
        }
    }
}