namespace Rift.Tests.Environments
{
	public class RiftTestsAvdancedEnv : IRiftEnvironment
	{
		public enum Triggers : int
		{
			Exit,
			CallMeMaybe,
		}

		public enum Functions : ushort
		{
			Sum = 0,
			Sum3 = 1,
			Square = 2,
			PI = 3,
			Call2 = 5,
			Call3 = 6,
		}

		private readonly RiftTestsNullEnv _nullEnv = new();

		public void Setup(IRiftCompiler compiler)
		{
			_nullEnv.Setup(compiler);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.Exit);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.CallMeMaybe);
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.Sum3, "sum3", typeof(float), new[] { typeof(float), typeof(float), typeof(float) });
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.Sum, "sum", typeof(float), new[] { typeof(float), typeof(float) });
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.Square, "square", typeof(float), new[] { typeof(float) });
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.PI, "pi", typeof(float));
			compiler.RegisterEnvironmentFunction((ushort)Functions.Call2, "call2", new[] { typeof(float), typeof(float) });
			compiler.RegisterEnvironmentFunction((ushort)Functions.Call3, "call3", new[] { typeof(float), typeof(float), typeof(float) });

			// compiler.RegisterStructRecursive(typeof(CustomStruct1));
		}
	}


	public unsafe struct RiftTestsAdvInterpretEnv : IRiftInterpretEnvironment, IDisposable
	{
		public RiftTestsNullInterpretEnv nullEnv;
		public List<float> calls;

		public RiftTestsAdvInterpretEnv()
		{
			nullEnv = new();
			calls = new();
		}

		public void Dispose()
		{
			nullEnv.Dispose();
		}

		public void Set(ulong dataTypeId, byte* attr, byte* value, int size)
		{
			throw new NotImplementedException();
		}

		public byte* Fetch(ulong dataTypeId, byte* attr, int size)
		{
			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret)
		{
			var _func = (RiftTestsAvdancedEnv.Functions)func;
			switch (_func)
			{
				case RiftTestsAvdancedEnv.Functions.PI:
					*(float*)ret = (float)Math.PI;
					return;
				default:
					throw new NotImplementedException();
			}

			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret, byte* attr1)
		{
			var _func = (RiftTestsAvdancedEnv.Functions)func;
			switch (_func)
			{
				case RiftTestsAvdancedEnv.Functions.Square:
					float s1 = *(float*)attr1;
					*(float*)ret = s1 * s1;

					return;
				default:
					throw new NotImplementedException();
			}

			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2)
		{
			var _func = (RiftTestsAvdancedEnv.Functions)func;
			switch (_func)
			{
				case RiftTestsAvdancedEnv.Functions.Sum:
					float s1 = *(float*)attr1;
					float s2 = *(float*)attr2;
					*(float*)ret = s1 + s2;
					return;
				default:
					throw new NotImplementedException();
			}

			throw new NotImplementedException();
		}

		public void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2, byte* attr3)
		{
			var _func = (RiftTestsAvdancedEnv.Functions)func;
			switch (_func)
			{
				case RiftTestsAvdancedEnv.Functions.Sum3:
					float s1 = *(float*)attr1;
					float s2 = *(float*)attr2;
					float s3 = *(float*)attr3;
					*(float*)ret = s1 + s2 + s3;
					return;
				default:
					throw new NotImplementedException();
			}

			throw new NotImplementedException();
		}

		public void Call(ushort func, byte* attr1)
		{
			nullEnv.Call(func, attr1);
		}

		public void Call(ushort func, byte* attr1, byte* attr2)
		{
			calls.Add(*(float*)attr1);
			calls.Add(*(float*)attr2);
		}

		public void Call(ushort func, byte* attr1, byte* attr2, byte* attr3)
		{
			calls.Add(*(float*)attr1);
			calls.Add(*(float*)attr2);
			calls.Add(*(float*)attr3);
		}
	}
}