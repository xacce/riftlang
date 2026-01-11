using System;
using System.Collections.Generic;
using System.Linq;
using Rift.Lang;
using Superpower;

namespace Rift
{
	public interface IRiftCompiler
	{
		public unsafe void WriteOpCode(RiftCompiledOpCode opcode);
		public unsafe void Write<T>(T value) where T : unmanaged;
		public void WritePointer(string name);
		public void RegisterStructRecursive(Type tstruct);
		public void RegisterEnvironmentDataType(ulong id, string alias, int size);
		public void RegisterEnvironmentFunction(ushort id, string alias, params Type[] args);
		public void RegisterEnvironmentFunctionRet(ushort id, string alias, Type ret, params Type[] args);
		public void RegisterExternalTrigger<T>(object index) where T : Enum;
		public void RegisterPrimitiveInput<TPrimitive>(string alias, RiftCompiledOpCode riftType) where TPrimitive : unmanaged;
	}

// public struct Sc
	public class RiftCompiler : IRiftCompiler
	{
		public struct IndentLog
		{
			public int level;
			public int writeOffset;
		}

		public struct StructureData
		{
			public int size;

			public short typeIndex;

			// public RiftPointerType pointerType;
			public Type type;
		}

		public struct VariableData
		{
			public string name;
			public RiftCompiledOpCode opType;
			public Type type;
			public RiftPointerType pointerType;
			public int rwIndex;
			public int offset;
			public short typeIndex;
			public int size;

			public void WritePointer(RiftCompiler compiler)
			{
				if (opType == RiftCompiledOpCode.Struct)
				{
					compiler.WritePointerType(pointerType);
					compiler.WriteDataType(opType);
					if ((pointerType & RiftPointerType.Local) != 0)
					{
						RiftLog.CLog($"Write local struct field offset: {offset} with {size} size");
						compiler.Write(offset);
					}
					else if ((pointerType & RiftPointerType.Settings) != 0)
					{
						RiftLog.CLog($"Write settings struct field offset: {offset} with {size} size");
						compiler.Write(offset);
					}
					else if ((pointerType & RiftPointerType.Input) != 0)
					{
						RiftLog.CLog($"Write input struct field offset: {offset} with {size} size");
						compiler.Write(rwIndex);
					}
					else
					{
						throw new Exception("Invalid datatype");
					}

					compiler.Write(size);
				}
				else if ((pointerType & RiftPointerType.Local) != 0)
				{
					compiler.WritePointerType(pointerType);
					compiler.WriteDataType(opType);
					compiler.Write(offset);
				}
				else if ((pointerType & RiftPointerType.Input) != 0)
				{
					compiler.WritePointerType(pointerType);
					compiler.WriteDataType(opType);
					compiler.Write(rwIndex);
				}
				else if ((pointerType & RiftPointerType.Settings) != 0)
				{
					compiler.WritePointerType(pointerType);
					compiler.WriteDataType(opType);
					compiler.Write(offset);
				}
			}
		}

		private CodeStream _mainStream;
		private CodeStream _activeStream;
		private byte[] activeStreamBytes => _activeStream.bytecode;

		private int activeStreamOffset
		{
			get => _activeStream.offset;
			set => _activeStream.offset = value;
		}

		private Dictionary<string, VariableData> _localVariables;
		private Dictionary<string, VariableData> _inputVariables;
		public Dictionary<string, VariableData> settings;

		public struct ExternalTrigger
		{
			public string name;
			public int offset;
			public int size;
		}

		public struct RiftExternalFunction
		{
			public Type[] args;
			public Type? ret;
			public ushort hash;
		}

		public class CodeStream
		{
			public byte[] bytecode;
			public int offset;

			public CodeStream(int size)
			{
				this.bytecode = new byte[size];
			}

			public byte[] Strip()
			{
				var stripped = new byte[offset];
				Array.Copy(bytecode, stripped, offset);
				return stripped;
			}
		}

		// private Dictionary<string, RoflanMemPointer> _input;
		private int _pointersOffset;
		private int _inputPointersOffset;
		private int _settingsOffset;
		private int _inputPointersIndex;
		private int _inputPointersLength;
		private byte _currentIndent;
		private List<IndentLog> _indentTree;
		private Dictionary<string, RiftExternalFunction> _externalFunctions;
		private HashSet<ushort> _functionHashes;
		private HashSet<string> _signatures;
		private Dictionary<string, CodeStream> _codeStreams;
		private Dictionary<string, StructureData> _structureData;
		private Dictionary<int, StructureData> _structureDataByIndex;

