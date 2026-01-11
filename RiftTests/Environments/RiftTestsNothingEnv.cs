namespace Rift.Tests.Environments
{
	public class RiftTestsNullEnv : IRiftEnvironment
	{
		public void Setup(IRiftCompiler compiler)
		{
			compiler.RegisterEnvironmentFunction(10000, "debugf", typeof(float));
			compiler.RegisterEnvironmentFunction(10001, "debugi", typeof(int));
			compiler.RegisterEnvironmentFunction(10002, "debugb", typeof(bool));
		}
	}

	public unsafe struct RiftTestsNullInterpretEnv : IRiftInterpretEnvironment, IDisposable
	{
		public int fcalls;
		public int icalls;
		public int bcalls;
		public List<float> lastF = new();
		public List<int> lastI = new();
		public List<bool> lastB = new();

		public RiftTestsNullInterpretEnv()
		{
			fcalls = 0;
			icalls = 0;
			bcalls = 0;
		}

		public void AssertNull()
		{
			Assert.That(fcalls, Is.EqualTo(0));
			Assert.That(icalls, Is.EqualTo(0));
			Assert.That(bcalls, Is.EqualTo(0));
		}

		public void ResetCalls()
		{
			fcalls = 0;
			icalls = 0;
			bcalls = 0;
			lastF.Clear();
			lastI.Clear();
			lastB.Clear();
		}

		public void AssertSingleCall<T>(T value) where T : unmanaged
		{
			if (typeof(T) == typeof(float) && value is float f)
			{
				Assert.That(lastF.Count, Is.EqualTo(1));
				Assert.That(lastF[0], Is.EqualTo(f));
				Assert.That(fcalls, Is.EqualTo(1));
				Assert.That(icalls, Is.EqualTo(0));
				Assert.That(bcalls, Is.EqualTo(0));
			}
			else if (typeof(T) == typeof(int) && value is int i)
			{
				Assert.That(lastI.Count, Is.EqualTo(1));
				Assert.That(lastI[0], Is.EqualTo(i));
				Assert.That(fcalls, Is.EqualTo(0));
				Assert.That(icalls, Is.EqualTo(1));
				Assert.That(bcalls, Is.EqualTo(0));
			}
			else if (typeof(T) == typeof(bool) && value is bool b)
			{
				Assert.That(lastB.Count, Is.EqualTo(1));
				Assert.That(lastB[0], Is.EqualTo(b));
				Assert.That(fcalls, Is.EqualTo(0));
				Assert.That(icalls, Is.EqualTo(0));
				Assert.That(bcalls, Is.EqualTo(1));
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public void AssertCall<T>(params T[] values) where T : unmanaged
		{
			if (values[0] is float)
			{
				Assert.That(lastF.Count, Is.EqualTo(values.Length));
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] is float f)
					{
						Assert.That(lastF[i], Is.EqualTo(f));
					}
				}
			}
			else if (values[0] is int)
			{
				Assert.That(lastI.Count, Is.EqualTo(values.Length));
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] is int f)
					{
						Assert.That(lastI[i], Is.EqualTo(f));
					}
				}
			}
			else if (values[0] is bool)
			{
				Assert.That(lastB.Count, Is.EqualTo(values.Length));
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] is bool f)
					{
						Assert.That(lastB[i], Is.EqualTo(f));
					}
				}
			}
		}

		public void Dispose()
		{
			// TODO release managed resources here
		}

		public void Call(ushort func, byte* attr1)
		{
			// Console.WriteLine( "assdasd");
			// Console.WriteLine( *(int*)attr1);
			switch (func)
			{
				case 10000:
					lastF.Add(*(float*)attr1);
					fcalls++;
					break;
				case 10001:
					lastI.Add(*(int*)attr1);
					icalls++;
					break;
				case 10002:
					lastB.Add(*(bool*)attr1);
					bcalls++;
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void Call(ushort func, byte* attr1, byte* attr2)
		{
			throw new NotImplementedException();
		}

		public void Call(ushort func, byte* attr1, byte* attr2, byte* attr3)
		{
			throw new NotImplementedException();
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
	}
}