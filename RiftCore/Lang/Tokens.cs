namespace Rift
{
	public enum RiftToken
	{
		None,
		Variable,
		Assign,
		Number,
		Input,
		Settings,
		IntType,
		BoolType,
		BoolValue,
		True,
		False,
		If,
		Else,
		Tab,
		Whitespace,
		NewLineN,
		NewLineR,
		Space,
		Print,
		Or,
		And,
		Greater,
		Less,
		GreaterThan,
		LessThan,
		Equal,
		NotEqual,
		Invert,
		Elif,
		Dot,
		FloatType,
		ExitJump,
		FetchFrom,
		From,
		Update,
		// DataTo
		Fetch,
		Add,
		Substract,
		Multiply,
		Divide,
		Ignore,

		Decimal,
		Comment
	};
	//
	// public enum DataType
	// {
	//     Invalid,
	//     Int32,
	//     Bool,
	//     Struct,
	// }
}