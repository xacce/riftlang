using System;

namespace Rift
{
	[Flags]
	public enum RiftPointerType : byte
	{
		Local = 1 << 0,
		Input = 1 << 1,
		Path = 1 << 2,
		Raw = 1 << 3,
		Settings = 1 << 4,
	}

	public enum RiftCompiledOpCode : byte
	{
		Invalid = 0,
		Assigment = 1,
		Free = 2,
		Print = 3,
		Indent = 4,
		Dedent = 5,
		Greater = 6,
		Less = 7,
		GreaterThan = 8,
		LessThan = 9,
		Equal = 10,
		NotEqual = 11,
		Invert = 12,
		If = 13,
		Elif = 14,
		Else = 15,
		DataGet = 16,
		CallRet = 17,
		Call = 18,
		DataSet = 19,
		Or = 20,
		And = 21,

		Int = 22,
		Uint = 23,

		Short = 24,
		Ushort = 25,

		Long = 26,
		Ulong = 27,

		Float = 28,
		Half = 29,
		Double = 30,

		Byte = 31,
		Sbyte = 32,
		Bool = 33,

		Struct = 34,
		Add = 35,
		Sub = 36,
		Mul = 37,
		Div = 38,
		AssigmentMath = 39,
		InvalidInstruction = 249,
		Stop = 250,
		AbnormalExit = 251,
		End = 252,
	}
}