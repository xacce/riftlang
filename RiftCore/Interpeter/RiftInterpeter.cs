using System;
using System.Runtime.CompilerServices;
#if UNITY_2020_1_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Rift
{
	public interface IRiftEnvironment
	{
		public void Setup(IRiftCompiler compiler);
	}

	public interface IRiftInterpretEnvironment
	{
		public unsafe void Set(ulong dataTypeId, byte* attr, byte* value, int size);
		public unsafe byte* Fetch(ulong dataTypeId, byte* attr, int size);
		public unsafe void CallRet(ushort func, byte* ret);
		public unsafe void CallRet(ushort func, byte* ret, byte* attr1);
		public unsafe void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2);
		public unsafe void CallRet(ushort func, byte* ret, byte* attr1, byte* attr2, byte* attr3);
		public unsafe void Call(ushort func, byte* attr1);
		public unsafe void Call(ushort func, byte* attr1, byte* attr2);
		public unsafe void Call(ushort func, byte* attr1, byte* attr2, byte* attr3);
	}


	public static partial class RiftInterpret<TMem, TSettings> where TMem : unmanaged, IRiftMem
		where TSettings : unmanaged, IRiftMem
	{
		public static readonly int IndendSize = 1;
		public static readonly int IndendBlockSkipSize = RiftUtility.SizeOf<int>();
		public static readonly int OpCodeSize = 1;

		public struct InterpretResponse
		{
			public RiftCompiledOpCode opCode;
		}

		public static unsafe void Trigger<TContext>(
			ref TContext ctx,
			int triggerIndex,
			byte* p,
			TMem mem,
			TSettings settings,
			RiftScriptMeta meta,
			RiftRwPointerMem rwPointerMem,
			out InterpretResponse response) where TContext : IRiftInterpretEnvironment
		{
			RiftLog.ILog($"Calling trigger {triggerIndex}");

			TryGetCodeBlockOffset(meta, p, triggerIndex, out var offset, out var ok);
			if (ok)
			{
				Interpretet(ref ctx, offset, p, mem, settings, meta, rwPointerMem, out response);
				RiftLog.ILog($"Call trigger {triggerIndex} was succed at: {offset}");
				return;
			}
			else
			{
				RiftLog.ILog($"Call trigger {triggerIndex} was failed");
			}

			response.opCode = RiftCompiledOpCode.InvalidInstruction;
		}

		public static unsafe void Interpretet<TContext>(
			ref TContext ctx,
			int offset,
			byte* p,
			TMem mem,
			TSettings settings,
			RiftScriptMeta meta,
			RiftRwPointerMem rwPointerMem,
			out InterpretResponse response) where TContext : IRiftInterpretEnvironment

		{
			// var scriptMem = new RiftMem(memSize);
			response = default;
			byte* current = p + offset;
			byte* end = p + meta.total;
			bool nextIndent = false;
			byte previousIndent = 0;
			byte currentIndent = 0;
			// byte ignoreIndentMin = 0;
			// byte ignoreIndentMin = 0;
			int indentSize = 0;
			byte lastConditionIndent = 0;
			bool taken = false;
			// byte blockedIndent = 0;
			while (current < end)
			{
				byte _op = *current;
				RiftCompiledOpCode op = (RiftCompiledOpCode)_op;
				RiftLog.ILog($"Read op code: {op}, at: {current - (p + offset)}");
				current += OpCodeSize;
				switch (op)
				{
					case RiftCompiledOpCode.Indent:
					{
						previousIndent = currentIndent;
						ReadIndend(ref current, out currentIndent, out indentSize);
						break;
					}
					case RiftCompiledOpCode.Invalid:
					{
						RiftLog.ILog("kek");
						break;
					}
					case RiftCompiledOpCode.If:
					{
						// var isValid = currentIndent == validIndent;
						// if (!isValid)
						// {
						//     current += indentSize;
						//     Log($"Skip if opcode with {indentSize} block size cause invalid indent, indent: {currentIndent}, valud: {validIndent}");
						//     break;
						// }
						if (currentIndent != lastConditionIndent)
						{
							taken = false;
							lastConditionIndent = currentIndent;
						}

						byte* offsetBeforeIf = current - OpCodeSize;
						var result = ResolveComparsions(ref current, mem, settings, rwPointerMem);
						RiftLog.ILog($"Result if: {result}");
						if (result)
						{
							// validIndent = (byte)(currentIndent + 1);
							// validIndent = (byte)(currentIndent);
							// blockedIndent = 0;
							taken = true;
							// Log($"Successfull if, valid indent: {taken}");
						}
						else
						{
							var compLenght = current - offsetBeforeIf;
							// Log($"Skip if {indentSize}-{compLenght} {indentSize - compLenght} bytes from {current - (p + offset)}  cause fail.");

							current += indentSize - compLenght;
							// blockedIndent = currentIndent;
						}

						break;
					}
					case RiftCompiledOpCode.Elif:
					{
						if (currentIndent > lastConditionIndent)
						{
							taken = false;
							lastConditionIndent = currentIndent;
						}

						// var isValid = currentIndent == validIndent || !indentValidation;
						if (taken)
						{
							// Log(
							// $"Skip elif opcode with {indentSize}  block size from {current - (p + offset)} cause invalid indent, indent: {currentIndent}, valud: {taken}");
							current += indentSize - OpCodeSize; //cause elif opcode included to indent
							break;
						}

						byte* offsetBeforeIf = current - OpCodeSize;
						var result = ResolveComparsions(ref current, mem, settings, rwPointerMem);
						RiftLog.ILog($"Result elif: {result}");
						if (result)
						{
							taken = true;
							// validIndent = (byte)(currentIndent + 1);
							// indentValidation = true;
							// Log($"Successfull elif, valid indent: {taken}");
						}
						else
						{
							var compLenght = current - offsetBeforeIf;
							// Log($"Skip elif {indentSize}-{compLenght} {indentSize - compLenght} bytes from {current - (p + offset)}  cause fail.");

							current += indentSize - compLenght;
						}

						break;
					}
					case RiftCompiledOpCode.Else:
					{
						if (currentIndent > lastConditionIndent)
						{
							RiftLog.Log($"asdasds {currentIndent} {lastConditionIndent}");
							taken = false;
							lastConditionIndent = currentIndent;
						}

						// var isValid = currentIndent == validIndent || !indentValidation;
						if (taken)
						{
							// Log(
							// $"Skip else opcode with {indentSize}  block size from {current - (p + offset)} cause invalid indent, indent: {currentIndent}, valud: {taken}");
							current += indentSize - OpCodeSize; //cause else opcode included to indent
							break;
						}

						byte* offsetBeforeIf = current - OpCodeSize;
						var result = true;
						RiftLog.ILog($"Result elif: {result}");
						if (result)
						{
							taken = true;
							// Log($"Successfull elif, valid indent: {taken}");
						}
						else
						{
							var compLenght = current - offsetBeforeIf;
							// Log($"Skip elif {indentSize}-{compLenght} {indentSize - compLenght} bytes from {current - (p + offset)}  cause fail.");

							current += indentSize - compLenght;
						}

						break;
					}
					case RiftCompiledOpCode.Print:
					{
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out var dt, out var ptype, out var l, out var len);
						if (dt == RiftCompiledOpCode.Int)
						{
							RiftLog.ILog($"{*(int*)l}");
						}
						else if (dt == RiftCompiledOpCode.Bool)
						{
							RiftLog.ILog($"{*(bool*)l}");
						}
						else if (dt == RiftCompiledOpCode.Float)
						{
							RiftLog.ILog($"{*(float*)l}");
						}
						else if (dt == RiftCompiledOpCode.Struct)
						{
							RiftLog.ILog("Structure pointer");
						}
						else
						{
							RiftLog.ILog($"Uknown data tags: {dt}");
						}

						break;
					}
					case RiftCompiledOpCode.Assigment or RiftCompiledOpCode.AssigmentMath:
					{
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out var dtl, out var ptypel, out var v, out var len);
						int lenlen;
						byte* vv;
						RiftCompiledOpCode dtr;
						if (op == RiftCompiledOpCode.AssigmentMath)
						{
							ResolveBasicMath(ref current, mem, rwPointerMem, settings, dtl, v, out dtr, out lenlen);
						}
						else
						{
							ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out dtr, out _, out vv, out lenlen);

							RiftLog.ILog($"Assign {dtl.ToString()}({len}b)={dtr.ToString()}({lenlen}b)");
							// if (dtL != dtR && t != RoflanCompiledOpCode.InputPointer && r != RoflanCompiledOpCode.InputPointer)
							if ((ptypel & RiftPointerType.Input) != 0)
							{
								rwPointerMem.UpdateValueForPointer(v, vv, len, dtl, dtr);
							}
							else if (dtl == RiftCompiledOpCode.Int)
							{
								*(int*)(v) = *(int*)vv;
							}
							else if (dtl == RiftCompiledOpCode.Float)
							{
								*(float*)(v) = dtr == RiftCompiledOpCode.Int ? (float)*(int*)vv : *(float*)vv;
							}
							else if (dtl == RiftCompiledOpCode.Bool)
							{
								*(byte*)(v) = *(byte*)vv;
							}
							else if (dtl == RiftCompiledOpCode.Struct)
							{
								Buffer.MemoryCopy(vv, v, len, len);
							}
						}

						break;
					}
					case RiftCompiledOpCode.DataGet:
					{
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vTo, out var l1);
						ReadEnvironmentDataTypeId(ref current, out var id, out int size);
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);

						Buffer.MemoryCopy(ctx.Fetch(id, vAttr1, size), vTo, l1, l1);
						break;
					}
					case RiftCompiledOpCode.DataSet:
					{
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vValue, out _);
						ReadEnvironmentDataTypeId(ref current, out var id, out int size);
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vTo, out _);
						ctx.Set(id, vTo, vValue, size);
						break;
					}
					case RiftCompiledOpCode.CallRet:
					{
						ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vTo, out _);
						ReadUshort(ref current, out var funcId);
						ReadSingleByte(ref current, out var argsCount);
						RiftLog.ILog($"Get {funcId}, args count {argsCount}");
						switch (argsCount)
						{
							case 0:
							{
								ctx.CallRet(funcId, vTo);
								break;
							}
							case 1:
							{
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								ctx.CallRet(funcId, vTo, vAttr1);
								break;
							}
							case 2:
							{
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr2, out _);
								ctx.CallRet(funcId, vTo, vAttr1, vAttr2);

								break;
							}
							case 3:
							{
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr2, out _);
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr3, out _);
								ctx.CallRet(funcId, vTo, vAttr1, vAttr2, vAttr3);
								break;
							}
							default:
								for (int i = 0; i < argsCount; i++)
								{
									//fallback skip
									ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								}

								break;
						}

						// vTo = ctx.Fetch(id, vAttr1);
						break;
					}
					case RiftCompiledOpCode.Call:
					{
						ReadUshort(ref current, out var funcId);
						ReadSingleByte(ref current, out var argsCount);
						RiftLog.ILog($"Get {funcId}, args count {argsCount}");
						switch (argsCount)
						{
							case 1:
							{
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								ctx.Call(funcId, vAttr1);
								break;
							}
							case 2:
							{
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr2, out _);
								ctx.Call(funcId, vAttr1, vAttr2);

								break;
							}
							case 3:
							{
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr2, out _);
								ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr3, out _);
								ctx.Call(funcId, vAttr1, vAttr2, vAttr3);
								break;
							}
							default:
								for (int i = 0; i < argsCount; i++)
								{
									//fallback skip
									ReadVariableWriteableMemory(ref current, mem, rwPointerMem, settings, out _, out _, out var vAttr1, out _);
								}

								break;
						}

						// vTo = ctx.Fetch(id, vAttr1);
						break;
					}
					case RiftCompiledOpCode.End:
						RiftLog.ILog($"Script end at: {current - (p + offset)}");
						current = end;
						response.opCode = RiftCompiledOpCode.End;
						break;
					case RiftCompiledOpCode.Stop:
						RiftLog.ILog($"Script stop at: {current - (p + offset)}");
						current = end;
						response.opCode = RiftCompiledOpCode.Stop;
						break;
					default:
						RiftLog.ILog("Abnormal end.");
						current = end;
						response.opCode = RiftCompiledOpCode.AbnormalExit;
						break;
				}

				RiftLog.ILog("Read done");
			}

			RiftLog.ILog("Script done");
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void ReadIndend(ref byte* o, out byte indent, out int size)
		{
			indent = *(byte*)o;
			o += IndendSize;
			size = *(int*)o;
			o += IndendBlockSkipSize;
			// Log($"Read indent. Tab size: {indent}, block size: {size}");
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void TryGetCodeBlockOffset(in RiftScriptMeta meta, byte* o, in int codeBlockIndex, out int offset, out bool ok)
		{
			var codeBlockOffsetPointer = meta.triggersBlockSize + codeBlockIndex * sizeof(int);
			offset = *(int*)(o + codeBlockOffsetPointer);
			ok = offset != int.MinValue;
			RiftLog.ILog($"Trying to extract code block offset for {codeBlockIndex} index, pointer at {codeBlockOffsetPointer}, offset value: {offset}, status: {ok}");
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void ReadSingleByte(ref byte* o, out byte value)
		{
			value = *(byte*)o;
			o += 1;
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void ReadUshort(ref byte* o, out ushort value)
		{
			value = *(ushort*)o;
			o += 2;
		}


#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void ReadEnvironmentDataTypeId(ref byte* o, out ulong id, out int size)
		{
			id = *(ulong*)o;
			o += 8;
			size = *(int*)o;
			o += 4;
		}


#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void ReadVariableWriteableMemory(ref byte* o, TMem mem, RiftRwPointerMem rwPointerMem, TSettings roMem, out RiftCompiledOpCode type,
			out RiftPointerType memType,
			out byte* v,
			out int length)
		{
			memType = *(RiftPointerType*)o;
			o += 1;

			type = *(RiftCompiledOpCode*)o;
			o += 1;
			RiftLog.ILog($"Read ptype: {memType}, op:{type}");
			if ((memType & RiftPointerType.Local) != 0)
			{
				ReadVariableWriteableMemory<TMem>(ref o, mem, type, memType, out v, out length);
				return;
			}

			if ((memType & RiftPointerType.Settings) != 0)
			{
				ReadVariableWriteableMemory<TSettings>(ref o, roMem, type, memType, out v, out length);
				return;
			}

			if ((memType & RiftPointerType.Input) != 0)
			{
				if ((memType & RiftPointerType.Path) != 0)
				{
					v = rwPointerMem.GetPointer(*(int*)o) + (*(int*)(o + 4));
					if (type == RiftCompiledOpCode.Struct)
					{
						length = (*(int*)(o + 8));
						o += 12;
					}
					else
					{
						length = RiftUtility.SizeOf(type);
						o += 8;
					}

					return;
				}

				if (type != RiftCompiledOpCode.Struct)
				{
					v = rwPointerMem.GetPointer(*(int*)o);
					length = RiftUtility.SizeOf(type);
					o += 4;
					return;
				}

				v = rwPointerMem.GetPointer(*(int*)o);
				length = *(int*)(o + 4);
				o += 8;
				return;
			}

			v = o;
			length = RiftUtility.SizeOf(type);
			o += length;
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void ReadVariableWriteableMemory<T>(ref byte* o, T mem, RiftCompiledOpCode type, RiftPointerType memType,
			out byte* v,
			out int length)
			where T : unmanaged, IRiftMem
		{
			if ((memType & RiftPointerType.Path) != 0)
			{
				v = mem.ReadRwRaw((*(int*)o));
				if (type == RiftCompiledOpCode.Struct)
				{
					length = (*(int*)(o + 4));
					o += 8;
				}
				else
				{
					length = RiftUtility.SizeOf(type);
					o += 4;
				}

				return;
			}

			if (type == RiftCompiledOpCode.Struct)
			{
				v = mem.ReadRwRaw((*(int*)o));
				length = *(int*)(o + 4);
				o += 8;
				return;
			}

			v = mem.ReadRwRaw((*(int*)o));
			o += 4;
			length = RiftUtility.SizeOf(type);
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void SkipValue(ref byte* o)
		{
			var memType = *(RiftPointerType*)o;
			o += 1;
			var type = *(RiftCompiledOpCode*)o;
			o += 1;
			RiftLog.ILog($"Skip {memType} {type}");
			if ((memType & RiftPointerType.Local) != 0 || (memType & RiftPointerType.Settings) != 0)
			{
				if ((memType & RiftPointerType.Path) != 0)
				{
					if (type == RiftCompiledOpCode.Struct)
					{
						o += 8;
					}
					else
					{
						o += 4;
					}

					return;
				}

				if (type == RiftCompiledOpCode.Struct)
				{
					o += 8;
					return;
				}

				o += 4;
				return;
			}

			if ((memType & RiftPointerType.Input) != 0)
			{
				if ((memType & RiftPointerType.Path) != 0)
				{
					if (type == RiftCompiledOpCode.Struct)
					{
						o += 12;
					}
					else
					{
						o += 8;
					}

					return;
				}

				if (type != RiftCompiledOpCode.Struct)
				{
					o += 4;
					return;
				}

				o += 8;
				return;
			}

			o += RiftUtility.SizeOf(type);
		}
#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe bool ResolveComparsions(ref byte* o, TMem mem, TSettings roMem, RiftRwPointerMem rwPointerMem)
		{
			bool result = ResolveComparsion(ref o, mem, roMem, rwPointerMem);
			RiftLog.ILog($"Result comparsion part: {result}");
			while (true)
			{
				var op = *(RiftCompiledOpCode*)o;
				RiftLog.ILog($"Next op: {op}");
				if (op != RiftCompiledOpCode.Or && op != RiftCompiledOpCode.And) break;
				o += OpCodeSize;

				if ((op == RiftCompiledOpCode.Or && result) || (op == RiftCompiledOpCode.And && !result))
				{
					SkipComparsion(ref o);
					return result;
				}

				result = ResolveComparsion(ref o, mem, roMem, rwPointerMem);
				RiftLog.ILog($"Result comparsion part: {result}");
			}

			return result;
		}


		// static unsafe void SkipComparsion(ref byte* o)
		// {
		//     o += OpCodeSize;
		//     SkipValue(ref o);
		//     SkipValue(ref o);
		//     var no = *(RiftCompiledOpCode*)o;
		//     //todo while pls
		//     if (no == RiftCompiledOpCode.Or || no == RiftCompiledOpCode.And)
		//     {
		//         SkipComparsion(ref o);
		//     }
		// }
#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe void SkipComparsion(ref byte* o)
		{
			while (true)
			{
				SkipValue(ref o); //left
				SkipValue(ref o); //right

				var nextOp = *(RiftCompiledOpCode*)o;
				if (nextOp is RiftCompiledOpCode.Greater or RiftCompiledOpCode.GreaterThan or RiftCompiledOpCode.Less or RiftCompiledOpCode.LessThan or RiftCompiledOpCode.Equal
				    or RiftCompiledOpCode.NotEqual)
				{
					o += OpCodeSize;
					nextOp = *(RiftCompiledOpCode*)o;
					if (nextOp is RiftCompiledOpCode.Or or RiftCompiledOpCode.And)
					{
						o += OpCodeSize;
						continue;
					}
				}

				break;
			}
		}

#if UNITY_2020_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static unsafe bool ResolveComparsion(ref byte* o, TMem mem, TSettings roMem, RiftRwPointerMem rwPointerMem)
		{
			ReadVariableWriteableMemory(ref o, mem, rwPointerMem, roMem, out var opLeft, out _, out var l, out _);
			ReadVariableWriteableMemory(ref o, mem, rwPointerMem, roMem, out var opRight, out _, out var r, out _);
			var op = *(RiftCompiledOpCode*)o;
			o += OpCodeSize;

			if (opLeft == RiftCompiledOpCode.Int)
			{
				int lv = *(int*)l;
				if (opRight == RiftCompiledOpCode.Int)
				{
					int rv = *(int*)r;
					switch (op)
					{
						case RiftCompiledOpCode.Equal: return lv == rv;
						case RiftCompiledOpCode.NotEqual: return lv != rv;
						case RiftCompiledOpCode.Greater: return lv > rv;
						case RiftCompiledOpCode.Less: return lv < rv;
						case RiftCompiledOpCode.LessThan: return lv <= rv;
						case RiftCompiledOpCode.GreaterThan: return lv >= rv;
						default:
							throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
					}
				}

				if (opRight == RiftCompiledOpCode.Float)
				{
					float rv = *(float*)r;
					float lf = lv;
					switch (op)
					{
						case RiftCompiledOpCode.Equal: return lf == rv;
						case RiftCompiledOpCode.NotEqual: return lf != rv;
						case RiftCompiledOpCode.Greater: return lf > rv;
						case RiftCompiledOpCode.GreaterThan: return lf >= rv;
						case RiftCompiledOpCode.Less: return lf < rv;
						case RiftCompiledOpCode.LessThan: return lf <= rv;
						default:
							throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
					}
				}

				throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
			}

			if (opLeft == RiftCompiledOpCode.Float)
			{
				float lf = *(float*)l;
				if (opRight == RiftCompiledOpCode.Float)
				{
					float rf = *(float*)r;
					switch (op)
					{
						case RiftCompiledOpCode.Equal: return lf == rf;
						case RiftCompiledOpCode.NotEqual: return lf != rf;
						case RiftCompiledOpCode.Greater: return lf > rf;
						case RiftCompiledOpCode.Less: return lf < rf;
						case RiftCompiledOpCode.GreaterThan: return lf >= rf;
						case RiftCompiledOpCode.LessThan: return lf <= rf;
						default:
							throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
					}
				}

				if (opRight == RiftCompiledOpCode.Int)
				{
					float rf = *(int*)r;
					switch (op)
					{
						case RiftCompiledOpCode.Equal: return lf == rf;
						case RiftCompiledOpCode.NotEqual: return lf != rf;
						case RiftCompiledOpCode.Greater: return lf > rf;
						case RiftCompiledOpCode.Less: return lf < rf;
						case RiftCompiledOpCode.GreaterThan: return lf >= rf;
						case RiftCompiledOpCode.LessThan: return lf <= rf;
						default:
							throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
					}
				}

				throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
			}

			if (opLeft == RiftCompiledOpCode.Bool)
			{
				byte lv = *(byte*)l;
				byte rv = *(byte*)r;
				switch (op)
				{
					case RiftCompiledOpCode.Equal: return lv == rv;
					case RiftCompiledOpCode.NotEqual: return lv != rv;
					default:
						throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
				}
			}

			throw new Exception($"Unknown data tags for comparsion: {opLeft} vs {opRight}");
		}
	}
}