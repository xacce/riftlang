using Rift;

namespace RiftTests;

public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void SingleLineBasicTypes()
	{
		var bI = RiftTestScriptNullEnv.Execute("update debugi from 2333");
		bI.interpretEnvironment.AssertSingleCall(2333);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));

		var bF = RiftTestScriptNullEnv.Execute("update debugf from 1.5");
		bF.interpretEnvironment.AssertSingleCall(1.5f);
		Assert.That(bF.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));

		var bB = RiftTestScriptNullEnv.Execute("update debugb from true");
		bB.interpretEnvironment.AssertSingleCall(true);
		Assert.That(bB.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));

		var bB2 = RiftTestScriptNullEnv.Execute("update debugb from false");
		bB2.interpretEnvironment.AssertSingleCall(false);
		Assert.That(bB2.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void LocalVariables()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"int i1
float f1
int i2
bool b1
float f2
bool b2
bool b3
i1 = 1
i2 = 2
f1 = 1.5
f2 = 2.5
b1 = true
b2 = false
b3 = true
update debugi from i1
update debugi from i2
update debugf from f1
update debugf from f2
update debugb from b1
update debugb from b2
update debugb from b3
");
		bI.interpretEnvironment.AssertCall(1, 2);
		bI.interpretEnvironment.AssertCall(1.5f, 2.5f);
		bI.interpretEnvironment.AssertCall(true, false, true);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void Assign()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"int i1
int i2
float f1
float f2
bool b1
bool b2
bool b3
i1 = 100
i2 = i1
i1 = 200
f1 = 1.5
f2 = f1
f1 = 2.5
b1 = true
b2 = b1
b1 = false
update debugi from i1
update debugi from i2
update debugf from f1
update debugf from f2
update debugb from b1
update debugb from b2
");
		bI.interpretEnvironment.AssertCall(200, 100);
		bI.interpretEnvironment.AssertCall(2.5f, 1.5f);
		bI.interpretEnvironment.AssertCall(false, true);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void Stop()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
update debugi from 1
stop
update debugi from 2
");
		bI.interpretEnvironment.AssertCall(1);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.Stop));
	}

	[Test]
	public void Stop2()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
update debugi from 1
if 1 == 1
	if 2 == 2
		stop
update debugi from 2
");
		bI.interpretEnvironment.AssertCall(1);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.Stop));
	}
}