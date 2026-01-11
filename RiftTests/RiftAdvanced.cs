using System.Runtime.Intrinsics.X86;
using Rift;
using Rift.Lang;
using Rift.Tests.Environments;

namespace RiftTests;

public class RiftAdvanced
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Functions()
	{
		var code = @"
float a1
float a2
float a3
float s2
float s3
float ppi

a1 = 5
a2 = 6
a3 = 11
s2 = fetch sum from a1 a2
s3 = fetch sum3 from a1 a2 a3
ppi = fetch pi
update debugf from s2
update debugf from s3
s3 = fetch square from s3
update debugf from s3
update debugf from ppi
update call2 from 100.0 200.0
update call3 from 1000.0 2000.0 3000.0

";
		using var c = RiftTestScriptAvdEnv.Execute(code);
		c.interpretEnvironment.nullEnv.AssertCall(11.0f, 22f, 22f * 22f, (float)Math.PI);
		Assert.That(c.interpretEnvironment.calls.Count, Is.EqualTo(5));
		Assert.That(c.interpretEnvironment.calls[0], Is.EqualTo(100.0f));
		Assert.That(c.interpretEnvironment.calls[1], Is.EqualTo(200.0f));
		Assert.That(c.interpretEnvironment.calls[2], Is.EqualTo(1000.0f));
		Assert.That(c.interpretEnvironment.calls[3], Is.EqualTo(2000.0f));
		Assert.That(c.interpretEnvironment.calls[4], Is.EqualTo(3000.0f));
	}
}