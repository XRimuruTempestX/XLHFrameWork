using System;
using System.Globalization;
using UnityEngine;

namespace FixIntMath
{
    public struct FixIntVector3 : IEquatable<FixIntVector3>, IFormattable
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

        //
        // 摘要:
        //     Z component of the vector.
        public FixInt z;

        private static readonly FixIntVector3 zeroVector = new FixIntVector3(0f, 0f, 0f);

        private static readonly FixIntVector3 oneVector = new FixIntVector3(1f, 1f, 1f);

        private static readonly FixIntVector3 upVector = new FixIntVector3(0f, 1f, 0f);

        private static readonly FixIntVector3 downVector = new FixIntVector3(0f, -1f, 0f);

        private static readonly FixIntVector3 leftVector = new FixIntVector3(-1f, 0f, 0f);

        private static readonly FixIntVector3 rightVector = new FixIntVector3(1f, 0f, 0f);

        private static readonly FixIntVector3 forwardVector = new FixIntVector3(0f, 0f, 1f);

        private static readonly FixIntVector3 backVector = new FixIntVector3(0f, 0f, -1f);

        public FixInt this[int index]
        {

            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    _ => throw new IndexOutOfRangeException("Invalid FixIntVector3 index!"),
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
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid FixIntVector3 index!");
                }
            }
        }

        //
        // 摘要:
        //     Returns this vector with a magnitude of 1 (Read Only).
        public FixIntVector3 normalized
        {

            get
            {
                return Normalize(this);
            }
        }

        //
        // 摘要:
        //     Returns the length of this vector (Read Only).
        public FixInt magnitude
        {

            get
            {
                return (FixInt)FixIntMath.Sqrt(x * x + y * y + z * z);
            }
        }

        //
        // 摘要:
        //     Returns the squared length of this vector (Read Only).
        public FixInt sqrMagnitude
        {

            get
            {
                return x * x + y * y + z * z;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(0, 0, 0).
        public static FixIntVector3 zero
        {

            get
            {
                return zeroVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(1, 1, 1).
        public static FixIntVector3 one
        {

            get
            {
                return oneVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(0, 0, 1).
        public static FixIntVector3 forward
        {

            get
            {
                return forwardVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(0, 0, -1).
        public static FixIntVector3 back
        {

            get
            {
                return backVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(0, 1, 0).
        public static FixIntVector3 up
        {

            get
            {
                return upVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(0, -1, 0).
        public static FixIntVector3 down
        {

            get
            {
                return downVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(-1, 0, 0).
        public static FixIntVector3 left
        {

            get
            {
                return leftVector;
            }
        }

        //
        // 摘要:
        //     Shorthand for writing FixIntVector3(1, 0, 0).
        public static FixIntVector3 right
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
        //     Creates a new vector with given x, y, z components.
        //
        // 参数:
        //   x:
        //
        //   y:
        //
        //   z:

        public FixIntVector3(FixInt x, FixInt y, FixInt z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public FixIntVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public FixIntVector3(UnityEngine.Vector3 v3)
        {
            this.x = v3.x;
            this.y = v3.y;
            this.z = v3.z;
        }
        //
        // 摘要:
        //     Creates a new vector with given x, y components and sets z to zero.
        //
        // 参数:
        //   x:
        //
        //   y:

        public FixIntVector3(FixInt x, FixInt y)
        {
            this.x = x;
            this.y = y;
            z = 0f;
        }
        public FixIntVector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            z = 0f;
        }
        public FixIntVector3(UnityEngine.Vector2 v2)
        {
            this.x = v2.x;
            this.y = v2.y;
            this.z = 0;
        }
        //
        // 摘要:
        //     Set x, y and z components of an existing FixIntVector3.
        //
        // 参数:
        //   newX:
        //
        //   newY:
        //
        //   newZ:
        #endregion

        #region Public Method
        //
        // 摘要:
        //     Linearly interpolates between two points.
        //
        // 参数:
        //   a:
        //     Start value, returned when t = 0.
        //
        //   b:
        //     End value, returned when t = 1.
        //
        //   t:
        //     Value used to interpolate between a and b.
        //
        // 返回结果:
        //     Interpolated value, equals to a + (b - a) * t.

        public static FixIntVector3 Lerp(FixIntVector3 a, FixIntVector3 b, FixInt t)
        {
            t = FixIntMath.Clamp01(t);
            return new FixIntVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        //
        // 摘要:
        //     Linearly interpolates between two vectors.
        //
        // 参数:
        //   a:
        //
        //   b:
        //
        //   t:

        public static FixIntVector3 LerpUnclamped(FixIntVector3 a, FixIntVector3 b, FixInt t)
        {
            return new FixIntVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        //
        // 摘要:
        //     Calculate a position between the points specified by current and target, moving
        //     no farther than the distance specified by maxDistanceDelta.
        //
        // 参数:
        //   current:
        //     The position to move from.
        //
        //   target:
        //     The position to move towards.
        //
        //   maxDistanceDelta:
        //     Distance to move current per call.
        //
        // 返回结果:
        //     The new position.

        public static FixIntVector3 MoveTowards(FixIntVector3 current, FixIntVector3 target, FixInt maxDistanceDelta)
        {
            FixInt num = target.x - current.x;
            FixInt num2 = target.y - current.y;
            FixInt num3 = target.z - current.z;
            FixInt num4 = num * num + num2 * num2 + num3 * num3;
            if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            FixInt num5 = (FixInt)FixIntMath.Sqrt(num4);
            return new FixIntVector3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
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
        public static FixIntVector3 SmoothDampfv3(FixIntVector3 current, FixIntVector3 target, ref FixIntVector3 currentVelocity, FixInt smoothTime,  FixInt deltaTime, int maxSpeed=999999)
        {
            FixInt num = 0f;
            FixInt num2 = 0f;
            FixInt num3 = 0f;
            smoothTime = FixIntMath.Max(0.0001f, smoothTime);
            FixInt num4 = 2f / smoothTime;
            FixInt num5 = num4 * deltaTime;
            FixInt num6 = 1f / (1f + num5 + 0.48f * num5 * num5 + 0.235f * num5 * num5 * num5);
            FixInt num7 = current.x - target.x;
            FixInt num8 = current.y - target.y;
            FixInt num9 = current.z - target.z;
            FixIntVector3 vector = target;
            FixInt num10 = maxSpeed * smoothTime;
            FixInt num11 = num10 * num10;
            FixInt num12 = num7 * num7 + num8 * num8 + num9 * num9;
            if (num12 > num11)
            {
                FixInt num13 = (FixInt)FixIntMath.Sqrt(num12);
                num7 = num7 / num13 * num10;
                num8 = num8 / num13 * num10;
                num9 = num9 / num13 * num10;
            }

            target.x = current.x - num7;
            target.y = current.y - num8;
            target.z = current.z - num9;
            FixInt num14 = (currentVelocity.x + num4 * num7) * deltaTime;
            FixInt num15 = (currentVelocity.y + num4 * num8) * deltaTime;
            FixInt num16 = (currentVelocity.z + num4 * num9) * deltaTime;
            currentVelocity.x = (currentVelocity.x - num4 * num14) * num6;
            currentVelocity.y = (currentVelocity.y - num4 * num15) * num6;
            currentVelocity.z = (currentVelocity.z - num4 * num16) * num6;
            num = target.x + (num7 + num14) * num6;
            num2 = target.y + (num8 + num15) * num6;
            num3 = target.z + (num9 + num16) * num6;
            FixInt num17 = vector.x - current.x;
            FixInt num18 = vector.y - current.y;
            FixInt num19 = vector.z - current.z;
            FixInt num20 = num - vector.x;
            FixInt num21 = num2 - vector.y;
            FixInt num22 = num3 - vector.z;
            if (num17 * num20 + num18 * num21 + num19 * num22 > 0f)
            {
                num = vector.x;
                num2 = vector.y;
                num3 = vector.z;
                currentVelocity.x = (num - vector.x) / deltaTime;
                currentVelocity.y = (num2 - vector.y) / deltaTime;
                currentVelocity.z = (num3 - vector.z) / deltaTime;
            }

            return new FixIntVector3(num, num2, num3);
        }
 

  
        public void Set(FixInt newX, FixInt newY, FixInt newZ)
        {
            x = newX;
            y = newY;
            z = newZ;
        }

        //
        // 摘要:
        //     Multiplies two vectors component-wise.
        //
        // 参数:
        //   a:
        //
        //   b:

        public static FixIntVector3 Scale(FixIntVector3 a, FixIntVector3 b)
        {
            return new FixIntVector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        //
        // 摘要:
        //     Multiplies every component of this vector by the same component of scale.
        //
        // 参数:
        //   scale:

        public void Scale(FixIntVector3 scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        //
        // 摘要:
        //     Cross Product of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixIntVector3 Cross(FixIntVector3 lhs, FixIntVector3 rhs)
        {
            return new FixIntVector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }


        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        //
        // 摘要:
        //     Returns true if the given vector is exactly equal to this vector.
        //
        // 参数:
        //   other:

        public override bool Equals(object other)
        {
            if (other is FixIntVector3 other2)
            {
                return Equals(other2);
            }

            return false;
        }


        public bool Equals(FixIntVector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        //
        // 摘要:
        //     Reflects a vector off the plane defined by a normal.
        //
        // 参数:
        //   inDirection:
        //
        //   inNormal:

        public static FixIntVector3 Reflect(FixIntVector3 inDirection, FixIntVector3 inNormal)
        {
            FixInt num = -2f * Dot(inNormal, inDirection);
            return new FixIntVector3(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y, num * inNormal.z + inDirection.z);
        }

        //
        // 摘要:
        //     Makes this vector have a magnitude of 1.
        //
        // 参数:
        //   value:

        public static FixIntVector3 Normalize(FixIntVector3 value)
        {
            FixInt num = Magnitude(value);
            if (num > 1E-05f)
            {
                return value / num;
            }

            return zero;
        }


        public void Normalize()
        {
            FixInt num = Magnitude(this);
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
        //     Dot Product of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixInt Dot(FixIntVector3 lhs, FixIntVector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        //
        // 摘要:
        //     Projects a vector onto another vector.
        //
        // 参数:
        //   vector:
        //
        //   onNormal:

        public static FixIntVector3 Project(FixIntVector3 vector, FixIntVector3 onNormal)
        {
            FixInt num = Dot(onNormal, onNormal);
            if (num < float.Epsilon)
            {
                return zero;
            }

            FixInt num2 = Dot(vector, onNormal);
            return new FixIntVector3(onNormal.x * num2 / num, onNormal.y * num2 / num, onNormal.z * num2 / num);
        }

        //
        // 摘要:
        //     Projects a vector onto a plane defined by a normal orthogonal to the plane.
        //
        // 参数:
        //   planeNormal:
        //     The direction from the vector towards the plane.
        //
        //   vector:
        //     The location of the vector above the plane.
        //
        // 返回结果:
        //     The location of the vector on the plane.

        public static FixIntVector3 ProjectOnPlane(FixIntVector3 vector, FixIntVector3 planeNormal)
        {
            FixInt num = Dot(planeNormal, planeNormal);
            if (num < float.Epsilon)
            {
                return vector;
            }

            FixInt num2 = Dot(vector, planeNormal);
            return new FixIntVector3(vector.x - planeNormal.x * num2 / num, vector.y - planeNormal.y * num2 / num, vector.z - planeNormal.z * num2 / num);
        }

        //
        // 摘要:
        //     Calculates the angle between vectors from and.
        //
        // 参数:
        //   from:
        //     The vector from which the angular difference is measured.
        //
        //   to:
        //     The vector to which the angular difference is measured.
        //
        // 返回结果:
        //     The angle in degrees between the two vectors.

        public static FixInt Angle(FixIntVector3 from, FixIntVector3 to)
        {
            FixInt num = (FixInt)FixIntMath.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (num < 1E-15f)
            {
                return 0f;
            }

            FixInt num2 = FixIntMath.Clamp(Dot(from, to) / num, -1f, 1f);
            return (FixInt)FixIntMath.Acos(num2) * 57.29578f;
        }

        //
        // 摘要:
        //     Calculates the signed angle between vectors from and to in relation to axis.
        //
        //
        // 参数:
        //   from:
        //     The vector from which the angular difference is measured.
        //
        //   to:
        //     The vector to which the angular difference is measured.
        //
        //   axis:
        //     A vector around which the other vectors are rotated.
        //
        // 返回结果:
        //     Returns the signed angle between from and to in degrees.

        public static FixInt SignedAngle(FixIntVector3 from, FixIntVector3 to, FixIntVector3 axis)
        {
            FixInt num = Angle(from, to);
            FixInt num2 = from.y * to.z - from.z * to.y;
            FixInt num3 = from.z * to.x - from.x * to.z;
            FixInt num4 = from.x * to.y - from.y * to.x;
            FixInt num5 = FixIntMath.Sign(axis.x * num2 + axis.y * num3 + axis.z * num4);
            return num * num5;
        }

        //
        // 摘要:
        //     Returns the distance between a and b.
        //
        // 参数:
        //   a:
        //
        //   b:

        public static FixInt Distance(FixIntVector3 a, FixIntVector3 b)
        {
            FixInt num = a.x - b.x;
            FixInt num2 = a.y - b.y;
            FixInt num3 = a.z - b.z;
            return FixIntMath.Sqrt(num * num + num2 * num2 + num3 * num3);
        }

        //
        // 摘要:
        //     Returns a copy of vector with its magnitude clamped to maxLength.
        //
        // 参数:
        //   vector:
        //
        //   maxLength:

        public static FixIntVector3 ClampMagnitude(FixIntVector3 vector, FixInt maxLength)
        {
            FixInt num = vector.sqrMagnitude;
            if (num > maxLength * maxLength)
            {
                FixInt num2 = (FixInt)FixIntMath.Sqrt(num);
                FixInt num3 = vector.x / num2;
                FixInt num4 = vector.y / num2;
                FixInt num5 = vector.z / num2;
                return new FixIntVector3(num3 * maxLength, num4 * maxLength, num5 * maxLength);
            }

            return vector;
        }


        public static FixInt Magnitude(FixIntVector3 vector)
        {
            return (FixInt)FixIntMath.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }


        public static FixInt SqrMagnitude(FixIntVector3 vector)
        {
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        //
        // 摘要:
        //     Returns a vector that is made from the smallest components of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixIntVector3 Min(FixIntVector3 lhs, FixIntVector3 rhs)
        {
            return new FixIntVector3(FixIntMath.Min(lhs.x, rhs.x), FixIntMath.Min(lhs.y, rhs.y), FixIntMath.Min(lhs.z, rhs.z));
        }

        //
        // 摘要:
        //     Returns a vector that is made from the largest components of two vectors.
        //
        // 参数:
        //   lhs:
        //
        //   rhs:

        public static FixIntVector3 Max(FixIntVector3 lhs, FixIntVector3 rhs)
        {
            return new FixIntVector3(FixIntMath.Max(lhs.x, rhs.x), FixIntMath.Max(lhs.y, rhs.y), FixIntMath.Max(lhs.z, rhs.z));
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

            return string.Format(formatProvider, "({0}, {1}, {2})", x.ToString(), y.ToString(), z.ToString());
        }
        /// <summary>
        /// FixIntVectors Convert UnityEngine.Vector2
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x.RawFloat, y.RawFloat, z.RawFloat);
        }
        public static FixInt AngleBetween(FixIntVector3 from, FixIntVector3 to)
        {
            return FixIntMath.Acos(FixIntMath.Clamp(Dot(from.normalized, to.normalized), -1f, 1f));
        }

        public static FixIntVector3 Exclude(FixIntVector3 excludeThis, FixIntVector3 fromThat)
        {
            return ProjectOnPlane(fromThat, excludeThis);
        }
        #endregion

        #region Operator

        public static FixIntVector3 operator +(FixIntVector3 a, FixIntVector3 b)
        {
            return new FixIntVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }


        public static FixIntVector3 operator -(FixIntVector3 a, FixIntVector3 b)
        {
            return new FixIntVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }


        public static FixIntVector3 operator -(FixIntVector3 a)
        {
            return new FixIntVector3(0f - a.x, 0f - a.y, 0f - a.z);
        }


        public static FixIntVector3 operator *(FixIntVector3 a, FixInt d)
        {
            return new FixIntVector3(a.x * d, a.y * d, a.z * d);
        }


        public static FixIntVector3 operator *(FixInt d, FixIntVector3 a)
        {
            return new FixIntVector3(a.x * d, a.y * d, a.z * d);
        }


        public static FixIntVector3 operator /(FixIntVector3 a, FixInt d)
        {
            return new FixIntVector3(a.x / d, a.y / d, a.z / d);
        }


        public static bool operator ==(FixIntVector3 lhs, FixIntVector3 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }


        public static bool operator !=(FixIntVector3 lhs, FixIntVector3 rhs)
        {
            return !(lhs == rhs);
        }
        #endregion
    }
}