namespace TransportLayer.SenderStates
{
    public class ReSend : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            context.SendMessage();
            context.SetState(new Waiting());
        }
    }
}