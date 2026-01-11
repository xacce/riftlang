using System;
#if UNITY_2020_1_OR_NEWER
using Unity.Collections;
#endif

namespace Rift
{
	[Serializable]
	public struct RiftScriptBinary
	{
		public byte[] input;
		public int inputLen;
		public int variablesLen;
	}


	[Serializable]
	public struct RiftScriptSerializable
	{
		public RiftScriptMeta meta;
		public byte[] bytecode;
	}

	[Serializable]
	public struct RiftScriptMeta
	{
		public int mainCodeBlockSize;
		public int settingsSize;
		public int variablesSize;
		public int triggersBlockSize;
		public int total;
	}

// 	public struct RiftScript
// 	{
// #if UNITY_2020_1_OR_NEWER
// 		public NativeArray<byte> input; //not used?
// 		public NativeArray<byte> bytecode;
// 		public RiftMem settings;
// 		public RiftMem mem;
// #else
// 		public RiftMem settings;
// 		public RiftMem mem;
// 		public byte[] bytecode;
// #endif
// 	}
}