using System;

namespace ImageProcessor
{
    public static class ThresholdExtension
    {
        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="source"></param>
        /// <param name="threshold">阈值</param>
        /// <returns></returns>
        public static Bitmap Threshold(this Bitmap source, byte threshold)
        {
            if (!(source.Clone() is Bitmap bitmap))
            {
                return null;
            }
            switch (bitmap.Type)
            {
                case 1:
                    for (var i = 0; i < bitmap.Paletta.Length; i++)
                    {
                        bitmap.Paletta[i].rgbRed = (byte)(bitmap.Paletta[i].rgbRed > threshold ? 255 : 0);
                        bitmap.Paletta[i].rgbGreen = (byte)(bitmap.Paletta[i].rgbGreen > threshold ? 255 : 0);
                        bitmap.Paletta[i].rgbBlue = (byte)(bitmap.Paletta[i].rgbBlue > threshold ? 255 : 0);
                    }
                    break;
                case 2:
                    for (var i = 0; i < -bitmap.InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < bitmap.InfoHeader.biWidth; j++)
                        {
                            bitmap.RGBData[i][j].rgbtRed = (byte)(bitmap.RGBData[i][j].rgbtRed > threshold ? 255 : 0);
                            bitmap.RGBData[i][j].rgbtGreen = (byte)(bitmap.RGBData[i][j].rgbtGreen > threshold ? 255 : 0);
                            bitmap.RGBData[i][j].rgbtBlue = (byte)(bitmap.RGBData[i][j].rgbtBlue > threshold ? 255 : 0);
                        }
                    }
                    break;
                case 3:
                    for (var i = 0; i < -bitmap.InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < bitmap.InfoHeader.biWidth; j++)
                        {
                            bitmap.RGBAData[i][j].rgbRed = (byte)(bitmap.RGBAData[i][j].rgbRed > threshold ? 255 : 0);
                            bitmap.RGBAData[i][j].rgbGreen = (byte)(bitmap.RGBAData[i][j].rgbGreen > threshold ? 255 : 0);
                            bitmap.RGBAData[i][j].rgbBlue = (byte)(bitmap.RGBAData[i][j].rgbBlue > threshold ? 255 : 0);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("不支持的图像类型");
            }
            return bitmap;
        }
    }
}
