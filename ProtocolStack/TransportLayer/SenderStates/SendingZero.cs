namespace TransportLayer.SenderStates
{
    public class SendingZero : SenderSuperState
    {
        public override void SendMessage(SenderStmContext context, byte[] buffer, int size)
        {
            context.SetUpBuffer(buffer, size);
            context.SetUpHeader(0, DataType.Data);
            context.MakeCheckSum();
            context.SendSegment();
            context.StartTimer();
            context.SetState(new WaitingZero());
        }
    }
}