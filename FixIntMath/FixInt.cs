using System;

namespace FixIntMath
{
    public struct FixInt : IEquatable<FixInt>, IComparable<FixInt>
    {
        #region 属性 字段
        public const Int64 MaxValue = 9223372036854775807;

        public const Int64 MinValue = -9223372036854775808;

        public static readonly FixInt One = new FixInt(1);
        public static readonly FixInt Zero = new FixInt(0);
        /// <summary>
        /// 左移或右移的次数
        /// </summary>
        private const int SHIFT = 10;
        /// <summary>
        /// 放大倍率
        /// </summary>
        public const int MUTIPLE = 1024;
        /// <summary>
        /// 当前定点数对应的真实数值
        /// </summary>
        private readonly long value;
        /// <summary>
        /// 外部访问的真实数值(放大后的数值)
        /// </summary>
        public long Value { get { return value; } }
        /// <summary>
        /// 外部访问的真实整形数据(放大后的数值)
        /// </summary>
        public int IntValue { get { return (int)value; } }
        /// <summary>
        /// 真实float的数据(没有放大倍率的数值,只能用作表现层的渲染）
        /// </summary>
        public float RawFloat { get { return (float)Math.Round(value / 1024.0f * 1000) / 1000; } }//精度为2为的小数
        /// <summary>
        /// 真实Int的数据（没有放大倍率的值,只能用作表现层的渲染）
        /// </summary>
        public int RawInt { get { return (int)Math.Round((value / 1024.0f)); } }
        /// <summary>
        /// 真实的double数据（没有放大倍率的值,只能用作表现层的渲染）
        /// </summary>
        public double RawDouble { get { return value / 1024.0d; } }

        #endregion

        #region 构造函数

        public FixInt(float value)
        {
            //这里通过乘发放大的原因是为了 更高的精度.
            this.value = (long)Math.Round((value * MUTIPLE));
        }
        public FixInt(double value)
        {

            this.value = (long)Math.Round((value * MUTIPLE));
        }
        public FixInt(int value)
        {
            //1 x 1024
            this.value = value << SHIFT;
        }
        /// <summary>
        /// 特殊构造函数，适用场景：当我们想把定点数值参与运算的时候使用。
        /// </summary>
        /// <param name="value">一定必须是Fixint放大后的值</param>
        public FixInt(long value)
        {
            this.value = value;
        }

        #endregion

        #region 显示转换 explicit
        //在FixInt值通过(float)转换为float值时触发
        public static explicit operator float(FixInt v)
        {
            return v.RawFloat;
        }

        public static explicit operator int(FixInt v)
        {
            return v.RawInt;
        }
        public static explicit operator long(FixInt v)
        {
            return v.value;
        }
        public static explicit operator double(FixInt v)
        {
            return v.RawDouble;
        }

        #endregion


        #region 隐式转换 implicit
        //在float值赋值FixInt值时触发
        public static implicit operator FixInt(float v)
        {
            return new FixInt(v);
        }
        //在int值赋值FixInt值时触发
        public static implicit operator FixInt(int v)
        {
            return new FixInt(v);
        }
        //在long值赋值FixInt值时触发
        public static implicit operator FixInt(long v)
        {
            return new FixInt(v);
        }
        //在double值赋值FixInt值时触发
        public static implicit operator FixInt(double v)
        {
            return new FixInt(v);
        }
        #endregion

        #region 操作符重载

        #region 操作符 + 运算
        public static FixInt operator +(FixInt f1, FixInt f2)
        {
            return new FixInt(f1.value + f2.value);
        }
        public static FixInt operator +(FixInt f1, int f2)
        {
            return f1 + (FixInt)f2;
        }
        public static FixInt operator +(FixInt f1, float f2)
        {
            return f1 + (FixInt)f2;
        }
        public static FixInt operator +(FixInt f1, double f2)
        {
            return f1 + (FixInt)f2;
        }
        #endregion

        #region 操作符 - 运算
        public static FixInt operator -(FixInt f1, FixInt f2)
        {
            return new FixInt(f1.value - f2.value);
        }
        public static FixInt operator -(FixInt f1, int f2)
        {
            return f1 - (FixInt)f2;
        }
        public static FixInt operator -(FixInt f1, float f2)
        {
            return f1 - (FixInt)f2;
        }
        public static FixInt operator -(FixInt f1, double f2)
        {
            return f1 - (FixInt)f2;
        }
        #endregion

        #region 操作符 / 运算
        public static FixInt operator /(FixInt f1, FixInt f2)
        {
            //两个long类型之间的除法运算，结果还是long类型。因为long类型构造函数，不会进行移位操作，所以分母需要提前进行移位操作
            FixInt a = f1.value << SHIFT;
            return a.value / f2.value;
        }
        public static FixInt operator /(FixInt f1, int f2)
        {
            return f1 / (FixInt)f2;
        }
        public static FixInt operator /(FixInt f1, float f2)
        {
            return f1 / (FixInt)f2;
        }
        public static FixInt operator /(FixInt f1, double f2)
        {
            return f1 / (FixInt)f2;
        }

        #endregion