		private Dictionary<Type, StructureData> _structureDataByType;

		// private HashSet<string> _triggerNames = new HashSet<string>();
		private List<ExternalTrigger> _triggers = new List<ExternalTrigger>();
		private List<string> _externalTriggers = new List<string>();
		private int lastInputTypeId;
		private Dictionary<string, EnvironmentType> _environmentDataTypeIdMap;
		private int _exitJumpOffset;
		private int _compilationRequiresIndentStep = int.MinValue;
		private byte _codeStreamAutoLeaveIndent = 0;

		public struct EnvironmentType
		{
			public ulong id;
			public int size;
		}

		public RiftCompiler(IRiftEnvironment env)
		{
			_indentTree = new List<IndentLog>();
			_environmentDataTypeIdMap = new Dictionary<string, EnvironmentType>();
			_localVariables = new Dictionary<string, VariableData>();
			_inputVariables = new Dictionary<string, VariableData>();
			_structureData = new Dictionary<string, StructureData>();
			_structureDataByIndex = new Dictionary<int, StructureData>();
			_structureDataByType = new Dictionary<Type, StructureData>();
			_externalFunctions = new Dictionary<string, RiftExternalFunction>();
			_signatures = new HashSet<string>();
			settings = new Dictionary<string, VariableData>();
			_functionHashes = new HashSet<ushort>();
			_mainStream = new CodeStream(1024 * 1024);
			_activeStream = _mainStream;
			activeStreamOffset = 0;
			_settingsOffset = 0;
			_codeStreams = new Dictionary<string, CodeStream>();
			_codeStreamAutoLeaveIndent = byte.MaxValue;
			env.Setup(this);
		}

		public void OpenTrigger(string name)
		{
			var t = new ExternalTrigger { name = name, offset = int.MinValue, size = Int32.MinValue };
			_triggers.Add(t);
			RiftLog.CLog($"Add internal trigger {name}");

			OpenCodeStream(name, 0);
		}

		public void OpenCodeStream(string name, byte autoLeaveIndent)
		{
			if (_codeStreams.ContainsKey(name))
			{
				throw new Exception($"Duplication code stream name {name}");
			}

			RiftLog.CLog($"Open code stream {name}");
			var stream = new CodeStream(1024 * 1024);
			_codeStreams.Add(name, stream);
			_activeStream = stream;
			_codeStreamAutoLeaveIndent = autoLeaveIndent;
		}

		public void CloseCodeStream()
		{
			if (_mainStream == _activeStream)
			{
				throw new Exception($"Invalid close main code stream");
			}

			WriteOpCode(RiftCompiledOpCode.End);
			RiftLog.CLog($"Close code stream {activeStreamOffset}");
			_codeStreamAutoLeaveIndent = byte.MaxValue;
			_activeStream = _mainStream;
		}


		public void RegisterEnvironmentFunctionRet(ushort id, string alias, Type ret, params Type[] args)
		{
			if (_externalFunctions.ContainsKey(alias))
			{
				throw new Exception($"Duplication function name {id} ({alias})");
			}

			if (_functionHashes.Contains(id))
			{
				throw new Exception($"Duplication function hash {id} ({alias})");
			}

			AddNewSignatureOrCrash(alias);
			RiftLog.CLog($"Add external function {alias} with hash {id}");
			_externalFunctions.TryAdd(alias, new RiftExternalFunction() { args = args, ret = ret, hash = id });
		}

		public void RegisterExternalTrigger<T>(object index) where T : Enum
		{
			_externalTriggers.Add(Enum.GetName(typeof(T), index));
			RiftLog.CLog($"Add external trigger {Enum.GetName(typeof(T), index)}");
		}

		public void RegisterEnvironmentFunction(ushort id, string alias, params Type[] args)
		{
			if (_externalFunctions.ContainsKey(alias))
			{
				throw new Exception($"Duplication function name {id} ({alias})");
			}

			if (_functionHashes.Contains(id))
			{
				throw new Exception($"Duplication function hash {id} ({alias})");
			}

			AddNewSignatureOrCrash(alias);
			RiftLog.CLog($"Add external function {alias} with hash {id}");
			_externalFunctions.TryAdd(alias, new RiftExternalFunction() { args = args ?? Array.Empty<Type>(), ret = null, hash = id });
		}

