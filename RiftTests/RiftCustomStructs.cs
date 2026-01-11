using System.Runtime.Intrinsics.X86;
using Rift;
using Rift.Lang;
using Rift.Tests.Environments;

namespace RiftTests;

public class RiftCustomStructs
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Basic()
	{
		var code = @"
CustomStruct1 cs1
CustomStruct1 cs2
CustomStruct1 cs3
cs1.i1 = 101
cs1.f1 = 101.0
cs1.b1 = true
cs2.i1 = 102
cs2.f1 = 102.0
cs2.b1 = true
cs3.i1 = 103
cs3.f1 = 103.0
cs3.b1 = true
update debugi from cs1.i1
update debugi from cs2.i1
update debugi from cs3.i1
update debugf from cs1.f1
update debugf from cs2.f1
update debugf from cs3.f1
update debugb from cs1.b1
update debugb from cs2.b1
update debugb from cs3.b1
";
		using var c = RiftTestScriptCustomStructsEnv.Execute(code);
		c.interpretEnvironment.AssertCall(101, 102, 103);
		c.interpretEnvironment.AssertCall(101.0f, 102.0f, 103.0f);
		c.interpretEnvironment.AssertCall(true, true, true);
	}

	[Test]
	public void Basic2()
	{
		var code = @"
CustomStruct1 cs1
CustomStruct2 cs2
CustomStruct3 cs3
cs1.i1 = 101
cs1.f1 = 101.0
cs1.b1 = true
cs2.i1 = 102
cs2.f1 = 102.0
cs2.b1 = true
cs3.i1 = 103
cs3.f1 = 103.0
cs3.b1 = true
update debugi from cs1.i1
update debugi from cs2.i1
update debugi from cs3.i1
update debugf from cs1.f1
update debugf from cs2.f1
update debugf from cs3.f1
update debugb from cs1.b1
update debugb from cs2.b1
update debugb from cs3.b1
";
		using var c = RiftTestScriptCustomStructsEnv.Execute(code);
		c.interpretEnvironment.AssertCall(101, 102, 103);
		c.interpretEnvironment.AssertCall(101.0f, 102.0f, 103.0f);
		c.interpretEnvironment.AssertCall(true, true, true);
	}

	[Test]
	public void Pathing()
	{
		var code = @"
CustomStruct1 cs1
CustomStruct2 cs2
CustomStruct3 cs3
cs1.i1 = 101
cs1.f1 = 101.0
cs1.b1 = true
cs2.i1 = 102
cs2.f1 = 102.0
cs2.b1 = true
cs3.i1 = 103
cs3.f1 = 103.0
cs3.b1 = true
cs2.cs3 = cs3
cs1.cs2 = cs2
update debugi from cs1.i1
update debugi from cs2.i1
update debugi from cs3.i1
update debugf from cs1.f1
update debugf from cs2.f1
update debugf from cs3.f1
update debugb from cs1.b1
update debugb from cs2.b1
update debugb from cs3.b1
update debugi from cs1.cs2.i1
update debugi from cs1.cs2.cs3.i1
update debugf from cs1.cs2.f1
update debugf from cs1.cs2.cs3.f1
update debugb from cs1.cs2.b1
update debugb from cs1.cs2.cs3.b1
";
		using var c = RiftTestScriptCustomStructsEnv.Execute(code);
		c.interpretEnvironment.AssertCall(101, 102, 103, 102, 103);
		c.interpretEnvironment.AssertCall(101.0f, 102.0f, 103.0f, 102.0f, 103.0f);
		c.interpretEnvironment.AssertCall(true, true, true, true, true);
	}

	[Test]
	public void PathingMath()
	{
		var code = @"
CustomStruct1 cs1
CustomStruct2 cs2
CustomStruct2 cs3
cs1.i1 = 101
cs1.f1 = 101.0
cs1.b1 = true
cs2.i1 = 102
cs2.f1 = 102.0
cs2.b1 = true
cs3.i1 = cs1.i1 + cs2.i1
cs3.f1 = cs1.f1 + cs2.f1


update debugi from cs3.i1
update debugf from cs3.f1

";
		using var c = RiftTestScriptCustomStructsEnv.Execute(code);
		c.interpretEnvironment.AssertCall(203);
		c.interpretEnvironment.AssertCall(203.0f);
	}

	[Test]
	public void PathingComparsion()
	{
		var code = @"
CustomStruct1 cs1
CustomStruct2 cs2
CustomStruct2 cs3
cs1.i1 = 101
cs1.f1 = 101.0
cs1.b1 = true
cs2.i1 = 102
cs2.f1 = 102.0
cs2.b1 = true
cs3.i1 = cs1.i1 + cs2.i1
cs3.f1 = cs1.f1 + cs2.f1

if cs3.i1 > 100 and cs3.f1 > 100.0
	update debugi from cs3.i1
	update debugf from cs3.f1
if cs3.i1 > 300 and cs3.f1 > 300.0
	update debugi from cs3.i1
	update debugf from cs3.f1
";
		using var c = RiftTestScriptCustomStructsEnv.Execute(code);
		c.interpretEnvironment.AssertCall(203);
		c.interpretEnvironment.AssertCall(203.0f);
	}

	[Test]
	public void Settings()
	{
		var code = @"settings
	CustomStruct1 cs1
	CustomStruct2 cs2
if cs1.i1 > 100 and cs2.f1 > 100.0
	update debugi from cs1.i1
	update debugf from cs2.f1
";
		using var c = RiftTestScriptCustomStructsEnv.Initialize(code);
		c.SetSettings(new RiftTestsCustomStructsEnv.CustomStruct1 { }, new RiftTestsCustomStructsEnv.CustomStruct2 { });
		c.Tick();
		c.interpretEnvironment.AssertNull();
	}
	
	[Test]
	public void Settings2()
	{
		var code = @"settings
	CustomStruct1 cs1
	CustomStruct2 cs2
if cs1.i1 > 100 and cs2.f1 > 100.0
	update debugi from cs1.i1
	update debugf from cs2.f1
";
		using var c = RiftTestScriptCustomStructsEnv.Initialize(code);
		c.SetSettings(new RiftTestsCustomStructsEnv.CustomStruct1 { i1 = 101, f1 = 101.0f }, new RiftTestsCustomStructsEnv.CustomStruct2 { i1 = 102, f1 = 102.0f });
		c.Tick();
		c.interpretEnvironment.AssertCall(101);
		c.interpretEnvironment.AssertCall(102.0f);
	}
}