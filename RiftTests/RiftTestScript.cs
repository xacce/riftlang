using Rift;
using Rift.Lang;
using Rift.Tests.Environments;
using Superpower.Model;

namespace RiftTests
{
	public class RiftTestScriptNullEnv : RiftTestScriptPredefinedEnv<RiftTestsNullEnv, RiftTestsNullInterpretEnv>
	{
		public RiftTestScriptNullEnv(string code) : base(code)
		{
		}
	}

	public class RiftTestScriptTriggersEnv : RiftTestScriptPredefinedEnv<RiftTestsTriggersEnv, RiftTestsNullInterpretEnv>
	{
		public RiftTestScriptTriggersEnv(string code) : base(code)
		{
		}
	}

	public class RiftTestScriptCustomStructsEnv : RiftTestScriptPredefinedEnv<RiftTestsCustomStructsEnv, RiftTestsNullInterpretEnv>
	{
		public RiftTestScriptCustomStructsEnv(string code) : base(code)
		{
		}
	}

	public class RiftTestScriptSetGetDataEnv : RiftTestScriptPredefinedEnv<RiftTestsSetGetDataEnv, RiftTestsSetGetDataInterpretEnv>
	{
		public RiftTestScriptSetGetDataEnv(string code) : base(code)
		{
		}
	}

	public class RiftTestScriptAvdEnv : RiftTestScriptPredefinedEnv<RiftTestsAvdancedEnv, RiftTestsAdvInterpretEnv>
	{
		public RiftTestScriptAvdEnv(string code) : base(code)
		{
		}
	}

	public class RiftTestScriptPredefinedEnv<T, TY> : RiftTestScript<T, TY>
		where T : IRiftEnvironment, new()
		where TY : IRiftInterpretEnvironment, IDisposable, new()
	{
		public RiftTestScriptPredefinedEnv(string code) : base(new T(), new TY(), code)
		{
		}

		public static RiftTestScriptPredefinedEnv<T, TY> Execute(string code)
		{
			var ex = new RiftTestScriptPredefinedEnv<T, TY>(code);
			ex.Tokenize().Parse().Compile().Initialize().SetSettings(1).Tick();
			return ex;
		}

		public static RiftTestScriptPredefinedEnv<T, TY> Trigger(int triggerIndex, string code)
		{
			var ex = new RiftTestScriptPredefinedEnv<T, TY>(code);
			ex.Tokenize().Parse().Compile().Initialize().SetSettings(1).Trigger(triggerIndex);
			return ex;
		}

		public static RiftTestScriptPredefinedEnv<T, TY> Compile(string code)
		{
			var ex = new RiftTestScriptPredefinedEnv<T, TY>(code);
			ex.Tokenize().Parse().Compile();
			return ex;
		}

		public static RiftTestScriptPredefinedEnv<T, TY> Initialize(string code)
		{
			var ex = new RiftTestScriptPredefinedEnv<T, TY>(code);
			ex.Tokenize().Parse().Compile().Initialize();
			return ex;
		}
	}

