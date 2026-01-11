using System.Runtime.Intrinsics.X86;
using Rift;
using Rift.Lang;

namespace RiftTests;

public class RiftComparsions
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Single()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 >= 2
	update debugi from 100
if 2 < 1
	update debugi from 100
if 2 <= 1
	update debugi from 100
if 2 == 1
	update debugi from 100
if 2 != 2
	update debugi from 100
if 2.5 < 2.4
	update debugi from 100
if 2.5 <= 2.4
	update debugi from 100
if 2.5 == 2.4
	update debugi from 100
if 2.5 != 2.5
	update debugi from 100
if true == false
	update debugi from 100
if false == true
	update debugi from 100
if true != true
	update debugi from 100
if false != false
	update debugi from 100

if 1 == 1
	update debugb from true
if 2 > 1
	update debugb from true
if 1 >= 1
	update debugb from true
if 1.0 == 1.0
	update debugb from true
if 2.5 > 2.4
	update debugb from true
if 2.5 >= 2.4
	update debugb from true
if true == true
	update debugb from true
if false == false
	update debugb from true

");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(4 + 4));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(9 + 4 + 4 + 4));
	}

	[Test]
	public void DynamicTyped()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 >= 2.0
	update debugi from 100
if 1.0 >= 2
	update debugi from 100
if 1 >= 1.0
	update debugb from true
if 1.0 >= 1
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(2));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(4));
	}

	[Test]
	public void Multi()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugi from 100
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == false 
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(1));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(2));
	}

	[Test]
	public void IfELif1()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugi from 100
elif 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfELif2()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugi from 100
elif 1 > 0 and 1 >= 1 and 1 == 1 and true == true
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(1));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElse()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugi from 100
else
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(1));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElse2()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == false 
	update debugi from 100
else
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(1));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElifElse()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugi from 100
elif 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugf from 100
else
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(1));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElifElse2()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugi from 100
elif 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == false 
	update debugf from 100
else
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(1));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElifElse3()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == false 
	update debugi from 100
elif 1 > 0 and 1 >= 1 and 1 == 1 and true == true and false == true 
	update debugf from 100
else
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(1));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElifElse4()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 111 
	update debugi from 100
elif 2 > 111
	update debugi from 100
elif 3 > 0
	update debugb from true
elif 4 > 111
	update debugi from 100
else
	update debugi from 100
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(1));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(1));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(3));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void IfElifElse5()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 111 and 2 > 111
	update debugi from 100
elif 1 > 111 and 2 > 111
	update debugi from 100
if 2 > 3 and 1 == 1 and  1==2
	update debugi from 100
elif 2 > 3 and 1 == 1 and  1==2
	update debugi from 100

update debugi from 101
");
		bI.interpretEnvironment.AssertCall(101);
	}

	[Test]
	public void IfElifElse6()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 111 or 2 > 111 or 1 > 0 or 1 > 0 or 1 > 0
	update debugi from 101
");
		bI.interpretEnvironment.AssertCall(101);
	}
//
// 	[Test]
// 	public void IfElifElse6()
// 	{
// 		var bI = RiftTestScriptNullEnv.Execute(@"
// if 1 > 111 or 2 > 111 or 1 == 1
// 	update debugi from 101
// elif 1 > 111 and 2 > 111
// 	update debugi from 100
// else	
// 	update debugi from 100
// if 1 == 2 or 1 == 1 or 3 == 2
// 	update debugi from 101
// elif 1 == 2 or 1 == 1 or 3 == 2
// 	update debugi from 100
// else
// 	update debugi from 100
//
// ");
// 		bI.interpretEnvironment.AssertCall(101,101);
// 	}


	[Test]
	public void Nested()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0
	update debugb from true
	if 2 > 0
		update debugb from true
		if 3 > 0
			update debugb from true
		else
			update debugi from 100
	else
		update debugi from 100
else
	update debugi from 100
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(3));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
		// Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElifStatement ? 1 : 0), Is.EqualTo(3));
		// Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is ElseStatement ? 1 : 0), Is.EqualTo(1));
	}

	[Test]
	public void Nested2()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 1
	update debugb from true
	if 2 > 1
		update debugb from true
		if 3 > 1
			update debugb from true
		else
			update debugb from true
	else
		update debugb from true
else
	update debugi from 100
");
		bI.interpretEnvironment.AssertCall(100);
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(1));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
	}

	[Test]
	public void Nested3()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 >= 1
	update debugi from 101
	if 2 > 3
		update debugb from true
		if 3 > 1
			update debugb from true
		else
			update debugb from true
	else
		update debugi from 102
else
	update debugb from 100
");
		bI.interpretEnvironment.AssertCall(101, 102);
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(2));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
	}

	[Test]
	public void Nested4()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 >= 1
	update debugi from 101
	if 2 >= 2
		update debugi from 102
		if 3 > 4
			update debugb from true
		else
			update debugi from 103
	else
		update debugb from true
else
	update debugb from true
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(3));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		bI.interpretEnvironment.AssertCall(101, 102, 103);
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
	}

	[Test]
	public void Nested5()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0
	update debugi from 101
	if 1 > 0
		update debugi from 102
		if 1 > 0
			update debugi from 103
		elif 2 > 0
			update debugi from 1
		else
			update debugi from 1
	elif 2 > 0
		update debugi from 1
	else
		update debugi from 1
elif 2 > 0
	update debugi from 1
else
	update debugi from 1

update debugi from 104
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(4));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		bI.interpretEnvironment.AssertCall(101, 102, 103, 104);
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
	}

	[Test]
	public void Nested6()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0
	update debugi from 101
	if 1 > 0
		update debugi from 102
		if 1 > 0
			update debugi from 103
		elif 2 > 0
			update debugi from 1
		else
			update debugi from 1
	elif 2 > 0
		update debugi from 1
	else
		update debugi from 1
	update debugi from 104
elif 2 > 0
	update debugi from 1
else
	update debugi from 1

update debugi from 105
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(5));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		bI.interpretEnvironment.AssertCall(101, 102, 103, 104, 105);
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
	}

	[Test]
	public void Nested7()
	{
		var bI = RiftTestScriptNullEnv.Execute(@"
if 1 > 0
	update debugi from 101
	update debugi from 101
	update debugi from 101
	if 1 > 0
		update debugi from 102
		update debugi from 102
		update debugi from 102
		if 1 > 0
			update debugi from 103
			update debugi from 103
			update debugi from 103
		elif 2 > 0
			update debugi from 1
		else
			update debugi from 1
	elif 2 > 0
		update debugi from 1
	else
		update debugi from 1
	update debugi from 104
	update debugi from 104
	update debugi from 104
elif 2 > 0
	update debugi from 1
else
	update debugi from 1

update debugi from 105
update debugi from 105
update debugi from 105
");
		Assert.That(bI.interpretEnvironment.fcalls, Is.EqualTo(0));
		Assert.That(bI.interpretEnvironment.icalls, Is.EqualTo(5 + 8 + 2));
		Assert.That(bI.interpretEnvironment.bcalls, Is.EqualTo(0));
		Assert.That(bI.lastResponse.opCode, Is.EqualTo(RiftCompiledOpCode.End));
		bI.interpretEnvironment.AssertCall(101, 101, 101, 102, 102, 102, 103, 103, 103, 104, 104, 104, 105, 105, 105);
		Assert.That(bI.parsed.sequence.Sum(statement => statement.statement is IfStatement ? 1 : 0), Is.EqualTo(3));
	}
}