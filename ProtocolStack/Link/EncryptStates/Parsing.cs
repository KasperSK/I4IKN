using System.Runtime.InteropServices;

namespace LinkLayer.EncryptStates
{
    public class Parsing : EncryptState
    {
        public override void ParseByte(EncryptStm context, byte b)
        {
            if (b == context.FrameChar)
            {
                context.AddByte(context.FrameEscape);
                context.AddByte(context.FrameSub1);
            } else if (b == context.FrameEscape)
            {
                context.AddByte(context.FrameEscape);
                context.AddByte(context.FrameSub2);
            }
            else
            {
                context.AddByte(b);
            }
        }

        public override int GetEncryptedMessage(EncryptStm context, out byte[] buffer)
        {
            context.AddByte(context.FrameChar);
            buffer = context.Buffer;
            context.SetState(new Idle());
            return context.BufferSize;
        }
    }
}