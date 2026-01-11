using Superpower.Model;
using System;

namespace Rift
{
	public abstract class RiftException : Exception
	{
		protected RiftException(string message) : base(message)
		{
		}
	}

	public class RiftCompilationException : RiftException
	{
		public RiftCompilationException(string message, Token<RiftToken> token) : base($"{message} at {token.Span.Position}")
		{
		}
	}

	public class RiftUndefinedCompilationException : RiftException
	{
		public RiftUndefinedCompilationException(string message) : base($"{message} at unknown line")
		{
		}
	}
}