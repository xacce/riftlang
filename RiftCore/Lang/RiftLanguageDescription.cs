using System;
using System.Globalization;
using System.Linq;
using Rift;
using Rift.Lang;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace Rift
{
	public static class RiftLanguageDescription
	{
		public static readonly TokenListParser<RiftToken, int> Whitespaces =
			Token.EqualTo(RiftToken.Whitespace).Many().Select(tokens => 1);

		public static readonly TokenListParser<RiftToken, int> Indent =
			Token.EqualTo(RiftToken.Tab).Many().Select(token => token.Length);

		public static readonly TokenListParser<RiftToken, NothingStatement> IndentSingle =
			Token.EqualTo(RiftToken.Tab).Select(token => new NothingStatement());

		public static readonly TokenListParser<RiftToken, NothingStatement> NewLine =
			Token.EqualTo(RiftToken.NewLineN).Or(Token.EqualTo(RiftToken.NewLineR).IgnoreThen(Token.EqualTo(RiftToken.NewLineN))).Select(t => new NothingStatement());

		private static readonly TokenListParser<RiftToken, DataTypeStatement> DataType =
			Token.EqualTo(RiftToken.BoolType).Select(token => new DataTypeStatement { value = RiftCompiledOpCode.Bool })
				.Or(Token.EqualTo(RiftToken.IntType).Select(token => new DataTypeStatement { value = RiftCompiledOpCode.Int }))
				.Or(Token.EqualTo(RiftToken.FloatType).Select(token => new DataTypeStatement { value = RiftCompiledOpCode.Float }));

		private static readonly TokenListParser<RiftToken, IntValueStatement> IntValue =
			from var2 in Token.EqualTo(RiftToken.Number)
			select new IntValueStatement { value = var2 };

		private static readonly TokenListParser<RiftToken, FloatValueStatement> FloatValue =
			from var2 in Token.EqualTo(RiftToken.Decimal)
			select new FloatValueStatement { value = var2 };

		private static readonly TokenListParser<RiftToken, BoolValueStatement> TrueValue =
			from var2 in Token.EqualTo(RiftToken.True)
			select new BoolValueStatement { value = var2 };

		private static readonly TokenListParser<RiftToken, BoolValueStatement> FalseValue =
			from var2 in Token.EqualTo(RiftToken.False)
			select new BoolValueStatement { value = var2 };

		private static readonly TokenListParser<RiftToken, BoolValueStatement> BoolValue =
			TrueValue.Select(value => value as BoolValueStatement)
				.Or(FalseValue.Select(value => value as BoolValueStatement));

		private static readonly TokenListParser<RiftToken, BasicMathOpStatement> BasicMathOp =
			Whitespaces.Optional().IgnoreThen(Parse.OneOf(
				Token.EqualTo(RiftToken.Add),
				Token.EqualTo(RiftToken.Substract),
				Token.EqualTo(RiftToken.Divide),
				Token.EqualTo(RiftToken.Multiply))).Select(token => new BasicMathOpStatement { type = token });

		private static readonly TokenListParser<RiftToken, Token<RiftToken>> _VariableName =
			from v in Token.EqualTo(RiftToken.Variable)
			select v;


		private static readonly TokenListParser<RiftToken, Token<RiftToken>> _DotVariableName =
			from _ in Token.EqualTo(RiftToken.Dot)
			from v in _VariableName
			select v;

		private static readonly TokenListParser<RiftToken, VariableRefStatement> VariableRef =
			from first in _VariableName
			from first2 in _DotVariableName.Many()
			select Merge(first, first2);

		private static readonly TokenListParser<RiftToken, ValueStatement> Value =
			IntValue.Select(f => f as ValueStatement)
				.Or(FloatValue.Select(f => f as ValueStatement))
				.Or(VariableRef.Select(f => f as ValueStatement))
				.Or(BoolValue.Select(f => f as ValueStatement));

		public static readonly TokenListParser<RiftToken, BasicMathStatement> BasicMath =
			from l in Whitespaces.Optional().IgnoreThen(Value)
			from c in Whitespaces.IgnoreThen(BasicMathOp)
			from r in Whitespaces.Optional().IgnoreThen(Value)
			select new BasicMathStatement() { left = l, right = r, type = c };


		private static readonly TokenListParser<RiftToken, ValueStatement> AdvancedValue =
			Parse.OneOf(BasicMath.Try().Select(statement => (ValueStatement)statement), Value.Select(statement => (ValueStatement)statement));

		private static readonly TokenListParser<RiftToken, ExprStatement> Variable =
			from dt in Parse.OneOf(DataType.Select(e => (ExprStatement)e),
				Token.EqualTo(RiftToken.Variable).Select(token => (ExprStatement)new DataTypeInputStatement() { value = token }))
			from var in Whitespaces.IgnoreThen(Token.EqualTo(RiftToken.Variable))
			select (ExprStatement)new VariableStatement { name = var, dataType = dt };

		private static readonly TokenListParser<RiftToken, ExprStatement> InputVariable =
			from dt in IndentSingle.IgnoreThen(Parse.OneOf(
				DataType.Select(e => (ExprStatement)e),
				Token.EqualTo(RiftToken.Variable)
					.Select(token => (ExprStatement)new DataTypeInputStatement() { value = token })))
			from var in Whitespaces.IgnoreThen(Token.EqualTo(RiftToken.Variable))
			from __ in NewLine.AtLeastOnce()
			select (ExprStatement)new InputVariableStatement { name = var, dataType = dt };

		private static readonly TokenListParser<RiftToken, ExprStatement> SettingVariable =
			from dt in IndentSingle.IgnoreThen(Parse.OneOf(
				DataType.Select(e => (ExprStatement)e),
				Token.EqualTo(RiftToken.Variable)
					.Select(token => (ExprStatement)new DataTypeInputStatement() { value = token })))
			from var in Whitespaces.IgnoreThen(Token.EqualTo(RiftToken.Variable))
			from __ in NewLine.AtLeastOnce()
			select (ExprStatement)new SettingsVariableStatement { name = var, dataType = dt };

		// private static readonly TokenListParser<AzazaToken, Expr> InputVariable =
		//     from dt in Token.EqualTo(AzazaToken.Variable).Select(token => (Expr)new DataTypeInput() { value = token.ToStringValue() })
		//     from var in Whitespaces.IgnoreThen(Token.EqualTo(AzazaToken.Variable))
		//     select (Expr)new VariableSt { name = var.ToStringValue(), dataType = dt };


		private static VariableRefStatement Merge(Token<RiftToken> p, Token<RiftToken>[] o)
		{
			var path = new Token<RiftToken>[o.Length + 1];
			path[0] = p;
			for (int i = 0; i < o.Length; i++)
			{
				path[i + 1] = o[i];
			}

			return new VariableRefStatement()
			{
				path = path
			};
		}


		private static readonly TokenListParser<RiftToken, ValueStatement> ValueNextSt =
			from _ in Token.EqualTo(RiftToken.Whitespace)
			from v in Value
			select v;

		private static readonly TokenListParser<RiftToken, ValuesStatement> Values =
			from f in Value
			from o in ValueNextSt.Many()
			select MargeValues(f, o);

		private static ValuesStatement MargeValues(ValueStatement valueStatement, ValueStatement[] values)
		{
			var path = new ValueStatement[values.Length + 1];
			path[0] = (ValueStatement)valueStatement;
			for (int i = 0; i < values.Length; i++)
			{
				path[i + 1] = (ValueStatement)values[i];
			}

			return new ValuesStatement()
			{
				values = path
			};
		}


		private static readonly TokenListParser<RiftToken, AbstractStatement> Assigment =
			from name in VariableRef
			from _ in Token.EqualTo(RiftToken.Whitespace).Many()
			from __ in Token.EqualTo(RiftToken.Assign)
			from ___ in Token.EqualTo(RiftToken.Whitespace).Many()
			from value in AdvancedValue
			select (AbstractStatement)new AssigmentStatement { to = name, @from = value };

		private static readonly TokenListParser<RiftToken, AbstractStatement> AssigmentMath =
			from name in VariableRef
			from _ in Token.EqualTo(RiftToken.Whitespace).Many()
			from op in BasicMathOp
			from __ in Token.EqualTo(RiftToken.Assign)
			from ___ in Token.EqualTo(RiftToken.Whitespace).Many()
			from value in Value
			select (AbstractStatement)new AssigmentStatement { to = name, @from = new BasicMathStatement { left = name, right = value, type = op } };

		// update  Mission of entity newMissionData
		// private static readonly TokenListParser<RiftToken, Statement> SetData =
		//     from frm in Token.EqualTo(RiftToken.DataSet)
		//         .IgnoreThen(Whitespaces.Optional())
		//         .IgnoreThen(VariableRef)
		//     from of in Token.EqualTo(RiftToken.DataTo)
		//         .IgnoreThen(VariableRef)
		//     select (Statement)new SetDataStatement { frm = frm, to = of };
		//
		private static readonly TokenListParser<RiftToken, AbstractStatement> CallRet =
			from to in VariableRef
			from dataType in Whitespaces.Optional()
				.IgnoreThen(Token.EqualTo(RiftToken.Assign))
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(Token.EqualTo(RiftToken.Fetch))
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(Token.EqualTo(RiftToken.Variable))
			from args in Token.EqualTo(RiftToken.From)
				.IgnoreThen(Values)
			select (AbstractStatement)new CallRetStatement { to = to, args = args, funcname = dataType };

		private static readonly TokenListParser<RiftToken, AbstractStatement> CallRetSingle =
			from to in VariableRef
			from dataType in Whitespaces.Optional()
				.IgnoreThen(Token.EqualTo(RiftToken.Assign))
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(Token.EqualTo(RiftToken.Fetch))
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(Token.EqualTo(RiftToken.Variable))
			select (AbstractStatement)new CallRetStatement { to = to, args = ValuesStatement.Empty, funcname = dataType };


		private static readonly TokenListParser<RiftToken, AbstractStatement> GetData =
			from to in VariableRef
			from dataType in Whitespaces.Optional()
				.IgnoreThen(Token.EqualTo(RiftToken.Assign))
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(Token.EqualTo(RiftToken.FetchFrom))
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(VariableRef)
			select (AbstractStatement)new GetDataStatement { @from = dataType, to = to };

		private static readonly TokenListParser<RiftToken, AbstractStatement> SetData =
			from to in Token.EqualTo(RiftToken.Update)
				.IgnoreThen(Whitespaces.Optional())
				.IgnoreThen(VariableRef)
			from frm in Token.EqualTo(RiftToken.From).IgnoreThen(Values)
			select (AbstractStatement)new SetDataStatement { @from = frm, to = to };

		private static readonly TokenListParser<RiftToken, AbstractStatement> Print =
			from var in Token.EqualTo(RiftToken.Print).IgnoreThen(Value.Select((value => (ValueStatement)value)).Or(VariableRef.Select(@ref => (ValueStatement)@ref)))
			select (AbstractStatement)new PrintStatement { v = var };

		private static readonly TokenListParser<RiftToken, ComparsionTypeStatement> ComparsionType =
			Whitespaces.Optional().IgnoreThen(Parse.OneOf(
				Token.EqualTo(RiftToken.LessThan),
				Token.EqualTo(RiftToken.Less),
				Token.EqualTo(RiftToken.Equal),
				Token.EqualTo(RiftToken.NotEqual),
				Token.EqualTo(RiftToken.GreaterThan),
				Token.EqualTo(RiftToken.Greater))).Select(token => { return new ComparsionTypeStatement { type = token }; });


		public static readonly TokenListParser<RiftToken, AtomStatement> ComparsionLogOp =
			Parse.OneOf(
				Token.EqualTo(RiftToken.Or).Select(token => (AtomStatement)new OrStatement()),
				Token.EqualTo(RiftToken.And).Select(token => (AtomStatement)new AndStatement())
			);

		public static readonly TokenListParser<RiftToken, AbstractStatement> Comparsion =
			from l in Whitespaces.Optional().IgnoreThen(Value)
			from c in Whitespaces.IgnoreThen(ComparsionType)
			from r in Whitespaces.Optional().IgnoreThen(Value)
			select (AbstractStatement)new ComparsionStatement() { left = l, right = r, type = c, next = new NoneStatement() };


		public static readonly TokenListParser<RiftToken, AbstractStatement> ComparsionWithLogOp =
			from op in ComparsionLogOp
			from l in Whitespaces.Optional().IgnoreThen(Value)
			from c in Whitespaces.IgnoreThen(ComparsionType)
			from r in Whitespaces.Optional().IgnoreThen(Value)
			select (AbstractStatement)new ComparsionStatement() { left = l, right = r, type = c, next = op };

		public static readonly TokenListParser<RiftToken, AbstractStatement> Comparsions =
			from c1 in Comparsion
			from c2 in ComparsionWithLogOp.Many()
			select MergeComparsions(c1, c2);

		private static AbstractStatement MergeComparsions(AbstractStatement c1, AbstractStatement[] c2)
		{
			var path = new ComparsionStatement[c2.Length + 1];
			path[0] = (ComparsionStatement)c1;
			for (int i = 0; i < c2.Length; i++)
			{
				path[i + 1] = (ComparsionStatement)c2[i];
			}

			return new ComparsionsStatement()
			{
				sequence = path,
			};
		}

		public static readonly TokenListParser<RiftToken, StopAbstractStatement> Stop = Token.EqualToValue(RiftToken.Variable, "stop")
			.Select(token => new StopAbstractStatement { });

		public static readonly TokenListParser<RiftToken, ExitStatement> Exit = Token.EqualToValue(RiftToken.Variable, "exit").Select(token => new ExitStatement { });

		public static readonly TokenListParser<RiftToken, IfStatement> If =
			from l in Token.EqualTo(RiftToken.If).IgnoreThen(Comparsions)
			select new IfStatement { statement = (ComparsionsStatement)l };

		public static readonly TokenListParser<RiftToken, ElifStatement> Elif =
			from l in Token.EqualTo(RiftToken.Elif).IgnoreThen(Comparsions)
			select new ElifStatement { statement = (ComparsionsStatement)l };

		public static readonly TokenListParser<RiftToken, ElseStatement> Else =
			from l in Token.EqualTo(RiftToken.Else)
			select new ElseStatement { };

		public static readonly TokenListParser<RiftToken, NothingStatement> Comment =
			from _ in Token.EqualTo(RiftToken.Comment)
			select new NothingStatement { };

		public static readonly TokenListParser<RiftToken, CodeBlockStatement> CodeBlock =
			from l in Token.EqualToValue(RiftToken.Variable, "trigger").IgnoreThen(Whitespaces.Optional()).IgnoreThen(Token.EqualTo(RiftToken.Variable))
			select new CodeBlockStatement { name = l };

		public static readonly TokenListParser<RiftToken, IndentStatement> Statement =
			from ind in Indent.OptionalOrDefault()
			from st in Parse.OneOf(
				SetData.Try().Select(statement => (AbstractStatement)statement),
				GetData.Try().Select(statement => (AbstractStatement)statement),
				CallRet.Try().Select(statement => (AbstractStatement)statement),
				CallRetSingle.Try().Select(statement => (AbstractStatement)statement),
				AssigmentMath.Try().Select(statement => (AbstractStatement)statement),
				Assigment.Try().Select(statement => (AbstractStatement)statement),
				Exit.Try().Select(statement => (AbstractStatement)statement),
				Stop.Try().Select(statement => (AbstractStatement)statement),
				Stop.Try().Select(statement => (AbstractStatement)statement),
				Comment.Try().Select(statement => (AbstractStatement)statement),
				CodeBlock.Try().Select(statement => (AbstractStatement)statement),
				Variable.Select(statement => (AbstractStatement)statement),
				If.Select(statement => (AbstractStatement)statement),
				Elif.Select(statement => (AbstractStatement)statement),
				Else.Select(statement => (AbstractStatement)statement),
				Print.Select(statement => (AbstractStatement)statement),
				NewLine.Select(statement => (AbstractStatement)new NothingStatement())
			)
			from _ in Whitespaces.Optional()
			from __ in NewLine.OptionalOrDefault()
			select new IndentStatement { statement = st, ind = (byte)ind };


		private static readonly TokenListParser<RiftToken, InputStatement> Input =
			from variables in Token.EqualTo(RiftToken.Input).IgnoreThen(NewLine.Many()).IgnoreThen(InputVariable.Many().Select(sts => sts))
			select new InputStatement { variables = variables.Cast<InputVariableStatement>().ToArray<InputVariableStatement>() };

		private static readonly TokenListParser<RiftToken, SettingsStatement> Settings =
			from variables in Token.EqualTo(RiftToken.Settings).IgnoreThen(NewLine.Many()).IgnoreThen(SettingVariable.Many().Select(sts => sts))
			select new SettingsStatement { variables = variables.Cast<SettingsVariableStatement>().ToArray<SettingsVariableStatement>() };


		public static readonly TokenListParser<RiftToken, RiftParsedScript> Script =
			from input in NewLine.Many().IgnoreThen(Input.OptionalOrDefault(new InputStatement()))
			from settings in NewLine.Many().IgnoreThen(Settings.OptionalOrDefault(new SettingsStatement()))
			from st in Statement.Many()
			select new RiftParsedScript
			{
				sequence = st,
				inputStatement = input,
				settings = settings
			};
		// Indend.Optional().Then(Variable.Select(statement => statement));

		// public static readonly TokenListParser<AzazaToken, Block> Block =
		// Statement.Many().Select(exprs => new Block { statements = exprs });


		// Input.Select(f => f as Expr)
		// public static readonly TokenListParser<AzazaToken, Block> Lang =
		//     from stmts in Statement.Then(_ => Token.EqualTo(AzazaToken.NewLine).Optional())
		//         .Many()
		//     select new Block { statements = stmts };
	}
}