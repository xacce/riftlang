using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rift;
#if UNITY_2020_1_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Rift
{
    public static class RiftUtility
    {
        public static int SizeOf<T>() where T : unmanaged
        {
#if UNITY_2020_1_OR_NEWER
            return UnsafeUtility.SizeOf<T>();
#else
        return Unsafe.SizeOf<T>();
#endif
        }

        public static int SizeOf(Type t)
        {
#if UNITY_2020_1_OR_NEWER
            return UnsafeUtility.SizeOf(t);
#else
        return Marshal.SizeOf(t);
#endif
        }

        public static bool IsStructureType(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        public static int FieldOffset(FieldInfo fi)
        {
#if UNITY_2020_1_OR_NEWER
            return UnsafeUtility.GetFieldOffset(fi);
#else
            return (int)Marshal.OffsetOf(fi.DeclaringType, fi.Name);
#endif
        }

        public static RiftCompiledOpCode GetTypeOfType(Type type)
        {
            if (type == typeof(float))
            {
                return RiftCompiledOpCode.Float;
            }
            if (type == typeof(int))
            {
                return RiftCompiledOpCode.Int;
            }
            if (type == typeof(short))
            {
                return RiftCompiledOpCode.Short;
            }

            if (type == typeof(ushort))
            {
                return RiftCompiledOpCode.Ushort;
            }

            if (type == typeof(long))
            {
                return RiftCompiledOpCode.Long;
            }

            if (type == typeof(ulong))
            {
                return RiftCompiledOpCode.Ulong;
            }

            if (type == typeof(bool))
            {
                return RiftCompiledOpCode.Bool;
            }

            if (type == typeof(byte))
            {
                return RiftCompiledOpCode.Byte;
            }

            if (type == typeof(sbyte))
            {
                return RiftCompiledOpCode.Sbyte;
            }

    
            if (type == typeof(double))
            {
                return RiftCompiledOpCode.Double;
            }

   

            //todo add half from unity math
            throw new Exception($"{type} not supported");
        }
        public static Type GetTypeOfOpCode(RiftCompiledOpCode code)
        {
            switch (code)
            {
                case RiftCompiledOpCode.Float:  return typeof(float);
                case RiftCompiledOpCode.Int:    return typeof(int);
                case RiftCompiledOpCode.Short:  return typeof(short);
                case RiftCompiledOpCode.Ushort: return typeof(ushort);
                case RiftCompiledOpCode.Long:   return typeof(long);
                case RiftCompiledOpCode.Ulong:  return typeof(ulong);
                case RiftCompiledOpCode.Bool:   return typeof(bool);
                case RiftCompiledOpCode.Byte:   return typeof(byte);
                case RiftCompiledOpCode.Sbyte:  return typeof(sbyte);
                case RiftCompiledOpCode.Double: return typeof(double);
                default:
                    throw new Exception($"{code} not supported");
            }
        }
        
        public static int SizeOf(RiftCompiledOpCode type)
        {
            if ((type & RiftCompiledOpCode.Bool) != 0)
            {
                return 1;
            }

            if ((type & RiftCompiledOpCode.Int) != 0)
            {
                return 4;
            }

            if ((type & RiftCompiledOpCode.Float) != 0)
            {
                return 4;
            }

            throw new ArgumentOutOfRangeException($"Invalid type: {type}");
        }
    }
}