		public void RegisterPrimitiveInput<TPrimitive>(string alias, RiftCompiledOpCode riftType) where TPrimitive : unmanaged
		{
			// if (_structureData.ContainsKey(alias))
			// {
			//     RiftLog.Exception($"Failing map primitive input. U are trying add input map type with alias {alias} AGAIN, is not okay.");
			//     return;
			// }
			//
			// var size = RiftUtility.SizeOf<TPrimitive>();
			// // _structureData.Add(alias, (size, lastInputTypeId, riftType));
			// var std = new StructureData
			// {
			//     typeIndex = lastInputTypeId,
			//     dt = riftType,
			//     size = size,
			//     type = null,
			// };
			// _structureData.Add(alias, std);
			// _structureDataByIndex.Add(lastInputTypeId, std);
			// _structureDataByType.Add(typeof(TPrimitive), std);
			// lastInputTypeId++;
		}

		private void AddNewSignatureOrCrash(string signature)
		{
			if (!_signatures.Add(signature))
			{
				throw new Exception($"Duplication language signature {signature}");
			}
		}

		public void RegisterEnvironmentDataType(ulong id, string alias, int size)
		{
			// AddNewSignatureOrCrash(alias);
			_environmentDataTypeIdMap[alias] = new EnvironmentType { id = id, size = size };
		}

		public bool TryGetEnvironmentDataTypeId(string alias, out EnvironmentType id)
		{
			return _environmentDataTypeIdMap.TryGetValue(alias, out id);
		}

		public bool TryGetEnvironmentFunction(string alias, out RiftExternalFunction ex)
		{
			return _externalFunctions.TryGetValue(alias, out ex);
		}

		public bool RegisterStruct<TStruct>(string alias) where TStruct : unmanaged
		{
			return RegisterStruct(typeof(TStruct));
		}

		public void RegisterStructRecursive(Type tstruct)
		{
			if (!RegisterStruct(tstruct))
			{
				return;
			}

			foreach (var field in tstruct.GetFields())
			{
				if (RiftUtility.IsStructureType(field.FieldType))
				{
					RegisterStructRecursive(field.FieldType);
				}
			}
		}

		public bool RegisterStruct(Type tstruct)
		{
			var alias = tstruct.Name;
			_signatures.Add(alias);
			if (_structureData.ContainsKey(alias))
			{
				return false;
			}

			var size = RiftUtility.SizeOf(tstruct);
			var dt = new StructureData
			{
				typeIndex = (short)lastInputTypeId,
				// pointerType = RiftDataTag.Struct,
				size = size,
				type = tstruct,
			};
			_structureData.Add(alias, dt);
			_structureDataByIndex.Add(lastInputTypeId, dt);
			_structureDataByType.Add(tstruct, dt);
			lastInputTypeId++;
			return true;
		}

		public void AddInput(RiftCompiledOpCode dt, string name)
		{
			AddInput(dt, String.Empty, name);
		}

		public void AddInput(RiftCompiledOpCode dt, string dataType, string name)
		{
			if (dt == RiftCompiledOpCode.Struct)
			{
				if (!_structureData.TryGetValue(dataType, out var dtData))
				{
					RiftLog.Exception($"Invalid input variable {name} with {dataType} type. This variable does not exists in input");
				}

				if (_localVariables.ContainsKey(name))
				{
					RiftLog.Exception($"Invalid input variable {name} with {dataType} type. This variable already presented in script variables section");
				}

				_inputVariables.Add(name, new VariableData
				{
					offset = 0,
					size = dtData.size,
					typeIndex = dtData.typeIndex,
					rwIndex = _inputPointersIndex,
					opType = dt,
					pointerType = RiftPointerType.Input
				});
				_inputPointersIndex++;
				_inputPointersOffset += dtData.size;
				RiftLog.CLog($"Add input variable {name} with {dtData.size} size, total input size: {_inputPointersOffset}");
			}
			else
			{
				var size = RiftUtility.SizeOf(dt);
				_inputVariables.Add(name, new VariableData
				{
					offset = 0,
					size = size,
					typeIndex = 0,
					rwIndex = _inputPointersIndex,
					opType = dt,
					pointerType = RiftPointerType.Input
				});
				_inputPointersIndex++;
				_inputPointersOffset += size;
				RiftLog.CLog($"Add input variable {name} with {size} size, total input size: {_inputPointersOffset}");
			}
		}

