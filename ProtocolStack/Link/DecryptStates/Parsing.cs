namespace LinkLayer.DecryptStates
{
    public class Parsing : DecryptState
    {
        public override bool ParseByte(DecryptStm context, byte b)
        {
            if (b == context.FrameChar)
            {
                context.SetState(new Idle());
                return true;
            }

            if (b == context.FrameEscape)
            {
                context.SetState(new EscapeParse());
                return false;
            }

            context.Buffer[context.BufferSize] = b;
            ++context.BufferSize;
            return false;
        }
    }
}