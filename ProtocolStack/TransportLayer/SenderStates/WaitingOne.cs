namespace TransportLayer.SenderStates
{
    public class WaitingOne : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            if (context.ReceiveAck() == 4)
                context.ReceiveMessage();
        }

        public override void ReceiveMessage(SenderStmContext context)
        {
            if (context.VerifyCheckSum())
                if (context.IsAck(1))
                {
                    context.StopTimer();
                    context.SetState(new SendingZero());
                }
        }

        public override void Timeout(SenderStmContext context)
        {
            context.StopTimer();
            context.SendSegment();
            context.StartTimer();
            context.SetState(this);
        }
    }
}