namespace TransportLayer.SenderStates
{
    public class Sending : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            context.Ready = true;
        }

        public override void SendData(SenderStmContext context, byte[] buffer, int offset, int size)
        {
            context.SetMessage(buffer, offset, size);
            context.SendMessage();
            context.SetState(new Waiting());
        }

        public override void Sync(SenderStmContext context)
        {
            context.ResetSequence();
            context.SetSyncMessage();
            context.SendMessage();
            context.SetState(new Waiting());
        }
    }
}