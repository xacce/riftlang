#if !UNITY_2020_1_OR_NEWER
using System;
using System.Runtime.CompilerServices;

namespace Rift
{
	public static class RiftLog
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string msg)
		{
			Console.WriteLine(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CLog(string msg)
		{
			Console.WriteLine(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ILog(string msg)
		{
			Console.WriteLine(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(object msg)
		{
			Console.WriteLine(msg.ToString());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(int msg)
		{
			Console.WriteLine(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Exception(string msg)
		{
			throw new Exception(msg);
		}

		public static void LogError(string s)
		{
			Console.WriteLine($"Error: {s}");
		}
	}
}
#endif