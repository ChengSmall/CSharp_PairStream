using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Cheng.Stream.PairToByteStreams.DataOirgnal;
namespace Cheng.Stream.PairToByteStreams
{
    namespace DataOirgnal
    {

        /// <summary>
        /// <para>字节流转化器</para>
        /// <para>可以将指定对象转化为字节流的方法，使用该接口实现自定义数据存储</para>
        /// <para>若遇到对象无法直接实例化，请使用<see cref="IReferenceTypeToByteStream{Obj}"/>接口</para>
        /// </summary>
        /// <typeparam name="Obj">转化的对象类型</typeparam>
        public interface IObjToByteStream<Obj>
        {
            /// <summary>
            /// 将字节流转化为对象
            /// </summary>
            /// <param name="byteStream">要转化的字节流数据</param>
            /// <returns>转化的对象</returns>
            Obj ToObj(byte[] byteStream);
            /// <summary>
            /// 将对象转化为字节流的方法
            /// </summary>
            /// <param name="obj">需要转化的对象</param>
            /// <returns>转化后的字节流数据</returns>
            /// <exception cref="ArgumentNullException">返回了一个null</exception>
            /// <exception cref="ArgumentOutOfRangeException">返回的数据长度超过<see cref="int.MaxValue"/></exception>
            byte[] ToByteStream(Obj obj);
        }
        /// <summary>
        /// 字节流转化器，转化引用的类型对象
        /// <para>该接口用于无法直接实例化的对象类型</para>
        /// </summary>
        /// <typeparam name="Obj">指定类型</typeparam>
        public interface IReferenceTypeToByteStream<Obj> : IObjToByteStream<Obj>
        {

            /// <summary>
            /// 将转化好的字节流写入对象
            /// </summary>
            /// <param name="byteStream">要写入对象的字节流数据</param>
            /// <param name="obj">待写入的对象</param>
            void ToObj(byte[] byteStream, ref Obj obj);
        }
        /// <summary>
        /// 公用类型接口字节流转化器
        /// <para>可以将派生对象转化为字节流的方法，使用该接口实现自定义数据存储</para>
        /// </summary>
        public interface IObjToByteStream
        {
            /// <summary>
            /// 将派生对象转化为字节流的方法
            /// </summary>
            /// <returns>转化后的字节流数据</returns>
            /// <exception cref="ArgumentNullException">返回了一个null</exception>
            /// <exception cref="ArgumentOutOfRangeException">返回的数据长度超过<see cref="int.MaxValue"/></exception>
            byte[] ToByteStream();
            /// <summary>
            /// 将字节流写入到该派生对象
            /// <para>该函数通过<paramref name="byteStream"/>参数修改已有数据</para>
            /// </summary>
            /// <param name="byteStream">要写入的字节流数据</param>
            void ToObj(byte[] byteStream);
        }


        /// <summary>
        /// 数据管理方法
        /// </summary>
        public unsafe static class OrignalDatas
        {
            #region 字节流

            /// <summary>
            /// <para>将字节数据转化为指定地址内存，自定义字节数据开始位置和长度</para>
            /// <para>写入的数据下标区间[begin,end)</para>
            /// </summary>
            /// <param name="bys">字节流</param>
            /// <param name="address">写入的内存起始地址</param>
            /// <param name="begin">字节数组要写入的起始下标</param>
            /// <param name="count">要写入字节数据的大小</param>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void ToStruckData(this byte[] bys, IntPtr address,uint begin, uint count)
            {
                if (bys is null)
                {
                    throw new ArgumentNullException("bys", "字节数组为null");
                }
                fixed (byte* p = bys)
                {
                    Buffer.MemoryCopy(p + begin, (void*)address, count, count);
                }
            }

            /// <summary>
            /// <para>将字节数据转化为指定地址的内存；规定转化字节长度</para>
            /// <para>将从字节数据的初始位置开始写入，<paramref name="size"/>个长度后停止；如果规定长度超过数组长度，则会在字节数据写完后停止</para>
            /// </summary>
            /// <param name="bys">字节流</param>
            /// <param name="address">需要写入内存的起始位置</param>
            /// <param name="size">想要写入的字节流长度</param>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void ToStruckData(this byte[] bys, IntPtr address, uint size)
            {
                if (bys is null)
                {
                    throw new ArgumentNullException("bys", "字节数组为null");
                }
                fixed (byte* p = bys)
                {
                    Buffer.MemoryCopy(p, (void*)address, size, size);
                }
            }

            /// <summary>
            /// <para>将字节流的值复制到指定地址下的内存</para>
            /// <para>地址<paramref name="address"/>开辟的空间至少等于字节数据内的长度</para>
            /// </summary>
            /// <param name="bys">字节流</param>
            /// <param name="address">转化后存储的位置，该位置必须是可用的内存</param>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void ToStruckData(this byte[] bys, IntPtr address)
            {
                if(bys is null)
                {
                    throw new ArgumentNullException("bys", "字节数组为null");
                }
                int size = bys.Length;
                fixed (byte* p = bys)
                {
                    Buffer.MemoryCopy(p, (void*)address, size, size);
                }
            }
            /// <summary>
            /// <para>数据内存转化为字节流</para>
            /// </summary>
            /// <param name="address">数据内存地址</param>
            /// <param name="size">数据内存子节大小</param>
            /// <returns>字节流</returns>
            public static byte[] ToStrByteArray(this IntPtr address, uint size)
            {
                byte[] arr = new byte[size];
                fixed (byte* parr = arr)
                {
                    Buffer.MemoryCopy(address.ToPointer(), parr, size, size);
                }
                return arr;
            }
            /// <summary>
            /// 布尔类型转字节流
            /// </summary>
            public static byte[] ToByteArray(this bool bo)
            {
                byte[] arr = new byte[sizeof(bool)];
                fixed (byte* p = arr)
                {
                    *(bool*)p = bo;
                }
                return arr;
            }
            /// <summary>
            /// 将整型值转化为字节流
            /// </summary>
            /// <returns>整型值所表示的字节流</returns>
            public static byte[] ToByteArray(this int Int)
            {
                byte[] arr = new byte[sizeof(int)];
                fixed (byte* p = arr)
                {
                    *(int*)p = Int;
                }
                return arr;
            }
            /// <summary>
            /// 将长整形转化为字节流
            /// </summary>
            /// <returns>表示该值的字节流</returns>
            public static byte[] ToByteArray(this long Long)
            {
                byte[] arr = new byte[sizeof(long)];
                fixed (byte* p = arr)
                {
                    *(long*)p = Long;
                }
                return arr;
            }
            /// <summary>
            /// 将单浮点值转化为字节流表示
            /// </summary>
            /// <returns>表示该浮点值的字节流</returns>
            public static byte[] ToByteArray(this float flo)
            {
                byte[] arr = new byte[sizeof(float)];
                fixed (byte* p = arr)
                {
                    *(float*)p = flo;
                }
                return arr;
            }
            /// <summary>
            /// 将双浮点值转化为字节流表示
            /// </summary>
            /// <returns>表示该双浮点值的字节流</returns>
            public static byte[] ToByteArray(this double dou)
            {
                byte[] arr = new byte[sizeof(double)];
                fixed (byte* p = arr)
                {
                    *(double*)p = dou;
                }
                return arr;
            }
            /// <summary>
            /// 将短整型转化为字节流
            /// </summary>
            public static byte[] ToByteArray(this short sh)
            {
                byte[] arr = new byte[sizeof(short)];
                fixed (byte* p = arr)
                {
                    *(short*)p = sh;
                }
                return arr;
            }
            /// <summary>
            /// 将字符转化为字节流
            /// </summary>
            public static byte[] ToByteArray(this char c)
            {
                byte[] arr = new byte[sizeof(char)];
                fixed (byte* p = arr)
                {
                    *(char*)p = c;
                }
                return arr;
            }
            /// <summary>
            /// 十进制转字节流
            /// </summary>
            public static byte[] ToByteArray(this decimal dec)
            {
                byte[] arr = new byte[sizeof(decimal)];
                fixed (byte* p = arr)
                {
                    *(decimal*)p = dec;
                }
                return arr;
            }
            /// <summary>
            /// 字节流转化短整型
            /// </summary>
            public static short ToShort(this byte[] bys)
            {
                short re;
                fixed (byte* p = bys)
                {
                    re = *(short*)p;
                }
                return re;
            }
            /// <summary>
            /// 字节流转化短整型
            /// </summary>
            /// <param name="bys"></param>
            /// <param name="begin">起始下标</param>
            /// <returns></returns>
            public static short ToShort(this byte[] bys, int begin)
            {
                short re;
                fixed (byte* p = bys)
                {
                    re = *(short*)(p + begin);
                }
                return re;
            }
            /// <summary>
            /// 字节流转布尔值
            /// </summary>
            public static bool ToBool(this byte[] bys)
            {
                byte b = bys[0];
                return *(bool*)&b;
            }
            /// <summary>
            /// 字节流转布尔值
            /// </summary>
            /// <param name="bys">字节流</param>
            /// <param name="index">指定转换下标</param>
            public static bool ToBool(this byte[] bys, int index)
            {
                bool b;
                byte* p = (byte*)&b;
                *p = bys[index];
                return b;
            }

            /// <summary>
            /// <para>字节流转32位整形数组</para>
            /// <para>从数组起始下标开始转化</para>
            /// </summary>
            /// <param name="bys">字节流</param>
            public static int ToInt32(this byte[] bys)
            {
                int re;
                fixed (byte* p = bys)
                {
                    re = *(int*)p;
                }
                return re;
            }
            /// <summary>
            /// <para>从指定下标开始转化32位整型值</para>
            /// </summary>
            /// <param name="begin">起始下标</param>
            /// <param name="bys">字节流</param>
            /// <returns>从起始下标向后延伸4个长度的数据转化的整数</returns>
            public static int ToInt32(this byte[] bys, int begin)
            {
                int re;
                fixed (byte* p = bys)
                {
                    re = *(int*)(p + begin);
                }
                return re;
            }
            /// <summary>
            /// <para>字节流转64位整形数组</para>
            /// <para>从数组起始下标开始转化</para>
            /// </summary>
            public static long ToInt64(this byte[] bys)
            {
                long re = 0;
                fixed (byte* p = bys)
                {
                    re = *(long*)p;
                }
                return re;
            }
            /// <summary>
            /// <para>从指定下标开始转化64位整型值</para>
            /// </summary>
            /// <param name="begin">起始下标</param>
            /// <param name="bys">字节流</param>
            /// <returns>从起始下标向后延伸8个长度的数据转化的整数</returns>
            public static long ToInt64(this byte[] bys, int begin)
            {
                long re = 0;
                fixed (byte* p = bys)
                {
                    re = *(long*)(p + begin);
                }
                return re;
            }
            /// <summary>
            /// 字节流转浮点值，从数组起始开始
            /// </summary>
            public static float ToFloat(this byte[] bys)
            {
                float re;
                fixed (byte* p = bys)
                {
                    re = *(float*)p;
                }
                return re;
            }
            /// <summary>
            /// 从指定位置转化浮点值
            /// </summary>
            /// <param name="begin">起始位置</param>
            /// <param name="bys">字节流</param>
            public static float ToFloat(this byte[] bys, int begin)
            {
                float re;
                fixed (byte* p = bys)
                {
                    re = *(float*)(p + begin);
                }
                return re;
            }
            /// <summary>
            /// 字节流转化双精度浮点值
            /// </summary>
            public static double ToDouble(this byte[] bys)
            {
                double re;
                fixed (byte* p = bys)
                {
                    re = *(double*)p;
                }
                return re;
            }
            /// <summary>
            /// 指定字节流起始位置转化双精度浮点值
            /// </summary>
            /// <param name="bys">字节流</param>
            /// <param name="begin">字节数组起始下标</param>
            public static double ToDouble(this byte[] bys, int begin)
            {
                double re;
                fixed (byte* p = bys)
                {
                    re = *(double*)(p + begin);
                }
                return re;
            }
            /// <summary>
            /// 字节流转化为十进制数
            /// </summary>
            /// <returns>字节数组在内存当中转化</returns>
            public static decimal ToDecimal(this byte[] bys)
            {
                decimal re;
                fixed (void* p = bys)
                {
                    re = *(decimal*)p;
                }
                return re;
            }
            /// <summary>
            /// 字节流转化字符
            /// </summary>
            public static char ToCharData(this byte[] bys)
            {
                char re;
                fixed (void* p = bys)
                {
                    re = *(char*)p;
                }
                return re;
            }
            /// <summary>
            /// 字符串转化字节流储存
            /// </summary>
            /// <returns>转化后的字节流</returns>
            public static byte[] ToByteData(this string str)
            {
                byte[] reb = new byte[str.Length * sizeof(char)];
                int length = str.Length;
                fixed (void* p = reb, strp = str)
                {
                    Buffer.MemoryCopy(strp, p, reb.Length, reb.Length);
                }
                return reb;
            }
            /// <summary>
            /// 将字节流转化为字符串
            /// </summary>
            /// <returns>字节流数据表示的字符串</returns>
            public static string ToStringByData(this byte[] bys)
            {
                //字符数量
                int count = bys.Length / sizeof(char);
                char[] cs = new char[count];
                Buffer.BlockCopy(bys, 0, cs, 0, bys.Length);
                return new string(cs);
            }
            /// <summary>
            /// 将非托管类型转化为字节流
            /// </summary>
            /// <typeparam name="T">非托管类型</typeparam>
            /// <param name="obj">要转化的对象</param>
            /// <returns>转化后的字节数组</returns>
            public static byte[] ToByteArray<T>(this T obj) where T : unmanaged
            {
                int size = sizeof(T);
                byte[] bys = new byte[size];
                fixed (void* p = bys)
                {
                    *(T*)p = obj;
                }
                return bys;
            }
            /// <summary>
            /// 将字节数组转化为非托管类型
            /// </summary>
            /// <typeparam name="T">非托管类型</typeparam>
            /// <param name="bys">要转化的数组</param>
            /// <returns>转化后的对象</returns>
            /// <exception cref="ArgumentException">类型大小超过和字节数组</exception>
            public static T ToStruct<T>(this byte[] bys) where T : unmanaged
            {
                if (sizeof(T) > bys.Length)
                {
                    throw new ArgumentException("类型大小超过和字节数组");
                }
                T re;
                fixed (void* p = bys)
                {
                    re = *(T*)p;
                }
                return re;
            }
            /// <summary>
            /// 将非托管类型写入字节数组，指定写入的起始索引
            /// </summary>
            /// <typeparam name="T">非托管类型</typeparam>
            /// <param name="obj">要写入的数据</param>
            /// <param name="bys">被写入的字节数组</param>
            /// <param name="index">被写入的字节数组，从该索引开始写入</param>
            /// <exception cref="ArgumentOutOfRangeException">类型大小超过数组写入长度</exception>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void WriteByteArray<T>(this byte[] bys, T obj,  int index) where T : unmanaged
            {
                if (bys == null)
                {
                    throw new ArgumentNullException("byte[]", "字节数组为null");
                }
                int size = sizeof(T);
                int length = bys.Length;
                if(size + index > length)
                {
                    throw new ArgumentOutOfRangeException("obj", "类型大小超过数组写入长度");
                }
                fixed (byte* p = bys)
                {
                    T* tp = (T*)(p + index);
                    *tp = obj;
                }
            }
            /// <summary>
            /// 将非托管类型写入字节数组
            /// </summary>
            /// <typeparam name="T">非托管类型</typeparam>
            /// <param name="obj">要写入的数据</param>
            /// <param name="bys">被写入的字节数组</param>
            /// <exception cref="ArgumentOutOfRangeException">类型大小超过数组写入长度</exception>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void WriteByteArray<T>(this byte[] bys, T obj) where T : unmanaged
            {
                WriteByteArray(bys, obj, 0);
            }
            /// <summary>
            /// 将指定byte集合中的一段字节数据转化为指定类型对象
            /// </summary>
            /// <typeparam name="Value">转化类型</typeparam>
            /// <param name="bys">集合</param>
            /// <param name="index">集合转化的字节数据起始索引</param>
            /// <returns>转化后的集合</returns>
            /// <exception cref="ArgumentOutOfRangeException">转化的集合长度不足</exception>
            /// <exception cref="ArgumentException">指定类型<typeparamref name="Value"/>不是一个可以转化为连续内存块的类型，无法获取类型大小</exception>
            /// <exception cref="OutOfMemoryException">在转化对象时没有足够的内存储存临时对象内存</exception>
            /// <exception cref="MissingMethodException">类型<typeparamref name="Value"/>没有公开的默认构造函数</exception>
            public static Value ToStruct<Value>(this IList<byte> bys, int index) where Value : unmanaged
            {
                int size = sizeof(Value);//类型大小
                Value value;
                int count = bys.Count;//集合长度
                if (index + size > count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("指定初始索引或类型大小超越集合界限");
                }
                byte* vp = (byte*)&value;
                int j = 0;
                for (int i = index; i < count && j < size; i++,j++)
                {
                    vp[j] = bys[i];
                }
                return value;
            }
            /// <summary>
            /// 将指定byte集合中的一段字节数据转化为指定类型对象
            /// </summary>
            /// <typeparam name="Value">转化类型</typeparam>
            /// <param name="bys">集合</param>
            /// <param name="index">集合转化的字节数据起始索引</param>
            /// <returns>转化后的集合</returns>
            /// <exception cref="ArgumentOutOfRangeException">转化的集合长度不足</exception>
            /// <exception cref="ArgumentNullException">字节数组不可为null</exception>
            public static Value ToStruct<Value>(this byte[] bys, int index) where Value : unmanaged
            {
                if (bys == null)
                {
                    throw new ArgumentNullException("byte[]", "字节数组为null");
                }
                int size = sizeof(Value);
                int length = bys.Length;
                if (size + index > length)
                {
                    throw new ArgumentOutOfRangeException("byte[]", "类型大小超过数组读取长度");
                }
                Value t;
                fixed (byte* p = bys)
                {
                    Value* tp = (Value*)(p + index);
                    t = *tp;
                }
                return t;
            }
            /// <summary>
            /// 将指定非托管内存的地址转化为指定非托管对象引用
            /// </summary>
            /// <typeparam name="Ref">转化的类型</typeparam>
            /// <param name="address">将要转化内存地址</param>
            /// <returns>对内存<paramref name="address"/>的<typeparamref name="Ref"/>类型的引用</returns>
            public static ref Ref ToRefValue<Ref>(this IntPtr address) where Ref : unmanaged
            {
                Ref* p = (Ref*)address;
                return ref *p;
            }
            /// <summary>
            /// 将非托管类型写入字节集合，指定写入的起始索引
            /// </summary>
            /// <typeparam name="T">非托管类型</typeparam>
            /// <param name="obj">要写入的数据</param>
            /// <param name="bys">被写入的字节集合</param>
            /// <param name="index">被写入的字节集合，从该索引开始写入</param>
            /// <exception cref="ArgumentOutOfRangeException">类型大小超过数组写入长度</exception>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void WriteByteArray<T>(this IList<byte> bys, T obj, int index) where T : unmanaged
            {
                if(bys == null)
                {
                    throw new ArgumentNullException("bys", "字节集合为null");
                }
                //类型大小
                int size = sizeof(T);
                //集合长度
                int count = bys.Count;
                if (index + size > count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("指定初始索引或类型大小超越集合界限");
                }
                byte* vp = (byte*)&obj;
                int j = 0;
                for (int i = index; i < count && j < size; i++, j++)
                {
                    bys[i] = vp[j];
                }
            }
            /// <summary>
            /// 将非托管类型写入字节集合
            /// </summary>
            /// <typeparam name="T">非托管类型</typeparam>
            /// <param name="obj">要写入的数据</param>
            /// <param name="bys">被写入的字节集合</param>
            /// <exception cref="ArgumentOutOfRangeException">类型大小超过数组写入长度</exception>
            /// <exception cref="ArgumentNullException">字节数组为null</exception>
            public static void WriteByteArray<T>(this IList<byte> bys, T obj) where T : unmanaged
            {
                WriteByteArray(bys, obj, 0);
            }

