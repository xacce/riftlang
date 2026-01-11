using Rift;
#if UNITY_2020_1_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#else
using System;
using System.Runtime.InteropServices;
#endif

//Хранит поинтеры на входные данные
//порядок важен
//len это кол-во поинтеров
public unsafe struct RiftRwPointerMem
{
    public int length;

#if UNITY_2020_1_OR_NEWER
    [NativeDisableUnsafePtrRestriction] public byte** data;
    private Allocator allocator;
#else
    private IntPtr dataPtr;
#endif

    public void Init(int length
#if UNITY_2020_1_OR_NEWER
        , Allocator allocator = Allocator.Temp
#endif
    )
    {
        this.length = length;
#if UNITY_2020_1_OR_NEWER
        this.allocator = allocator;
        data = (byte**)UnsafeUtility.Malloc(length * sizeof(byte*), 4, allocator);
#else
        dataPtr = Marshal.AllocHGlobal(length * sizeof(byte*));
#endif
    }

    public void SetPointer<T>(int index, T* ptr) where T : unmanaged
    {
#if UNITY_2020_1_OR_NEWER
        data[index] = (byte*)ptr;
#else
        ((byte**)dataPtr)[index] = (byte*)ptr;
#endif
    }

    public byte* GetPointer(int index)
    {
#if UNITY_2020_1_OR_NEWER
        return data[index];
#else
        return ((byte**)dataPtr)[index];
#endif
    }

    public void UpdateValueForPointer(int index, byte* value, int size)
    {
        byte* dst = GetPointer(index);

#if UNITY_2020_1_OR_NEWER
        UnsafeUtility.MemCpy(dst, value, (int)size);
#else
        System.Buffer.MemoryCopy(value, dst, size, size);
#endif
    }

    public void UpdateValueForPointer(byte* dst, byte* value, int size, RiftCompiledOpCode dstTag, RiftCompiledOpCode srcTag)
    {
        if (dstTag == RiftCompiledOpCode.Float && srcTag == RiftCompiledOpCode.Int)
        {
            *(float*)dst = (float)*(int*)value;
            return;
        }

        if (dstTag == RiftCompiledOpCode.Int && srcTag == RiftCompiledOpCode.Float)
        {
            *(int*)dst = (int)*(float*)value;
            return;
        }

#if UNITY_2020_1_OR_NEWER
        UnsafeUtility.MemCpy(dst, value, size);
#else
        System.Buffer.MemoryCopy(value, dst, size, size);
#endif
    }


    public void Dispose()
    {
#if UNITY_2020_1_OR_NEWER
        if (data != null)
            UnsafeUtility.Free(data, allocator);
        data = null;
#else
        if (dataPtr != IntPtr.Zero)
            Marshal.FreeHGlobal(dataPtr);
        dataPtr = IntPtr.Zero;
#endif
        length = 0;
    }
}