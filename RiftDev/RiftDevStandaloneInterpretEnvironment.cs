using System.Runtime.InteropServices;
using Rift;

namespace Unity.Entities
{
	public struct Entity
	{
		public int Index;
		public int Version;
	}

	public struct TestComponent1
	{
		public int intValue;
		public float floatValue;
		public bool boolValue;
	}

	public unsafe struct RiftDevStandaloneInterpretEnvironment : IRiftInterpretEnvironment, IDisposable
	{
		private Dictionary<int, IntPtr> _storage;

		public RiftDevStandaloneInterpretEnvironment()
		{
			_storage = new Dictionary<int, IntPtr>();
		}

		public void Set(ulong dataTypeId, byte* attr, byte* value, int size)
		{
			var e = (Entity*)attr;
			var key = e->Index;

			if (!_storage.TryGetValue(key, out var ptr))
			{
				ptr = Marshal.AllocHGlobal(sizeof(TestComponent1));
				_storage[key] = ptr;
			}

			Buffer.MemoryCopy(
				value,
				(void*)ptr,
				sizeof(TestComponent1),
				sizeof(TestComponent1)
			);

			var v = (TestComponent1*)ptr;
			RiftLog.Log($"Script wants set intvalue to {v->intValue} for entity index {e->Index}");
		}

		public byte* Fetch(ulong dataTypeId, byte* attr, int size)
		{
			var e = (Entity*)attr;
			var key = e->Index;

			if (!_storage.TryGetValue(key, out var ptr))
			{
				RiftLog.Log($"Script wants get intvalue for entity index {e->Index}, but no data");
				return null;
			}
			else
			{
				var v = (TestComponent1*)ptr;
				RiftLog.Log($"Script wants get intvalue {v->intValue} for entity index {e->Index}");
				return (byte*)ptr;
			}
		}

		public void CallRet(ushort func, byte* ret)
		{
			throw new NotImplementedException();
		}


		public void CallRet(ushort _func, byte* ret, byte* attr1)
		{
			var func = (RiftDevStandaloneEnvironment.Functions)_func;
			switch (func)
			{
				case RiftDevStandaloneEnvironment.Functions.Square:
				{
					float s1 = *(float*)attr1;
					*(float*)ret = s1 * s1;
					break;
				}
			}
		}

		public void CallRet(ushort _func, byte* ret, byte* attr1, byte* attr2)
		{
			var func = (RiftDevStandaloneEnvironment.Functions)_func;
			switch (func)
			{
				case RiftDevStandaloneEnvironment.Functions.TestFn:
				{
					Entity* s1 = (Entity*)attr1;
					Entity* s2 = (Entity*)attr2;
					*(TestComponent1*)ret = new TestComponent1 { intValue = 100 };
					break;
				}
			}
		}

		public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2, byte* attr3)
		{
			throw new NotImplementedException();
		}

		public void Call(ushort func, byte* attr1)
		{
			throw new NotImplementedException();
		}

		public void Call(ushort _func, byte* attr1, byte* attr2)
		{
			var func = (RiftDevStandaloneEnvironment.Functions)_func;
			switch (func)
			{
				case RiftDevStandaloneEnvironment.Functions.TestFnRet:
				{
					RiftLog.Log("CALLLLLLLED");
					break;
				}
			}
		}

		public void Call(ushort func, byte* attr1, byte* attr2, byte* attr3)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			foreach (var ptr in _storage.Values)
				Marshal.FreeHGlobal(ptr);

			_storage.Clear();
		}
	}
}