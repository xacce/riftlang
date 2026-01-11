using System;
using System.Globalization;
using Superpower;
using Superpower.Model;

namespace Rift.Lang
{
	public abstract class ExprStatement : ACompileableStatement
	{
	}

	public class InputStatement : ExprStatement, ICompileable_v1
	{
		public InputVariableStatement[] variables = Array.Empty<InputVariableStatement>();

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			foreach (var st in variables)
			{
				st.Compile(1, 0, compiler);
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			if (variables.Length > 0)
			{
				return variables[0].TryGetPrimaryToken(out token);
			}

			token = default;
			return false;
		}
	}

	public class StopAbstractStatement : AbstractStatement, ICompileable_v1
	{
		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			compiler.WriteOpCode(RiftCompiledOpCode.Stop);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;
			return false;
		}
	}

	public class SettingsStatement : ExprStatement, ICompileable_v1
	{
		public SettingsVariableStatement[] variables = Array.Empty<SettingsVariableStatement>();

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			foreach (var st in variables)
			{
				st.Compile(1, 0, compiler);
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			if (variables.Length > 0)
			{
				return variables[0].TryGetPrimaryToken(out token);
			}

			token = default;
			return false;
		}
	}

	public class VariableStatement : AbstractStatement, ICompileable_v1
	{
		public Token<RiftToken> name;
		public ExprStatement dataType;

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = name;
			return true;
		}

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			if (dataType is DataTypeInputStatement dti)
			{
				RiftLog.Log($"Variable {name.ToStringValue()} was  type {dti.value} was registered");
				compiler.AddStruct(dti.value.ToStringValue(), name.ToStringValue());
			}
			else if (dataType is DataTypeStatement dte)
			{
				RiftLog.Log($"Variable {name.ToStringValue()} was blit type {dte.value} was registered");
				compiler.AddBlitVariable(dte.value, name.ToStringValue());
			}
			else
			{
				throw new Exception($"Invalid data type {dataType} cant be converted to input data type, cause not supported?");
			}
		}
	}

	public class InputVariableStatement : AbstractStatement, ICompileable_v1
	{
		public Token<RiftToken> name;
		public ExprStatement dataType;

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			if (dataType is DataTypeInputStatement dti)
			{
				compiler.AddInput(RiftCompiledOpCode.Struct, dti.value.ToStringValue(), name.ToStringValue());
			}
			else if (dataType is DataTypeStatement dte)
			{
				compiler.AddInput(dte.value, name.ToStringValue());
			}
			else
			{
				throw new Exception($"Invalid data type {dataType} cant be converted to input data type,cause not supported?");
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = name;
			return true;
		}
	}

	public class SettingsVariableStatement : AbstractStatement, ICompileable_v1
	{
		public Token<RiftToken> name;
		public ExprStatement dataType;

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			if (dataType is DataTypeInputStatement dti)
			{
				compiler.AddSettings(RiftCompiledOpCode.Struct, dti.value.ToStringValue(), name.ToStringValue());
			}
			else if (dataType is DataTypeStatement dte)
			{
				compiler.AddSettings(dte.value, name.ToStringValue());
			}
			else
			{
				throw new Exception($"Invalid data type {dataType} cant be converted to settings data type, cause not supported?");
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = name;
			return true;
		}
	}

	public class DataTypeStatement : ExprStatement
	{
		public Token<RiftToken> token;
		public RiftCompiledOpCode value;

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = this.token;
			return true;
		}
	}

	public class DataTypeInputStatement : ExprStatement
	{
		public Token<RiftToken> value;

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = value;
			return true;
		}
	}

	public class VariableRefStatement : ValueStatement, ICompileable_v1
	{
		public Token<RiftToken>[] path = Array.Empty<Token<RiftToken>>();

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			if (path.Length == 1)
			{
				compiler.WritePointer(path[0].ToStringValue());
			}
			else if (path.Length > 1)
			{
				compiler.WritePointerPath(path.Select(t => t.ToStringValue()).ToArray());
			}
			else
			{
				throw new Exception();
			}
		}


		public void CheckWriteAccess(RiftCompiler compiler)
		{
			if (compiler.IsReadonlyEntry(path[0].ToStringValue()))
			{
				throw new Exception($"Cannot write to readonly variable {this} {String.Join(".", path)}");
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			if (path.Length > 0)
			{
				token = path[0];
				return true;
			}

			token = default;
			return false;
		}
	}

