using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Rift;
using Superpower.Model;
using Unity.Entities;


var s = new Test();

internal partial class Test
{
	public struct Velocity
	{
		public Vector3 x;
	}

	public struct CharacterData
	{
		public int speed;
		public Velocity velocity;

		[MarshalAs(UnmanagedType.U1)] public bool run;
	}


	public unsafe Test()
	{
		var expression = @"input
	int lala
settings
	int lalal2
	int lalal3
int a
int a2
a = 100
trigger event1
	print 100003

trigger event2
	print 100002
trigger justCall2
	print 111134
trigger exit
	print 10000
if a == 100
	print 112333
if a == 100
	a = 101
//print a
//print a
//print a
//print 111
//print a
//print 34334
print 3434
print 324234234
/*
print 324234234
print 324234234
print 324234234
print 324234234
print 324234234
*/
print 11
";


		var ctx = new RiftDevStandaloneInterpretEnvironment();
		TokenList<RiftToken> tokenList = new RiftTokenizer().Tokenize(expression);
		foreach (var token in tokenList)
		{
			RiftLog.Log($"{token.Kind} {token.Position}");
		}

		var script = new RiftParser().Parse(tokenList);

		foreach (var VARIABLE in script.inputStatement.variables)
		{
			RiftLog.Log($"{VARIABLE.dataType} {VARIABLE.name}");
		}

		foreach (var _e in script.sequence)
		{
			RiftLog.Log($"435345 {_e.ind} {_e.statement}");
		}

		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();

		var environment = new RiftDevStandaloneEnvironment();
		var compiler = new RiftCompiler(environment);

		var compiled = compiler.Compile(script);
		stopWatch.Stop();
		RiftLog.Log("--- Compilation report ---");
		RiftLog.Log($"--- Script size {Math.Round(compiled.bytecode.Length / 1024f, 2)} kb ({compiled.bytecode.Length} bytes)");
		RiftLog.Log($"--- Memory size {Math.Round(compiled.meta.variablesSize / 1024f, 2)} kb ({compiled.meta.variablesSize} bytes)");
		RiftLog.Log($"--- Compilation time: {stopWatch.Elapsed.TotalMilliseconds} ms");

		var i1 = 1;
		var i2 = 2;
		var i3 = 3;
		var i4 = 4;
		var b1 = true;
		var character = new CharacterData { run = true, speed = 999, velocity = new Velocity { x = new Vector3 { X = 10, Y = 20, Z = 30 } } };
		var entity = new Entity { Index = 99, Version = 98 };
		Stopwatch stopWatchInt = new Stopwatch();
		stopWatchInt.Start();
		var inputRw = new RiftRwPointerMem();
		inputRw.Init(7);
		inputRw.SetPointer(0, (byte*)&i1);
		inputRw.SetPointer(1, (byte*)&i2);
		inputRw.SetPointer(2, (byte*)&i3);
		inputRw.SetPointer(3, (byte*)&i4);
		inputRw.SetPointer(4, (byte*)&b1);
		inputRw.SetPointer(5, (byte*)&character);
		inputRw.SetPointer(6, (byte*)&entity);

		var settings = new RiftMem(12);
		var variables = new RiftMem(compiled.meta.variablesSize);
		settings.WriteBlind(1);
		settings.WriteBlind(2);
		settings.WriteBlind(3);
		fixed (byte* p = compiled.bytecode)
		{
			RiftInterpret<RiftMem, RiftMem>.Trigger(ref ctx, (int)RiftDevStandaloneEnvironment.Triggers.exit, p, variables, settings,compiled.meta, inputRw, out var response);
			RiftInterpret<RiftMem, RiftMem>.Trigger(ref ctx, (int)RiftDevStandaloneEnvironment.Triggers.justCall, p, variables, settings,compiled.meta, inputRw, out var response2);
			RiftInterpret<RiftMem, RiftMem>.Trigger(ref ctx, (int)RiftDevStandaloneEnvironment.Triggers.justCall2, p, variables, settings,compiled.meta, inputRw, out var response3);
			RiftLog.Log($"Last op code: {response.opCode}");
		}

		// RiftInterpret.Interpretet(ctx, 0, riftscript, compiled.meta, inputRw, out var response);
		stopWatchInt.Stop();
		
		RiftLog.Log($"--- Execution time: {stopWatchInt.Elapsed.TotalMilliseconds} ms");
		// // var value = 100;
		// // inputRw.Update(1, (byte*)&value, 4);
		// // inputRw.Update(2, (byte*)&value, 4);
		// // inputRw.Update(3, (byte*)&value, 4);
		// // var ptr1 = inputRw.Get(0);
		// // byte* ptr2 = inputRw.Get(1);
		// // *(int*)ptr2 = 100;
		//
		// RiftLog.Log(i1);
		// RiftLog.Log(i2);
		// RiftLog.Log(i3);
		// RiftLog.Log(i4);
		// RiftLog.Log(b1);
		// RiftLog.Log(entity.Index);
		// RiftLog.Log(entity.Version);
		// RiftLog.Log(character.speed);
		// RiftLog.Log(character.run);
		RiftLog.Log(character.velocity.x);
		inputRw.Dispose();
	}
}