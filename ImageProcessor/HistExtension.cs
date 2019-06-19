using System;
using System.Linq;

namespace ImageProcessor
{
    public static class HistExtension
    {
        /// <summary>
        /// 计算直方图参数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static (double[] R, double[] G, double[] B) CalcHist(this Bitmap source)
        {
            var dictR = new int[256];
            var dictG = new int[256];
            var dictB = new int[256];
            for (var i = 0; i < 256; i++)
            {
                dictR[i] = 0;
                dictG[i] = 0;
                dictB[i] = 0;
            }
            //根据不同图像类型对 RGB 积分
            switch (source.Type)
            {
                case 1:
                    foreach (var i in source.IndexData)
                    {
                        foreach (var j in i)
                        {
                            dictR[source.Paletta[j].rgbRed]++;
                            dictG[source.Paletta[j].rgbGreen]++;
                            dictB[source.Paletta[j].rgbBlue]++;
                        }
                    }
                    break;
                case 2:
                    foreach (var i in source.RGBData)
                    {
                        foreach (var j in i)
                        {
                            dictR[j.rgbtRed]++;
                            dictG[j.rgbtGreen]++;
                            dictB[j.rgbtBlue]++;
                        }
                    }
                    break;
                case 3:
                    foreach (var i in source.RGBAData)
                    {
                        foreach (var j in i)
                        {
                            dictR[j.rgbRed]++;
                            dictG[j.rgbGreen]++;
                            dictB[j.rgbBlue]++;
                        }
                    }
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }
            return (dictR.Select(i => -Convert.ToDouble(i) / source.InfoHeader.biWidth / source.InfoHeader.biHeight).ToArray(),
                dictG.Select(i => -Convert.ToDouble(i) / source.InfoHeader.biWidth / source.InfoHeader.biHeight).ToArray(),
                dictB.Select(i => -Convert.ToDouble(i) / source.InfoHeader.biWidth / source.InfoHeader.biHeight).ToArray());
        }

        /// <summary>
        /// 直方图均衡化
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap EqualizeHist(this Bitmap source)
        {
            if (!(source.Clone() is Bitmap bitmap))
            {
                return null;
            }

            var (R, G, B) = bitmap.CalcHist(); // 计算直方图
            var Sr = new double[256];
            var Sg = new double[256];
            var Sb = new double[256];

            // 进行直方图均衡化

            for (var i = 0; i < 256; i++)
            {
                Sr[i] = i == 0 ? R[i] : Sr[i - 1] + R[i];
                Sg[i] = i == 0 ? G[i] : Sg[i - 1] + G[i];
                Sb[i] = i == 0 ? B[i] : Sb[i - 1] + B[i];
            }

            switch (bitmap.Type)
            {
                case 1:
                    for (var i = 0; i < bitmap.Paletta.Length; i++)
                    {
                        bitmap.Paletta[i].rgbRed = (byte)(Sr[bitmap.Paletta[i].rgbRed] * 255);
                        bitmap.Paletta[i].rgbGreen = (byte)(Sg[bitmap.Paletta[i].rgbGreen] * 255);
                        bitmap.Paletta[i].rgbBlue = (byte)(Sb[bitmap.Paletta[i].rgbBlue] * 255);
                    }
                    break;
                case 2:
                    for (var i = 0; i < -bitmap.InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < bitmap.InfoHeader.biWidth; j++)
                        {
                            bitmap.RGBData[i][j].rgbtRed = (byte)(Sr[bitmap.RGBData[i][j].rgbtRed] * 255);
                            bitmap.RGBData[i][j].rgbtGreen = (byte)(Sg[bitmap.RGBData[i][j].rgbtGreen] * 255);
                            bitmap.RGBData[i][j].rgbtBlue = (byte)(Sb[bitmap.RGBData[i][j].rgbtBlue] * 255);
                        }
                    }
                    break;
                case 3:
                    for (var i = 0; i < -bitmap.InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < bitmap.InfoHeader.biWidth; j++)
                        {
                            bitmap.RGBAData[i][j].rgbRed = (byte)(Sr[bitmap.RGBAData[i][j].rgbRed] * 255);
                            bitmap.RGBAData[i][j].rgbGreen = (byte)(Sg[bitmap.RGBAData[i][j].rgbGreen] * 255);
                            bitmap.RGBAData[i][j].rgbBlue = (byte)(Sb[bitmap.RGBAData[i][j].rgbBlue] * 255);
                        }
                    }
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }

            return bitmap;
        }
    }
}
