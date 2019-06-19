namespace ImageProcessor
{
    public static class ConvolutionExtension
    {
        /// <summary>
        /// 应用卷积操作
        /// </summary>
        /// <param name="source"></param>
        /// <param name="kernel">卷积核</param>
        /// <param name="size">卷积核大小</param>
        /// <returns></returns>
        public static Bitmap ApplyConvolution(this Bitmap source, int[][] kernel, int size)
        {
            // 转换为 24 位图
            var bitmap = source.ConvertTo24Bitmap();
            var backup = source.ConvertTo24Bitmap();
            var offset = size / 2;
            for (var i = offset; i < -bitmap.InfoHeader.biHeight - offset; i++)
            {
                for (var j = offset; j < bitmap.InfoHeader.biWidth - offset; j++)
                {
                    var sumB = 0;
                    var sumG = 0;
                    var sumR = 0;
                    for (var a = -offset; a <= offset; a++)
                    {
                        for (var b = -offset; b <= offset; b++)
                        {
                            sumB += kernel[offset + a][offset + b] * backup.RGBData[i + a][j + b].rgbtBlue;
                            sumG += kernel[offset + a][offset + b] * backup.RGBData[i + a][j + b].rgbtGreen;
                            sumR += kernel[offset + a][offset + b] * backup.RGBData[i + a][j + b].rgbtRed;
                        }
                    }

                    // 灰度边界处理
                    if (sumB > 255)
                    {
                        sumB = 255;
                    }

                    if (sumG > 255)
                    {
                        sumG = 255;
                    }

                    if (sumR > 255)
                    {
                        sumR = 255;
                    }

                    if (sumB < 0)
                    {
                        sumB = 0;
                    }

                    if (sumG < 0)
                    {
                        sumG = 0;
                    }

                    if (sumR < 0)
                    {
                        sumR = 0;
                    }

                    bitmap.RGBData[i][j].rgbtBlue = (byte)sumB;
                    bitmap.RGBData[i][j].rgbtGreen = (byte)sumG;
                    bitmap.RGBData[i][j].rgbtRed = (byte)sumR;
                }
            }
            backup.Dispose();

            return bitmap;
        }
    }
}