            /// <summary>
            /// 将内存块拷贝到另一个内存块当中；此API不兼容CLS
            /// </summary>
            /// <param name="copyAddress">需要拷贝的内存首地址</param>
            /// <param name="toAddress">考拷贝到的内存首地址</param>
            /// <param name="size">内存块字节大小</param>
            public static void MemoryCopy(this IntPtr copyAddress, IntPtr toAddress, uint size)
            {
                Buffer.MemoryCopy(copyAddress.ToPointer(), toAddress.ToPointer(), size, size);
            }
            /// <summary>
            /// 将两个内存块内的数据交换；此API不兼容CLS
            /// </summary>
            /// <param name="address1">内存1</param>
            /// <param name="address2">内存2</param>
            /// <param name="size">内存块大小</param>
            public static void MemorySwap(this IntPtr address1, IntPtr address2, ushort size)
            {
                //开临时栈
                byte* temp = stackalloc byte[(int)size];
                void* a1 = address1.ToPointer(), a2 = address2.ToPointer();
                //交换
                Buffer.MemoryCopy(a1, temp, size, size);
                Buffer.MemoryCopy(a2, a1, size, size);
                Buffer.MemoryCopy(temp, a2, size, size);
            }
            /// <summary>
            /// 将单字节值转化为标准布尔值
            /// </summary>
            /// <param name="byt">值</param>
            /// <returns>布尔值</returns>
            public static bool ToBool(this byte byt)
            {
                if (byt == 0) return false;
                return true;
            }
            /// <summary>
            /// 将布尔值转化为通用整形值
            /// </summary>
            /// <param name="bol">布尔值</param>
            /// <returns>整形值</returns>
            public static byte ToByte(this bool bol)
            {
                if (bol) return 1;
                return 0;
            }
            #endregion

