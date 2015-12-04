using System;

namespace TransportLayer.ReceivingStates
{
    public class Receiving : ReceiverSuperState
    {
        public override void OnEnter(ReceiverStmContext context)
        {
            context.Ready = false;
        }

        public override void MessageReceived(ReceiverStmContext context)
        {
            if (context.ValidateMessage())
            {
                if (context.MessageType == DataType.Syn && context.ValidSync())
                {
                    Console.WriteLine("ReceivingState\t Valid Sync");
                    context.UpdateSequence();
                    context.SetAckReply();
                    context.IncrementSequence();
                    context.SendReply();
                    return;
                }

                if (context.MessageType == DataType.Data && context.ValidData())
                {
                    Console.WriteLine("ReceivingState\t Valid Data");
                    if (context.ValidSequence())
                    {
                        context.SetAckReply();
                        context.IncrementSequence();
                        context.Ready = true;
                    }
                    
                    context.SendReply();
                    return;
                }
                    
            }
        }
    }
}