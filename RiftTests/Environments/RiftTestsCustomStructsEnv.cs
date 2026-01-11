namespace Rift.Tests.Environments
{
	public class RiftTestsCustomStructsEnv : IRiftEnvironment
	{
		public struct CustomStruct1
		{
			public int i1;
			public float f1;
			public bool b1;
			public CustomStruct2 cs2;
		}

		public struct CustomStruct2
		{
			public int i1;
			public float f1;
			public bool b1;
			public CustomStruct3 cs3;
		}

		public struct CustomStruct3
		{
			public int i1;
			public float f1;
			public bool b1;
		}

		public enum Triggers : int
		{
			TestTrigger1,
			TestTrigger2,
			TestTrigger3,
		}

		private readonly RiftTestsNullEnv _nullEnv = new();

		public void Setup(IRiftCompiler compiler)
		{
			_nullEnv.Setup(compiler);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.TestTrigger1);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.TestTrigger2);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.TestTrigger3);
			compiler.RegisterStructRecursive(typeof(CustomStruct1));
		}
	}
	//
	// public unsafe struct RiftTestsTriggersInterpretEnv : IRiftInterpretEnvironment, IDisposable
	// {
	// 	public RiftTestsNullInterpretEnv nullEnv;
	//
	// 	public RiftTestsTriggersInterpretEnv()
	// 	{
	// 		nullEnv = new();
	// 	}
	//
	// 	public void Dispose()
	// 	{
	// 		nullEnv.Dispose();
	// 	}
	//
	// 	public void Set(ulong dataTypeId, byte* attr, byte* value, int size)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public byte* Fetch(ulong dataTypeId, byte* attr, int size)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public void CallRet(ushort func, byte* ret)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public void CallRet(ushort func, byte* ret, byte* attr1)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2, byte* attr3)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public void Call(ushort func, byte* attr1)
	// 	{
	// 		nullEnv.Call(func, attr1);
	// 	}
	//
	// 	public void Call(ushort func, byte* attr1, byte* attr2)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	//
	// 	public void Call(ushort func, byte* attr1, byte* attr2, byte* attr3)
	// 	{
	// 		throw new NotImplementedException();
	// 	}
	// }
}