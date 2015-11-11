using System;
using LinkLayer.EncryptStates;

namespace LinkLayer
{
    public abstract class EncryptState
    {
        public virtual void OnEnter(EncryptStm context)
        {
        }

        public virtual void NewMessage(EncryptStm context, int length)
        {
            context.Buffer = new byte[length * 2 + 2];
            context.BufferSize = 0;
            context.AddByte(context.FrameChar);
            context.SetState(new Parsing());
        }

        public virtual void ParseByte(EncryptStm context, byte b)
        {
            throw new ArgumentException();
        }

        public virtual int GetEncryptedMessage(EncryptStm context, out byte[] buffer)
        {
            throw new ArgumentException();
        }


    }

    public class EncryptStm : IEncrypt
    {
        public readonly byte FrameChar;
        private EncryptState _state;
        public readonly byte FrameEscape;
        public readonly byte FrameSub1;
        public readonly byte FrameSub2;

        public byte[] Buffer { get; set; }
        public int BufferSize;

        public EncryptStm(byte frameChar = 65, byte frameEscape = 66, byte frameSub1 = 67, byte frameSub2 = 68)
        {
            FrameChar = frameChar;
            FrameEscape = frameEscape;
            FrameSub1 = frameSub1;
            FrameSub2 = frameSub2;
            Buffer = null;
            SetState(new Idle());
        }

        public void SetState(EncryptState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        public void AddByte(byte b)
        {
            Buffer[BufferSize] = b;
            ++BufferSize;
        }

        public void ParseByte(byte b)
        {
            _state.ParseByte(this, b);
        }

        public int GetEncryptedMessage(out byte[] buffer)
        {
            return _state.GetEncryptedMessage(this, out buffer);
        }

        public void NewMessage(int length)
        {
            _state.NewMessage(this, length);
        }
    }
}