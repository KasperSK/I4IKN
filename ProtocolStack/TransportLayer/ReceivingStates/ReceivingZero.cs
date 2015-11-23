namespace TransportLayer.ReceivingStates
{
    public class ReceivingZero : ReceiverSuperState
    {
        public override void ReceiveMessage(ReceiverStmContext context)
        {
            context.ReadSerial();
            if (context.VerifyCheckSum() && context.CheckSequence(0))
            {
                context.SetUpAck(0);
                context.SendAck();
                context.CopyToReturnBuffer();
            }
            else
            {
                context.SetUpAck(1);
                context.SendAck();
                context.SetReturnZero();
            }
        }
    }
}