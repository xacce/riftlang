using System.Runtime.Intrinsics.X86;
using Rift;
using Rift.Lang;
using Rift.Tests.Environments;

namespace RiftTests;

public class RiftTriggers
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void LocalNull()
	{
		var code = @"
trigger testTrigger1
	update debugi from 101
";
		var bI = RiftTestScriptNullEnv.Execute(code);
		bI.interpretEnvironment.AssertNull();
	}

	[Test]
	public void LocalNull2()
	{
		var code = @"
update debugi from 100
trigger testTrigger1
	update debugi from 102
update debugi from 103
";
		var bI = RiftTestScriptNullEnv.Execute(code);
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(2));
		bI.interpretEnvironment.AssertCall(100, 103);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void LocalNull3()
	{
		var code = @"
update debugi from 100
trigger testTrigger1
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger testTrigger2
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger testTrigger3
	update debugi from 102
	update debugi from 102
	update debugi from 102
update debugi from 103
";
		var bI = RiftTestScriptNullEnv.Execute(code);
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(2));
		bI.interpretEnvironment.AssertCall(100, 103);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void LocalNull4()
	{
		var code = @"
update debugi from 100
trigger testTrigger1
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger testTrigger2
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger testTrigger3
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger TestTrigger1
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger TestTrigger2
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger TestTrigger3
	update debugi from 102
	update debugi from 102
	update debugi from 102
update debugi from 103
";
		var bI = RiftTestScriptTriggersEnv.Execute(code);
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(2));
		bI.interpretEnvironment.AssertCall(100, 103);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void LocalCall()
	{
		var code = @"
update debugi from 100

trigger testTrigger1
	update debugi from 102
	update debugi from 102
	update debugi from 102
trigger testTrigger2
	update debugi from 103
	update debugi from 103
	update debugi from 103
trigger testTrigger3
	update debugi from 104
	update debugi from 104
	update debugi from 104
trigger TestTrigger1
	update debugi from 105
	update debugi from 105
	update debugi from 105
trigger TestTrigger2
	update debugi from 106
	update debugi from 106
	update debugi from 106
trigger TestTrigger3
	update debugi from 107
	update debugi from 107
	update debugi from 107

update debugi from 100
";
		var bI = RiftTestScriptTriggersEnv.Trigger((int)RiftTestsTriggersEnv.Triggers.TestTrigger1, code);
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(3));
		bI.interpretEnvironment.AssertCall(105, 105, 105);
		bI.interpretEnvironment.ResetCalls();
		bI.Trigger((int)RiftTestsTriggersEnv.Triggers.TestTrigger2);
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(3));
		bI.interpretEnvironment.AssertCall(106, 106, 106);
		bI.interpretEnvironment.ResetCalls();
		bI.Trigger((int)RiftTestsTriggersEnv.Triggers.TestTrigger3);
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(3));
		bI.interpretEnvironment.AssertCall(107, 107, 107);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}
}