		public void AddSettings(RiftCompiledOpCode dt, string name)
		{
			AddSettings(dt, String.Empty, name);
		}

		public void AddSettings(RiftCompiledOpCode dt, string dataType, string name)
		{
			AddNewSignatureOrCrash(name);
			if (dt == RiftCompiledOpCode.Struct)
			{
				if (!_structureData.TryGetValue(dataType, out var dtData))
				{
					RiftLog.Exception($"Invalid input variable {name} with {dataType} type. This variable does not exists in input");
				}

				settings.Add(name, new VariableData
				{
					name = name,
					offset = _settingsOffset,
					size = dtData.size,
					opType = dt,
					pointerType = RiftPointerType.Settings,
					type = dtData.type,
				});
				_settingsOffset += dtData.size;
				RiftLog.CLog($"Add settings variable {name} with {dtData.size} size, total settings size: {_settingsOffset}");
			}
			else
			{
				var size = RiftUtility.SizeOf(dt);
				settings.Add(name, new VariableData
				{
					name = name,
					offset = _settingsOffset,
					size = size,
					opType = dt,
					pointerType = RiftPointerType.Settings,
					type = RiftUtility.GetTypeOfOpCode(dt),
				});
				_settingsOffset += size;
				RiftLog.CLog($"Add settings variable {name} with {size} size, total settings size: {_settingsOffset}");
			}
		}

		public void AddBlitVariable(RiftCompiledOpCode tags, string name)
		{
			AddNewSignatureOrCrash(name);
			if (_localVariables.ContainsKey(name))
			{
				throw new Exception($"Invalid  variable {name} with {tags} type. This variable already presented in script variables section");
			}

			if (_inputVariables.ContainsKey(name))
			{
				throw new Exception($"Invalid  variable {name}. This variable already presented in script input");
			}


			var size = RiftUtility.SizeOf(tags);
			_localVariables.Add(name, new VariableData
			{
				size = size,
				offset = _pointersOffset,
				typeIndex = -1,
				opType = tags,
				pointerType = RiftPointerType.Local
			});
			_pointersOffset += size;
		}

		public void AddStruct(string typeName, string name)
		{
			AddNewSignatureOrCrash(name);
			if (_localVariables.ContainsKey(name))
			{
				throw new Exception($"Invalid  variable {name}. This variable already presented in script variables section");
			}

			if (_inputVariables.ContainsKey(name))
			{
				throw new Exception($"Invalid  variable {name}. This variable already presented in script input");
			}

			if (!_structureData.TryGetValue(typeName, out var std))
			{
				throw new Exception($"Invalid  structure name {typeName}. Not presented in registered structs");
			}

			var size = RiftUtility.SizeOf(std.type);
			_localVariables.Add(name, new VariableData
			{
				size = size,
				offset = _pointersOffset,
				typeIndex = std.typeIndex,
				pointerType = RiftPointerType.Local,
				opType = RiftCompiledOpCode.Struct
			});
			_pointersOffset += size;
		}

		public void WritePointerAuto(string[] name)
		{
			if (name.Length == 1)
			{
				WritePointer(name[0]);
			}
			else
			{
				WritePointerPath(name);
			}
		}

		public void WritePointer(string name)
		{
			if (_localVariables.TryGetValue(name, out var local))
			{
				local.WritePointer(this);
			}
			else if (_inputVariables.TryGetValue(name, out var input))
			{
				input.WritePointer(this);
			}
			else if (settings.TryGetValue(name, out var setting))
			{
				setting.WritePointer(this);
			}
			else
			{
				throw new Exception($"Invalid  variable {name}. This variable does not exists in any context");
			}
		}

		public EnvironmentType GetEnvironmentTypeId(string[] path)
		{
			var entry = path[0];
			VariableData entryPointer;
			if (!_localVariables.TryGetValue(entry, out entryPointer) && !_inputVariables.TryGetValue(entry, out entryPointer))
			{
				throw new Exception($"{entry} not presented in variables");
			}

			Type current = _structureDataByIndex[entryPointer.typeIndex].type;
			for (int i = 1; i < path.Length; i++)
			{
				current = current.GetField(path[i]).FieldType;
			}

			return _environmentDataTypeIdMap[current.Name];
		}

