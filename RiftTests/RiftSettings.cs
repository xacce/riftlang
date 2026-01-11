using System.Runtime.Intrinsics.X86;
using Rift;
using Rift.Lang;
using Rift.Tests.Environments;

namespace RiftTests;

public class RiftSettings
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Settings()
	{
		var code = @"settings
	int i1
	float f1
	bool b1
update debugi from i1
update debugf from f1
update debugb from b1
";
		using var c = RiftTestScriptNullEnv.Initialize(code);
		c.SetSettings(101, 102.5f, true);
		c.Tick();
		c.interpretEnvironment.AssertCall(101);
		c.interpretEnvironment.AssertCall(102.5f);
		c.interpretEnvironment.AssertCall(true);
	}
}