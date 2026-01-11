namespace Rift.Tests.Environments
{
	public class RiftTestsSetGetDataEnv : IRiftEnvironment
	{
		public struct Target : IEquatable<Target>
		{
			public int i1;

			public bool Equals(Target other)
			{
				return i1 == other.i1;
			}

			public override bool Equals(object? obj)
			{
				return obj is Target other && Equals(other);
			}

			public override int GetHashCode()
			{
				return i1;
			}
		}

		public struct RootStruct
		{
			public int i1;
			public ChildStruct cs2;
		}

		public struct ChildStruct
		{
			public int i1;
		}

		public enum EnvironmentDataType : ulong
		{
			RootStruct = 1,
			ChildStruct = 2,
			Target = 3,
		}

		private readonly RiftTestsNullEnv _nullEnv = new();

		public void Setup(IRiftCompiler compiler)
		{
			_nullEnv.Setup(compiler);
			compiler.RegisterEnvironmentDataType((ulong)EnvironmentDataType.RootStruct, "RootStruct", RiftUtility.SizeOf(typeof(RootStruct)));
			compiler.RegisterEnvironmentDataType((ulong)EnvironmentDataType.ChildStruct, "ChildStruct", RiftUtility.SizeOf(typeof(ChildStruct)));
			compiler.RegisterEnvironmentDataType((ulong)EnvironmentDataType.Target, "Target", RiftUtility.SizeOf(typeof(Target)));
			compiler.RegisterStructRecursive(typeof(RootStruct));
			compiler.RegisterStructRecursive(typeof(ChildStruct));
			compiler.RegisterStructRecursive(typeof(Target));
		}
	}

	public unsafe struct RiftTestsSetGetDataInterpretEnv : IRiftInterpretEnvironment, IDisposable
	{
		public RiftTestsNullInterpretEnv nullEnv;
		public Dictionary<RiftTestsSetGetDataEnv.Target, RiftTestsSetGetDataEnv.RootStruct> data;

		public RiftTestsSetGetDataInterpretEnv()
		{
			nullEnv = new();
			data = new Dictionary<RiftTestsSetGetDataEnv.Target, RiftTestsSetGetDataEnv.RootStruct>();
		}

		public void Dispose()
		{
			nullEnv.Dispose();
		}

		public void Set(ulong dataTypeId, byte* attr, byte* value, int size)
		{
			var target = *(RiftTestsSetGetDataEnv.Target*)attr;
			var root = *(RiftTestsSetGetDataEnv.RootStruct*)value;
			data[target] = root;
		}

		public byte* Fetch(ulong dataTypeId, byte* attr, int size)
		{
			var target = *(RiftTestsSetGetDataEnv.Target*)attr;
			if (!data.TryGetValue(target, out var value))
			{
				var d = default(RiftTestsSetGetDataEnv.RootStruct);
				return (byte*)&d;
			}

			return (byte*)&value;
		}

		public void CallRet(ushort func, byte* ret)
		{
			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret, byte* attr1)
		{
			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2)
		{
			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2, byte* attr3)
		{
			throw new NotImplementedException();
		}

		public void Call(ushort func, byte* attr1)
		{
			nullEnv.Call(func, attr1);
		}

		public void Call(ushort func, byte* attr1, byte* attr2)
		{
			throw new NotImplementedException();
		}

		public void Call(ushort func, byte* attr1, byte* attr2, byte* attr3)
		{
			throw new NotImplementedException();
		}
	}
}