        #region 操作符 * 运算
        public static FixInt operator *(FixInt f1, FixInt f2)
        {
            //value1 value2 s=> 1024 > 2  1*1 =1  1024*1024=
            return new FixInt((f1.value * f2.value) / MUTIPLE);
        }
        public static FixInt operator *(FixInt f1, int f2)
        {
            return f1 * (FixInt)f2;
        }
        public static FixInt operator *(FixInt f1, float f2)
        {
            return f1 * (FixInt)f2;
        }
        public static FixInt operator *(FixInt f1, double f2)
        {
            return f1 * (FixInt)f2;
        }

        #endregion

        #region 操作符 == 运算
        public static bool operator ==(FixInt f1, FixInt f2)
        {
            return f1.value == f2.value;
        }
        public static bool operator ==(FixInt f1, int f2)
        {
            return f1 == (FixInt)f2;
        }
        public static bool operator ==(FixInt f1, float f2)
        {
            return f1 == (FixInt)f2;
        }
        public static bool operator ==(FixInt f1, double f2)
        {
            return f1 == (FixInt)f2;
        }
        #endregion

        #region 操作符 != 运算
        public static bool operator !=(FixInt f1, FixInt f2)
        {
            return f1.value != f2.value;
        }
        public static bool operator !=(FixInt f1, int f2)
        {
            return f1 != (FixInt)f2;
        }
        public static bool operator !=(FixInt f1, float f2)
        {
            return f1 != (FixInt)f2;
        }
        public static bool operator !=(FixInt f1, double f2)
        {
            return f1 != (FixInt)f2;
        }
        #endregion

        #region 操作符 >= 运算
        public static bool operator >=(FixInt f1, FixInt f2)
        {
            return f1.value >= f2.value;
        }
        public static bool operator >=(FixInt f1, int f2)
        {
            return f1 >= (FixInt)f2;
        }
        public static bool operator >=(FixInt f1, float f2)
        {
            return f1 >= (FixInt)f2;
        }
        public static bool operator >=(FixInt f1, double f2)
        {
            return f1 >= (FixInt)f2;
        }
        #endregion

        #region 操作符 <= 运算
        public static bool operator <=(FixInt f1, FixInt f2)
        {
            return f1.value <= f2.value;
        }
        public static bool operator <=(FixInt f1, int f2)
        {
            return f1 <= (FixInt)f2;
        }
        public static bool operator <=(FixInt f1, float f2)
        {
            return f1 <= (FixInt)f2;
        }
        public static bool operator <=(FixInt f1, double f2)
        {
            return f1 <= (FixInt)f2;
        }
        #endregion

        #region 操作符 > 运算
        public static bool operator >(FixInt f1, FixInt f2)
        {
            return f1.value > f2.value;
        }
        public static bool operator >(FixInt f1, int f2)
        {
            return f1 > (FixInt)f2;
        }
        public static bool operator >(FixInt f1, float f2)
        {
            return f1 > (FixInt)f2;
        }
        public static bool operator >(FixInt f1, double f2)
        {
            return f1 > (FixInt)f2;
        }
        #endregion

        #region 操作符 < 运算
        public static bool operator <(FixInt f1, FixInt f2)
        {
            return f1.value < f2.value;
        }
        public static bool operator <(FixInt f1, int f2)
        {
            return f1 < (FixInt)f2;
        }
        public static bool operator <(FixInt f1, float f2)
        {
            return f1 < (FixInt)f2;
        }
        public static bool operator <(FixInt f1, double f2)
        {
            return f1 < (FixInt)f2;
        }
        #endregion

        #region  操作符  % - << >>
        public static FixInt operator %(FixInt f1, FixInt f2)
        {
            return new FixInt(f1.value % f2.value);
        }
        public static FixInt operator -(FixInt f1)
        {
            return new FixInt(-f1.value);
        }
        public static FixInt operator <<(FixInt f1, int count)
        {
            return new FixInt(f1.value << count);
        }
        public static FixInt operator >>(FixInt f1, int count)
        {
            return new FixInt(f1.value >> count);
        }
        #endregion

        #endregion

        #region 外部方法

        /// <summary>
        /// 是否等于判断
        /// </summary>
        /// <param name="f1"></param>
        /// <returns></returns>
        public readonly bool Equals(FixInt f1)
        {
            return value == f1.value;
        }
        /// <summary>
        /// 是否等于判断
        /// </summary>
        /// <param name="obj">任意数值类型</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return value == ((FixInt)obj).value;
        }
        /// <summary>
        /// 获取唯一的HashCode码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return RawFloat.ToString();
        }

        /// <summary>
        /// 将当前实例与另一个对象进行比较，返回值表示当前实例的值是大于另一个实例的值还是小于 
        /// </summary>
        /// <param name="f1"></param>
        /// <returns>返回值：小于0 表示当前实例小于目标值，等于0说明当前值与目标值相等，大于0表示当前值大于目标值</returns>
        public readonly int CompareTo(FixInt f1)
        {
            return value.CompareTo(f1.value);
        }
        /// <summary>
        /// 将当前实例与另一个对象进行比较，返回值表示当前实例的值是大于另一个实例的值还是小于 
        /// </summary>
        /// <param name="f1"></param>
        /// <returns>返回值：小于0 表示当前实例小于目标值，等于0说明当前值与目标值相等，大于0表示当前值大于目标值</returns>
        public readonly int CompareTo(object f1)
        {
            return value.CompareTo(((FixInt)f1).value);
        }
        #endregion
    }
}