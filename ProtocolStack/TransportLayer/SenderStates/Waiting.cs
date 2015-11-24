namespace TransportLayer.SenderStates
{
    public class Waiting : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            context.Ready = false;
        }

        public override void ReceivedMessage(SenderStmContext context, Message message)
        {
            if (context.ValidateReply())
            {
                context.IncrementSequence();
                context.SetState(new Sending());
                return;
            }
            context.SetState(new ReSend());
        }

        public override void Timeout(SenderStmContext context)
        {
            context.SetState(new ReSend());
        }
    }
}