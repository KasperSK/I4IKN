using System;
using LinkLayer.DecryptStates;

namespace LinkLayer
{
    public abstract class DecryptState
    {
        public virtual void OnEnter(DecryptStm context)
        {
        }

        public virtual bool ParseByte(DecryptStm context, byte b)
        {
            throw new ArgumentException();
        }

        public virtual void NewMessage(DecryptStm context, byte[] buffer)
        {
            throw new ArgumentException();
        }

        
        public void Reset(DecryptStm context)
        {
            context.SetState(new Idle());
        }
        
    }

    public class DecryptStm : IDecrypt
    {
        public readonly byte FrameChar;
        private DecryptState _state;
        public readonly byte FrameEscape;
        public readonly byte FrameSub1;
        public readonly byte FrameSub2;


        public DecryptStm(byte frameChar = 65, byte frameEscape = 66, byte frameSub1 = 67, byte frameSub2 = 68)
        {
            FrameChar = frameChar;
            FrameEscape = frameEscape;
            FrameSub1 = frameSub1;
            FrameSub2 = frameSub2;
            Buffer = null;
            SetState(new Idle());
        }

        public byte[] Buffer { get; set; }
        public int BufferSize { get; set; }

        public void NewMessage(byte[] buffer)
        {
            _state.NewMessage(this, buffer);
        }

        public bool ParseByte(byte b)
        {
            return _state.ParseByte(this, b);
        }

        public void SetState(DecryptState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        public void AddByte(byte b)
        {
            Buffer[BufferSize] = b;
            ++BufferSize;
        }

        public void Reset()
        {
            _state.Reset(this);
        }
    }
}