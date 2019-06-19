using System;

namespace ImageProcessor
{
    public static class PlusMinusExtension
    {
        /// <summary>
        /// 图像相加
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bitmap2"></param>
        /// <returns></returns>
        public static Bitmap Plus(this Bitmap source, Bitmap bitmap2, int frac = 2)
        {
            // 转换为 24 位图
            var bitmap = source.ConvertTo24Bitmap();
            bitmap2 = bitmap2.ConvertTo24Bitmap();

            for (var i = 0; i < Math.Min(-bitmap.InfoHeader.biHeight, -bitmap2.InfoHeader.biHeight); i++)
            {
                for (var j = 0; j < Math.Min(bitmap.InfoHeader.biWidth, bitmap2.InfoHeader.biWidth); j++)
                {
                    bitmap.RGBData[i][j] += bitmap2.RGBData[i][j] / frac;
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 图像相减
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bitmap2"></param>
        /// <returns></returns>
        public static Bitmap Minus(this Bitmap source, Bitmap bitmap2)
        {
            // 转换为 24 位图
            var bitmap = source.ConvertTo24Bitmap();
            bitmap2 = bitmap2.ConvertTo24Bitmap();

            for (var i = 0; i < Math.Min(-bitmap.InfoHeader.biHeight, -bitmap2.InfoHeader.biHeight); i++)
            {
                for (var j = 0; j < Math.Min(bitmap.InfoHeader.biWidth, bitmap2.InfoHeader.biWidth); j++)
                {
                    bitmap.RGBData[i][j] -= bitmap2.RGBData[i][j];
                }
            }
            return bitmap;
        }
    }
}
