using System;

namespace FixIntMath
{
    public class FixIntRandomSeed
    {
        /// <summary>
        /// 随机种子
        /// </summary>
        public int SeedId { get; private set; }
        /// <summary>
        /// 随机数生成器
        /// </summary>
        private Random mRandomGenerator;
        /// <summary>
        /// 定点数随机种子随即器
        /// </summary>
        /// <param name="seedId">随机种子</param>
        public FixIntRandomSeed(int seedId)
        {
            this.SeedId = seedId;
            this.mRandomGenerator = new Random(seedId);
        }
        /// <summary>
        /// 在最小值min和最大值max之间随机一个数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public int Range(int min, int max)
        {
            return mRandomGenerator.Next(min, max);
        }
        /// <summary>
        /// 在最小值min和最大值max之间随机一个数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public int Range(FixInt min, FixInt max)
        {
            return mRandomGenerator.Next(min.IntValue, max.IntValue) / FixInt.MUTIPLE;
        }
    }
}