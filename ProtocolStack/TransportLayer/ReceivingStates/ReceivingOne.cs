namespace TransportLayer.ReceivingStates
{
    public class ReceivingOne : ReceiverSuperState
    {
        public override void ReceiveMessage(ReceiverStmContext context)
        {
            context.ReadSerial();
            if (context.VerifyCheckSum() && context.CheckSequence(1))
            {
                context.SetUpAck(1);
                context.SendAck();
                context.CopyToReturnBuffer();
            }
            else
            {
                context.SetUpAck(0);
                context.SendAck();
                context.SetReturnZero();
            }
        }
    }
}