// public class StructPath: Value, ICompileable_v1
// {
//     public string[] path;
//
//     public void Compile_v1(byte _, RoflanCompiler_v1 compiler)
//     {
//         // compiler.WritePointer(name);
//     }
// }

	public abstract class ValueStatement : ExprStatement
	{
	}

	public class IntValueStatement : ValueStatement, ICompileable_v1
	{
		public Token<RiftToken> value;

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			var intValue = Int32.Parse(value.ToStringValue());
			compiler.WritePointerType(RiftPointerType.Raw);
			compiler.WriteDataType(RiftCompiledOpCode.Int);
			compiler.Write(intValue);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = value;
			return true;
		}
	}

	public class FloatValueStatement : ValueStatement, ICompileable_v1
	{
		public Token<RiftToken> value;

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			var floatValue = (float)float.Parse(value.ToStringValue(), CultureInfo.InvariantCulture);
			compiler.WritePointerType(RiftPointerType.Raw);
			compiler.WriteDataType(RiftCompiledOpCode.Float);
			compiler.Write(floatValue);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = value;
			return true;
		}
	}

	public class BoolValueStatement : ValueStatement, ICompileable_v1
	{
		public Token<RiftToken> value;

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			compiler.WritePointerType(RiftPointerType.Raw);
			compiler.WriteDataType(RiftCompiledOpCode.Bool);
			compiler.WriteBool(value.Kind == RiftToken.True);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = value;
			return true;
		}
	}

	public abstract class AbstractRiftStatement
	{
		public abstract bool TryGetPrimaryToken(out Token<RiftToken> token);
	}

	public abstract class ACompileableStatement : AbstractRiftStatement
	{
		public void Compile(int version, byte indent, IRiftCompiler compiler)
		{
			var cmpv1 = compiler as RiftCompiler;
			if (version == 1 && this is ICompileable_v1 cv1)
			{
				try
				{
					cv1.Compile_v1(indent, cmpv1);
				}
				catch (ParseException e)
				{
					throw;
				}
				catch (Exception re)
				{
					if (TryGetPrimaryToken(out var token))
					{
						throw new RiftCompilationException(re.Message, token);
					}
					else
					{
						throw new RiftUndefinedCompilationException(re.Message);
					}
				}
			}
		}
	}

	public interface ICompileable_v1
	{
		public void Compile_v1(byte indent, RiftCompiler compiler);
	}

	public class AssigmentStatement : AbstractStatement, ICompileable_v1
	{
		public VariableRefStatement to;
		public ACompileableStatement from;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			to.CheckWriteAccess(compiler);
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			if (from is BasicMathStatement)
			{
				compiler.WriteOpCode(RiftCompiledOpCode.AssigmentMath);
			}
			else
			{
				compiler.WriteOpCode(RiftCompiledOpCode.Assigment);
			}

			to.Compile(1, 0, compiler);
			from.Compile(1, 0, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return to.TryGetPrimaryToken(out token);
		}
	}

	public class PrintStatement : AbstractStatement, ICompileable_v1
	{
		public ValueStatement v;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			compiler.WriteOpCode(RiftCompiledOpCode.Print);
			v.Compile(1, indent, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return v.TryGetPrimaryToken(out token);
		}
	}

	public abstract class AbstractStatement : ExprStatement
	{
	}

	public class NothingStatement : AbstractStatement
	{
		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;
			return false;
		}
	}

	public class IndentStatement : ExprStatement, ICompileable_v1
	{
		public byte ind;
		public AbstractStatement statement;

		public void Compile_v1(byte _, RiftCompiler compiler)
		{
			statement.Compile(1, ind, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return statement.TryGetPrimaryToken(out token);
		}
	}

	public class RiftParsedScript
	{
		public InputStatement inputStatement;
		public SettingsStatement settings;
		public IndentStatement[] sequence;
	}

	public class ComparsionsStatement : AbstractStatement, ICompileable_v1
	{
		public ComparsionStatement[] sequence;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			foreach (var statement in sequence)
			{
				statement.Compile_v1(indent, compiler);
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			if (sequence.Length == 0)
			{
				return sequence[0].TryGetPrimaryToken(out token);
			}

			token = default;
			return false;
		}
	}

	public class ComparsionTypeStatement : AbstractStatement, ICompileable_v1
	{
		public Token<RiftToken> type;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			RiftCompiledOpCode op;
			switch (type.Kind)
			{
				case RiftToken.Greater:
					op = RiftCompiledOpCode.Greater;
					break;
				case RiftToken.GreaterThan:
					op = RiftCompiledOpCode.GreaterThan;
					break;
				case RiftToken.Less:
					op = RiftCompiledOpCode.Less;
					break;
				case RiftToken.LessThan:
					op = RiftCompiledOpCode.LessThan;
					break;
				case RiftToken.Equal:
					op = RiftCompiledOpCode.Equal;
					break;
				case RiftToken.NotEqual:
					op = RiftCompiledOpCode.NotEqual;
					break;
				default:
					throw new Exception($"Unsupported comparison type: {type}");
			}

			compiler.WriteOpCode(op);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = type;
			return true;
		}
	}


	public class ValuesStatement : AbstractStatement, ICompileable_v1
	{
		public ValueStatement[] values = Array.Empty<ValueStatement>();

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.WriteByte((byte)values.Length); //ArgumentsCountDataSize
			foreach (var value in values)
			{
				value.Compile(1, indent, compiler);
			}
		}

		public static ValuesStatement Empty => new ValuesStatement() { values = Array.Empty<ValueStatement>() };

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			if (values.Length > 0)
			{
				return values[0].TryGetPrimaryToken(out token);
			}

			token = default;
			return false;
		}
	}

	public class GetDataStatement : AbstractStatement, ICompileable_v1
	{
		public VariableRefStatement to;
		public VariableRefStatement from;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			compiler.WriteOpCode(RiftCompiledOpCode.DataGet);
			var toDt = compiler.GetEnvironmentTypeId(to.path.Select(t => t.ToStringValue()).ToArray());
			// var toDt = compiler.GetEnvironmentTypeId(from.path);
			to.Compile(1, 0, compiler);
			compiler.WriteEnvironmentDataTypeId(toDt);
			from.Compile(1, 0, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return to.TryGetPrimaryToken(out token);
		}
	}

	public class CallRetStatement : AbstractStatement, ICompileable_v1
	{
		public VariableRefStatement to;
		public Token<RiftToken> funcname;
		public ValuesStatement args;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			to.CheckWriteAccess(compiler);
			if (compiler.TryGetEnvironmentFunction(funcname.ToStringValue(), out var fn))
			{
				compiler.WriteOpCode(RiftCompiledOpCode.CallRet);
				to.Compile(1, 0, compiler);
				compiler.WriteEnvironmentFunction(fn);
				args.Compile(1, 0, compiler);
			}
			else
			{
				throw new Exception($"Invalid signature: {funcname}");
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = funcname;
			return true;
		}
	}


	public class SetDataStatement : AbstractStatement, ICompileable_v1
	{
		public ValuesStatement from;
		public VariableRefStatement to;


		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			if (to.path.Length == 1 && compiler.TryGetEnvironmentFunction(to.path[0].ToStringValue(), out var fn))
			{
				to.CheckWriteAccess(compiler);
				compiler.WriteOpCode(RiftCompiledOpCode.Call);
				compiler.WriteEnvironmentFunction(fn);
				from.Compile(1, 0, compiler);
			}
			else if (from.values.Length == 1 && from.values[0] is VariableRefStatement vr)
			{
				var fromDt = compiler.GetEnvironmentTypeId(vr.path.Select(t => t.ToStringValue()).ToArray());
				compiler.WriteOpCode(RiftCompiledOpCode.DataSet);
				vr.Compile(1, 0, compiler);
				compiler.WriteEnvironmentDataTypeId(fromDt);
				to.Compile(1, 0, compiler);
			}
			else
			{
				throw new Exception($"Invalid signature: {to.path}");
			}
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return from.TryGetPrimaryToken(out token);
		}
	}


	public class IfStatement : AbstractStatement, ICompileable_v1
	{
		public ComparsionsStatement statement;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			// resolve rare pattern
			// if cond
			// if cond
			compiler.PreIndent(indent);
			compiler.CheckIndent((byte)(indent + 1), true);
			compiler.RequireIndentStep(indent + 1);
			compiler.WriteOpCode(RiftCompiledOpCode.If);
			statement.Compile_v1(indent, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return statement.TryGetPrimaryToken(out token);
		}
	}

	public class ExitStatement : AbstractStatement, ICompileable_v1
	{
		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent(indent);
			compiler.SetExitJumpOffset();
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;

			return false;
		}
	}

	public class ElifStatement : AbstractStatement, ICompileable_v1
	{
		public ComparsionsStatement statement;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent((byte)(indent + 1), true);
			compiler.RequireIndentStep(indent + 1);
			compiler.WriteOpCode(RiftCompiledOpCode.Elif);
			statement.Compile_v1(indent, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return statement.TryGetPrimaryToken(out token);
		}
	}

	public class ElseStatement : AbstractStatement, ICompileable_v1
	{
		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.PreIndent(indent);
			compiler.CheckIndent((byte)(indent + 1), true);
			compiler.RequireIndentStep(indent + 1);
			compiler.WriteOpCode(RiftCompiledOpCode.Else);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;
			return false;
		}
	}

	public abstract class AtomStatement : AbstractStatement
	{
	}

	public class AndStatement : AtomStatement, ICompileable_v1
	{
		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.WriteOpCode(RiftCompiledOpCode.And);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;
			return false;
		}
	}

	public class OrStatement : AtomStatement, ICompileable_v1
	{
		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			compiler.WriteOpCode(RiftCompiledOpCode.Or);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;
			return false;
		}
	}

	public class NoneStatement : AtomStatement
	{
		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = default;
			return false;
		}
	}


	public class ComparsionStatement : AbstractStatement, ICompileable_v1
	{
		public ValueStatement left;
		public ValueStatement right;
		public ComparsionTypeStatement type;
		public AtomStatement next;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			next.Compile(1, indent, compiler);
			left.Compile(1, indent, compiler);
			right.Compile(1, indent, compiler);
			type.Compile(1, indent, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return left.TryGetPrimaryToken(out token);
		}
	}

	public class BasicMathOpStatement : AbstractStatement, ICompileable_v1
	{
		public Token<RiftToken> type;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			RiftCompiledOpCode op;
			switch (type.Kind)
			{
				case RiftToken.Add:
					op = RiftCompiledOpCode.Add;
					break;
				case RiftToken.Substract:
					op = RiftCompiledOpCode.Sub;
					break;
				case RiftToken.Multiply:
					op = RiftCompiledOpCode.Mul;
					break;
				case RiftToken.Divide:
					op = RiftCompiledOpCode.Div;
					break;
				default:
					throw new Exception($"Unsupported math op: {type}");
			}

			compiler.WriteOpCode(op);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = type;
			return true;
		}
	}

	public class BasicMathStatement : ValueStatement, ICompileable_v1
	{
		public ValueStatement left;
		public ValueStatement right;
		public BasicMathOpStatement type;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			left.Compile(1, indent, compiler);
			right.Compile(1, indent, compiler);
			type.Compile(1, indent, compiler);
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			return left.TryGetPrimaryToken(out token);
		}
	}


	public class CodeBlockStatement : AbstractStatement, ICompileable_v1
	{
		public Token<RiftToken> name;

		public void Compile_v1(byte indent, RiftCompiler compiler)
		{
			if (indent != 0)
			{
				throw new Exception($"Invalid indent for code block: {name.ToStringValue()}");
			}

			compiler.PreIndent(0);
			compiler.CheckIndent(0, true);
			compiler.OpenTrigger(name.ToStringValue());
		}

		public override bool TryGetPrimaryToken(out Token<RiftToken> token)
		{
			token = name;
			return true;
		}
	}
}