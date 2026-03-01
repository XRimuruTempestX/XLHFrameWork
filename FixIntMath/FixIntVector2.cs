using System;
using System.Globalization;
using UnityEngine;

namespace FixIntMath
{
     public struct FixIntVector2 : IEquatable<FixIntVector2>, IFormattable
    {
        #region Property
        //
        // 摘要:
        //     X component of the vector.
        public FixInt x;

        //
        // 摘要:
        //     Y component of the vector.
        public FixInt y;

        private static readonly FixIntVector2 zeroVector = new FixIntVector2(0f, 0f);

        private static readonly FixIntVector2 oneVector = new FixIntVector2(1f, 1f);

        private static readonly FixIntVector2 upVector = new FixIntVector2(0f, 1f);

        private static readonly FixIntVector2 downVector = new FixIntVector2(0f, -1f);

        private static readonly FixIntVector2 leftVector = new FixIntVector2(-1f, 0f);

        private static readonly FixIntVector2 rightVector = new FixIntVector2(1f, 0f);

        public FixInt this[int index]
        {

            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    _ => throw new IndexOutOfRangeException("Invalid FixIntVector2 index!"),
                };
            }

            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid FixIntVector2 index!");
                }
            }
        }



        //
        // 摘要:
        //     Returns this vector with a magnitude of 1 (Read Only).
        public FixIntVector2 normalized
        {

            get
            {
                FixIntVector2 result = new FixIntVector2(x, y);
                result.Normalize();
                return result;
            }
        }

        //
        // 摘要:
        //     Returns the length of this vector (Read Only).
        public FixInt magnitude
        {

            get
            {
                return FixIntMath.Sqrt(x * x + y * y);
            }
        }

        //
        // 摘要:
        //     Returns the squared length of this vector (Read Only).
        public FixInt sqrMagnitude
        {

            get
            {
                return x * x + y * y;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector2(0, 0).
        public static FixIntVector2 zero
        {

            get
            {
                return zeroVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector2(1, 1).
        public static FixIntVector2 one
        {

            get
            {
                return oneVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector2(0, 1).
        public static FixIntVector2 up
        {

            get
            {
                return upVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector2(0, -1).
        public static FixIntVector2 down
        {

            get
            {
                return downVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector2(-1, 0).
        public static FixIntVector2 left
        {

            get
            {
                return leftVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector2(1, 0).
        public static FixIntVector2 right
        {

            get
            {
                return rightVector;
            }
        }
        #endregion

        #region Constructor

        //
        // 摘要:
        //     Constructs a new vector with given x, y components.
        //
        // 参数:
        //   x:
        //
        //   y:

        public FixIntVector2(FixInt x, FixInt y)
        {
            this.x = x;
            this.y = y;
        }
        public FixIntVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public FixIntVector2(UnityEngine.Vector2 v2)
        {
            this.x = v2.x;
            this.y = v2.y;
        }
        #endregion

        #region Public Method

        //
        // 摘要:
        //     Set x and y components of an existing FixIntVector2.
        //
        // 参数:
        //   newX:
        //
        //   newY:

        public void Set(FixInt newX, FixInt newY)
        {
            x = newX;
            y = newY;
        }

        //
        // 摘要:
        //     Linearly interpolates between vectors a and b by t.
        //
        // 参数:
        //   a:
        //
        //   b:
        //
        //   t:

        public static FixIntVector2 Lerp(FixIntVector2 a, FixIntVector2 b, FixInt t)
        {
            t = FixIntMath.Clamp01(t);
            return new FixIntVector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        //
        // 摘要:
        //     Linearly interpolates between vectors a and b by t.
        //
        // 参数:
        //   a:
        //
        //   b:
        //
        //   t:

        public static FixIntVector2 LerpUnclamped(FixIntVector2 a, FixIntVector2 b, FixInt t)
        {
            return new FixIntVector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        //
        // 摘要:
        //     Moves a point current towards target.
        //
        // 参数:
        //   current:
        //
        //   target:
        //
        //   maxDistanceDelta:不能使用Time.deltaTime，要使用帧同步计算出来的时间插值 fixedDeltaTime

        public static FixIntVector2 MoveTowards(FixIntVector2 current, FixIntVector2 target, FixInt maxDistanceDelta)
        {
            FixInt num = target.x - current.x;
            FixInt num2 = target.y - current.y;
            FixInt num3 = num * num + num2 * num2;
            if (num3 == 0f || (maxDistanceDelta >= 0f && num3 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            FixInt num4 = (FixInt)FixIntMath.Sqrt(num3);
            return new FixIntVector2(current.x + num / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta);
        }

        //
        // 摘要:
        //     Multiplies two vectors component-wise.
        //
        // 参数:
        //   a:
        //
        //   b:

        public static FixIntVector2 Scale(FixIntVector2 a, FixIntVector2 b)
        {
            return new FixIntVector2(a.x * b.x, a.y * b.y);
        }

        //
        // 摘要:
        //     Multiplies every component of this vector by the same component of scale.
        //
        // 参数:
        //   scale:

        public void Scale(FixIntVector2 scale)
        {
            x *= scale.x;
            y *= scale.y;
        }

        //
        // 摘要:
        //     Makes this vector have a magnitude of 1.

        public void Normalize()
        {
            FixInt num = magnitude;
            if (num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = zero;
            }
        }

        //
        // 摘要:
        //     Returns a formatted string for this vector.
        //
        // 参数:
        //   format:
        //     A numeric format string.
        //
        //   formatProvider:
        //     An object that specifies culture-specific formatting.

        public override string ToString()
        {
            return ToString(null, null);
        }

        //
        // 摘要:
        //     Returns a formatted string for this vector.
        //
        // 参数:
        //   format:
        //     A numeric format string.
        //
        //   formatProvider:
        //     An object that specifies culture-specific formatting.

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        //
        // 摘要:
        //     Returns a formatted string for this vector.
        //
        // 参数:
        //   format:
        //     A numeric format string.
        //
        //   formatProvider:
        //     An object that specifies culture-specific formatting.

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "F2";
            }

            if (formatProvider == null)
            {
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            }
            return string.Format(formatProvider, "({0}, {1})", x.ToString(), y.ToString());
        }
        /// <summary>
        /// FixIntVectors Convert UnityEngine.Vector2
        /// </summary>
        /// <returns></returns>
        public Vector2 ToVector2()
        {
            return new Vector2(x.RawFloat, y.RawFloat);
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        //
        // 摘要:
        //     Returns true if the given vector is exactly equal to this vector.
        //
        // 参数:
        //   other:

        public override bool Equals(object other)
        {
            if (other is FixIntVector2 other2)
            {
                return Equals(other2);
            }

            return false;
        }


        public bool Equals(FixIntVector2 other)
        {
            return x == other.x && y == other.y;
        }

        //
        // 摘要:
        //     Reflects a vector off the vector defined by a normal.
        //
        // 参数:
        //   inDirection:
        //
        //   inNormal:

        public static FixIntVector2 Reflect(FixIntVector2 inDirection, FixIntVector2 inNormal)
        {
            FixInt num = -2f * Dot(inNormal, inDirection);
            return new FixIntVector2(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y);
        }

        //
        // 摘要:
        //     Returns the 2D vector perpendicular to this 2D vector. The result is always rotated
        //     90-degrees in a counter-clockwise direction for a 2D coordinate system where
        //     the positive Y axis goes up.
        //
        // 参数:
        //   inDirection:
        //     The input direction.
        //
        // 返回结果:
        //     The perpendicular direction.

        public static FixIntVector2 Perpendicular(FixIntVector2 inDirection)
        {
            return new FixIntVector2(0f - inDirection.y, inDirection.x);
        }

        //
        // 摘要:
        //     Dot Product of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixInt Dot(FixIntVector2 lhs, FixIntVector2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        //
        // 摘要:
        //     Gets the unsigned angle in degrees between from and to.
        //
        // 参数:
        //   from:
        //     The vector from which the angular difference is measured.
        //
        //   to:
        //     The vector to which the angular difference is measured.
        //
        // 返回结果:
        //     The unsigned angle in degrees between the two vectors.

        public static FixInt Angle(FixIntVector2 from, FixIntVector2 to)
        {
            FixInt num = FixIntMath.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (num < 1E-15f)
            {
                return 0f;
            }

            FixInt num2 = FixIntMath.Clamp(Dot(from, to) / num, -1f, 1f);
            return FixIntMath.Acos(num2) * 57.29578f;
        }

        //
        // 摘要:
        //     Gets the signed angle in degrees between from and to.
        //
        // 参数:
        //   from:
        //     The vector from which the angular difference is measured.
        //
        //   to:
        //     The vector to which the angular difference is measured.
        //
        // 返回结果:
        //     The signed angle in degrees between the two vectors.

        public static FixInt SignedAngle(FixIntVector2 from, FixIntVector2 to)
        {
            FixInt num = Angle(from, to);
            FixInt num2 = FixIntMath.Sign(from.x * to.y - from.y * to.x);
            return num * num2;
        }

        //
        // 摘要:
        //     Returns the distance between a and b.
        //
        // 参数:
        //   a:
        //
        //   b:

        public static FixInt Distance(FixIntVector2 a, FixIntVector2 b)
        {
            FixInt num = a.x - b.x;
            FixInt num2 = a.y - b.y;
            return FixIntMath.Sqrt(num * num + num2 * num2);
        }

        //
        // 摘要:
        //     Returns a copy of vector with its magnitude clamped to maxLength.
        //
        // 参数:
        //   vector:
        //
        //   maxLength:

        public static FixIntVector2 ClampMagnitude(FixIntVector2 vector, FixInt maxLength)
        {
            FixInt num = vector.sqrMagnitude;
            if (num > maxLength * maxLength)
            {
                FixInt num2 = (FixInt)FixIntMath.Sqrt(num);
                FixInt num3 = vector.x / num2;
                FixInt num4 = vector.y / num2;
                return new FixIntVector2(num3 * maxLength, num4 * maxLength);
            }

            return vector;
        }


        public static FixInt SqrMagnitude(FixIntVector2 a)
        {
            return a.x * a.x + a.y * a.y;
        }


        public FixInt SqrMagnitude()
        {
            return x * x + y * y;
        }

        //
        // 摘要:
        //     Returns a vector that is made from the smallest components of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixIntVector2 Min(FixIntVector2 lhs, FixIntVector2 rhs)
        {
            return new FixIntVector2(FixIntMath.Min(lhs.x, rhs.x), FixIntMath.Min(lhs.y, rhs.y));
        }

        //
        // 摘要:
        //     Returns a vector that is made from the largest components of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixIntVector2 Max(FixIntVector2 lhs, FixIntVector2 rhs)
        {
            return new FixIntVector2(FixIntMath.Max(lhs.x, rhs.x), FixIntMath.Max(lhs.y, rhs.y));
        }


        /// <summary>
        /// 随时间推移将一个向量逐渐改变为所需目标。向量通过某个类似于弹簧-阻尼的函数（它从不超过目标）进行平滑。
        /// </summary>
        /// <param name="current">当前位置</param>
        /// <param name="target">尝试达到的目标</param>
        /// <param name="currentVelocity">当前速度，此值由函数在每次调用时进行修改。</param>
        /// <param name="smoothTime">达到目标所需的近似时间。值越小，达到目标的速度越快。</param>
        /// <param name="maxSpeed">可以选择允许限制最大速度。</param>
        /// <param name="deltaTime">自上次调用此函数以来的时间，必须是帧同步的时间增量，若为Time.deltaTime,会导致定点数计算结果不一致，产生误差。</param>
        /// <returns></returns>
        public static FixIntVector2 SmoothDamp(FixIntVector2 current, FixIntVector2 target, ref FixIntVector2 currentVelocity, FixInt smoothTime, FixInt fixedDeltaTime, int maxSpeed = 99999)
        {

            smoothTime = FixIntMath.Max(0.0001f, smoothTime);
            FixInt num = 2f / smoothTime;
            FixInt num2 = num * fixedDeltaTime;
            FixInt num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            FixInt num4 = current.x - target.x;
            FixInt num5 = current.y - target.y;
            FixIntVector2 vector = target;
            FixInt num6 = maxSpeed * smoothTime;
            FixInt num7 = num6 * num6;
            FixInt num8 = num4 * num4 + num5 * num5;
            if (num8 > num7)
            {
                FixInt num9 = (FixInt)FixIntMath.Sqrt(num8);
                num4 = num4 / num9 * num6;
                num5 = num5 / num9 * num6;
            }

            target.x = current.x - num4;
            target.y = current.y - num5;
            FixInt num10 = (currentVelocity.x + num * num4) * fixedDeltaTime;
            FixInt num11 = (currentVelocity.y + num * num5) * fixedDeltaTime;
            currentVelocity.x = (currentVelocity.x - num * num10) * num3;
            currentVelocity.y = (currentVelocity.y - num * num11) * num3;
            FixInt num12 = target.x + (num4 + num10) * num3;
            FixInt num13 = target.y + (num5 + num11) * num3;
            FixInt num14 = vector.x - current.x;
            FixInt num15 = vector.y - current.y;
            FixInt num16 = num12 - vector.x;
            FixInt num17 = num13 - vector.y;
            if (num14 * num16 + num15 * num17 > 0f)
            {
                num12 = vector.x;
                num13 = vector.y;
                currentVelocity.x = (num12 - vector.x) / fixedDeltaTime;
                currentVelocity.y = (num13 - vector.y) / fixedDeltaTime;
            }

            return new FixIntVector2(num12, num13);
        }
        #endregion

        #region Operator

        public static FixIntVector2 operator +(FixIntVector2 a, FixIntVector2 b)
        {
            return new FixIntVector2(a.x + b.x, a.y + b.y);
        }


        public static FixIntVector2 operator -(FixIntVector2 a, FixIntVector2 b)
        {
            return new FixIntVector2(a.x - b.x, a.y - b.y);
        }


        public static FixIntVector2 operator *(FixIntVector2 a, FixIntVector2 b)
        {
            return new FixIntVector2(a.x * b.x, a.y * b.y);
        }


        public static FixIntVector2 operator /(FixIntVector2 a, FixIntVector2 b)
        {
            return new FixIntVector2(a.x / b.x, a.y / b.y);
        }


        public static FixIntVector2 operator -(FixIntVector2 a)
        {
            return new FixIntVector2(0f - a.x, 0f - a.y);
        }


        public static FixIntVector2 operator *(FixIntVector2 a, FixInt d)
        {
            return new FixIntVector2(a.x * d, a.y * d);
        }


        public static FixIntVector2 operator *(FixInt d, FixIntVector2 a)
        {
            return new FixIntVector2(a.x * d, a.y * d);
        }


        public static FixIntVector2 operator /(FixIntVector2 a, FixInt d)
        {
            return new FixIntVector2(a.x / d, a.y / d);
        }


        public static bool operator ==(FixIntVector2 lhs, FixIntVector2 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }


        public static bool operator !=(FixIntVector2 lhs, FixIntVector2 rhs)
        {
            return !(lhs == rhs);
        }


        public static implicit operator FixIntVector2(Vector3 v)
        {
            return new FixIntVector2(v.x, v.y);
        }


        public static implicit operator FixIntVector3(FixIntVector2 v)
        {
            return new FixIntVector3(v.x, v.y, 0f);
        }
        #endregion
    }
}