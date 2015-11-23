using System;

namespace TransportLayer.SenderStates
{
    public class WaitingZero : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            Console.WriteLine("Waiting for ACK");
            if(context.ReceiveAck() == 4)
                context.ReceiveMessage();
            Console.WriteLine("Ack received");
        }

        public override void ReceiveMessage(SenderStmContext context)
        {
            if(context.VerifyCheckSum())
                if (context.IsAck(0))
                {
                    context.StopTimer();
                    context.SetState(new SendingOne());
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