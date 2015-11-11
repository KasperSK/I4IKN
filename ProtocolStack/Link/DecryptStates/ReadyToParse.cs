using System;

namespace LinkLayer.DecryptStates
{
    public class ReadyToParse : DecryptState
    {
        public override bool ParseByte(DecryptStm context, byte b)
        {
            if (b != context.FrameChar) throw new ArgumentException();

            context.SetState(new Parsing());
            return false;
        }
    }
}