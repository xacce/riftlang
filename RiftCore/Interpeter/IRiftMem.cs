namespace Rift
{
	public interface IRiftMem : IDisposable
	{
		public unsafe void WriteBlind<T>(T v) where T : unmanaged;

		public unsafe void WriteBlind<T>(T v, int off) where T : unmanaged;

		public unsafe ref readonly T ReadRoBlit<T>(int offset) where T : unmanaged;

		public unsafe ref T ReadRwBlit<T>(int offset) where T : unmanaged;

		public unsafe byte* ReadRwRaw(int offset);
	}
}