		public void WritePointerPath(string[] path)
		{
			var entry = path[0];
			VariableData entryPointer;
			bool isLocal = _localVariables.ContainsKey(entry);
			bool isSettings = settings.ContainsKey(entry);
			int offset = 0;
			if (!_localVariables.TryGetValue(entry, out entryPointer) && !_inputVariables.TryGetValue(entry, out entryPointer) && !settings.TryGetValue(entry, out entryPointer))
			{
				throw new Exception($"{entry} not presented in variables");
			}

			if (entryPointer.pointerType == RiftPointerType.Local)
			{
				offset += entryPointer.offset;
			}

			if (entryPointer.pointerType == RiftPointerType.Settings)
			{
				offset += entryPointer.offset;
			}

			if (entryPointer.opType != RiftCompiledOpCode.Struct)
			{
				throw new Exception($"U are trying access struct variable, but {entry} is not structure, variable tags: {entryPointer.opType}");
			}


			Type current = _structureDataByIndex[entryPointer.typeIndex].type;

			for (int i = 1; i < path.Length; i++)
			{
				var f = current.GetField(path[i]);
				if (f == null)
				{
					throw new Exception($"Invalid field name {path[i]} for parent structure {path[i - 1]}");
				}

				if (i != path.Length - 1)
				{
					if (!RiftUtility.IsStructureType(f.FieldType))
					{
						throw new Exception($"U are trying to access not struct variable, but is struct variable {path[i]} (inside {path[i - 1]}");
					}

					if (!_structureDataByType.TryGetValue(f.FieldType, out var std))
					{
						RegisterStruct(f.FieldType);
					}

					offset += RiftUtility.FieldOffset(f);
					current = f.FieldType;
				}
				else
				{
					RiftLog.CLog($"{f.FieldType} {f.Name}");
					offset += RiftUtility.FieldOffset(f);
					if (RiftUtility.IsStructureType(f.FieldType))
					{
						if (!_structureDataByType.TryGetValue(f.FieldType, out var std))
						{
							RegisterStruct(f.FieldType);
						}

						if (isLocal)
						{
							// WriteDataType(RiftDataTag.Struct | RiftDataTag.Local | RiftDataTag.Path);
							WritePointerType(RiftPointerType.Local | RiftPointerType.Path);
							WriteDataType(RiftCompiledOpCode.Struct);
							RiftLog.CLog($"Write local struct field offset: {offset} with {std.size} size");
							Write(offset);
							Write(std.size);
						}
						else if (isSettings)
						{
							// WriteDataType(RiftDataTag.Struct | RiftDataTag.Local | RiftDataTag.Path);
							WritePointerType(RiftPointerType.Settings | RiftPointerType.Path);
							WriteDataType(RiftCompiledOpCode.Struct);
							RiftLog.CLog($"Write settings struct field offset: {offset} with {std.size} size");
							Write(offset);
							Write(std.size);
						}
						else
						{
							WritePointerType(RiftPointerType.Input | RiftPointerType.Path);
							WriteDataType(RiftCompiledOpCode.Struct);
							Write(entryPointer.rwIndex);
							Write(offset);
							Write(std.size);
						}
					}
					else
					{
						if (isLocal)
						{
							RiftLog.CLog($"Write local struct blit field offset: {offset} with {f.FieldType} type");
							WritePointerType(RiftPointerType.Local | RiftPointerType.Path);
							WriteDataType(RiftUtility.GetTypeOfType(f.FieldType));
							Write(offset);
						}
						else if (isSettings)
						{
							RiftLog.CLog($"Write settings struct blit field offset: {offset} with {f.FieldType} type");
							WritePointerType(RiftPointerType.Settings | RiftPointerType.Path);
							WriteDataType(RiftUtility.GetTypeOfType(f.FieldType));
							Write(offset);
						}
						else
						{
							WritePointerType(RiftPointerType.Input | RiftPointerType.Path);
							WriteDataType(RiftUtility.GetTypeOfType(f.FieldType));
							Write(entryPointer.rwIndex);
							Write(offset);
						}
					}
				}
			}
		}

		public void RequireIndentStep(int next)
		{
			RiftLog.CLog($"Require indent step {next}");
			_compilationRequiresIndentStep = next;
		}


