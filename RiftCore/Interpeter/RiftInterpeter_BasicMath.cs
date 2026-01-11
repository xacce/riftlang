using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rift;
#if UNITY_2020_1_OR_NEWER
using Unity.Mathematics;
#endif

namespace Rift
{
	public static partial class RiftInterpret<TMem, TSettings> where TMem : unmanaged, IRiftMem
		where TSettings : unmanaged, IRiftMem
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static unsafe void WriteMath(RiftCompiledOpCode toType, byte* result, int value)
		{
			if (toType == RiftCompiledOpCode.Float)
			{
				*(float*)result = (float)value;
			}
			else
			{
				*(int*)result = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static unsafe void WriteMath(RiftCompiledOpCode toType, byte* result, float value1)
		{
			if (toType == RiftCompiledOpCode.Int)
			{
				*(int*)result = (int)value1;
			}
			else
			{
				*(float*)result = value1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static unsafe void ResolveBasicMath(ref byte* o, TMem mem, RiftRwPointerMem rwPointerMem, TSettings roMem, RiftCompiledOpCode resultType, byte* result,
			out RiftCompiledOpCode type, out int length)
		{
			ReadVariableWriteableMemory(ref o, mem, rwPointerMem, roMem, out var o1, out _, out var v1, out _);
			ReadVariableWriteableMemory(ref o, mem, rwPointerMem, roMem, out var o2, out _, out var v2, out _);
			var op = *(RiftCompiledOpCode*)o;
			o += OpCodeSize;
			type = o1;

			switch (o1)
			{
				case RiftCompiledOpCode.Int:
				{
					var vt1 = *(int*)v1;
					var vt2 = o2 == RiftCompiledOpCode.Float ? (int)*(float*)v2 : *(int*)v2;

					switch (op)
					{
						case RiftCompiledOpCode.Add: WriteMath(resultType, result, vt1 + vt2); break;
						case RiftCompiledOpCode.Sub: WriteMath(resultType, result, vt1 - vt2); break;
						case RiftCompiledOpCode.Mul: WriteMath(resultType, result, vt1 * vt2); break;
						case RiftCompiledOpCode.Div: WriteMath(resultType, result, vt1 / vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 4;
					break;
				}
				case RiftCompiledOpCode.Float:
				{
					var vt1 = *(float*)v1;
					var vt2 = o2 == RiftCompiledOpCode.Int ? (float)*(int*)v2 : *(float*)v2;
					if (o2 == RiftCompiledOpCode.Int)
					{
						Console.WriteLine($"azazaza vt1: {vt1}, vt2: {vt2}");
					}

					switch (op)
					{
						case RiftCompiledOpCode.Add: WriteMath(resultType, result, vt1 + vt2); break;
						case RiftCompiledOpCode.Sub: WriteMath(resultType, result, vt1 - vt2); break;
						case RiftCompiledOpCode.Mul: WriteMath(resultType, result, vt1 * vt2); break;
						case RiftCompiledOpCode.Div: WriteMath(resultType, result, vt1 / vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 4;
					break;
				}
				case RiftCompiledOpCode.Double:
				{
					var vt1 = *(double*)v1;
					var vt2 = o2 == RiftCompiledOpCode.Int ? *(int*)v2 : o2 == RiftCompiledOpCode.Float ? *(float*)v2 : *(double*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(double*)result = vt1 + vt2; break;
						case RiftCompiledOpCode.Sub: *(double*)result = vt1 - vt2; break;
						case RiftCompiledOpCode.Mul: *(double*)result = vt1 * vt2; break;
						case RiftCompiledOpCode.Div: *(double*)result = vt1 / vt2; break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 8;
					break;
				}
				case RiftCompiledOpCode.Long:
				{
					var vt1 = *(long*)v1;
					var vt2 = *(long*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(long*)result = vt1 + vt2; break;
						case RiftCompiledOpCode.Sub: *(long*)result = vt1 - vt2; break;
						case RiftCompiledOpCode.Mul: *(long*)result = vt1 * vt2; break;
						case RiftCompiledOpCode.Div: *(long*)result = vt1 / vt2; break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 8;
					break;
				}
				case RiftCompiledOpCode.Ulong:
				{
					var vt1 = *(ulong*)v1;
					var vt2 = *(ulong*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(ulong*)result = vt1 + vt2; break;
						case RiftCompiledOpCode.Sub: *(ulong*)result = vt1 - vt2; break;
						case RiftCompiledOpCode.Mul: *(ulong*)result = vt1 * vt2; break;
						case RiftCompiledOpCode.Div: *(ulong*)result = vt1 / vt2; break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 8;
					break;
				}
				case RiftCompiledOpCode.Uint:
				{
					var vt1 = *(uint*)v1;
					var vt2 = *(uint*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(uint*)result = vt1 + vt2; break;
						case RiftCompiledOpCode.Sub: *(uint*)result = vt1 - vt2; break;
						case RiftCompiledOpCode.Mul: *(uint*)result = vt1 * vt2; break;
						case RiftCompiledOpCode.Div: *(uint*)result = vt1 / vt2; break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 8;
					break;
				}
				case RiftCompiledOpCode.Short:
				{
					var vt1 = *(short*)v1;
					var vt2 = *(short*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(short*)result = (short)(vt1 + vt2); break;
						case RiftCompiledOpCode.Sub: *(short*)result = (short)(vt1 - vt2); break;
						case RiftCompiledOpCode.Mul: *(short*)result = (short)(vt1 * vt2); break;
						case RiftCompiledOpCode.Div: *(short*)result = (short)(vt1 / vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 2;
					break;
				}
				case RiftCompiledOpCode.Ushort:
				{
					var vt1 = *(ushort*)v1;
					var vt2 = *(ushort*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(ushort*)result = (ushort)(vt1 + vt2); break;
						case RiftCompiledOpCode.Sub: *(ushort*)result = (ushort)(vt1 - vt2); break;
						case RiftCompiledOpCode.Mul: *(ushort*)result = (ushort)(vt1 * vt2); break;
						case RiftCompiledOpCode.Div: *(ushort*)result = (ushort)(vt1 / vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 2;
					break;
				}
				case RiftCompiledOpCode.Sbyte:
				{
					var vt1 = *(sbyte*)v1;
					var vt2 = *(sbyte*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(sbyte*)result = (sbyte)(vt1 + vt2); break;
						case RiftCompiledOpCode.Sub: *(sbyte*)result = (sbyte)(vt1 - vt2); break;
						case RiftCompiledOpCode.Mul: *(sbyte*)result = (sbyte)(vt1 * vt2); break;
						case RiftCompiledOpCode.Div: *(sbyte*)result = (sbyte)(vt1 / vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 1;
					break;
				}
				case RiftCompiledOpCode.Byte:
				{
					var vt1 = *(byte*)v1;
					var vt2 = *(byte*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(byte*)result = (byte)(vt1 + vt2); break;
						case RiftCompiledOpCode.Sub: *(byte*)result = (byte)(vt1 - vt2); break;
						case RiftCompiledOpCode.Mul: *(byte*)result = (byte)(vt1 * vt2); break;
						case RiftCompiledOpCode.Div: *(byte*)result = (byte)(vt1 / vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 1;
					break;
				}

				case RiftCompiledOpCode.Half:
				{
#if UNITY_2021_1_OR_NEWER
					var vt1 = *(half*)v1;
					var vt2 = *(half*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(half*)result = (half)((float)vt1 + (float)vt2); break;
						case RiftCompiledOpCode.Sub: *(half*)result = (half)((float)vt1 - (float)vt2); break;
						case RiftCompiledOpCode.Mul: *(half*)result = (half)((float)vt1 * (float)vt2); break;
						case RiftCompiledOpCode.Div: *(half*)result = (half)((float)vt1 / (float)vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 2;

#else
					var vt1 = *(Half*)v1;
					var vt2 = *(Half*)v2;
					switch (op)
					{
						case RiftCompiledOpCode.Add: *(Half*)result = (Half)((float)vt1 + (float)vt2); break;
						case RiftCompiledOpCode.Sub: *(Half*)result = (Half)((float)vt1 - (float)vt2); break;
						case RiftCompiledOpCode.Mul: *(Half*)result = (Half)((float)vt1 * (float)vt2); break;
						case RiftCompiledOpCode.Div: *(Half*)result = (Half)((float)vt1 / (float)vt2); break;
						default: throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
					}

					length = 2;

#endif
					break;
				}
				default:
					throw new Exception($"Invalid math instruction. Math op: {op}, type1: {o1}, type2: {o2}");
			}
		}
	}
}