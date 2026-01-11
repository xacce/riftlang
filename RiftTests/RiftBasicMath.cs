using Rift;

namespace RiftTests;

public class RiftBasicMath
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void IntMath()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"int i1
int i2
int i3
int i4
int i5
int i6
int i7
int i8
int i9
int i10
i1 = 100
i2 = 100
i3 = i1 + i2
i4 = i1 - i2
i5 = i1 * i2
i6 = i1 / i2
i7 = 1 + 1
i8 = 2 - 1
i9 = 2 * 2
i10 = 50 / 2
update debugi from i1
update debugi from i2
update debugi from i3
update debugi from i4
update debugi from i5
update debugi from i6
update debugi from i7
update debugi from i8
update debugi from i9
update debugi from i10
");
		bI.interpretEnvironment.AssertCall(
			100,
			100,
			200,
			0,
			10000,
			1,
			2,
			1,
			4,
			25
		);
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void FloatMath()
	{
		var b = RiftTestScriptNullEnv.Execute(@"float i1
float i2
float i3
float i4
float i5
float i6
float i7
float i8
float i9
float i10
i1 = 100.0
i2 = 100.0
i3 = i1 + i2
i4 = i1 - i2
i5 = i1 * i2
i6 = i1 / i2
i7 = 1.0 + 1.0
i8 = 2.0 - 1.0
i9 = 2.0 * 2.0
i10 = 50.0 / 2.0
update debugf from i1
update debugf from i2
update debugf from i3
update debugf from i4
update debugf from i5
update debugf from i6
update debugf from i7
update debugf from i8
update debugf from i9
update debugf from i10
");
		b.interpretEnvironment.AssertCall(
			100.0f,
			100.0f,
			200.0f,
			0.0f,
			10000.0f,
			1.0f,
			2.0f,
			1.0f,
			4.0f,
			25.0f
		);
		Assert.That(b.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void MathDynamicType()
	{
		var b = RiftTestScriptNullEnv.Execute(@"float a1
int a2
float r1
int r2
a1 = 100.0
a2 = 100
r1 = a1 + a2
r2 = a1 + a2
update debugf from r1
update debugi from r2
");
		b.interpretEnvironment.AssertCall(200.0f);
		b.interpretEnvironment.AssertCall(200);
		Assert.That(b.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}

	[Test]
	public void MathDynamicInvType()
	{
		var b = RiftTestScriptNullEnv.Execute(@"float a1
int a2
float r1
int r2
a1 = 100.0
a2 = 100
r1 = a2 + a1
r2 = a2 + a1
update debugf from r1
update debugi from r2
");
		b.interpretEnvironment.AssertCall(200.0f);
		b.interpretEnvironment.AssertCall(200);
		Assert.That(b.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
	}
}