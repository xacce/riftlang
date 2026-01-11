using System.Runtime.InteropServices;

namespace Rift
{
	public unsafe struct RiftMem : IRiftMem
	{
		private IntPtr ptr;
		private int offset;

		public RiftMem(int length)
		{
			ptr = Marshal.AllocHGlobal(length);
		}

		public RiftMem(byte[] mem)
		{
			ptr = GCHandle.Alloc(mem, GCHandleType.Pinned).AddrOfPinnedObject();
		}

		public unsafe void WriteBlind<T>(T v) where T : unmanaged
		{
			*(T*)((byte*)ptr + offset) = v;
			offset += RiftUtility.SizeOf<T>();
		}

		public unsafe void WriteBlind<T>(T v, int off) where T : unmanaged
		{
			*(T*)((byte*)ptr + off) = v;
			offset += RiftUtility.SizeOf<T>();
		}

		public unsafe ref readonly T ReadRoBlit<T>(int offset) where T : unmanaged
		{
			return ref *(T*)((byte*)ptr + offset);
		}

		public unsafe ref T ReadRwBlit<T>(int offset) where T : unmanaged
		{
			return ref *(T*)((byte*)ptr + offset);
		}

		public byte* ReadRwRaw(int offset)
		{
			return (byte*)ptr + offset;
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(ptr);
			ptr = IntPtr.Zero;
		}
	}
}