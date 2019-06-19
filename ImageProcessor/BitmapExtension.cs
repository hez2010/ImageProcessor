using System;
using System.Linq;

namespace ImageProcessor
{
    public static class BitmapExtension
    {
        /// <summary>
        /// 将图像转换为 24 位图
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo24Bitmap(this Bitmap source)
        {
            if (!(source.Clone() is Bitmap bitmap))
            {
                return null;
            }

            switch (bitmap.Type)
            {
                case 1:
                    bitmap.RGBData = bitmap.IndexData.Select(i =>
                            i.Select(j =>
                                new RGBTriple
                                {
                                    rgbtRed = bitmap.Paletta[j].rgbRed,
                                    rgbtGreen = bitmap.Paletta[j].rgbGreen,
                                    rgbtBlue = bitmap.Paletta[j].rgbBlue
                                }).ToArray())
                        .ToArray();
                    break;
                case 2:
                    break;
                case 3:
                    bitmap.RGBData = bitmap.RGBAData.Select(i =>
                            i.Select(j =>
                                new RGBTriple
                                {
                                    rgbtRed = j.rgbRed,
                                    rgbtBlue = j.rgbBlue,
                                    rgbtGreen = j.rgbGreen
                                }).ToArray())
                        .ToArray();
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }
            return bitmap;
        }
    }
}