		public unsafe void PreIndent(byte v, bool write = false)
		{
			if (_compilationRequiresIndentStep != int.MinValue && _compilationRequiresIndentStep != v)
			{
				throw new Exception($"Invalid indent step {_compilationRequiresIndentStep} != {v}");
			}

			_compilationRequiresIndentStep = int.MinValue;

			if (_currentIndent == v && !write) return;

			for (int i = _indentTree.Count - 1; i >= 0; i--)
			{
				var lg = _indentTree[i];
				if ((write && lg.level >= v) || (!write && lg.level > v))
				{
					fixed (byte* p = &activeStreamBytes[lg.writeOffset])
					{
						*(int*)(p) = activeStreamOffset - (lg.writeOffset + 4); //+4 is block size (this value)
						RiftLog.CLog($" Indent tree updated at: {lg.writeOffset}, block length: {activeStreamOffset - (lg.writeOffset + 4)}");
					}

					_indentTree.RemoveAt(i);
				}
			}

			if (_codeStreamAutoLeaveIndent != byte.MaxValue && _codeStreamAutoLeaveIndent == v)
			{
				RiftLog.CLog($"Close code stream cause indent fitted");
				CloseCodeStream();
			}
		}

		public void CheckIndent(byte v, bool write = false)
		{
			WriteOpCode(RiftCompiledOpCode.Indent);
			Write(v);
			RiftLog.CLog($" Write Indent {v},offset point at {activeStreamOffset}");
			if (write)
			{
				_indentTree.Add(new IndentLog { level = v, writeOffset = activeStreamOffset });
			}

			Write(0);
			_currentIndent = v;
		}

		public unsafe void WriteOpCode(RiftCompiledOpCode opcode)
		{
			var size = 1;
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(byte*)p = (byte)opcode;
			}

			RiftLog.CLog($" Write op code {opcode} at  {activeStreamOffset} with {size}");
			activeStreamOffset += size;
		}

		public unsafe void WriteDataType(RiftCompiledOpCode dt)
		{
			var size = 1;
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(byte*)p = (byte)dt;
			}

