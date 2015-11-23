namespace TransportLayer.SenderStates
{
    public class SendingOne : SenderSuperState
    {
        public override void SendMessage(SenderStmContext context, byte[] buffer, int size)
        {
            context.SetUpBuffer(buffer, size);
            context.MakeCheckSum();
            context.SetUpHeader(1, DataType.Data);
            context.SendSegment();
            //context.StartTimer();
            context.SetState(new WaitingOne());
        }
    }
}