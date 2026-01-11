#if UNITY_2020_1_OR_NEWER
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace Rift
{
	public static class RiftLog
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("ENABLE_RIFT_INTERPETER_LOGS")]
		public static void ILog(string msg)
		{
			Debug.Log($"[RiftCore][Interpeter] {msg}");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("ENABLE_RIFT_COMPILER_LOGS")]
		public static void CLog(string msg)
		{
			Debug.Log($"[RiftCore][Compiler] {msg}");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string msg)
		{
			Debug.Log(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LogError(string msg)
		{
			Debug.LogError(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(int msg)
		{
			Debug.Log(msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Exception(string msg)
		{
			throw new Exception(msg);
		}
	}
}
#endif