			RiftLog.CLog($" Write data type code {dt} at  {activeStreamOffset} with {size}");
			activeStreamOffset += size;
		}

		public bool IsReadonlyEntry(string name)
		{
			foreach (var key in settings.Keys)
			{
				RiftLog.CLog($"lalla {key}");
			}

			return settings.ContainsKey(name);
		}

		public unsafe void WritePointerType(RiftPointerType dt)
		{
			var size = 1;
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(byte*)p = (byte)dt;
			}

			RiftLog.CLog($" Write pointer type code {dt} at  {activeStreamOffset} with {size}");
			activeStreamOffset += size;
		}

		public unsafe void WriteEnvironmentDataTypeId(EnvironmentType dt)
		{
			RiftLog.CLog($" Write environment dataType {dt.id} at {activeStreamOffset}");
			var size = 8;
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(ulong*)p = (ulong)dt.id;
			}

			activeStreamOffset += size;
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(int*)p = (int)dt.size;
			}

			activeStreamOffset += 4;
		}

		public unsafe void WriteEnvironmentFunction(RiftExternalFunction fn)
		{
			RiftLog.CLog($" Write environment function call {fn.hash}() at {activeStreamOffset}");
			var size = 2;
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(ushort*)p = (ushort)fn.hash;
			}

			activeStreamOffset += size;
		}

		public unsafe void Write<T>(T value) where T : unmanaged
		{
			int size = sizeof(T);
			RiftLog.CLog($" Write at {activeStreamOffset} with {size} bytes of {value.GetType()}");
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(T*)p = value;
			}

			activeStreamOffset += size;
		}

		public unsafe void Write(byte[] value)
		{
			RiftLog.CLog($" Write array of bytes at {activeStreamOffset} with {value.Length} bytes");
			value.CopyTo(activeStreamBytes, activeStreamOffset);
			activeStreamOffset += value.Length;
		}

		public unsafe void WriteBool(bool b)
		{
			RiftLog.CLog($" Write at {activeStreamOffset} with 1 bytes of Boolean");
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(byte*)p = (byte)(b ? 1 : 0);
			}

			activeStreamOffset += 1;
		}

		public unsafe void WriteByte(byte b)
		{
			RiftLog.CLog($" Write at {activeStreamOffset} with 1 bytes");
			fixed (byte* p = &activeStreamBytes[activeStreamOffset])
			{
				*(byte*)p = b;
			}

			activeStreamOffset += 1;
		}

		public void SetExitJumpOffset()
		{
			RiftLog.CLog($" Write exit jump offset {activeStreamOffset}");
			_exitJumpOffset = activeStreamOffset;
		}

		public RiftScriptSerializable Compile(RiftParsedScript script)
		{
			return _Compile(script);
		}

		private RiftScriptSerializable _Compile(RiftParsedScript script)
		{
			script.inputStatement.Compile(1, 0, this);
			script.settings.Compile(1, 0, this);
			foreach (var statementInd in script.sequence)
			{
				if (statementInd is ACompileableStatement acp)
				{
					if (acp is ICompileable_v1 cmpv1)
					{
						RiftLog.CLog($" Compile statement {statementInd.statement}");
						cmpv1.Compile_v1(0, this);
					}
					else
					{
						throw new Exception($"{acp} is not supported current compiler version");
					}
				}
				else
				{
					RiftLog.CLog($" Compile unknown statement {statementInd.statement}");
				}
			}

			PreIndent(0);
			CheckIndent(0);
			WriteOpCode(RiftCompiledOpCode.End);
			var pointers = _localVariables.Values.ToList();
			pointers.Sort((a, b) => a.offset.CompareTo(b.offset));
			var c = pointers.Count > 0 ? pointers[^1].offset + pointers[^1].size : 0;

			//Код тригеров складывается сразу после основного
			//Далее идут указатели (int) на оффсеты этих блоков с кодами тригеров выстроенные в порядке принадлежности к внешним вызовам:
			//Сначала 0-N указываются оффсеты внешних триггеров, чтобы их было просто найти, а далее идут все внутренные триггеры 
			//Так так внутренние триггеры сейчас НЕЛЬЗЯ вызвать, то никакой стадии ремапинга указалетей на внутренние тригеры нет и этот функционал по приколу пока что существует
			List<ExternalTrigger> triggersCopy = new List<ExternalTrigger>(_triggers);
			List<ExternalTrigger> triggersSorted = new List<ExternalTrigger>();
			var i = 0;
			foreach (var external in _externalTriggers)
			{
				bool found = false;
				foreach (var trigger in triggersCopy)
				{
					if (trigger.name == external)
					{
						RiftLog.CLog($"Trigger {trigger.name} placed at: {i} cause is external");
						triggersSorted.Add(trigger);
						triggersCopy.Remove(trigger);
						i++;
						found = true;
						break;
					}
				}

				if (!found)
				{
					triggersSorted.Add(new ExternalTrigger { name = external, offset = int.MinValue });
					RiftLog.CLog($"Trigger {external} placed at nowhere cause is not implemented ");
				}
			}

			foreach (var trigger in triggersCopy)
			{
				triggersSorted.Add(trigger);
				RiftLog.CLog($"Trigger {trigger.name} placed at: {i}");
				i++;
			}

			var byteCodeSize = _activeStream.offset;
			for (int j = 0; j < triggersSorted.Count; j++)
			{
				var trigger = triggersSorted[j];
				if (_codeStreams.TryGetValue(trigger.name, out var stream))
				{
					var triggerStripped = stream.Strip();
					var triggerOffset = _activeStream.offset;
					var triggerSize = triggerStripped.Length;
					RiftLog.CLog($"Trigger {trigger.name} bytecode location at: {triggerOffset} with {triggerSize} bytes");
					Write(triggerStripped);
					trigger.offset = triggerOffset;
					trigger.size = triggerSize;
					triggersSorted[j] = trigger;
				}
			}

			var triggersCodeAt = _activeStream.offset;

			for (int j = 0; j < triggersSorted.Count; j++)
			{
				RiftLog.CLog($"Write trigger pointer {triggersSorted[j].name}: {triggersSorted[j].offset}");
				Write(triggersSorted[j].offset);
			}

			var totalSize = _activeStream.offset;
			var stripped = _mainStream.Strip();
			RiftLog.CLog($"Meta info, bytecode size: {byteCodeSize}, trigger pointers at: {triggersCodeAt}, total size: {totalSize}");
			return new RiftScriptSerializable
			{
				bytecode = stripped,
				meta = new RiftScriptMeta
				{
					mainCodeBlockSize = byteCodeSize,
					triggersBlockSize = triggersCodeAt,
					total=totalSize,
					settingsSize = _settingsOffset,
					variablesSize = c,
				}
			};
		}
	}
}