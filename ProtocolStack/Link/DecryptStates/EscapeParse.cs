using System;

namespace LinkLayer.DecryptStates
{
    public class EscapeParse : DecryptState
    {
        public override bool ParseByte(DecryptStm context, byte b)
        {
            if (b == context.FrameSub1)
            {
                context.AddByte(context.FrameChar);
                context.SetState(new Parsing());
                return false;
            }

            if (b == context.FrameSub2)
            {
                context.AddByte(context.FrameEscape);
                context.SetState(new Parsing());
                return false;
            }

            throw new ArgumentException("Hola");
        }
    }
}