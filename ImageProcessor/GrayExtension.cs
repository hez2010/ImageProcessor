using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    public static class GrayExtension
    {
        /// <summary>
        /// RGB 图像转灰度图
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap RGB2Gray(this Bitmap source)
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
                        var rgb = bitmap.Paletta[i];
                        var gray = (rgb.rgbRed * 38 + rgb.rgbGreen * 75 + rgb.rgbBlue * 15) >> 7;
                        bitmap.Paletta[i].rgbRed = bitmap.Paletta[i].rgbGreen = bitmap.Paletta[i].rgbBlue = (byte)gray;
                    }
                    break;
                case 2:
                    for (var i = 0; i < -bitmap.InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < bitmap.InfoHeader.biWidth; j++)
                        {
                            var rgb = bitmap.RGBData[i][j];
                            var gray = (rgb.rgbtRed * 38 + rgb.rgbtGreen * 75 + rgb.rgbtBlue * 15) >> 7;
                            bitmap.RGBData[i][j].rgbtBlue = bitmap.RGBData[i][j].rgbtGreen = bitmap.RGBData[i][j].rgbtRed = (byte)gray;
                        }
                    }
                    break;
                case 3:
                    for (var i = 0; i < -bitmap.InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < bitmap.InfoHeader.biWidth; j++)
                        {
                            var rgba = bitmap.RGBAData[i][j];
                            var gray = (rgba.rgbRed * 38 + rgba.rgbGreen * 75 + rgba.rgbBlue * 15) >> 7;
                            bitmap.RGBAData[i][j].rgbBlue = bitmap.RGBAData[i][j].rgbGreen = bitmap.RGBAData[i][j].rgbRed = (byte)gray;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("不支持的图像类型");
            }

            // 标记灰度属性
            bitmap.IsGray = true;
            return bitmap;
        }
    }
}
