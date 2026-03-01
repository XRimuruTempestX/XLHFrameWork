using System;
using UnityEngine;

namespace FixIntMath
{
    public partial class FixIntMath
    {
        /// <summary>
        /// 绝对值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FixInt Abs(FixInt value)
        {
            return value.Value > 0 ? value : new FixInt(-value.Value);
        }
        /// <summary>
        /// 最大值
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns>返回两个指定数字中的较大值</returns>
        public static FixInt Max(FixInt value1, FixInt value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        /// <summary>
        /// 最小值 
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns>返回两个指定数字中的较小值</returns>
        public static FixInt Min(FixInt value1, FixInt value2)
        {
            return value1 < value2 ? value1 : value2;
        }
        /// <summary>
        /// 随机数
        /// </summary>
        /// <param name="random">要随机的数</param>
        /// <param name="min">最小随机范围</param>
        /// <param name="max">最大随机范围</param>
        /// <returns>大于或等于 0 且小于 Int32.MaxValue 的 32 位有符号整数。</returns>
        public static FixInt Range(System.Random random, FixInt min, FixInt max)
        {
            return random.Next(min.IntValue, max.IntValue) / FixInt.MUTIPLE;
        }
        /// <summary>
        ///  Returns the sign of f.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static FixInt Sign(FixInt f)
        {
            return (f >= FixInt.Zero) ? FixInt.One : (-FixInt.One);
        }
        /// <summary>
        /// 固定value值的取值范围
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回 value 固定到 min 和 max的非独占范围</returns>
        public static FixInt Clamp(FixInt value, FixInt min, FixInt max)
        {
            return value < min ? min : value > max ? max : value;
        }
        public static FixInt Clamp01(FixInt value)
        {
            if (value < FixInt.Zero)
            {
                return FixInt.Zero;
            }

            if (value > FixInt.One)
            {
                return FixInt.One;
            }

            return value;
        }
        /// <summary>
        /// 四舍五入
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FixInt Round(FixInt value)
        {
            return new FixInt(Math.Round(value.RawFloat));
        }
        /// <summary>
        /// 幂运算
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static FixInt Pow(FixInt value, int count)
        {
            if (count == 1) return value;
            FixInt result = FixInt.Zero;
            FixInt tmp = Pow(value, count >> 1);
            if ((count & 1) != 0) //奇数    
            {
                result = value * tmp * tmp;
            }
            else
            {
                result = tmp * tmp;
            }
            return result;
        }
        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FixInt Floor(FixInt value)
        {
            //清除小数部分
            return ((ulong)value.RawFloat & ~0xFFFFFFFFFFFFF000);
        }
        /// <summary>
        /// 向上取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FixInt Ceiling(FixInt value)
        {
            bool hasFractionalPart = ((ulong)value.RawFloat & 0x0000000000000FFF) != 0;
            //如果有小数部分，则加 1
            return hasFractionalPart ? Floor(value) + FixInt.One : value;
        }
        /// <summary>
        /// 平方根
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static FixInt Sqrt(FixInt f, int numberIterations)
        {
            if (f.Value < 0)
            {
                throw new ArithmeticException("sqrt error");
            }

            if (f.Value == 0)
                return FixInt.Zero;

            FixInt k = f + FixInt.One >> 1;
            for (int i = 0; i < numberIterations; i++)
                k = (k + (f / k)) >> 1;

            if (k.Value < 0)
                throw new ArithmeticException("Overflow");
            else
                return k;
        }
        /// <summary>
        /// 平方根
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static FixInt Sqrt(FixInt f)
        {
            byte numberOfIterations = 8;
            if (f.Value > 0x64000)
                numberOfIterations = 12;
            if (f.Value > 0x3e8000)
                numberOfIterations = 16;
            return Sqrt(f, numberOfIterations);
        }

        /// <summary>
        /// 四象限反正切
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static FixInt Atan2(float fy, float fx)
        {
            int y = (int)(fy * FixInt.MUTIPLE); int x = (int)(fx * FixInt.MUTIPLE);
            int num;
            int num2;
            if (x < 0)
            {
                if (y < 0)
                {
                    x = -x;
                    y = -y;
                    num = 1;
                }
                else
                {
                    x = -x;
                    num = -1;
                }
                num2 = -31416;
            }
            else
            {
                if (y < 0)
                {
                    y = -y;
                    num = -1;
                }
                else
                {
                    num = 1;
                }
                num2 = 0;
            }
            int dIM = Atan2LookupTable.DIM;
            long num3 = (long)(dIM - 1);
            long b = (long)((x >= y) ? x : y);
            int num4 = (int)Divide((long)x * num3, b);
            int num5 = (int)Divide((long)y * num3, b);
            int num6 = Atan2LookupTable.table[num5 * dIM + num4];
            return new FixInt((num6 + num2) * num)/ 10000f;
        }
        /// <summary>
        /// 反余弦函数
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public static FixInt Acos(FixInt nom)
        {
            int num = (int)Divide(nom.Value * (long)AcosLookupTable.HALF_COUNT, FixInt.MUTIPLE) + AcosLookupTable.HALF_COUNT;
            num = Mathf.Clamp(num, 0, AcosLookupTable.COUNT);
            return new FixInt(AcosLookupTable.table[num] / 10000f);
        }
        /// <summary>
        /// 反余弦函数
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public static FixInt Acos(FixInt nom, long den)
        {
            int num = (int)Divide(nom.Value * (long)AcosLookupTable.HALF_COUNT, den) + AcosLookupTable.HALF_COUNT;
            num = Mathf.Clamp(num, 0, AcosLookupTable.COUNT);
            return AcosLookupTable.table[num] / 10000f;
        }
        /// <summary>
        /// 正弦
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public static FixInt Sin(FixInt nom)
        {
            int index = SinCosLookupTable.getIndex(nom.Value, FixInt.MUTIPLE);
            return new FixInt(SinCosLookupTable.sin_table[index] / 10000f);
        }

        /// <summary>
        /// 余弦
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public static FixInt Cos(FixInt nom)
        {
            int index = SinCosLookupTable.getIndex(nom.Value, FixInt.MUTIPLE);
            return new FixInt(SinCosLookupTable.cos_table[index] / 10000f);
        }
        /// <summary>
        /// 插值运算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long Divide(long a, long b)
        {
            long num = (long)((ulong)((a ^ b) & -9223372036854775808L) >> 63);
            long num2 = num * -2L + 1L;
            return (a + b / 2L * num2) / b;
        }
        /// <summary>
        /// 插值运算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Divide(int a, int b)
        {
            int num = (int)((uint)((a ^ b) & -2147483648) >> 31);
            int num2 = num * -2 + 1;
            return (a + b / 2 * num2) / b;
        }
    }
}