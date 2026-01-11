using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Rift
{
	public class RiftTokenizer
	{
		private readonly Tokenizer<RiftToken> _tokenizer = new TokenizerBuilder<RiftToken>()
			// .Ignore(Character.EqualTo('\r'))
			.Match(Span.Regex(@"//[^\n]*(\r\n|\r|\n|$)"), RiftToken.Comment)
			.Match(Span.Regex(@"/\*[\s\S]*?\*/"), RiftToken.Comment)
			.Match(Parse.Sequence(Span.EqualTo("fetch from"), Character.EqualTo(' ')), RiftToken.FetchFrom)
			.Match(Parse.Sequence(Span.EqualTo("fetch"), Character.EqualTo(' ')), RiftToken.Fetch)
			.Match(Parse.Sequence(Span.EqualTo("update"), Character.EqualTo(' ')), RiftToken.Update)
			.Match(Parse.Sequence(Character.EqualTo(' '), Span.EqualTo("from"), Character.EqualTo(' ')), RiftToken.From)
			// .Match(Parse.Sequence(Character.EqualTo(' '),Span.EqualTo("to"),Character.EqualTo(' ')),RiftToken.DataTo)
			.Match(Parse.Sequence(Character.EqualTo(' '), Span.EqualTo("or"), Character.EqualTo(' ')), RiftToken.Or)
			.Match(Parse.Sequence(Character.EqualTo(' '), Span.EqualTo("and"), Character.EqualTo(' ')), RiftToken.And)
			.Match(Parse.Sequence(Character.EqualTo(' '), Character.EqualTo(' '), Character.EqualTo(' '), Character.EqualTo(' ')), RiftToken.Tab)
			.Match(Character.EqualTo(' '), RiftToken.Whitespace)
			.Match(Character.EqualTo('\t'), RiftToken.Tab)
			// .Ignore(Character.WhiteSpace)
			.Match(Span.EqualTo(">="), RiftToken.GreaterThan)
			.Match(Span.EqualTo("<="), RiftToken.LessThan)
			.Match(Span.EqualTo("!="), RiftToken.NotEqual)
			.Match(Span.EqualTo("!"), RiftToken.Invert)
			.Match(Span.EqualTo("=="), RiftToken.Equal)
			.Match(Character.EqualTo('>'), RiftToken.Greater)
			.Match(Character.EqualTo('<'), RiftToken.Less)
			.Match(Character.EqualTo('+'), RiftToken.Add)
			.Match(Character.EqualTo('-'), RiftToken.Substract)
			.Match(Character.EqualTo('*'), RiftToken.Multiply)
			.Match(Character.EqualTo('/'), RiftToken.Divide)
			
			.Match(Character.EqualTo('='), RiftToken.Assign)
			.Match(Span.EqualTo("int "), RiftToken.IntType)
			.Match(Span.EqualTo("float "), RiftToken.FloatType)
			.Match(Span.EqualTo("bool "), RiftToken.BoolType)
			.Match(Span.EqualTo("print "), RiftToken.Print)
			.Match(Span.EqualTo("\n"), RiftToken.NewLineN)
			.Match(Character.EqualTo('\r'), RiftToken.NewLineR)
			.Match(Span.EqualTo("if "), RiftToken.If)
			.Match(Span.EqualTo("elif "), RiftToken.Elif)
			.Match(Span.EqualTo("else"), RiftToken.Else)
			.Match(Parse.Sequence(Character.EqualTo('-'), Numerics.Integer, Character.EqualTo('.'), Numerics.Integer), RiftToken.Decimal)
			.Match(Parse.Sequence(Numerics.Integer, Character.EqualTo('.'), Numerics.Integer), RiftToken.Decimal)
			.Match(Numerics.Integer, RiftToken.Number)
			// .Match(Numerics.DecimalDouble, RiftToken.Number)
			.Match(Span.EqualTo("true"), RiftToken.True)
			.Match(Span.EqualTo("false"), RiftToken.False)
			.Match(Parse.Sequence(Span.EqualTo("input"), Span.EqualTo('\n').Or(Span.EqualTo("\r\n"))), RiftToken.Input)
			.Match(Parse.Sequence(Span.EqualTo("settings"), Span.EqualTo('\n').Or(Span.EqualTo("\r\n"))), RiftToken.Settings)
			.Match(Character.EqualTo('.'), RiftToken.Dot)
			.Match(Identifier.CStyle, RiftToken.Variable)
			.Build();


		public TokenList<RiftToken> Tokenize(string text)
		{
			return _tokenizer.Tokenize(text);
		}
	}
}