using Unity.Entities;

namespace Rift
{
	public class RiftDevStandaloneEnvironment : IRiftEnvironment
	{
		public enum Functions : ushort
		{
			TestFn,
			TestFnRet,
			Square,
		}

		public enum Triggers : ushort
		{
			exit,
			justCall,
			justCall2,
		}

		public static bool TestFn(Entity arg, Entity arg1)
		{
			return false;
		}

		public void Setup(IRiftCompiler compiler)
		{
			compiler.RegisterExternalTrigger<Triggers>(Triggers.exit);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.justCall);
			compiler.RegisterExternalTrigger<Triggers>(Triggers.justCall2);
			compiler.RegisterPrimitiveInput<int>("int", RiftCompiledOpCode.Int);
			compiler.RegisterPrimitiveInput<bool>("bool", RiftCompiledOpCode.Bool);
			// compiler.RegisterStructRecursive(typeof(Test.CharacterData), "CharacterData");
			// compiler.RegisterStructRecursive(typeof(TestComponent1), "TestComponent1");
			// compiler.RegisterStructRecursive(typeof(Entity), "Entity");
			// compiler.RegisterEnvironmentDataType(100, "TestComponent1");
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.TestFn, "customfunc", typeof(bool), new[] { typeof(Entity), typeof(Entity) });
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.TestFnRet, "customfunc2", typeof(bool), new[] { typeof(Entity), typeof(Entity) });
			compiler.RegisterEnvironmentFunctionRet((ushort)Functions.Square, "square", typeof(float), new[] { typeof(float) });
		}
	}
}