	public class RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> : IDisposable where TIRiftEnvironment : IRiftEnvironment
		where TInterpretEnvironment : IRiftInterpretEnvironment
	{
		private TInterpretEnvironment _interpretEnvironment;
		private readonly TIRiftEnvironment _environment;
		private readonly string _code;
		private TokenList<RiftToken> _tokenList;
		private RiftParsedScript _parsed;
		private RiftCompiler _compiler;
		private RiftScriptSerializable _compiled;
		private RiftMem _variables;
		private RiftMem _settings;
		private readonly RiftRwPointerMem _inputRw;
		private RiftInterpret<RiftMem, RiftMem>.InterpretResponse _response;

		public RiftInterpret<RiftMem, RiftMem>.InterpretResponse lastResponse => _response;

		public TokenList<RiftToken> tokenList => _tokenList;

		public RiftParsedScript parsed => _parsed;

		public RiftCompiler compiler => _compiler;

		public RiftScriptSerializable compiled => _compiled;

		public RiftMem variables => _variables;

		public RiftTestScript(TIRiftEnvironment environment, TInterpretEnvironment interpretEnvironment, string code)
		{
			_interpretEnvironment = interpretEnvironment;
			_environment = environment;
			_code = code;
			_inputRw = new RiftRwPointerMem();
			_inputRw.Init(0);
			// var settings = new RiftMem(12);
			// settings.WriteBlind(1);
			// settings.WriteBlind(2);
			// settings.WriteBlind(3);
			// RiftInterpret.Interpretet(ctx, 0, riftscript, compiled.meta, inputRw, out var response);
		}

		public ref TInterpretEnvironment interpretEnvironment => ref _interpretEnvironment;

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Tokenize()
		{
			Assert.DoesNotThrow(() => _tokenList = new RiftTokenizer().Tokenize(_code));
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Parse()
		{
			Assert.DoesNotThrow(() => _parsed = new RiftParser().Parse(_tokenList));
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Compile()
		{
			Assert.DoesNotThrow(() => _compiler = new RiftCompiler(_environment));
			Assert.DoesNotThrow(() => _compiled = _compiler.Compile(_parsed));
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> SetSettings<T>(T value) where T : unmanaged
		{
			Assert.DoesNotThrow(() => _settings = new RiftMem(RiftUtility.SizeOf<T>()));
			Assert.DoesNotThrow(() => _settings.WriteBlind(value));
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> SetSettings(RiftMem settings)
		{
			_settings = settings;
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> SetSettings<T, TY>(T value, TY value2) where T : unmanaged where TY : unmanaged
		{
			Assert.DoesNotThrow(() => _settings = new RiftMem(RiftUtility.SizeOf<T>() + RiftUtility.SizeOf<TY>()));
			Assert.DoesNotThrow(() => _settings.WriteBlind(value));
			Assert.DoesNotThrow(() => _settings.WriteBlind(value2));
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> SetSettings<T, TY, TZ>(T value, TY value2, TZ value3)
			where T : unmanaged where TY : unmanaged where TZ : unmanaged
		{
			Assert.DoesNotThrow(() => _settings = new RiftMem(RiftUtility.SizeOf<T>() + RiftUtility.SizeOf<TY>() + RiftUtility.SizeOf<TZ>()));
			Assert.DoesNotThrow(() => _settings.WriteBlind(value));
			Assert.DoesNotThrow(() => _settings.WriteBlind(value2));
			Assert.DoesNotThrow(() => _settings.WriteBlind(value3));
			return this;
		}

		public RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Initialize()
		{
			Assert.DoesNotThrow(() => _variables = new RiftMem(_compiled.meta.variablesSize));
			return this;
		}

		public unsafe RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Tick()
		{
			TestDelegate aaa = () =>
			{
				fixed (byte* p = compiled.bytecode)
				{
					RiftInterpret<RiftMem, RiftMem>.Interpretet(ref _interpretEnvironment, 0, p, _variables, _settings, _compiled.meta, _inputRw, out var response2);
					_response = response2;
				}
			};
			Assert.DoesNotThrow(aaa);

			return this;
		}

		public unsafe RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Trigger(int triggerIndex)
		{
			TestDelegate aaa = () =>
			{
				fixed (byte* p = compiled.bytecode)
				{
					RiftInterpret<RiftMem, RiftMem>.Trigger(ref _interpretEnvironment, triggerIndex, p, _variables, _settings,  _compiled.meta, _inputRw, out var response2);
					_response = response2;
				}
			};
			Assert.DoesNotThrow(aaa);

			return this;
		}

		public static RiftTestScript<TIRiftEnvironment, TInterpretEnvironment> Execute(TIRiftEnvironment environment, TInterpretEnvironment interpretEnvironment,
			string code)
		{
			var ex = new RiftTestScript<TIRiftEnvironment, TInterpretEnvironment>(environment, interpretEnvironment, code);
			ex.Tokenize().Parse().Compile().SetSettings(1).Initialize().Tick();
			return ex;
		}

		public void Dispose()
		{
			_inputRw.Dispose();
			_variables.Dispose();
			_settings.Dispose();
		}
	}
}