namespace Link.LinkStates
{
    public class Idle : LinkState
    {
        public override void SendMsg(LinkStateMachine context, byte[] msg)
        {
            context.SetBuffer(msg);
            context.SetState(new MoreToSend());
        }
    }
}