            #region 辅助工具
            /// <summary>
            /// 将集合内元素拷贝到另一个集合
            /// </summary>
            /// <param name="copy">被拷贝的集合</param>
            /// <param name="to">拷贝到的集合</param>
            /// <param name="begin">被拷贝集合的起始位置</param>
            /// <param name="size">被拷贝集合的数量</param>
            public static void ArrCopy<T>(this IList<T> copy, IList<T> to, int begin, int size)
            {
                int end = begin + size;
                for (int i = 0; i < to.Count && begin < end; i++, begin++)
                {
                    to[i] = copy[begin];
                }
            }
            /// <summary>
            /// <para>连接两个数组为一个新数组</para>
            /// <para>若有一方为null值，则直接返回另一方；若两个都为null，返回null</para>
            /// </summary>
            /// <typeparam name="T">数组类型</typeparam>
            /// <param name="arr1">前数组</param>
            /// <param name="arr2">后数组</param>
            /// <returns></returns>
            public static T[] LinkArray<T>(this T[] arr1, T[] arr2)
            {
                if(arr1 == null || arr2 == null)
                {
                    return arr1 ?? arr2;
                }
                T[] re = new T[arr1.Length + arr2.Length];
                int index = 0;

                for(int i = 0; i < arr1.Length; i++,index++)
                {
                    re[index] = arr1[i];
                }
                for (int i = 0; i < arr2.Length; i++, index++)
                {
                    re[index] = arr2[i];
                }
                return re;
            }
            /// <summary>
            /// <para>将集合中指定范围的元素存放到新数组中</para>
            /// </summary>
            /// <typeparam name="T">集合元素类型</typeparam>
            /// <param name="list">指定集合</param>
            /// <param name="beginIndex">起始下标</param>
            /// <param name="count">返回元素个数</param>
            /// <returns>指定范围的元素数组</returns>
            public static T[] DivisionArray<T>(this IList<T> list, int beginIndex, int count)
            {
                T[] re = new T[count];
                for(int i = 0; i < count; i++)
                {
                    re[i] = list[beginIndex + i];
                }
                return re;
            }
            /// <summary>
            /// <para>将集合中指定范围的元素存放到新数组中</para>
            /// </summary>
            /// <typeparam name="T">集合元素类型</typeparam>
            /// <param name="list">指定集合</param>
            /// <param name="beginIndex">起始下标</param>
            /// <param name="count">返回元素个数</param>
            /// <returns>指定范围的元素数组</returns>
            public static T[] DivisionArrayLong<T>(this T[] list, long beginIndex, long count)
            {
                T[] re = new T[count];
                for (long i = 0; i < count; i++)
                {
                    re[i] = list[beginIndex + i];
                }
                return re;
            }
            /// <summary>
            /// 对比两个字节数组中所有元素是否相等
            /// </summary>
            /// <param name="b1">集合</param>
            /// <param name="b2">要对比的集合</param>
            /// <returns>如果两个集合每个位置的值都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool ComparerEquals(this byte[] b1, byte[] b2)
            {
                if(b1 == null || b2 == null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                if(b1 == b2)
                {
                    return true;
                }
                var size = b1.LongLength;
                if (size != b2.Length)
                {
                    return false;
                }

                for (long i = 0; i < size; i++)
                {
                    if (b1[i] != b2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 对比两个集合中所有元素是否相等；使用默认比较器
            /// </summary>
            /// <typeparam name="T">集合元素</typeparam>
            /// <param name="t1">集合</param>
            /// <param name="t2">要对比的集合</param>
            /// <returns>如果两个集合每个位置的元素都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool ComparerEquals<T>(this T[] t1, T[] t2)
            {
                if (t1 is null || t2 is null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                var size = t1.LongLength;
                if (size != t2.Length)
                {
                    return false;
                }
                for (long i = 0; i < size; i++)
                {
                    var ts = t1[i];
                    var ts2 = t2[i];
                    if(ReferenceEquals(ts,null))
                    {
                        if (ReferenceEquals(ts2, null))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (!ts.Equals(ts2))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 对比两个集合中所有元素是否相等；使用自定义比较器
            /// </summary>
            /// <typeparam name="T">集合元素</typeparam>
            /// <param name="t1">集合</param>
            /// <param name="t2">要对比的集合</param>
            /// <param name="comparer">比较器</param>
            /// <returns>如果两个集合每个位置的元素都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool ComparerEquals<T>(this T[] t1, T[] t2, IEqualityComparer<T> comparer)
            {
                if (t1 is null || t2 is null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                var size = t1.LongLength;
                if (size != t2.Length)
                {
                    return false;
                }
                for (long i = 0; i < size; i++)
                {
                    var ts = t1[i];
                    var ts2 = t2[i];
                    if (!comparer.Equals(ts,ts2))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 对比两个集合中所有元素是否相等；使用默认比较器接口
            /// </summary>
            /// <typeparam name="T">集合元素</typeparam>
            /// <param name="t1">集合</param>
            /// <param name="t2">要对比的集合</param>
            /// <returns>如果两个集合每个位置的元素都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool Comparer<T>(this T[] t1, T[] t2) where T : IEquatable<T>
            {
                if (t1 is null || t2 is null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                if (t1 == t2) return true;
                var size = t1.LongLength;
                if (size != t2.Length)
                {
                    return false;
                }
                for (long i = 0; i < size; i++)
                {
                    var ts = t1[i];
                    var ts2 = t2[i];
                    if (ReferenceEquals(ts, null))
                    {
                        if (ReferenceEquals(ts2, null))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (!ts.Equals(ts2))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 对比两个集合中所有元素是否相等；使用默认比较器
            /// </summary>
            /// <typeparam name="T">集合元素</typeparam>
            /// <param name="t1">集合</param>
            /// <param name="t2">要对比的集合</param>
            /// <returns>如果两个集合每个位置的元素都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool ComparerEquals<T>(this IList<T> t1, IList<T> t2)
            {
                if (t1 is null || t2 is null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                var size = t1.Count;
                if (size != t2.Count)
                {
                    return false;
                }
                for (int i = 0; i < size; i++)
                {
                    var ts = t1[i];
                    var ts2 = t2[i];
                    if (ReferenceEquals(ts, null))
                    {
                        if (ReferenceEquals(ts2, null))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (!ts.Equals(ts2))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 对比两个集合中所有元素是否相等；使用自定义比较器
            /// </summary>
            /// <typeparam name="T">集合元素</typeparam>
            /// <param name="t1">集合</param>
            /// <param name="t2">要对比的集合</param>
            /// <param name="comparer">比较器</param>
            /// <returns>如果两个集合每个位置的元素都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool ComparerEquals<T>(this IList<T> t1, IList<T> t2, IEqualityComparer<T> comparer)
            {
                if (t1 is null || t2 is null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                var size = t1.Count;
                if (size != t2.Count)
                {
                    return false;
                }
                for (int i = 0; i < size; i++)
                {
                    if (!comparer.Equals(t1[i], t2[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 对比两个集合中所有元素是否相等；使用默认比较器接口
            /// </summary>
            /// <typeparam name="T">集合元素</typeparam>
            /// <param name="t1">集合</param>
            /// <param name="t2">要对比的集合</param>
            /// <returns>如果两个集合每个位置的元素都相等，返回true；否则为false</returns>
            /// <exception cref="ArgumentNullException">比较集合为null</exception>
            public static bool Comparer<T>(this IList<T> t1, IList<T> t2) where T : IEquatable<T>
            {
                if (t1 is null || t2 is null)
                {
                    throw new ArgumentNullException("比较对象为null");
                }
                var size = t1.Count;
                if (size != t2.Count)
                {
                    return false;
                }
                for (int i = 0; i < size; i++)
                {
                    var ts = t1[i];
                    var ts2 = t2[i];
                    if (ReferenceEquals(ts, null))
                    {
                        if (ReferenceEquals(ts2, null))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (!ts.Equals(ts2))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 比较两个内存块的值是否一致
            /// </summary>
            /// <param name="address1">地址</param>
            /// <param name="address2">比较的地址</param>
            /// <param name="size">内存块字节大小</param>
            /// <returns>若两块指定大小地址内的值一致返回true；否则返回false</returns>
            /// <exception cref="AccessViolationException">可能引发的异常</exception>
            /// <exception cref="NullReferenceException">可能引发的异常</exception>
            public static bool ComparerEquals(this IntPtr address1, IntPtr address2, int size)
            {
                int* p1 = (int*)address1;
                int* p2 = (int*)address2;
                int isize = size / 4;
                int ys = size % 4;
                bool y = ys != 0;
                //4字节对齐判断
                for(int i = 0; i < isize; p1++, p2++, i++)
                {
                    if (*p1 != *p2)
                    {
                        return false;
                    }
                }
                //剩余无对齐内存比较
                if (y)
                {
                    p1--;
                    p2--;
                    byte* b1 = (byte*)p1, b2 = (byte*)p2;
                    for(int i = 0; i < ys; i++)
                    {
                        if (b1[i] != b2[i])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            /// <summary>
            /// 验证当前程序的大小端存储方式
            /// </summary>
            /// <returns>返回true表示为大端存储，false表示小端存储</returns>
            public static bool LargeEndStorage
            {
                get
                {
                    //小端：低位存低地址，高位存高地址；
                    //大端：高位存低地址，低位存高地址；
                    short s = 0x1;
                    byte* p = (byte*)&s;
                    if(*p == 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            /// <summary>
            /// 比较键值对数据的键和键的数量是否相同
            /// </summary>
            /// <param name="p1">键值对集合</param>
            /// <param name="p2">键值对集合</param>
            /// <returns>若两参数的键是相同数量的相等字符串，无论顺序，返回true；否则false</returns>
            public static bool KeyEquals(this IDictionary<string, object> p1, IDictionary<string,object> p2)
            {
                if (p1 is null || p2 is null)
                {
                    throw new ArgumentNullException("pair", "参数不可为null");
                }
                if (p1 == p2)
                {
                    return true;
                }

                var k = p1.Keys;
                if (k.Count != p2.Count) return false;

                foreach (var item in k)
                {
                    if (!p2.ContainsKey(item))
                    {
                        return false;
                    }
                }
                return true;
            }

            #endregion

            #region 位域
            /// <summary>
            /// 获取值类型内存的二进制数据
            /// </summary>
            /// <typeparam name="Value">类型</typeparam>
            /// <param name="value">获取的值</param>
            /// <param name="fen">每显示一个byte的分隔符</param>
            /// <returns>使用16进制表示的内存数据</returns>
            public static string BinaryNumber<Value>(this Value value, string fen) where Value : unmanaged
            {
                //获取类型大小
                int size = sizeof(Value);
                StringBuilder str = new StringBuilder();
                
                //读取内存头
                byte* bp = (byte*)&value;
                //获取每一位的byte
                int cou = size - 1;
                for (int index = 0; index <= cou; index++)
                {
                    byte b = bp[index]; //获取本位byte
                    str.Append(b.ToString("x").ToUpper());
                    if (index < cou)
                    {
                        str.Append(fen);//分隔符
                    }
                }
                return str.ToString();
            }
            /// <summary>
            /// 获取指定内存的二进制数据
            /// </summary>
            /// <param name="ptr">内存首地址</param>
            /// <param name="size">读取的字节长度</param>
            /// <param name="fen">每显示一个byte的分隔符</param>
            /// <returns>使用16进制表示的内存数据</returns>
            public static string BinaryNumber(this IntPtr ptr, uint size, string fen)
            {
                StringBuilder str = new StringBuilder();
                
                byte* bp = (byte*)ptr;
                //获取每一位的byte
                uint cou = size - 1;
                for (uint index = 0; index <= cou; index++)
                {
                    byte b = bp[index];//获取本位byte
                    str.Append(b.ToString("x").ToUpper());

                    if (index < cou)
                    {
                        str.Append(fen);//分隔符
                    }
                }
                return str.ToString();
            }
            /// <summary>
            /// 获取指定内存的二进制数据
            /// </summary>
            /// <param name="ptr">内存首地址</param>
            /// <param name="size">读取的字节长度</param>
            /// <param name="format">每一个字节值的字符串表现格式</param>
            /// <param name="fen">每显示一个byte的分隔符</param>
            /// <returns>值的内存数据</returns>
            /// <exception cref="FormatException">值类型字符串输出格式错误</exception>
            public static string BinaryNumber(this IntPtr ptr, uint size, string format, string fen)
            {
                StringBuilder str = new StringBuilder();

                byte* bp = (byte*)ptr;
                //获取每一位的byte
                uint cou = size - 1;
                for (int index = 0; index <= cou; index++)
                {
                    byte b = bp[index];//获取本位byte
                    str.Append(b.ToString(format));

                    if (index < cou)
                    {
                        str.Append(fen);//分隔符
                    }
                }
                return str.ToString();
            }
            /// <summary>
            /// 获取指定值的二进制数据
            /// </summary>
            /// <typeparam name="Value">值类型</typeparam>
            /// <param name="value">值</param>
            /// <param name="format">值类型的字符串表现格式</param>
            /// <param name="fen">每一个byte的分隔符</param>
            /// <returns>值的内存数据</returns>
            /// <exception cref="FormatException">字符串输出格式错误</exception>
            public static string BinaryNumber<Value>(this Value value, string format, string fen) where Value : unmanaged
            {
                uint size = (uint)sizeof(Value);
                return BinaryNumber(new IntPtr(&value), size, format, fen);
            }
            /// <summary>
            /// 获取指定内存的二进制数据
            /// </summary>
            /// <param name="ptr">内存首地址</param>
            /// <param name="size">读取的字节长度</param>
            /// <param name="byteToStringFunc">每一个byte的字符串转化方法</param>
            /// <param name="fen">每显示一个byte的分隔符</param>
            /// <returns>值的内存数据</returns>
            /// <exception cref="ArgumentNullException">参数为null</exception>
            public static string BinaryNumber(this IntPtr ptr, uint size, Func<byte,string> byteToStringFunc, string fen)
            {
                if (byteToStringFunc == null) throw new ArgumentNullException(nameof(byteToStringFunc), "参数为null");
                StringBuilder str = new StringBuilder();

                byte* bp = (byte*)ptr;
                //获取每一位的byte
                uint cou = size - 1;
                for (uint index = 0; index <= cou; index++)
                {
                    byte b = bp[index];//获取本位byte

                    str.Append(byteToStringFunc.Invoke(b));

                    if (index < cou)
                    {
                        str.Append(fen);//分隔符
                    }
                }
                return str.ToString();
            }
            /// <summary>
            /// 获取指定值的二进制数据
            /// </summary>
            /// <param name="value">值</param>
            /// <param name="byteToStringFunc">每一个byte的字符串转化方法</param>
            /// <param name="fen">每显示一个byte的分隔符</param>
            /// <returns>值的内存数据</returns>
            /// <exception cref="ArgumentNullException">参数为null</exception>
            public static string BinaryNumber<Value>(this Value value, Func<byte, string> byteToStringFunc, string fen) where Value : unmanaged
            {
                uint size = (uint)sizeof(Value);
                return BinaryNumber(new IntPtr(&value), size, byteToStringFunc, fen);
            }

            /// <summary>
            /// 将二进制数据写入指定内存
            /// </summary>
            /// <param name="ptr">写入的地址</param>
            /// <param name="binaryStr">字符串表示的二进制数据，其长度要和8对齐，多余的字符将不会写入</param>
            /// <exception cref="FormatException">字符串内有0和1之外的字符</exception>
            public static void BinaryWrite(this IntPtr ptr, string binaryStr)
            {
                //获取类型大小
                int size = binaryStr.Length;
                StringBuilder binaryString = new StringBuilder(size);

                for (int si = size - 1; si >= 0; si--)
                {
                    binaryString.Append(binaryStr[si]);
                }
                //起始位置
                byte* p = (byte*)ptr;
                int byteCount = size / 8;
                if(size % 8 != 0)
                {
                    byteCount--;
                }
                char c;
                int begin;
                for(int i = 0; i < byteCount; i++)
                {
                    begin = i * 8;

                    c = binaryString[begin];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b1) != 0) p[i] -= 0b1;
                            break;
                        case '1':
                            p[i] |= 0b1;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }
                    
                    c = binaryString[begin + 1];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b10) != 0) p[i] -= 0b10;
                            break;
                        case '1':
                            p[i] |= 0b10;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }

                    c = binaryString[begin + 2];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b100) != 0) p[i] -= 0b100;
                            break;
                        case '1':
                            p[i] |= 0b100;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }

                    c = binaryString[begin + 3];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b1000) != 0)
                                p[i] -= 0b1000;
                            break;
                        case '1':
                            p[i] |= 0b1000;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }

                    c = binaryString[begin + 4];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b10000) != 0)
                                p[i] -= 0b10000;
                            break;
                        case '1':
                            p[i] |= 0b10000;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }

                    c = binaryString[begin + 5];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b100000) != 0)
                                p[i] -= 0b100000;
                            break;
                        case '1':
                            p[i] |= 0b100000;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }

                    c = binaryString[begin + 6];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b1000000) != 0)
                                p[i] -= 0b1000000;
                            break;
                        case '1':
                            p[i] |= 0b1000000;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }

                    c = binaryString[begin + 7];
                    switch (c)
                    {
                        case '0':
                            if ((p[i] & 0b10000000) != 0)
                                p[i] -= 0b10000000;
                            break;
                        case '1':
                            p[i] |= 0b10000000;
                            break;
                        default:
                            throw new FormatException("字符串内有0和1之外的字符");
                    }
                }

            }
            /// <summary>
            /// 将二进制数据写入指定非托管对象
            /// </summary>
            /// <typeparam name="Value">非托管类型</typeparam>
            /// <param name="binaryStr">字符串表示的二进制数据，其长度要和8对齐，多余的字符将不会写入</param>
            /// <param name="value">要写入的对象引用</param>
            /// <exception cref="FormatException">字符串内有0和1之外的字符</exception>
            public static void BinaryWrite<Value>(this string binaryStr, out Value value) where Value : unmanaged
            {
                Value v;
                BinaryWrite(new IntPtr(&v), binaryStr);
                value = v;
            }
            /// <summary>
            /// 写入指定地址的指定位域偏移
            /// </summary>
            /// <param name="ptr">地址</param>
            /// <param name="byteBit">地址字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位值，从0-7对应一个byte下的二进制bit位</param>
            /// <param name="value">该位值的布尔值表达形式，true表示1，false表示0</param>
            public static void BinaryWrite(this IntPtr ptr, int byteBit, byte bit, bool value)
            {
                //定位地址byte位
                byte* p = (byte*)(ptr + byteBit);
                if (value)
                {
                    switch (bit)
                    {
                        case 0:
                            *p |= b1;
                            break;
                        case 1:
                            *p |= b2;
                            break;
                        case 2:
                            *p |= b3;
                            break;
                        case 3:
                            *p |= b4;
                            break;
                        case 4:
                            *p |= b5;
                            break;
                        case 5:
                            *p |= b6;
                            break;
                        case 6:
                            *p |= b7;
                            break;
                        case 7:
                            *p |= b8;
                            break;
                        default:
                            break;
                    }
                    return;
                }
                switch (bit)
                {
                    case 0:
                        *p &= sb1;
                        break;
                    case 1:
                        *p &= sb2;
                        break;
                    case 2:
                        *p &= sb3;
                        break;
                    case 3:
                        *p &= sb4;
                        break;
                    case 4:
                        *p &= sb5;
                        break;
                    case 5:
                        *p &= sb6;
                        break;
                    case 6:
                        *p &= sb7;
                        break;
                    case 7:
                        *p &= sb8;
                        break;
                    default:
                        break;
                }
            }
            /// <summary>
            /// 读取指定地址的指定位域偏
            /// </summary>
            /// <param name="ptr">地址</param>
            /// <param name="byteBit">读取的地址字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <returns>该位下的位域值，true表示1，false表示0</returns>
            /// <exception cref="ArgumentOutOfRangeException">指定位域范围超过byte位数</exception>
            public static bool BinaryRead(this IntPtr ptr, int byteBit, byte bit)
            {
                byte* p = (byte*)(ptr + byteBit);
                switch (bit)
                {
                    case 0:
                        return (*p & b1) == 0 ? false : true;
                    case 1:
                        return (*p & b2) == 0 ? false : true;
                    case 2:
                        return (*p & b3) == 0 ? false : true;
                    case 3:
                        return (*p & b4) == 0 ? false : true;
                    case 4:
                        return (*p & b5) == 0 ? false : true;
                    case 5:
                        return (*p & b6) == 0 ? false : true;
                    case 6:
                        return (*p & b7) == 0 ? false : true;
                    case 7:
                        return (*p & b8) == 0 ? false : true;
                    default:
                        throw new ArgumentOutOfRangeException("byteBit", "指定位域范围超过byte位数");
                }
            }
            /// <summary>
            /// 写入指定值的位域
            /// </summary>
            /// <typeparam name="Value">非托管类型</typeparam>
            /// <param name="obj">值</param>
            /// <param name="byteBit">值的字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <param name="value">该位下写入的位域值，true表示1，false表示0</param>
            public static void BinaryWrite<Value>(this ref Value obj, int byteBit, byte bit, bool value) where Value : unmanaged
            {
                //结构大小
                int size = sizeof(Value);
                if(byteBit < 0 || byteBit > size)
                {
                    throw new ArgumentOutOfRangeException("byteBit", "偏移超出范围");
                }
                fixed (Value* ptr = &obj)
                {
                    byte* p = (((byte*)ptr) + byteBit);
                    if (value)
                    {
                        switch (bit)
                        {
                            case 0:
                                *p |= b1;
                                break;
                            case 1:
                                *p |= b2;
                                break;
                            case 2:
                                *p |= b3;
                                break;
                            case 3:
                                *p |= b4;
                                break;
                            case 4:
                                *p |= b5;
                                break;
                            case 5:
                                *p |= b6;
                                break;
                            case 6:
                                *p |= b7;
                                break;
                            case 7:
                                *p |= b8;
                                break;
                            default:
                                break;
                        }
                        return;
                    }
                    switch (bit)
                    {
                        case 0:
                            *p &= sb1;
                            break;
                        case 1:
                            *p &= sb2;
                            break;
                        case 2:
                            *p &= sb3;
                            break;
                        case 3:
                            *p &= sb4;
                            break;
                        case 4:
                            *p &= sb5;
                            break;
                        case 5:
                            *p &= sb6;
                            break;
                        case 6:
                            *p &= sb7;
                            break;
                        case 7:
                            *p &= sb8;
                            break;
                        default:
                            break;
                    }
                }
            }
            /// <summary>
            /// 读取指定值类型的位域
            /// </summary>
            /// <typeparam name="Value">非托管类型</typeparam>
            /// <param name="obj">值</param>
            /// <param name="byteBit">字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <returns>该位下的位域值，true表示1，false表示0</returns>
            /// <exception cref="ArgumentOutOfRangeException">指定位域范围超过byte位数</exception>
            public static bool BinaryRead<Value>(this ref Value obj, int byteBit, byte bit) where Value : unmanaged
            {
                fixed (Value* ptr = &obj)
                {
                    byte* p = (((byte*)ptr) + byteBit);
                    switch (bit)
                    {
                        case 0:
                            return (*p & b1) == 0 ? false : true;
                        case 1:
                            return (*p & b2) == 0 ? false : true;
                        case 2:
                            return (*p & b3) == 0 ? false : true;
                        case 3:
                            return (*p & b4) == 0 ? false : true;
                        case 4:
                            return (*p & b5) == 0 ? false : true;
                        case 5:
                            return (*p & b6) == 0 ? false : true;
                        case 6:
                            return (*p & b7) == 0 ? false : true;
                        case 7:
                            return (*p & b8) == 0 ? false : true;
                        default:
                            throw new ArgumentOutOfRangeException("byteBit", "指定位域范围超过byte位数");
                    }
                }              
            }
            /// <summary>
            /// 读取指定基元数组中的内存位域
            /// </summary>
            /// <param name="array">基元数组基类</param>
            /// <param name="byteBit">从索引0开始向后的字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <param name="value">该位下写入的位域值，true表示1，false表示0</param>
            /// <exception cref="ArgumentException">array不是基元</exception>
            /// <exception cref="ArgumentNullException">数组参数为null</exception>
            /// <exception cref="OverflowException">array大于2GB</exception>
            /// <exception cref="ArgumentOutOfRangeException">参数范围超出数组范围</exception>
            public static void BinaryWrite(this System.Array array, int byteBit, byte bit, bool value)
            {
                byte b = Buffer.GetByte(array, byteBit);
                if (value)
                {
                    switch (bit)
                    {
                        case 0:
                            b |= b1;
                            break;
                        case 1:
                            b |= b2;
                            break;
                        case 2:
                            b |= b3;
                            break;
                        case 3:
                            b |= b4;
                            break;
                        case 4:
                            b |= b5;
                            break;
                        case 5:
                            b |= b6;
                            break;
                        case 6:
                            b |= b7;
                            break;
                        case 7:
                            b |= b8;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (bit)
                    {
                        case 0:
                            b &= sb1;
                            break;
                        case 1:
                            b &= sb2;
                            break;
                        case 2:
                            b &= sb3;
                            break;
                        case 3:
                            b &= sb4;
                            break;
                        case 4:
                            b &= sb5;
                            break;
                        case 5:
                            b &= sb6;
                            break;
                        case 6:
                            b &= sb7;
                            break;
                        case 7:
                            b &= sb8;
                            break;
                        default:
                            break;
                    }
                }
                Buffer.SetByte(array, byteBit, b);
            }
            /// <summary>
            /// 读取指定基元数组的位域
            /// </summary>
            /// <param name="array">数组</param>
            /// <param name="byteBit">从索引0开始向后的字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <returns>该位下的位域值，true表示1，false表示0</returns>
            /// <exception cref="ArgumentOutOfRangeException">指定位域范围超过byte位数</exception>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException">数组参数为null</exception>
            /// <exception cref="OverflowException">array大于2GB</exception>
            public static bool BinaryRead(this System.Array array, int byteBit, byte bit)
            {
                byte b = Buffer.GetByte(array, byteBit);
                switch (bit)
                {
                    case 0:
                        return (b & b1) == 0 ? false : true;
                    case 1:
                        return (b & b2) == 0 ? false : true;
                    case 2:
                        return (b & b3) == 0 ? false : true;
                    case 3:
                        return (b & b4) == 0 ? false : true;
                    case 4:
                        return (b & b5) == 0 ? false : true;
                    case 5:
                        return (b & b6) == 0 ? false : true;
                    case 6:
                        return (b & b7) == 0 ? false : true;
                    case 7:
                        return (b & b8) == 0 ? false : true;
                    default:
                        throw new ArgumentOutOfRangeException("byteBit", "指定位域范围超过byte位数");
                }
            }
            /// <summary>
            /// 读取指定字节数组的位域
            /// </summary>
            /// <param name="bys">数组</param>
            /// <param name="byteBit">从索引0开始向后的字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <returns>该位下的位域值，true表示1，false表示0</returns>
            /// <exception cref="ArgumentNullException">数组参数为null</exception>
            /// <exception cref="ArgumentOutOfRangeException">参数超过范围</exception>
            public static bool BinaryRead(this byte[] bys, int byteBit, byte bit)
            {
                if (bys == null) throw new ArgumentNullException("byte[]", "参数为null");
                if (byteBit < 0 || byteBit >= bys.Length) throw new ArgumentOutOfRangeException("byteBit", "索引偏移超出数组范围");

                fixed (byte* ptr = bys)
                {
                    byte* p = ptr + byteBit;
                    switch (bit)
                    {
                        case 0:
                            return (*p & b1) == 0 ? false : true;
                        case 1:
                            return (*p & b2) == 0 ? false : true;
                        case 2:
                            return (*p & b3) == 0 ? false : true;
                        case 3:
                            return (*p & b4) == 0 ? false : true;
                        case 4:
                            return (*p & b5) == 0 ? false : true;
                        case 5:
                            return (*p & b6) == 0 ? false : true;
                        case 6:
                            return (*p & b7) == 0 ? false : true;
                        case 7:
                            return (*p & b8) == 0 ? false : true;
                        default:
                            throw new ArgumentOutOfRangeException("byteBit", "指定位域范围超过byte位数");
                    }
                }
            }
            /// <summary>
            /// 读取指定基元数组中的内存位域
            /// </summary>
            /// <param name="bys">字节数组</param>
            /// <param name="byteBit">从索引0开始向后的字节偏移</param>
            /// <param name="bit"><paramref name="byteBit"/>偏移下的位域位置，从0-7对应一个byte下的二进制bit位</param>
            /// <param name="value">该位下写入的位域值，true表示1，false表示0</param>
            /// <exception cref="ArgumentNullException">数组参数为null</exception>
            /// <exception cref="ArgumentOutOfRangeException">参数范围超出数组范围</exception>
            public static void BinaryWrite(this byte[] bys, int byteBit, byte bit, bool value)
            {
                if (bys == null) throw new ArgumentNullException("byte[]", "参数为null");
                if (byteBit < 0 || byteBit >= bys.Length) throw new ArgumentOutOfRangeException("byteBit", "索引偏移超出数组范围");

                fixed (byte* ptr = bys)
                {
                    byte* p = (ptr + byteBit);
                    if (value)
                    {
                        switch (bit)
                        {
                            case 0:
                                *p |= b1;
                                break;
                            case 1:
                                *p |= b2;
                                break;
                            case 2:
                                *p |= b3;
                                break;
                            case 3:
                                *p |= b4;
                                break;
                            case 4:
                                *p |= b5;
                                break;
                            case 5:
                                *p |= b6;
                                break;
                            case 6:
                                *p |= b7;
                                break;
                            case 7:
                                *p |= b8;
                                break;
                            default:
                                break;
                        }
                        return;
                    }
                    switch (bit)
                    {
                        case 0:
                            *p &= sb1;
                            break;
                        case 1:
                            *p &= sb2;
                            break;
                        case 2:
                            *p &= sb3;
                            break;
                        case 3:
                            *p &= sb4;
                            break;
                        case 4:
                            *p &= sb5;
                            break;
                        case 5:
                            *p &= sb6;
                            break;
                        case 6:
                            *p &= sb7;
                            break;
                        case 7:
                            *p &= sb8;
                            break;
                        default:
                            break;
                    }
                }
            }

            /// <summary>
            /// 将位值压缩数组转化为byte数组，按位封装
            /// </summary>
            /// <param name="array">位值数组</param>
            /// <returns>byte数组，与8bit对齐，多余bit忽略</returns>
            public unsafe static byte[] ToByteArray(this BitArray array)
            {
                if (array is null) throw new ArgumentNullException("array", "参数为null");

                //实际比特位
                int bitsize = array.Length;
                //byte位
                int bytesize = bitsize / 8;

                //与8bit对齐位+1
                int duiqSize;
                int duiqiyi = bytesize % 8;
                if (duiqiyi == 0)
                {
                    duiqSize = bytesize;
                }
                else
                {
                    duiqSize = bytesize + 1;
                }
                byte[] arr = new byte[bytesize];

                fixed (byte* p = arr)
                {
                    int i;
                    byte* pb = p;
                    byte bt;
                    for (i = 0; i < bytesize; i++)
                    {
                        bt = 0;
                        bt |= array[(i * 8)] ? b1 : b0;
                        bt |= array[(i * 8) + 1] ? b2 : b0;
                        bt |= array[(i * 8) + 2] ? b3 : b0;
                        bt |= array[(i * 8) + 3] ? b4 : b0;

                        bt |= array[(i * 8) + 4] ? b5 : b0;
                        bt |= array[(i * 8) + 5] ? b6 : b0;
                        bt |= array[(i * 8) + 6] ? b7 : b0;
                        bt |= array[(i * 8) + 7] ? b8 : b0;
                        p[i] = bt;
                    }

                }

                return arr;
            }

            #region 常量
            const byte b1 = 0b0000_0001;
            const byte b2 = 0b0000_0010;
            const byte b3 = 0b0000_0100;
            const byte b4 = 0b0000_1000;
            const byte b5 = 0b0001_0000;
            const byte b6 = 0b0010_0000;
            const byte b7 = 0b0100_0000;
            const byte b8 = 0b1000_0000;
            const byte b0 = 0;
            const byte ball = 0b11111111;
            const byte sb1 = 0b1111_1110;
            const byte sb2 = 0b1111_1101;
            const byte sb3 = 0b1111_1011;
            const byte sb4 = 0b1111_0111;
            const byte sb5 = 0b1110_1111;
            const byte sb6 = 0b1101_1111;
            const byte sb7 = 0b1011_1111;
            const byte sb8 = 0b0111_1111;
            #endregion

            #endregion

            #region 字符串
            /// <summary>
            /// 删除字符串中所有指定的字符
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="cs">要删除的字符集合</param>
            /// <returns>删除指定字符集合后的字符串；若字符串是一个null引用则返回null</returns>
            public static string RemoveChars(this string str, params char[] cs)
            {
                if (str == null || str == "") return str;

                StringBuilder re = new StringBuilder(str);
                int size = re.Length;
                char c;
                int csize = cs.Length;
                for(int ci = 0; ci < csize; ci++)
                {
                    c = cs[ci];
                    for (int i = 0; i < size; )
                    {
                        if(re[i] == c)
                        {
                            re.Remove(i, 1);
                            size--;
                            continue;
                        }
                        i++;
                    }
                }
                return re.ToString();
            }

            /// <summary>
            /// 截取字符串
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="index">起始索引</param>
            /// <param name="count">截取的字符数量</param>
            /// <returns>截取的字符串</returns>
            /// <exception cref="ArgumentNullException">字符串为null</exception>
            /// <exception cref="ArgumentOutOfRangeException">索引截取范围超出字符串长度</exception>
            public static string Intercept(this string str, int index, int count)
            {
                if (str == null)
                {
                    throw new ArgumentNullException("str", "字符串为null");
                }
                if (index < 0 || count < 0 || index + count >= str.Length)
                {
                    throw new ArgumentOutOfRangeException("index", "索引截取范围超过字符产范围");
                }
                //声明截取数组
                char[] cs = new char[count];
                int arrIndex = 0;
                for (int i = index; arrIndex < count; arrIndex++, i++)
                {
                    cs[arrIndex] = str[i];
                }
                return new string(cs);
            }
            #endregion
        }

        namespace Presets
        {
            /// <summary>
            /// <para>字节流转化器预设--可序列化对象转字节流方法</para>
            /// <para>使可序列化的对象存入键值对集合<see cref="PairToByteStream"/>的方法</para>
            /// </summary>
            public class SerializableToByteStream : IObjToByteStream<object>
            {
                /// <summary>
                /// 使用给定的序列化代理选择器初始化
                /// </summary>
                /// <param name="selector"></param>
                /// <param name="context"></param>
                public SerializableToByteStream(ISurrogateSelector selector, StreamingContext context)
                {
                    bin = new BinaryFormatter(selector, context);
                }
                /// <summary>
                /// 默认序列化格式
                /// </summary>
                public SerializableToByteStream()
                {
                    bin = new BinaryFormatter();
                }
                private readonly BinaryFormatter bin;
                /// <summary>
                /// 将二进制数据反序列化读取对象
                /// </summary>
                /// <param name="byteStream">流的二进制数据</param>
                /// <returns>反序列化的对象</returns>
                public object ToObj(byte[] byteStream)
                {
                    MemoryStream mer = new MemoryStream(byteStream);
                    return bin.Deserialize(mer);
                }
                /// <summary>
                /// 将对象序列化后的字节流创建字节数组
                /// </summary>
                /// <param name="obj">对象</param>
                /// <returns>字节数组</returns>
                /// <exception cref="ArgumentNullException">对象为null</exception>
                /// <exception cref="SerializationException">对象无法序列化</exception>
                /// <exception cref="System.Security.SecurityException">调用方没有所需权限</exception>
                public byte[] ToByteStream(object obj)
                {
                    MemoryStream mer = new MemoryStream();
                    bin.Serialize(mer, obj);
                    return mer.ToArray();
                }
            }

        }

    }
    
    #region 扩展
    namespace Extend
    {

        /// <summary>
        /// <para>文件操作扩展</para>
        /// <para>该类封装了<see cref="FileStream"/>类的文件操作方法，使其变得更加便利和简单</para>
        /// </summary>
        public static class SmoothFileOperation
        {

            #region 读写操作
            /// <summary>
            /// <para>读取文件流当前位置开始的所有字节流数据</para>
            /// <para>该文件流必须拥有读取权限</para>
            /// </summary>
            /// <param name="file">文件流</param>
            /// <returns>文件的所有数据</returns>
            public static byte[] FileReadAll(this System.IO.Stream file)
            {
                List<byte> list = new List<byte>();
                while (true)
                {
                    byte[] b = new byte[1024];
                    int rsize = file.Read(b, 0, 1024);
                    if (rsize < 1024)
                    {
                        for (int i = 0; i < rsize; i++)
                        {
                            list.Add(b[i]);
                        }
                        break;
                    }
                    list.AddRange(b);
                }
                return list.ToArray();
            }
            /// <summary>
            /// 读取流的所有数据，使用函数枚举器
            /// </summary>
            /// <param name="file">读取的流资源</param>
            /// <param name="readSize">一次读取的字节大小</param>
            /// <returns>一个函数枚举器，每次推进返回此次读取的数据</returns>
            public static IEnumerable<byte[]> FileReadAll(this System.IO.Stream file, int readSize)
            {
                while (true)
                {
                    byte[] b = new byte[readSize];
                    int rsize = file.Read(b, 0, readSize);
                    if (rsize < readSize)
                    {
                        byte[] over = new byte[rsize];
                        for(int i = 0; i < rsize; i++)
                        {
                            over[i] = b[i];
                        }
                        yield return over;
                        break;
                    }
                    yield return b;
                }
            }

            /// <summary>
            /// 读取文件流当前位置开始的数据，指定读取字节大小
            /// </summary>
            /// <param name="file">文件流</param>
            /// <param name="size">字节大小</param>
            /// <returns>实际读取到的字节流</returns>
            public static byte[] FileRead(this System.IO.Stream file, int size)
            {
                byte[] bs = new byte[size];
                int rsize = file.Read(bs, 0, size);
                if(rsize < size)
                {
                    return bs.DivisionArray(0, rsize);
                }
                return bs;
            }
            /// <summary>
            /// 将字节流数据全部写入文件流
            /// </summary>
            /// <param name="file">文件流</param>
            /// <param name="data">写入的字节流</param>
            public static void FileWriteAll(this System.IO.Stream file, byte[] data)
            {
                long offset = 0;
                long size = data.LongLength;
                while (true)
                {
                    byte[] temp = new byte[1024];
                    int rsize = file.Read(temp, 0, 1024);
                    for (long i = 0; i < rsize && offset < size; i++, offset++)
                    {
                        data[offset] = temp[i];
                    }
                    if (rsize < 1024 || offset >= size)
                    {
                        return;
                    }
                }

            }
            #endregion

            #region 打开和创建文件

            /// <summary>
            /// <para>确认文件是否存在</para>
            /// <para>若目录不存在，返回0</para>
            /// <para>若存在目录，但不存在文件，返回1</para>
            /// <para>若存在文件，返回2</para>
            /// </summary>
            /// <param name="path">判断的路径</param>
            /// <returns>返回判断值</returns>
            public static int FileExists(string path)
            {
                if(path == null || path == string.Empty)
                {
                    return 0;
                }
                string ml = Path.GetDirectoryName(path);
                if (ml == string.Empty)
                {
                    return 0;
                }
                if (!Directory.Exists(ml))
                {
                    return 0;
                }
                if (File.Exists(path))
                {
                    return 2;
                }
                return 1;
            }
            /// <summary>
            /// <para>在指定目录使用读写权限打开已有文件</para>
            /// <para>指定文件目录不存在，则返回false，参数<paramref name="file"/>设为null</para>
            /// <para>当目录存在时，返回值为true；</para>
            /// <para>只有指定文件存在目录当中，参数<paramref name="file"/>才会被实例化，否则为null</para>
            /// </summary>
            /// <param name="path">文件路径和名称；可使用绝对路径和相对路径。</param>
            /// <param name="file">接收文件流的引用；如果为null，则文件不存在</param>
            /// <returns>返回值为false时，指定目录不存在；true表示目录存在</returns>
            public static bool OpenFiles(string path, out FileStream file)
            {
                string fileDirectory = Path.GetDirectoryName(path);

                if (!(Directory.Exists(fileDirectory) || fileDirectory == string.Empty))
                {
                    file = null;
                    return false;
                }
                if (!File.Exists(path))
                {
                    file = null;
                    return true;
                }
                file = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                return true;
            }
            /// <summary>
            /// <para>在指定目录打开已有文件,指定访问权限</para>
            /// <para>该重载功能一致</para>
            /// </summary>
            /// <param name="path">文件路径和名称</param>
            /// <param name="file">接收文件流的引用；如果为null，则文件不存在</param>
            /// <param name="access">文件读写权限</param>
            /// <returns>返回值为false时，指定目录不存在；true表示目录存在</returns>
            public static bool OpenFiles(string path, out FileStream file, FileAccess access)
            {
                string fileDirectory = Path.GetDirectoryName(path);
                if (!(Directory.Exists(fileDirectory) || fileDirectory == string.Empty))
                {
                    file = null;
                    return false;
                }
                if (!File.Exists(path))
                {
                    file = null;
                    return true;
                }
                file = File.Open(path, FileMode.Open, access);
                return true;
            }

            /// <summary>
            /// <para>在指定目录创建一个文件并使用读写权限打开</para>
            /// </summary>
            /// <param name="path">创建的文件所在目录</param>
            /// <param name="file">创建文件后接收的文件流</param>
            /// <returns>
            /// <para>若不存在目录，则会创建目录和新的文件；返回0</para>
            /// <para>若指定目录中不存在文件，则创建新文件，返回1</para>
            /// <para>若指定目录存在文件，则会覆盖为新的空文件；并返回2</para>
            /// </returns>
            public static int CreatDirFiles(string path, out FileStream file)
            {
                string fileDirectory = Path.GetDirectoryName(path);
                int re;
                if (Directory.Exists(fileDirectory) || fileDirectory == string.Empty)
                {
                    re = 1;
                }
                else
                {
                    re = 0;
                }
                if (File.Exists(path))
                {
                    file = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
                    re = 2;
                }
                else
                {
                    file = File.Open(path, FileMode.CreateNew, FileAccess.ReadWrite);

                }
                return re;
            }
            /// <summary>
            /// <para>在指定目录创建一个文件并使用指定权限打开</para>
            /// <para>自定义权限的重载函数</para>
            /// </summary>
            /// <param name="path">创建的文件所在目录</param>
            /// <param name="file">创建文件后接收的文件流</param>
            /// <param name="access">指定权限</param>
            /// <returns></returns>
            public static int CreatDirFiles(string path, out FileStream file, FileAccess access)
            {
                string fileDirectory = Path.GetDirectoryName(path);
                int re;
                if (Directory.Exists(fileDirectory) || fileDirectory == string.Empty)
                {
                    re = 1;
                }
                else
                {
                    re = 0;
                }
                if (File.Exists(path))
                {
                    file = File.Open(path, FileMode.Create, access);
                    re = 2;
                }
                else
                {
                    file = File.Open(path, FileMode.CreateNew, access);
                }
                return re;
            }

            /// <summary>
            /// <para>在已有的路径下创建新的文件，并以读写权限打开</para>
            /// <para>当参数<paramref name="file"/>被设为null值时，表示路径不存在</para>
            /// <para>无论文件是否存在，都将创建或覆盖一个新的空文件打开</para>
            /// </summary>
            /// <param name="path">指定路径和文件名</param>
            /// <param name="file">接收文件流的引用</param>
            /// <returns>true表示旧文件被覆盖，false表示原本路径中没有文件</returns>
            public static bool CreatFiles(string path, out FileStream file)
            {
                string fileDirectory = Path.GetDirectoryName(path);

                if (!(Directory.Exists(fileDirectory) || fileDirectory == string.Empty))
                {
                    file = null;
                    return false;
                }
                if (File.Exists(path))
                {
                    file = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
                    return true;
                }
                file = File.Open(path, FileMode.CreateNew, FileAccess.ReadWrite);
                return false;
            }
            /// <summary>
            /// <para>在指定目录使用可写权限追加文件信息，指定是否拥有读取权限</para>
            /// <para>若给定目录不存在，返回null;</para>
            /// <para>若给定目录下没有指定文件，则新建一个文件并返回流</para>
            /// <para>若给定目录下存在指定文件，则打开文件并将指针标记到文件末尾</para>
            /// <para>返回的实例默认拥有写入权限，可以手动追加读取权限</para>
            /// </summary>
            /// <param name="path">给定目录，可以为绝对目录和相对目录</param>
            /// <param name="read">是否追加读取权限，默认为false</param>
            /// <returns>打开的文件流，若目录不存在或无法打开则返回null</returns>
            public static FileStream AppendFiles(string path, bool read = false)
            {
                if(path == null || path == string.Empty)
                {
                    return null;
                }
                string fileDirectory = Path.GetDirectoryName(path);
                if (!(Directory.Exists(fileDirectory) || fileDirectory == string.Empty))
                {
                    return null;
                }
                FileAccess acc = FileAccess.Write;
                if (read)
                {
                    acc |= FileAccess.Read;
                }
                try
                {
                    return new FileStream(path, FileMode.Append, acc);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            #endregion

        }

        
    }
    #endregion

    /// <summary>
    /// <para>键值对数据转化器所引发的异常</para>
    /// </summary>
    public class PairToByteDataException : Exception
    {
        /// <summary>
        /// 联系邮箱
        /// </summary>
        public const string Email = "cheng_small@163.com";
        /// <summary>
        /// b站主页
        /// </summary>
        public const string bilibili = "https://space.bilibili.com/35747641";
        /// <summary>
        /// GitHub
        /// </summary>
        public const string Github = "https://github.com/ChengSmall/CSharp_PairStream";

        /// <summary>
        /// 初始化异常
        /// </summary>
        public PairToByteDataException()
        {
            message = "键值对数据转化器引发的异常";
            excp = null;
        }
        /// <summary>
        /// 使用指定消息初始化异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public PairToByteDataException(string message)
        {
            this.message = message;
            excp = null;
        }
        /// <summary>
        /// 使用消息和内部异常实例化
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="excp">引发异常的异常</param>
        public PairToByteDataException(string message, Exception excp)
        {
            this.message = message;
            this.excp = excp;
        }
        private Exception excp;
        /// <summary>
        /// 重写根源函数
        /// </summary>
        /// <returns></returns>
        public override Exception GetBaseException()
        {
            return excp;
        }
        /// <summary>
        /// 消息
        /// </summary>
        protected string message;
        private string link = Github;
        /// <summary>
        /// 异常信息
        /// </summary>
        public override string Message => message;
        /// <summary>
        /// 获取或设置求助链接
        /// </summary>
        public override string HelpLink
        {
            get => link;
            set => link = value;
        }
    }

    /// <summary>
    /// <see cref="PairToByteStream"/>数据可存类型
    /// </summary>
    public enum DataType : byte
    {
        /// <summary>
        /// 一个字节
        /// </summary>
        Byte = 1,
        /// <summary>
        /// 布尔值
        /// </summary>
        Bool,
        /// <summary>
        /// 16位整形
        /// </summary>
        Short,
        /// <summary>
        /// 32位整形
        /// </summary>
        Int,
        /// <summary>
        /// 64位整形
        /// </summary>
        Long,
        /// <summary>
        /// 单精度浮点类型
        /// </summary>
        Float,
        /// <summary>
        /// 双精度浮点类型
        /// </summary>
        Double,
        /// <summary>
        /// 字符类型
        /// </summary>
        Char,
        /// <summary>
        /// 字符串类型
        /// </summary>
        String,
        /// <summary>
        /// 字节数组byte[]
        /// </summary>
        ByteStream
    }
    /// <summary>
    /// <para>键值对数据流转化器</para>
    /// <para>使用<see cref="string"/>作为键，可以添加任何数据的键值对集合；键的字符数量不能超过16383</para>
    /// <para>该类封装了键值对<see cref="Dictionary{TKey, TValue}"/>集合，可以将其转换为流数据</para>
    /// </summary>
    public unsafe class PairToByteStream : IEquatable<PairToByteStream>, IDictionary<string,object>
    {
        #region 初始化
        /// <summary>
        /// <para>使用指定流数据初始化</para>
        /// <para>初始化后流的位置处于该数据末尾的后一位或结尾</para>
        /// </summary>
        /// <param name="stream">流数据</param>
        /// <exception cref="ArgumentNullException">stream参数不能为null</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        public PairToByteStream(System.IO.Stream stream)
        {
            FileRead(stream);
        }
        /// <summary>
        /// <para>使用指定流数据和键比较器初始化</para>
        /// <para>初始化后流的位置处于该数据末尾的后一位或结尾</para>
        /// </summary>
        /// <param name="stream">流数据</param>
        /// <param name="comp">键比较器</param>
        /// <exception cref="ArgumentNullException">stream参数不能为null</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        public PairToByteStream(System.IO.Stream stream, IEqualityComparer<string> comp)
        {
            hash = new Dictionary<string, object>(comp);
            CoverStream(stream);
        }

        /// <summary>
        /// 使用指定路径的文件初始化，若无法成功读取文件或文件不存在，则使用默认初始化
        /// </summary>
        /// <param name="path"></param>
        public PairToByteStream(string path)
        {
            try
            {
                using (FileStream file = new FileStream(path,FileMode.Open,FileAccess.Read))
                {
                    FileRead(file);
                }
            }
            catch (Exception)
            {
                hash = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// 使用转化完毕的字节流初始化键值对集合
        /// </summary>
        /// <exception cref="PairToByteDataException">在转化的过程中发生错误</exception>
        public PairToByteStream(byte[] byteStream)
        {
            ReadStream(byteStream);
        }
        /// <summary>
        /// 完全空构造
        /// </summary>
        /// <param name="ours">是否实例化键值对数据</param>
        internal PairToByteStream(bool ours)
        {
            if (ours)
            {
                hash = new Dictionary<string, object>();
            }
        }
        /// <summary>
        /// 默认初始化键值对实例，实例元素为空
        /// </summary>
        public PairToByteStream() 
        {
            hash = new Dictionary<string, object>();
        }
        /// <summary>
        /// <para>使用指定比较器初始化键值对</para>
        /// </summary>
        /// <param name="comp">比较器</param>
        public PairToByteStream(IEqualityComparer<string> comp)
        {
            hash = new Dictionary<string, object>(comp);
        }
        /// <summary>
        /// <para>初始化并将另一个键值对集合数据拷贝到当前实例，使用默认比较器</para>
        /// </summary>
        /// <param name="dic">键值对集合</param>
        public PairToByteStream(PairToByteStream dic)
        {
            hash = new Dictionary<string, object>(dic.hash);
            //s_char = dic.s_char;
        }
        /// <summary>
        /// <para>初始化键值对并将另一个键值对集合复制到当前实例，使用指定比较器</para>
        /// </summary>
        /// <param name="dic">键值对集合</param>
        /// <param name="comparer">自定义键比较器</param>
        public PairToByteStream(PairToByteStream dic,IEqualityComparer<string> comparer)
        {
            hash = new Dictionary<string, object>(dic.hash, comparer);
            //s_char = dic.s_char;
        }
        /// <summary>
        /// 初始化元素为空的实例，指定初始容量大小
        /// </summary>
        /// <param name="capacity">包含元素数</param>
        public PairToByteStream(int capacity)
        {
            hash = new Dictionary<string, object>(capacity);
        }
        /// <summary>
        /// 指定初始容量大小和自定义比较器
        /// </summary>
        public PairToByteStream(int capacity, IEqualityComparer<string> comparer)
        {
            hash = new Dictionary<string, object>(capacity, comparer);
        
        }
        #endregion

        #region 公用构造
        static PairToByteStream()
        {
            if (OrignalDatas.LargeEndStorage)
            {
                MemoryStorage = MemoryStorage.Large;
            }
            else
            {
                MemoryStorage = MemoryStorage.Small;
            }
        }
        #endregion

        #region 数据
        /// <summary>
        /// 当前程序的存储方式
        /// </summary>
        public static readonly MemoryStorage MemoryStorage;
        /// <summary>
        /// 内部键值对数据
        /// </summary>
        protected Dictionary<string, object> hash;
        #endregion

        #region 访问接口

        #region 数据转化

        #region 函数封装
        /// <summary>
        /// 分割字节流数组
        /// </summary>
        /// <param name="arr">要分割的数组</param>
        /// <param name="begin">起始下标</param>
        /// <param name="size">字节长度</param>
        /// <returns>分割后返回的数组</returns>
        private byte[] ToByteFenG(byte[] arr, int begin, int size)
        {
            byte[] re = new byte[size];
            Buffer.BlockCopy(arr, begin, re, 0, size);
            return re;
        }

        /// <summary>
        /// 字节流重新转化为一对键值
        /// </summary>
        /// <param name="data">转化的字节流</param>
        /// <param name="beginIndex">从该下标开始向后读取</param>
        /// <param name="key">转化后的键</param>
        /// <param name="value">转化后的值</param>
        /// <param name="fen">分隔符</param>
        /// <param name="nowIndex">终止符后的下标；如果无法读取完整返回-1</param>
        /// <returns>true代表可以完成读取，false表示数据不完整</returns>
        private bool ToPairByteStream(byte[] data, int beginIndex, out string key, out object value, out byte fen, ref int nowIndex)
        {
            int index = beginIndex; /*起始下标*/
            nowIndex = -1;
            if (data.Length < beginIndex + 5)
            {
                key = null;
                value = default;
                fen = 0;
                return false;
            }
            /*[键的字节长度] short 2*/
            short keylong = data.ToShort(index);
            index += 2;

            int strendIndex = index + keylong - 1; /*读取键的最后字节数组下标*/
            if (strendIndex >= data.Length)
            {
                key = null;
                value = default;
                fen = 0;
                return false;
            }
            /*键值 字节流 [键的长度]*/
            key = ToByteFenG(data, index, keylong).ToStringByData();
            index += keylong;

            if (index + 1 >= data.Length)
            {
                value = default;
                fen = 0;
                return false;
            }
            /*值类型 1*/
            DataType vtype = (DataType)data[index];
            index++;

            int vsize = ToSizeByType(vtype); /*值类型大小*/
            /*判断是否不可预估大小 -> 值大小 [int] 4*/
            if (vsize == 0)
            {
                if (index + 4 >= data.Length)
                {
                    value = default;
                    fen = 0;
                    return false;
                }
                vsize = ToByteFenG(data, index, 4).ToInt32();
                index += 4;
            }
            if (index + vsize >= data.Length)
            {
                value = default;
                fen = 0;
                return false;
            }

            byte[] vab = ToByteFenG(data, index, vsize); /*储存值类型的字节流*/
            /*值 [直大小]*/
            value = ToValueByType(vab, vtype);
            index += vab.Length;

            if (index >= data.Length)
            {
                fen = 0;
                return false;
            }

            fen = data[index];
            nowIndex = index+1;
            return true;
        }
        /// <summary>
        /// 文件字节流重新转化为一对键值，从当前位置开始读取一对数据
        /// </summary>
        /// <param name="file">转化的文件流</param>
        /// <param name="key">转化后的键</param>
        /// <param name="value">转化后的值</param>
        /// <param name="fen">分隔符</param>
        /// <returns>true代表可以完成读取，false表示数据不完整</returns>
        private bool ToPairByteStream(System.IO.Stream file, out string key, out object value, out byte fen)
        {
            /*[键的字节长度] short 2*/
            byte[] shb = new byte[2];
            if(file.Read(shb, 0, 2) < 2)
            {
                key = null;
                value = default;
                fen = 0;
                return false;
            }
            short keylong = shb.ToShort();


            /*键值 字节流 [键的长度]*/
            byte[] keybyte = new byte[keylong];
            if(file.Read(keybyte, 0, keylong) < keylong)
            {
                key = null;
                value = default;
                fen = 0;
                return false;
            }
            key = ToByteFenG(keybyte, 0, keylong).ToStringByData();

            /*值类型 1*/
            byte[] typedata = new byte[1];
            if(file.Read(typedata, 0, 1) == 0)
            {
                value = default;
                fen = 0;
                return false;
            }
            DataType vtype = (DataType)typedata[0];

            int vsize = ToSizeByType(vtype); /*值类型大小*/
            /*判断是否不可预估大小 -> 值大小 [int] 4*/
            if (vsize == 0)
            {
                byte[] Int32b = new byte[4];
                if(file.Read(Int32b, 0, 4) < 4)
                {
                    value = default;
                    fen = 0;
                    return false;
                }
                vsize = Int32b.ToInt32();
            }

            byte[] vab = new byte[vsize]; /*储存值类型的字节流*/
            if(file.Read(vab, 0, vsize) < vsize)
            {
                value = default;
                fen = 0;
                return false;
            }

            /*转化值 */
            value = ToValueByType(vab, vtype);

            byte[] fenb = new byte[1];
            if(file.Read(fenb, 0, 1) == 0)
            {
                fen = 0;
                return false;
            }
            fen = fenb[0];
            return true;
        }

        /// <summary>
        /// 反字节流转化，给定正确长度字节流和类型，返回相应的实例
        /// </summary>
        /// <param name="obj">数据</param>
        /// <param name="type">类型</param>
        /// <returns>值</returns>
        private unsafe object ToValueByType(byte[] obj, DataType type)
        {
            switch (type)
            {
                case DataType.Byte:
                    return obj[0];
                case DataType.Bool:
                    return obj.ToBool();
                case DataType.Short:
                    return obj.ToShort();
                case DataType.Int:
                    return obj.ToInt32();
                case DataType.Long:
                    return obj.ToInt64();
                case DataType.Float:
                    return obj.ToFloat();
                case DataType.Double:
                    return obj.ToDouble();
                case DataType.Char:
                    return obj.ToCharData();
                case DataType.String:
                    return obj.ToStringByData();
                case DataType.ByteStream:
                    return obj;
                default:
                    throw new PairToByteDataException("类型不是可存类型");
            }
        }

        /// <summary>
        /// 输入类型，返回大小；如果类型不可预估，返回0
        /// </summary>
        /// <returns>字节大小</returns>
        int ToSizeByType(DataType type)
        {
            switch (type)
            {
                case DataType.Byte:
                    return 1;
                case DataType.Bool:
                    return sizeof(bool);
                case DataType.Short:
                    return 2;
                case DataType.Int:
                    return 4;
                case DataType.Long:
                    return 8;
                case DataType.Float:
                    return 4;
                case DataType.Double:
                    return 8;
                case DataType.Char:
                    return sizeof(char);
                case DataType.String:
                    return 0;
                case DataType.ByteStream:
                    return 0;
                default:
                    throw new PairToByteDataException("类型枚举错误");
            }
        }

        /// <summary>
        /// 将类型和集合传入，根据类型向后写值
        /// </summary>
        /// <param name="list">待写集合</param>
        /// <param name="type">类型</param>
        /// <param name="value">转化写入的值</param>
        unsafe void ValueWrite(ref List<byte> list, DataType type, ref object value)
        {
            switch (type)
            {
                case DataType.Byte:
                    list.Add((byte)value);
                    return;
                case DataType.Bool:
                    list.AddRange(((bool)value).ToByteArray());
                    return;
                case DataType.Short:
                    list.AddRange(((short)value).ToByteArray());
                    return;
                case DataType.Int:
                    list.AddRange(((int)value).ToByteArray());
                    return;
                case DataType.Long:
                    list.AddRange(((long)value).ToByteArray());
                    return;
                case DataType.Float:
                    list.AddRange(((float)value).ToByteArray());
                    return;
                case DataType.Double:
                    list.AddRange(((double)value).ToByteArray());
                    return;
                case DataType.Char:
                    char c = (char)value;
                    list.AddRange(c.ToByteArray());
                    return;
                case DataType.String:
                    list.AddRange(((string)value).ToByteData());
                    return;
                case DataType.ByteStream:
                    list.AddRange((byte[])value);
                    return;
                default:
                    throw new Exception("类型错误");
            }
        }

        /// <summary>
        /// 判断值得类型和值字节大小
        /// </summary>
        /// <param name="obj">值</param>
        /// <param name="size">大小</param>
        /// <returns></returns>
        private DataType IsObjType(object obj, out int size)
        {
            if (obj is byte)
            {
                size = 1;
                return DataType.Byte;
            }
            else if (obj is bool)
            {
                size = 1;
                return DataType.Bool;
            }
            else if (obj is int)
            {
                size = sizeof(int);
                return DataType.Int;
            }
            else if (obj is short)
            {
                size = sizeof(short);
                return DataType.Short;
            }
            else if (obj is long)
            {
                size = sizeof(long);
                return DataType.Long;
            }
            else if (obj is float)
            {
                size = sizeof(float);
                return DataType.Float;
            }
            else if (obj is double)
            {
                size = sizeof(double);
                return DataType.Double;
            }
            else if (obj is char)
            {
                size = sizeof(char);
                return DataType.Char;
            }
            else if (obj is string)
            {
                size = ((string)obj).Length * sizeof(char);
                return DataType.String;
            }
            else if (obj is byte[])
            {
                size = ((byte[])obj).Length;
                return DataType.ByteStream;
            }
            throw new PairToByteDataException("存入了无法保存的数据");
        }

        /// <summary>
        /// 读取并写入键值对数据开头；并返回值判断是否有数据
        /// </summary>
        /// <param name="byteStream">包含数据开头的字节流</param>
        private bool SetPairHand(byte[] byteStream)
        {
            //s_char = byteStream[0];
            if (byteStream[1] == 1)
            {
                return true;
            }
            if(byteStream[1] != 0)
            {
                throw new PairToByteDataException("检测到数据异常");
            }
            return false;
        }
        /// <summary>
        /// 当前位置读取并写入键值对数据开头；并返回值判断是否有数据
        /// </summary>
        /// <param name="byteStream">包含数据开头的文件流</param>
        /// <exception cref="PairToByteDataException">检测到数据异常</exception>
        private bool SetPairHand(System.IO.Stream byteStream)
        {
            byte[] fen = new byte[1];
            byteStream.Read(fen, 0, 1);
            if(fen[0] == 1)
            {
                return true;
            }
            if(fen[0] != 0)
            {
                throw new PairToByteDataException("检测到数据异常");
            }
            return false;
        }
        /// <summary>
        /// 向键值对添加字节流转化数据，添加过程中遇到相等的键则忽略，对齐一个分隔符后端位置
        /// </summary>
        /// <param name="byteStream">数据</param>
        /// <param name="index">从指定下标开始添加数据；</param>
        /// <returns>如果数据完整并成功写入键值对集合，返回null；如果转化过程中遇见无法转化成一对的字节流，则返回当前确实数据的字节流</returns>
        private byte[] SetPairByStream(byte[] byteStream, int index)
        {
            /*开始循环读取*/
            while (index < byteStream.Length)
            {
                string key;
                object value;

                byte fen;
                int endindex = index;
                bool ib = ToPairByteStream(byteStream, index, out key, out value, out fen, ref endindex);

                if (!ib)
                {
                    return ToByteFenG(byteStream,index,byteStream.Length - index);
                }
                if (!hash.ContainsKey(key))
                {_Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
                    hash.Add(key, value);
                }                
                index = endindex;
            }
            return null;
        }
        /// <summary>
        /// 添加一个键值对文件流转化数据，添加过程中遇到相等的键则忽略，对齐一个分隔符后端位置
        /// <para>若无法添加，则文件流位置不变</para>
        /// </summary>
        /// <param name="file">文件流</param>
        /// <param name="fen">当前数据后的分隔符</param>
        /// <returns>是否添加成功</returns>
        private bool SetOnePairByStream(System.IO.Stream file, out byte fen)
        {
            string key;
            object value; /*声明键值*/
            long point = file.Position;
            bool r = ToPairByteStream(file, out key, out value, out fen);
            if (r)
            {
                if (!hash.ContainsKey(key))
                {_Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
                    hash.Add(key, value);
                }
            }
            else
            {
                file.Position = point;
            }
            return r;
        }

        /// <summary>
        /// 从当前位置的文件流读取数据，直到结束符；遇到同键不覆盖
        /// </summary>
        /// <param name="file"></param>
        private void SetPairByStream(System.IO.Stream file)
        {
            byte fen = 1;
            while (fen == 1)
            {
                SetOnePairByStream(file, out fen);
            }
            
        }

        /// <summary>
        /// 向键值对添加字节流转化数据，添加过程中遇到相等的键则覆盖前者数据，对齐一个分隔符后端位置
        /// </summary>
        /// <param name="byteStream">数据</param>
        /// <param name="index">从指定下标开始添加数据；</param>
        /// <returns>如果数据完整并成功写入键值对集合，返回null；如果转化过程中遇见无法转化成一对的字节流，则返回当前确实数据的字节流</returns>
        private byte[] SetPairByStreamOnCover(byte[] byteStream, int index)
        {
            while (index < byteStream.Length)
            {
                string key;
                object value;

                byte fen;
                int endindex = index;
                bool ib = ToPairByteStream(byteStream, index, out key, out value, out fen, ref endindex);

                if (!ib)
                {
                    return ToByteFenG(byteStream, index, byteStream.Length - index);
                }
                hash[key] = value;
                if(fen == 0)
                {
                    return null;
                }
                index = endindex;
            }
            return null;
        }
        /// <summary>
        /// 从当前位置读取数据添加一个键值对，以覆盖形式添加
        /// <para>无法添加完整数据时文件流指针不变</para>
        /// </summary>
        /// <param name="file">文件流</param>
        /// <param name="fen">数据后的分隔符</param>
        /// <returns></returns>
        private bool SetOnePairByStreamOnCover(System.IO.Stream file, out byte fen)
        {
            string key;
            object value; /*声明键值*/
            long point = file.Position;
            bool r = ToPairByteStream(file, out key, out value, out fen);
            if (r)
            {_Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
                hash[key] = value;
            }
            else
            {
                file.Position = point;
            }
            return r;
        }
        /// <summary>
        /// 从当前位置读取一对键值，覆盖已有的键；若当前实例没有读取到的键，则不替换
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fen">分隔符</param>
        /// <returns>成功</returns>
        private bool m_replaceOnePair(System.IO.Stream file, out byte fen)
        {
            string key;
            object value; /*声明键值*/
            long point = file.Position;
            bool r = ToPairByteStream(file, out key, out value, out fen);
            if (r)
            {
                if (hash.ContainsKey(key))
                {_Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
                    hash[key] = value;
                }
            }
            else
            {
                file.Position = point;
            }
            return r;
        }

        /// <summary>
        /// 从当前位置的文件流读取数据，直到结束符；同键覆盖
        /// </summary>
        /// <param name="file"></param>
        private void SetPairByStreamOnCover(System.IO.Stream file)
        {
            byte fen = 1;
            while (fen == 1)
            {
                SetOnePairByStreamOnCover(file, out fen);
            }
        }
        /// <summary>
        /// 从当前位置覆盖已有的键，知道结束符；若当前键内没有读取到的键，则不添加
        /// </summary>
        /// <param name="file"></param>
        private void m_replacePair(System.IO.Stream file)
        {
            byte fen = 1;
            while (fen == 1)
            {
                if(!m_replaceOnePair(file, out fen))
                {
                    throw new PairToByteDataException("转化时发生了无法预估的错误");
                }
            }
        }

        #endregion

        #region 数据转化接口
        /// <summary>
        /// 向键值对替换新数据
        /// <para>从当前流位置开始读取数据；不会添加新数据，仅替换键名相同的数据</para>
        /// </summary>
        /// <param name="stream">流</param>
        /// <exception cref="PairToByteDataException">转化时发生不可预估的错误</exception>
        /// <exception cref="NotSupportedException">流没有读取或写入权限</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="NotSupportedException">流没有读取权限</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public virtual void ReplaceStream(System.IO.Stream stream)
        {
            if (SetPairHand(stream))
            {
                m_replacePair(stream);
            }
        }
        /// <summary>
        /// <para>向键值对集合替换新数据，使用迭代器累加</para>
        /// <para>每次迭代添加<paramref name="count"/>对数据，功能同<see cref="ReplaceStream(System.IO.Stream)"/></para>
        /// </summary>
        /// <param name="stream">要替换的流数据</param>
        /// <param name="count">每返回一次迭代所需的添加数量</param>
        /// <returns>迭代器</returns>
        /// <exception cref="PairToByteDataException">迭代器在转化时发生不可预估的错误</exception>
        /// <exception cref="NotSupportedException">流没有读取或写入权限</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public virtual IEnumerable ReplaceStream(System.IO.Stream stream, int count)
        {
            if (SetPairHand(stream))
            {
                int cou = 0;
                byte fen = 1;
                while (fen == 1)
                {
                    if (cou >= count)
                    {
                        cou = 0;
                        yield return null;
                    }
                    bool br = m_replaceOnePair(stream, out fen);
                    if (!br)
                    {
                        throw new PairToByteDataException("转化时发生不可预估的错误");
                    }
                    cou++;
                }
            }
        }
        /// <summary>
        /// <para>向键值对集合添加新数据</para>
        /// <para>该函数不会破坏该实例原有的数据，只会在该实例的基础上增加新数据；<br/>
        /// 若字节流中出现与当前实例相同的键，则保留原有的值</para>
        /// <para>请确保添加的数据是由该类转化而成的字节流</para>
        /// </summary>
        /// <param name="stream">要添加的字节流</param>
        public virtual void AddStream(byte[] stream)
        {         
            //if (!IsCharSize(stream))
            //{
            //    SetPairHand(stream);
            //}
            //if (Isdata(stream))
            //{
            //    SetPairByStream(stream, 2);
            //}

            if (SetPairHand(stream))
            {
                SetPairByStream(stream, 2);
            }
        }
        /// <summary>
        /// <para>向键值对集合添加新数据</para>
        /// <para>该函数不会破坏该实例原有的数据，只会在该实例的基础上增加新数据；<br/>
        /// 若字节流中出现与当前实例相同的键，则保留原有的值</para>
        /// <para>请确保添加的文件是由该类写入的</para>
        /// <para>添加完毕后文件流位置会在该数据末尾的后一位</para>
        /// </summary>
        /// <param name="stream">要添加的文件流</param>
        /// <exception cref="PairToByteDataException">转化时发生不可预估的错误</exception>
        /// <exception cref="NotSupportedException">流没有读取或写入权限</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public virtual void AddStream(System.IO.Stream stream)
        {
            if (SetPairHand(stream))
            {
                SetPairByStream(stream);
            }
        }
        /// <summary>
        /// <para>向键值对集合添加新数据，使用迭代器累加</para>
        /// <para>每次迭代添加<paramref name="count"/>对数据，功能同<see cref="AddStream(System.IO.Stream)"/></para>
        /// </summary>
        /// <param name="stream">要添加的流数据</param>
        /// <param name="count">每返回一次迭代所需的添加数量</param>
        /// <returns>迭代器</returns>
        /// <exception cref="PairToByteDataException">迭代器转化时发生不可预估的错误</exception>
        /// <exception cref="NotSupportedException">流没有读取或写入权限</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public virtual IEnumerable AddStream(System.IO.Stream stream, int count)
        {           
            if (SetPairHand(stream))
            {
                int cou = 0;
                byte fen = 1;
                while (fen == 1)
                {
                    if(cou >= count)
                    {
                        cou = 0;
                        yield return null;
                    }
                    bool br = SetOnePairByStream(stream, out fen);
                    if (!br)
                    {
                        throw new PairToByteDataException("转化时发生无法预估的错误");
                    }
                    cou++;
                }               
            }        
        }
        /// <summary>
        /// <para>使用新的字节流数据覆盖原数据</para>
        /// <para>该函数将字节流作为新的数据，以覆盖的形加入该实例；<br/>
        /// 若字节流数据存在与当前实例相同的键，则将对应的值覆盖；否则添加为新值</para>
        /// <para>请确保添加的数据是由该类转化而成的字节流</para>
        /// </summary>
        /// <param name="stream">待覆盖的新字节流数据</param>
        /// <exception cref="PairToByteDataException">转化时发生不可预估的错误</exception>
        public virtual void CoverStream(byte[] stream)
        {
            if (SetPairHand(stream))
            {
                SetPairByStreamOnCover(stream, 2);
            }
        }
        /// <summary>
        /// <para>使用新的文件数据覆盖原数据</para>
        /// <para>该函数将文件作为新的数据，以覆盖的形加入该实例；<br/>
        /// 若文件存在与当前实例相同的键，则将对应的值覆盖；否则添加为新值</para>
        /// <para>请确保添加的数据是由该类转化而成的字节流</para>
        /// <para>添加完毕后文件流位置会在该数据末尾的后一位</para>
        /// </summary>
        /// <param name="stream">待覆盖的新字节流数据</param>
        /// <exception cref="PairToByteDataException">转化时发生不可预估的错误</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="NotSupportedException">流没有读取和写入权限</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public virtual void CoverStream(System.IO.Stream stream)
        {
            if (SetPairHand(stream))
            {
                SetPairByStreamOnCover(stream);
            }
        }
        /// <summary>
        /// 迭代器覆盖添加数据，每次迭代添加<paramref name="count"/>对数据
        /// <para>功能同<see cref="CoverStream(System.IO.Stream)"/></para>
        /// </summary>
        /// <param name="file">文件流</param>
        /// <param name="count">返回一次迭代的添加数量</param>
        /// <returns>函数迭代器</returns>
        /// <exception cref="PairToByteDataException">转化时发生不可预估的错误</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="NotSupportedException">流没有读取和写入权限</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public virtual IEnumerable CoverStream(System.IO.Stream file, int count)
        {
            if (SetPairHand(file))
            {
                int cou = 0;
                byte fen = 1;
                while(fen == 1)
                {
                    if (cou >= count)
                    {
                        cou = 0;
                        yield return null;
                    }
                    bool br = SetOnePairByStreamOnCover(file, out fen);
                    if (!br)
                    {
                        throw new PairToByteDataException("转化时发生不可预估的错误");
                    }
                }
                
            }
        }

        /// <summary>
        /// 将当前的键值对集合全部转化为字节流数据
        /// </summary>
        /// <returns>当前键值对集合的所有数据</returns>
        public byte[] ToByteStream()
        {
            List<byte> arr = new List<byte>();
            foreach (var item in ToByteStream(1024))
            {
                arr.AddRange(item);
            }
            return arr.ToArray();
        }
        /// <summary>
        /// <para>将当前的键值对集合转化为字节流数据</para>
        /// <para>使用枚举函数器，指定每次返回的最少字节数量；</para>
        /// </summary>
        /// <param name="bytes">每次返回的最少字节流</param>
        /// <returns>每次返回指定字节长度的数据</returns>
        /// <exception cref="InvalidOperationException">在转换的途中修改了键值对数据</exception>
        public virtual IEnumerable<byte[]> ToByteStream(int bytes)
        {
            #region 准备
            List<byte> barr = new List<byte>(bytes); /*待返回字节数组*/
            //barr.Add(s_char); /*先写第一位是字符大小*/
            
            Dictionary<string, object>.Enumerator Enum = hash.GetEnumerator(); /*集合枚举*/
            bool beginbbb = Enum.MoveNext(); /*换至第一位*/

            if (!beginbbb)
            {
                /*没有数据呢*/
                barr.Add(0); /*分隔符结束标志*/
                yield return barr.ToArray();
                yield break;
            }
            barr.Add(1); /*分隔符*/
            #endregion

            /*循环写入*/
            while (true) 
            {
                string key = Enum.Current.Key;

                object value = Enum.Current.Value;
                /*读取当前key和value*/

                byte[] keyByte = key.ToByteData(); /*字符串转字节流*/

                short keySize = (short)keyByte.Length; /*键的字节长度*/
                
                int vsize; /*值的字节大小*/
                DataType type = IsObjType(value,out vsize);
                /*当前值类型*/

                /*开转！*/
                barr.AddRange(keySize.ToByteArray());/*键的字节长度 2 short*/

                barr.AddRange(key.ToByteData()); /*字符串转数据流 长度是上一个短整型数据*/

                barr.Add((byte)type); /*值类型 1 byte*/

                /*遇到不可探知长度的值，开四字节空间写字节长度 int*/
                if(type == DataType.String || type == DataType.ByteStream)
                {
                    barr.AddRange(vsize.ToByteArray());
                }
                ValueWrite(ref barr, type,ref value); /*写值 值的长度根据上方类型决定*/

                /*写完一组*/

                if (!Enum.MoveNext())
                {
                    /*到头啦*/
                    barr.Add(0); /*结束标识分隔符*/
                    break;
                }
                barr.Add(1); /*分隔符，未结束标志*/
                if (barr.Count > bytes)
                {
                    yield return barr.ToArray();
                    barr.Clear();
                }
            }

            yield return barr.ToArray();
        }      

        /// <summary>
        /// <para>使用新的数据覆盖当前键值对集合</para>
        /// <para>该函数将完全清空当前键值对集合内的所有数据，并使用新的字节流重新初始化该实例</para>
        /// </summary>
        /// <param name="stream">新的数据</param>
        /// <exception cref="PairToByteDataException">转化时发生错误</exception>
        public virtual void ReadStream(byte[] stream)
        {
            hash = new Dictionary<string, object>();
            if (SetPairHand(stream))
            {
                SetPairByStreamOnCover(stream, 2);
            }
        }

        #endregion

        #endregion

        #region 访问属性
        /// <summary>
        /// 键值对数据中的每一个键，其中的字符数不得超过该值
        /// </summary>
        public const short keyMaxSize = 16383;

        /// <summary>
        /// <para>返回表示当前键值对数据的<see cref="Dictionary{TKey, TValue}"/>新实例</para>
        /// </summary>
        public Dictionary<string ,object> dictionary
        {
            get
            {
                return new Dictionary<string, object>(hash, hash.Comparer);
            }
        }

        /// <summary>
        /// 返回完全相同数据的新实例
        /// </summary>
        public PairToByteStream NewPairToByteData
        {
            get
            {
                PairToByteStream pair = new PairToByteStream(false);
                pair.hash = new Dictionary<string, object>(hash, hash.Comparer);
                //pair.s_char = s_char;
                return pair;
            }
        }

        /// <summary>
        /// 所有键的集合
        /// </summary>
        public Dictionary<string, object>.KeyCollection Keys => hash.Keys;

        /// <summary>
        /// 所有值的集合
        /// </summary>
        public Dictionary<string, object>.ValueCollection Values => hash.Values;

        /// <summary>
        /// <para>集合当中的键值对数量</para>
        /// </summary>
        public int Count => hash.Count;

        /// <summary>
        /// 获取用于确定键是否相等的比较器<see cref="IEqualityComparer{T}"/>
        /// </summary>
        public IEqualityComparer<string> Comparer
        {
            get => hash.Comparer;
        }

        /// <summary>
        /// 通过键设置或访问值
        /// </summary>
        /// <returns>键所对应的值</returns>
        /// <value>值必须是<see cref="DataType"/>枚举列表内的类型</value>
        /// <exception cref="PairToByteDataException">无法设置<see cref="DataType"/>枚举类型列表之外的类型</exception>
        /// <exception cref="ArgumentNullException">将值设置为null引用或<paramref name="key"/>为null</exception>
        /// <exception cref="KeyNotFoundException">使用了不存在的键</exception>
        public object this[string key]
        {
            get
            {
                return hash[key];
            }
            set
            {
                _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
                if(value is null)
                {
                    throw new ArgumentNullException("value","参数无法设置为null引用");
                }
                if (!IsDataType(value))
                {
                    throw new PairToByteDataException($"无法更改为{typeof(DataType).FullName}枚举类型列表之外的类型");
                }
                hash[key] = value;
            }
        }
        /// <summary>
        /// 参数类型是否可存
        /// </summary>
        /// <param name="obj">参数</param>
        /// <returns>true表示该参数是基础可存类型，false表示不是基础可存类型；如果obj为null，也将返回false，因为值不支持null值</returns>
        public static bool IsDataType(object obj)
        {
            return (obj != null) && (obj is byte[] || obj is int || obj is long || obj is string | obj is float || obj is double || obj is byte || obj is short || obj is char);
        }

        #endregion

        #region 增删查改

        #region 曾
        /// <summary>
        /// 获取指定键所在的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">获取的值引用</param>
        /// <returns>获取到返回true；若无法找到key则返回false</returns>
        /// <exception cref="ArgumentNullException">键为null</exception>
        public bool TryGetValue(string key, out object value)
        {
            try
            {
                value = hash[key];
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }
        /// <summary>
        /// 添加一对数据，数据类型必须是<see cref="DataType"/>枚举其中之一
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        /// <returns>是否添加成功，若数据<paramref name="obj"/>不是<see cref="DataType"/>枚举之一，则不做添加并返回false</returns>
        /// <exception cref="ArgumentNullException">键或值为Null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        public bool Add(string key, object obj)
        {
            DataType type;
            bool istype = IsType(obj, out type);
            if (!istype)
            {
                return false;
            }
            switch (type)
            {
                case DataType.Byte:
                    Add(key, (byte)obj);
                    break;
                case DataType.Bool:
                    Add(key, (bool)obj);
                    break;
                case DataType.Short:
                    Add(key, (short)obj);
                    break;
                case DataType.Int:
                    Add(key, (int)obj);
                    break;
                case DataType.Long:
                    Add(key, (long)obj);
                    break;
                case DataType.Float:
                    Add(key, (float)obj);
                    break;
                case DataType.Double:
                    Add(key, (double)obj);
                    break;
                case DataType.Char:
                    Add(key, (char)obj);
                    break;
                case DataType.String:
                    Add(key, (string)obj);
                    break;
                case DataType.ByteStream:
                    Add(key, (byte[])obj);
                    break;
                default:
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 添加非托管类型对象，使用封送内存块转化字节数组
        /// </summary>
        /// <typeparam name="Value">指定非托管类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        public void Add<Value>(string key, Value obj) where Value : unmanaged
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            byte[] bys = obj.ToByteArray();
            Add(key,bys);
        }
        /// <summary>
        /// <para>添加自定义对象，使用自定义字节流转换器</para>
        /// </summary>
        /// <typeparam name="Value">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        /// <param name="tobyte">字节流转换器</param>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        /// <exception cref="ArgumentNullException">引用类型为null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void Add<Value>(string key, Value obj, IObjToByteStream<Value> tobyte)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if (tobyte is null)
            {
                throw new ArgumentNullException("转化器为引用到实例");
            }
            byte[] bs;
            try
            {
                bs = tobyte.ToByteStream(obj);
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            Add(key, bs);
        }
        /// <summary>
        /// <para>添加自定义对象，派生自字节流转化器</para>
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">派生自<see cref="IObjToByteStream"/>接口的对象</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        /// <exception cref="PairToByteDataException">转化器异常</exception>
        /// <exception cref="ArgumentException">键已存在</exception>
        public void Add(string key, IObjToByteStream obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if(obj == null)
            {
                throw new ArgumentNullException("值为null");
            }
            byte[] bs;
            try
            {
                bs = obj.ToByteStream();
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            Add(key, bs);
        }
        /// <summary>
        /// <para>添加自定义对象，派生自字节流转化器</para>
        /// </summary>
        /// <typeparam name="Value">值类型派生对象</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">派生自<see cref="IObjToByteStream"/>接口的值类型对象</param>
        /// <exception cref="ArgumentNullException">key为null</exception>
        /// <exception cref="PairToByteDataException">转化器异常</exception>
        /// <exception cref="ArgumentException">键已存在</exception>
        public void AddValue<Value>(string key, Value obj) where Value : struct, IObjToByteStream
        {
            _Cheskey(key, "key为null", "键的字符数超过16383");
            byte[] bs;
            try
            {
                bs = obj.ToByteStream();
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            Add(key, bs);
        }
        /// <summary>
        /// 添加一个键值对数据，转化为字节流储存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="pair">值</param>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        /// <exception cref="ArgumentNullException">引用类型为null</exception>
        public void Add(string key, PairToByteStream pair)
        {_Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if(pair is null)
            {
                throw new ArgumentNullException("值为null");
            }
            byte[] bs = pair.ToByteStream();
            Add(key, bs);
        }

        /// <summary>
        /// <para>添加一个键值对集合</para>
        /// <para>如果参数<paramref name="pair"/>中的键与当前实例相同，则不再添加这对键值</para>
        /// </summary>
        /// <param name="pair">另一个键值对数据</param>
        /// <exception cref="ArgumentNullException">参数<paramref name="pair"/>未引用到指定实例</exception>
        public void Add(PairToByteStream pair)
        {
            if(pair is null)
            {
                throw new ArgumentNullException("pair","参数不能为null");
            }
            using (var en = pair.hash.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    var item = en.Current;
                    if (!hash.ContainsKey(item.Key))
                    {
                        hash.Add(item.Key, item.Value);
                    }
                }
            }
        }
        /// <summary>
        /// 写入字节流
        /// <para>函数<see cref="Add{Value}(string, Value)"/>和<see cref="Add{Value}(string, Value, IObjToByteStream{Value})"/>储存的最终类型和该函数相同</para>
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="bys">字节流数组，该数组元素个数不得超过<see cref="int.MaxValue"/></param>
        /// <exception cref="ArgumentNullException">键或值为Null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        /// <exception cref="ArgumentOutOfRangeException">字节流数据长度过大</exception>
        public void Add(string key, byte[] bys)
        {
            _Cheskey(key, "key为null", "键的字符数量超过16383");
            if (bys is null)
            {
                throw new ArgumentNullException("byte[]", "参数不能为Null");
            }
            if (bys.LongLength > 2147483647)
            {
                throw new ArgumentOutOfRangeException("bys", "字节流数据长度过大");
            }
            hash.Add(key, bys);
        }
        /// <summary>
        /// 添加整型值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, int obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加长整形
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, long obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加字节数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, byte obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加短整型
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, short obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加浮点型
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        public void Add(string key, float obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加双浮点
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, double obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加布尔值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, bool obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加字符类型
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        public void Add(string key, char obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash.Add(key, obj);
        }
        /// <summary>
        /// 添加字符串
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">写入的值</param>
        /// <exception cref="PairToByteDataException">键的字符数不得大于2048</exception>
        /// <exception cref="ArgumentNullException">键和参数不能为Null</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        public void Add(string key, string obj)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if (obj is null)
            {
                throw new ArgumentNullException("obj","字符串参数不能为Null");
            }
            hash.Add(key, obj);
        }
        #endregion

        #region 删
        /// <summary>
        /// 移除当前实例内所有数据
        /// </summary>
        public void Clear()
        {
            hash.Clear();
        }
        /// <summary>
        /// <para>移除一对指定的键值</para>
        /// </summary>
        /// <param name="key">指定键</param>
        /// <returns>true表示移除成功；false表示不存在指定键，无法移除</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public bool Remove(string key)
        {         
            return hash.Remove(key);
        }
        /// <summary>
        /// 移除指定键集合所对应的所有值，如果集合内的键没有包含在键值对集合中，则跳过
        /// </summary>
        /// <param name="keys">键的集合</param>
        /// <exception cref="ArgumentNullException"><paramref name="keys"/>为null或其中有元素为null</exception>
        public void RemoveList(IEnumerable<string> keys)
        {
            foreach (var item in keys)
            {
                hash.Remove(item);
            }
        }
        /// <summary>
        /// 移除指定键数组所对应的所有值，如果数组内的键没有包含在键值对集合中，则跳过
        /// </summary>
        /// <param name="keys">键的集合</param>
        /// <exception cref="ArgumentNullException"><paramref name="keys"/>为null或有元素为null</exception>
        public void RemoveList(params string[] keys)
        {
            int size = keys.Length;
            for(int i = 0; i < size; i++)
            {
                hash.Remove(keys[i]);
            }
        }
        #endregion

        #region 查
        /// <summary>
        /// 使用非托管类型读取字节流数据
        /// </summary>
        /// <typeparam name="Value">读取的封装类型</typeparam>
        /// <param name="key">指定的键</param>
        /// <returns>键所对应的值</returns>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="ArgumentException">字节数组与读取类型大小不匹配</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public Value AtStruck<Value>(string key) where Value : unmanaged
        {
            byte[] bys = ByteStream(key);
            int size = sizeof(Value);
            int bsize = bys.Length;
            if(size != bsize)
            {
                throw new ArgumentException("字节数组与读取类型大小不匹配");
            }
            return bys.ToStruct<Value>();
        }
        /// <summary>
        /// <para>读取自定义类型对象，使用字节流转化器</para>
        /// </summary>
        /// <typeparam name="Value">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="tobyte">字节流转化器</param>
        /// <returns>值</returns>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>或<paramref name="tobyte"/>为null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public Value At<Value>(string key, IObjToByteStream<Value> tobyte)
        {
            if (tobyte is null)
            {
                throw new ArgumentNullException("转化器为引用到实例");
            }
            var bys = ByteStream(key);
            try
            {
                return tobyte.ToObj(bys);
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
        }
        /// <summary>
        /// 使用已有的引用类型读取自定义对象
        /// </summary>
        /// <typeparam name="Ref">指定引用的类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">用于接收数据的对象</param>
        /// <param name="tobyte">字节流转化器</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void At<Ref>(string key, ref Ref obj, IReferenceTypeToByteStream<Ref> tobyte)
        {
            if (tobyte is null)
            {
                throw new ArgumentNullException("转化器为引用到实例");
            }
            var bys = ByteStream(key);
            try
            {
                tobyte.ToObj(bys,ref obj);
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
        }
        /// <summary>
        /// 使用已有的对象读取对象
        /// </summary>
        /// <typeparam name="Ref">派生自<see cref="IObjToByteStream"/>接口的对象</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">读取数据的对象引用</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void AtValue<Ref>(string key, ref Ref obj) where Ref : IObjToByteStream
        {
            
            if (obj == null) throw new ArgumentNullException("obj", "引用是一个null，无法调用成员函数");

            var bys = ByteStream(key);
            try
            {
                obj.ToObj(bys);
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
        }

        /// <summary>
        /// 读取字节流数据并将其转化为键值对数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>使用字节流实例化的键值对数据</returns>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="PairToByteDataException">读取时发生错误</exception>
        public PairToByteStream AtPair(string key)
        {
            byte[] bs = ByteStream(key);
            return new PairToByteStream(bs);
        }
        /// <summary>
        /// 读取并拆箱为字节数组
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        public byte[] ByteStream(string key)
        {
            return (byte[])hash[key];
        }
        /// <summary>
        /// 判断指定键是否存在于集合中
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>存在true，否则false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public bool ContainsKey(string key)
        {
            return hash.ContainsKey(key);
        }
        /// <summary>
        /// 使用键访问值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>键所对应的值，若指定键不存在，则返回null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public object At(string key)
        {
            try
            {
                return hash[key];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
        /// <summary>
        /// 使用键访问值并拆箱
        /// </summary>
        /// <typeparam name="Value">要读取的数据类型</typeparam>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public Value At<Value>(string key)
        {
            return (Value)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为指定类型
        /// </summary>
        /// <typeparam name="Value">要读取的类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">接收值的对象</param>
        /// <returns>若指定键存在，则将值赋值给<paramref name="value"/>参数，并返回true；否则将值赋值为默认值，并返回false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>等于null</exception>
        /// <exception cref="InvalidCastException">该键所对应的值无法转换为指定类型<typeparamref name="Value"/></exception>
        public bool At<Value>(string key, out Value value)
        {
            object o;
            var b = hash.TryGetValue(key, out o);
            if (b)
            {
                value = (Value)o;
            }
            else
            {
                value = default;
            }
            return b;
        }
        /// <summary>
        /// 读取并拆箱为指定类型
        /// </summary>
        /// <typeparam name="Value">要读取的类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="invalidExc">在读取并将值拆箱的过程中是否引发了<see cref="InvalidCastException"/>异常</param>
        /// <param name="value">接收值的对象</param>
        /// <returns>
        /// <para>若指定键存在，则将值拆箱给<paramref name="value"/>参数，并返回true；</para>
        /// <para>若指定键不存在，则将参数<paramref name="value"/>赋默认值，返回false</para>
        /// <para>若在拆箱的过程中出现<see cref="InvalidCastException"/>异常，则将参数<paramref name="value"/>赋默认值，并返回true</para>
        /// </returns>
        public bool At<Value>(string key, out bool invalidExc, out Value value)
        {
            object o;
            var b = hash.TryGetValue(key, out o);
            if (b)
            {
                try
                {
                    value = (Value)o;
                    invalidExc = false;
                }
                catch (Exception)
                {
                    invalidExc = true;
                    value = default;
                }
            }
            else
            {
                invalidExc = false;
                value = default;
            }
            return b;

        }
        /// <summary>
        /// 读取并拆箱为整形
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public int Int(string key)
        {
            return (int)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为浮点型
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public float Float(string key)
        {
            return (float)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为双浮点
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public double Double(string key)
        {
            return (double)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为长整形
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public long Long(string key)
        {
            return (long)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为字节类型数据
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public byte Byte(string key)
        {
            return (byte)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为字符串数据
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public string String(string key)
        {
            return (string)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为布尔类型
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public bool Boolen(string key)
        {
            return (bool)hash[key];
        }
        /// <summary>
        /// 读取并拆箱为短整型
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="InvalidCastException">无效的拆箱行为</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public short Short(string key)
        {
            return (short)hash[key];
        }

        /// <summary>
        /// 获取指定键对应值的对象类型
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定对象类型</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        /// <exception cref="KeyNotFoundException">无效的<paramref name="key"/>参数</exception>
        /// <exception cref="PairToByteDataException">存入了非指定的数据</exception>
        public DataType IsTypeData(string key)
        {
            object obj = hash[key];
            Type type = obj.GetType();
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    return DataType.Bool;
                case TypeCode.Char:
                    return DataType.Char;
                case TypeCode.Byte:
                    return DataType.Byte;
                case TypeCode.Int16:
                    return DataType.Short;
                case TypeCode.Int32:
                    return DataType.Int;
                case TypeCode.Int64:
                    return DataType.Long;
                case TypeCode.Single:
                    return DataType.Float;
                case TypeCode.Double:
                    return DataType.Double;
                case TypeCode.String:
                    return DataType.String;
                default:
                    break;
            }
            if (type.IsAssignableFrom(typeof(byte[])))
            {
                return DataType.ByteStream;
            }
            throw new PairToByteDataException("存入了非指定的数据");
        }
        /// <summary>
        /// 判断数据的类型
        /// </summary>
        /// <param name="obj">判断的参数</param>
        /// <returns>该参数的可存类型；如果参数类型不是任何<see cref="DataType"/>枚举中的类型，则默认返回<see cref="DataType.ByteStream"/></returns>
        public static DataType IsType(object obj)
        {
            if (obj is null)
            {
                return DataType.ByteStream;
            }
            Type type = obj.GetType();
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    return DataType.Bool;
                case TypeCode.Char:
                    return DataType.Char;
                case TypeCode.Byte:
                    return DataType.Byte;
                case TypeCode.Int16:
                    return DataType.Short;
                case TypeCode.Int32:
                    return DataType.Int;
                case TypeCode.Int64:
                    return DataType.Long;
                case TypeCode.Single:
                    return DataType.Float;
                case TypeCode.Double:
                    return DataType.Double;
                case TypeCode.String:
                    return DataType.String;
                default:
                    return DataType.ByteStream;
            }
        }
        /// <summary>
        /// 判断数据的类型
        /// </summary>
        /// <param name="obj">判断的参数</param>
        /// <param name="dataType">接收返回的可存类型枚举引用；如果参数类型不是<see cref="DataType"/>枚举中的类型，则默认返回<see cref="DataType.ByteStream"/></param>
        /// <returns>参数<paramref name="obj"/>是否为<see cref="DataType"/>枚举中的基础可存类型，如果是其中之一，返回true；否则false；如果为null，返回false</returns>
        public static bool IsType(object obj, out DataType dataType)
        {
            dataType = DataType.ByteStream;
            if (obj is null)
            {
                return false;
            }
            if (obj is byte[])
            {
                return true;
            }
            if (obj is int)
            {
                dataType = DataType.Int;
                return true;
            }
            if (obj is long)
            {
                dataType = DataType.Long;
                return true;
            }
            if (obj is string)
            {
                dataType = DataType.String;
                return true;
            }
            if (obj is float)
            {
                dataType = DataType.Float;
                return true;
            }
            if (obj is double)
            {
                dataType = DataType.Double;
                return true;
            }
            if(obj is bool)
            {
                dataType = DataType.Bool;
                return true;
            }
            if (obj is byte)
            {
                dataType = DataType.Byte;
                return true;
            }
            if (obj is short)
            {
                dataType = DataType.Short;
                return true;
            }
            if (obj is char)
            {
                dataType = DataType.Char;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断数据的类型
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="obj">判断的参数</param>
        /// <param name="dataType">接收返回的可存类型枚举引用；如果参数类型不是<see cref="DataType"/>枚举中的类型，则默认返回<see cref="DataType.ByteStream"/></param>
        /// <returns>参数<paramref name="obj"/>是否为<see cref="DataType"/>枚举中的基础可存类型，如果是其中之一，返回true；否则false</returns>
        public static bool IsType<T>(T obj, out DataType dataType)
        {
            if (ReferenceEquals(obj,null))
            {
                dataType = DataType.ByteStream;
                return false;
            }
            return IsType(obj.GetType(), out dataType);
        }
        /// <summary>
        /// 判断数据的类型
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <param name="dataType">接收返回的可存类型枚举引用；如果参数类型不是<see cref="DataType"/>枚举中的类型，则默认返回<see cref="DataType.ByteStream"/></param>
        /// <returns>类型<paramref name="type"/>是否为<see cref="DataType"/>枚举中的基础可存类型，如果是其中之一，返回true；否则false</returns>
        public static bool IsType(Type type, out DataType dataType)
        {
            TypeCode code = Type.GetTypeCode(type);           
            switch (code)
            {
                case TypeCode.Object:
                    break;
                case TypeCode.Boolean:
                    dataType = DataType.Bool;
                    return true;
                case TypeCode.Char:
                    dataType = DataType.Char;
                    return true;
                case TypeCode.Byte:
                    dataType = DataType.Byte;
                    return true;
                case TypeCode.Int16:
                    dataType = DataType.Short;
                    return true;
                case TypeCode.Int32:
                    dataType = DataType.Int;
                    return true;
                case TypeCode.Int64:
                    dataType = DataType.Long;
                    return true;
                case TypeCode.Single:
                    dataType = DataType.Float;
                    return true;
                case TypeCode.Double:
                    dataType = DataType.Double;
                    return true;
                case TypeCode.String:
                    dataType = DataType.String;
                    return true;
                case TypeCode.Empty:
                    dataType = DataType.ByteStream;
                    return false;
                default:
                    break;
            }
            dataType = DataType.ByteStream;
            Type bty = typeof(byte[]);
            if (bty.IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断数据的类型
        /// </summary>
        /// <typeparam name="CType">要判断的类型</typeparam>
        /// <param name="dataType">接收返回的可存类型枚举引用；如果参数类型不是<see cref="DataType"/>枚举中的类型，则默认返回<see cref="DataType.ByteStream"/></param>
        /// <returns>类型<typeparamref name="CType"/>是否为<see cref="DataType"/>枚举中的基础可存类型，如果是其中之一，返回true；否则false</returns>
        public static bool IsType<CType>(out DataType dataType)
        {
            Type type = typeof(CType);
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    dataType = DataType.Bool;
                    return true;
                case TypeCode.Char:
                    dataType = DataType.Char;
                    return true;
                case TypeCode.Byte:
                    dataType = DataType.Byte;
                    return true;
                case TypeCode.Int16:
                    dataType = DataType.Short;
                    return true;
                case TypeCode.Int32:
                    dataType = DataType.Int;
                    return true;
                case TypeCode.Int64:
                    dataType = DataType.Long;
                    return true;
                case TypeCode.Single:
                    dataType = DataType.Float;
                    return true;
                case TypeCode.Double:
                    dataType = DataType.Double;
                    return true;
                case TypeCode.String:
                    dataType = DataType.String;
                    return true;
                default:
                    break;
            }
            dataType = DataType.ByteStream;
            if (type.IsAssignableFrom(typeof(byte[])))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 比较两个可存类型的值是否一致
        /// </summary>
        /// <param name="obj1">封装对象1</param>
        /// <param name="obj2">封装对象2</param>
        /// <returns>
        /// <para>参数<paramref name="obj1"/>和<paramref name="obj2"/>的对象类型必须为<see cref="DataType"/>枚举指定的类型；</para>
        /// <para>若两个对象类型不同，则返回false；若类型相同，但值不同，返回false；只有类型和值相同时，返回true</para>
        /// </returns>
        /// <exception cref="ArgumentException">对象为<see cref="DataType"/>枚举指定之外的类型</exception>
        public static bool ValueEquals(object obj1, object obj2)
        {
            DataType t1, t2;
            if(!(IsType(obj1, out t1) && IsType(obj2, out t2)))
            {
                throw new ArgumentException($"参数类型不可为{typeof(DataType).FullName}枚举变量以外的类型");
            }
            if (t1 != t2) return false;
            return _valueEqualsBeType(obj1, obj2, t1);
        }

        private static bool _valueEqualsBeType(object obj1, object obj2, DataType type)
        {
            switch (type)
            {
                case DataType.Byte:
                    return (byte)obj1 == (byte)obj2;
                case DataType.Bool:
                    return (bool)obj1 == (bool)obj2;
                case DataType.Short:
                    return (short)obj1 == (short)obj2;
                case DataType.Int:
                    return (int)obj1 == (int)obj2;
                case DataType.Long:
                    return (long)obj1 == (long)obj2;
                case DataType.Float:
                    return (float)obj1 == (float)obj2;
                case DataType.Double:
                    return (double)obj1 == (double)obj2;
                case DataType.Char:
                    return (char)obj1 == (char)obj2;
                case DataType.String:
                    return (string)obj1 == (string)obj2;
                case DataType.ByteStream:
                    break;
            }
             byte[] b1 = (byte[])obj1;
            byte[] b2 = (byte[])obj2;
            return b1.ComparerEquals(b2);
        }

        #endregion

        #region 改
        /// <summary>
        /// 重新设置或添加指定键的值，使用封送内存块转化字节数组
        /// </summary>
        /// <typeparam name="Value">指定的类型</typeparam>
        /// <param name="key">修改或添加的键</param>
        /// <param name="value">修改的值</param>
        /// <exception cref="ArgumentException"><typeparamref name="Value"/>不是连续内存块的可格式化类型</exception>
        /// <exception cref="ArgumentException">指定键已存在</exception>
        /// <exception cref="ArgumentNullException">键为null</exception>
        public void Set<Value>(string key, Value value) where Value : unmanaged
        {
            _Cheskey(key, "key为null", "键的字符数超过16383");
            hash[key] = value.ToByteArray();
        }
        /// <summary>
        /// 重新设置或添加指定键的值，使用字节流转换器
        /// </summary>
        /// <typeparam name="Value"></typeparam>
        /// <param name="key">需要更改或添加的键</param>
        /// <param name="value">值</param>
        /// <param name="toByte">指定字节流转化器</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null或<paramref name="toByte"/>为null；或者实现的转化器返回的是一个null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void Set<Value>(string key, Value value, IObjToByteStream<Value> toByte)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if (toByte is null)
            {
                throw new ArgumentNullException("toByte","转化器引用为null");
            }
            byte[] b;
            try
            {
                b = toByte.ToByteStream(value);
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            if (b is null) throw new ArgumentNullException("value", "转化器返回的byte[]是一个null引用");

            hash[key] = b;
        }
        /// <summary>
        /// 重新设置或添加指定键的值，使用公共接口字节流转换器
        /// </summary>
        /// <param name="key">需要更改或添加的键</param>
        /// <param name="value">派生自<see cref="IObjToByteStream"/>接口的对象</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null或参数为null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void Set(string key, IObjToByteStream value)
        {
            _Cheskey(key, "key为null", "键的字符数超过16383");
            if(value == null) throw new ArgumentNullException("toByte", "转化器引用为null");
            byte[] b;
            try
            {
                b = value.ToByteStream();
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            if (b is null) throw new ArgumentNullException("value", "转化器返回的byte[]是一个null引用");
            hash[key] = b;
        }
        /// <summary>
        /// 重新设置或添加指定键的值，使用公共接口字节流转换器
        /// </summary>
        /// <typeparam name="Value">派生自<see cref="IObjToByteStream"/>接口的值类型</typeparam>
        /// <param name="key">需要更改或添加的键</param>
        /// <param name="value">派生自<see cref="IObjToByteStream"/>接口的值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null或参数为null</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void SetValue<Value>(string key, Value value) where Value : struct, IObjToByteStream
        {
            _Cheskey(key, "key为null", "键的字符数超过16383");
            byte[] b;
            try
            {
                b = value.ToByteStream();
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            if (b is null) throw new ArgumentNullException("value", "转化器返回的byte[]是一个null引用");
            hash[key] = b;
        }

        /// <summary>
        /// 重新设置或添加指定键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="pair">值，转化为字节流数据储存</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null或<paramref name="pair"/>为null</exception>
        /// <exception cref="PairToByteDataException">转化时发生错误</exception>
        /// <exception cref="PairToByteDataException">字节流转化器出现异常</exception>
        public void Set(string key, PairToByteStream pair)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if (pair is null)
            {
                throw new ArgumentNullException("值为null");
            }
            byte[] bs;
            try
            {
                bs = pair.ToByteStream();
            }
            catch (Exception ex)
            {
                throw new PairToByteDataException("字节流转化器出现异常", ex);
            }
            hash[key] = bs;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, int value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, long value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, short value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, byte value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, char value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null或<paramref name="value"/>为null</exception>
        public void Set(string key, string value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if (value == null)
            {
                throw new ArgumentNullException("参数value不得为null");
            }
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, float value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, double value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null</exception>
        public void Set(string key, bool value)
        {
            _Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            hash[key] = value;
        }
        /// <summary>
        /// 添加或重新设置为指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">指定值</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>为null或<paramref name="value"/>为null</exception>
        public void Set(string key, byte[] value)
        {_Cheskey(key, "key为null", "键的字符数超过" + keyMaxSize);
            if(value == null)
            {
                throw new ArgumentNullException("参数value不得为null");
            }
            hash[key] = value;
        }
        #endregion

        #endregion

        #region 文件读写
        /// <summary>
        /// <para>将键值对集合数据从流数据当前位置向后写入</para>
        /// </summary>
        /// <param name="stream">要写入的文件流，需要读写权限</param>
        /// <returns>对文件流写入的数据大小，单位字节</returns>
        /// <exception cref="ArgumentNullException">file参数不能为null</exception>
        /// <exception cref="NotSupportedException">流没有读取或写入权限</exception>
        /// <exception cref="ArgumentException">流缓冲区无法承载一次传输的字节量</exception>
        /// <exception cref="IOException">流错误</exception>
        /// <exception cref="ObjectDisposedException">流是关闭状态</exception>
        /// <exception cref="InvalidOperationException">在写入的过程中修改键值对数据</exception>
        public long FileWrite(System.IO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("file参数不能为null");
            }

            long size = 0;
            IEnumerator<byte[]> en = ToByteStream(1024).GetEnumerator();
            while (en.MoveNext())
            {
                byte[] b = en.Current;
                stream.Write(b, 0, b.Length);
                size += b.Length;
            }
            return size;
        }
        /// <summary>
        /// <para>使用函数枚举器，将键值对集合数据从流的当前位置向后写入</para>
        /// </summary>
        /// <param name="stream">要写入的文件流，需要写入权限</param>
        /// <param name="maxWriteByte">每次写入的最大字节数</param>
        /// <returns>将数据写入的函数枚举器，期间返回每次写入的字节；写入期间请不要将<paramref name="stream"/>引用的实例释放</returns>
        /// <exception cref="ArgumentNullException">file参数不能为null</exception>
        /// <exception cref="NotSupportedException">流没有读取或写入权限</exception>
        /// <exception cref="ArgumentException">流缓冲区无法承载一次传输的字节量</exception>
        /// <exception cref="IOException">流错误</exception>
        /// <exception cref="ObjectDisposedException">流是关闭状态</exception>
        /// <exception cref="InvalidOperationException">在写入的过程中修改键值对数据</exception>
        public IEnumerable<int> FileWrite(System.IO.Stream stream, int maxWriteByte)
        {
            if (stream is null)
            {
                throw new ArgumentNullException("file参数不能为null");
            }
            IEnumerator<byte[]> en = ToByteStream(maxWriteByte).GetEnumerator();
            while (en.MoveNext())
            {
                byte[] b = en.Current;
                int size = b.Length;
                stream.Write(b, 0, size);
                yield return size;
            }
        }
        /// <summary>
        /// <para>读取流从当前位置开始的数据到键值对集合</para>
        /// <para><paramref name="stream"/>必须要有读取权限</para>
        /// <para>该函数将会对当前实例的旧值清空</para>
        /// <para>请确保指定文件是该类数据转化，否则可能会引发未知异常</para>
        /// </summary>
        /// <param name="stream">给定文件路径</param>
        /// <exception cref="ArgumentException">缓冲区长度不足以承载一次传输的量</exception>
        /// <exception cref="ArgumentNullException">file参数不能为null</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="NotSupportedException">流没有读取权限</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public void FileRead(System.IO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("file参数不能为null");
            }
            hash = new Dictionary<string, object>();
            CoverStream(stream);
        }
        /// <summary>
        /// <para>读取流数据并将数据添加到键值对集合</para>
        /// <para>若在流数据中发现了与该实例相同的键，则覆盖为新值</para>
        /// <para>请确保指定文件是该类数据转化</para>
        /// </summary>
        /// <exception cref="ArgumentException">缓冲区长度不足以承载一次传输的量</exception>
        /// <exception cref="ArgumentNullException">file参数不能为null</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="NotSupportedException">流没有读取权限</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public void FileReadOnCover(System.IO.Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException("file参数不能为null");
            }
            CoverStream(stream);
        }

        /// <summary>
        /// <para>读取流数据并将数据添加键值对集合</para>
        /// <para>若在流数据中发现了与该实例相同的键，则保留旧值</para>
        /// <para>请确保指定文件是该类数据转化</para>
        /// </summary>
        /// <exception cref="ArgumentException">缓冲区长度不足以承载一次传输的量</exception>
        /// <exception cref="ArgumentNullException">file参数不能为null</exception>
        /// <exception cref="IOException">IO错误</exception>
        /// <exception cref="PairToByteDataException">读取时检测到数据异常</exception>
        /// <exception cref="NotSupportedException">流没有读取权限</exception>
        /// <exception cref="ObjectDisposedException">流已关闭</exception>
        public void FileReadAdd(System.IO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("file参数不能为null");
            }
            AddStream(stream);
        }
        /// <summary>
        /// 将当前集合数据写入在指定路径新建的文件当中
        /// </summary>
        /// <param name="filePath">文件所在路径，使用绝对路径或相对路径</param>
        /// <returns>如果成功写入返回true；如果无法写入或指定路径格式不正确，或者引发其它任何异常，返回false</returns>
        public bool FileWrite(string filePath)
        {
            try
            {
                using (FileStream file = new FileStream(filePath,FileMode.Create, FileAccess.ReadWrite))
                {
                    FileWrite(file);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 其它

        private static void _Cheskey(string key, string keynullMessage, string keyMaxSizeMessage)
        {
            if(key == null)
            {
                throw new ArgumentNullException("key", keynullMessage);
            }
            if(key.Length > keyMaxSize)
            {
                throw new ArgumentException(keyMaxSizeMessage);
            }
        }

        /// <summary>
        /// 返回当前的循环访问的枚举器
        /// </summary>
        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return hash.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string,object>>.GetEnumerator()
        {
            return hash.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return hash.GetEnumerator();
        }
        /// <summary>
        /// 当前实例储存个数
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return hash.Count.ToString();
        }
        /// <summary>
        /// 对比两个数据的键是否相同
        /// </summary>
        /// <param name="p1">p1</param>
        /// <param name="p2">p2</param>
        /// <returns>若两参数的键是相同数量的相等字符串，无论顺序，返回true；否则false</returns>
        public static bool KeyEquals(PairToByteStream p1, PairToByteStream p2)
        {
            if(p1 is null || p2 is null)
            {
                throw new ArgumentNullException("pair", "参数不可为null");
            }
            if (ReferenceEquals(p1, p2))
            {
                return true;
            }
            bool b = true;

            var k = p1.hash.Keys;
            if (k.Count != p2.hash.Count) return false;
            foreach (var item in k)
            {
                if (!p2.ContainsKey(item))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }

        /// <summary>
        /// 判断参数的键值对数据是否等于该实例的数据
        /// </summary>
        /// <param name="obj">参数</param>
        /// <returns>若类型匹配且满足键值对数据比对条件返回true；否则返回false</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            
            if (obj is PairToByteStream p)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                return _equals(hash, p.hash);
            }
            if(obj is Dictionary<string,object> d)
            {
                if (ReferenceEquals(hash, obj))
                {
                    return true;
                }
                return _equals(hash, d);
            }
            return false;
        }
        /// <summary>
        /// 判断参数的键值对数据是否等于该实例的数据
        /// </summary>
        /// <param name="obj">参数</param>
        /// <returns>若参数键值对数据的每一个键和值都能在当前实例中找到对应的键和值，且值得类型和数据相等，返回true；否则返回false</returns>
        public bool Equals(PairToByteStream obj)
        {
            if (obj == null) return false;
            return _equals(hash, obj.hash);
        }

        /// <summary>
        /// 对比两键值对键和值，无论顺序
        /// 参数必须引用实例
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static bool _equals(Dictionary<string,object> p1, Dictionary<string, object> p2)
        {
            KeyValuePair<string, object> ks;
            object v2;
            object v1;
            bool b = true;
            DataType t1;
            DataType t2;
            if(p1.Count != p2.Count)
            {
                return false;
            }

            using (var e = p1.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    ks = e.Current;
                    v1 = ks.Value;
                    try
                    {
                        v2 = p2[ks.Key];
                    }
                    catch (KeyNotFoundException)
                    {
                        //p2未找到p1的键
                        b = false;
                        break;
                    }
                    //判断值类型
                    b = IsType(v1, out t1);
                    if (!b)
                    {
                        //b == false,不是内置类型
                        break;
                    }
                    
                    //value拿到判断类型
                    b = IsType(v2, out t2);
                    if (!b)
                    {
                        //b == false,不是内置类型
                        break;
                    }
                    if(t1 != t2)
                    {
                        //类型不匹配
                        b = false;
                        break;
                    }
                    b = _valueEqualsBeType(v1, v2, t2);
                    if (!b)
                    {
                        break;
                    }
                }
            }
            return b;
        }
        /// <summary>
        /// 获取字典的哈希代码
        /// </summary>
        /// <returns>字典的哈希代码</returns>
        public override int GetHashCode()
        {
            return hash.GetHashCode();
        }
        /// <summary>
        /// 使用公开键值对枚举初始化键值对集合，自动筛选无法存储的类型
        /// </summary>
        /// <param name="dicPair">键值对集合</param>
        /// <returns>基于<paramref name="dicPair"/>实例化的新实例</returns>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public static PairToByteStream InitDictionaryPair(IEnumerable<KeyValuePair<string, object>> dicPair)
        {
            if(dicPair == null)
            {
                throw new ArgumentNullException("dicPair", "参数为null");
            }
            int count;
            if(dicPair is ICollection<KeyValuePair<string,object>> c)
            {
                count = c.Count;
            }
            else
            {
                count = 4;
            }
            PairToByteStream pair = new PairToByteStream(count);
            foreach (var item in dicPair)
            {
                if (item.Key.Length <= keyMaxSize && IsDataType(item.Value))
                {
                    pair.hash.Add(item.Key, item.Value);
                }
            }
            return pair;
        }

        #endregion

        #endregion

        #region 派生
        void IDictionary<string,object>.Add(string key, object obj)
        {
            DataType type;
            bool istype = IsType(obj, out type);
            if (!istype)
            {
                throw new PairToByteDataException("参数类型不是DateType枚举类型");
            }
            switch (type)
            {
                case DataType.Byte:
                    Add(key, (byte)obj);
                    break;
                case DataType.Bool:
                    Add(key, (bool)obj);
                    break;
                case DataType.Short:
                    Add(key, (short)obj);
                    break;
                case DataType.Int:
                    Add(key, (int)obj);
                    break;
                case DataType.Long:
                    Add(key, (long)obj);
                    break;
                case DataType.Float:
                    Add(key, (float)obj);
                    break;
                case DataType.Double:
                    Add(key, (double)obj);
                    break;
                case DataType.Char:
                    Add(key, (char)obj);
                    break;
                case DataType.String:
                    Add(key, (string)obj);
                    break;
                case DataType.ByteStream:
                    Add(key, (byte[])obj);
                    break;
                default:
                    break;
            }
        }
        ICollection<string> IDictionary<string, object>.Keys => hash.Keys;
        ICollection<object> IDictionary<string, object>.Values => hash.Values;
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<string, object>>)hash).IsReadOnly;
            }
        }
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            if(!Add(item.Key, item.Value))
            {
                throw new PairToByteDataException("键的类型不是DataType枚举之一");
            }
        }
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            ICollection<KeyValuePair<string, object>> c = hash;
            return c.Contains(item);
        }
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            ICollection<KeyValuePair<string, object>> c = hash;
            return c.Remove(item);
        }
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ICollection<KeyValuePair<string, object>> c = hash;
            c.CopyTo(array, arrayIndex);
        }
        #endregion
    }

    /// <summary>
    /// 内存存储方式
    /// </summary>
    public enum MemoryStorage : byte
    {
        /// <summary>
        /// 小端存储
        /// </summary>
        Small,
        /// <summary>
        /// 大端存储
        /// </summary>
        Large
    }

}
