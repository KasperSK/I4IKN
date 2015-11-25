using System;

namespace TransportLayer.ReceivingStates
{
    public class MissingSync : ReceiverSuperState
    {
        public override void OnEnter(ReceiverStmContext context)
        {
            context.Ready = false;
        }

        public override void MessageReceived(ReceiverStmContext context)
        {
            if (context.ValidateMessage())
            {
                Console.WriteLine("Valid Message: " + (char)context.MessageType);
                if (context.MessageType == DataType.Syn && context.ValidSync())
                {
                    Console.WriteLine("Got sync");
                    context.UpdateSequence();
                    context.SetAckReply();
                    context.IncrementSequence();
                    context.SendReply();
                    context.SetState(new Receiving());
                }
            }
        }
    }
}