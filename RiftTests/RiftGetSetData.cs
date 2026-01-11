using System.Runtime.Intrinsics.X86;
using Rift;
using Rift.Lang;
using Rift.Tests.Environments;

namespace RiftTests;

public class RiftGetSetData
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Basic()
	{
		var code = @"settings
	Target target
	int tick
RootStruct preset
ChildStruct presetchild
presetchild.i1 = 2
preset.i1 = 1
preset.cs2 = presetchild

RootStruct cached
if tick == 1
	update debugi from target.i1
if tick == 2 and target.i1 == 1
	update debugi from 101
if tick == 3 and target.i1 == 1
	cached = fetch from target
	update debugi from cached.i1
	update debugi from cached.cs2.i1
if tick == 4 and target.i1 > 0
	update target from preset
if tick == 5 and target.i1 != 0
	cached = fetch from target
	update debugi from cached.i1
	update debugi from cached.cs2.i1
if tick == 6 and target.i1 != 0
	int ssm
	ssm = cached.i1 + cached.cs2.i1
	update debugi from ssm
";
		using var c = RiftTestScriptSetGetDataEnv.Initialize(code);

		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 0);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertNull();
		c.interpretEnvironment.nullEnv.ResetCalls();

		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 1);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertCall(1);
		c.interpretEnvironment.nullEnv.ResetCalls();

		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 2);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertCall(101);
		c.interpretEnvironment.nullEnv.ResetCalls();

		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 3);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertCall(0, 0);
		c.interpretEnvironment.nullEnv.ResetCalls();

		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 4);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertNull();
		c.interpretEnvironment.nullEnv.ResetCalls();

		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 5);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertCall(1, 2);
		c.interpretEnvironment.nullEnv.ResetCalls();
		
		c.SetSettings(new RiftTestsSetGetDataEnv.Target { i1 = 1 }, 6);
		c.Tick();
		c.interpretEnvironment.nullEnv.AssertCall(3);
		c.interpretEnvironment.nullEnv.ResetCalls();
	}
}