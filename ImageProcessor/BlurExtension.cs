using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageProcessor
{
    public static class BlurExtension
    {
        /// <summary>
        /// 添加椒盐噪声
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n">数量</param>
        /// <returns></returns>
        public static Bitmap AddSaltNoise(this Bitmap source, int n)
        {
            var listR = new List<List<byte>>();
            var listG = new List<List<byte>>();
            var listB = new List<List<byte>>();
            switch (source.Type)
            {
                case 1:
                    foreach (var i in source.IndexData)
                    {
                        listR.Add(i.Select(j => source.Paletta[j].rgbRed).ToList());
                        listG.Add(i.Select(j => source.Paletta[j].rgbGreen).ToList());
                        listB.Add(i.Select(j => source.Paletta[j].rgbBlue).ToList());
                    }
                    break;
                case 2:
                    foreach (var i in source.RGBData)
                    {
                        listR.Add(i.Select(j => j.rgbtRed).ToList());
                        listG.Add(i.Select(j => j.rgbtGreen).ToList());
                        listB.Add(i.Select(j => j.rgbtBlue).ToList());
                    }
                    break;
                case 3:
                    foreach (var i in source.RGBAData)
                    {
                        listR.Add(i.Select(j => j.rgbRed).ToList());
                        listG.Add(i.Select(j => j.rgbGreen).ToList());
                        listB.Add(i.Select(j => j.rgbBlue).ToList());
                    }
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }

            var bitmap = source.Clone() as Bitmap;
            List<List<byte>> AddSalt(List<List<byte>> img)
            {
                var rand = new Random();
                var height = img.Count;
                var width = img[0].Count;
                var dstImage = new byte[height][];
                for (var i = 0; i < dstImage.Length; i++)
                {
                    dstImage[i] = img[i].ToArray();
                }

                for (int k = 0; k < n; k++)
                {
                    int i = rand.Next() % height;
                    int j = rand.Next() % width;
                    dstImage[i][j] = 255;
                }

                for (int k = 0; k < n; k++)
                {
                    int i = rand.Next() % height;
                    int j = rand.Next() % width;
                    dstImage[i][j] = 0;
                }

                return dstImage.Select(i => i.ToList()).ToList();
            }
            listR = AddSalt(listR);
            if (!source.IsGray)
            {
                listB = AddSalt(listB);
                listG = AddSalt(listG);
            }

            switch (source.Type)
            {
                case 1:
                    var rgbs = listR.Select(
                        (elem1, index1) =>
                            elem1.Select((elem2, index2) =>
                                (
                                    listR[index1][index2],
                                    source.IsGray ? listR[index1][index2] : listG[index1][index2],
                                    source.IsGray ? listR[index1][index2] : listB[index1][index2]
                                )).ToArray()
                            ).ToArray();

                    (bitmap.IndexData, bitmap.Paletta) = PalettaExtension.RGBArrayToPaletta(rgbs);

                    break;
                case 2:
                    bitmap.RGBData = listR.Select(
                        (elem1, index1) =>
                            elem1.Select((elem2, index2) =>
                                new RGBTriple
                                {
                                    rgbtRed = listR[index1][index2],
                                    rgbtGreen = source.IsGray ? listR[index1][index2] : listG[index1][index2],
                                    rgbtBlue = source.IsGray ? listR[index1][index2] : listB[index1][index2]
                                }).ToArray()
                            ).ToArray();
                    break;
                case 3:
                    bitmap.RGBAData = listR.Select(
                        (elem1, index1) =>
                            elem1.Select((elem2, index2) =>
                                new RGBQuad
                                {
                                    rgbRed = listR[index1][index2],
                                    rgbGreen = source.IsGray ? listR[index1][index2] : listG[index1][index2],
                                    rgbBlue = source.IsGray ? listR[index1][index2] : listB[index1][index2]
                                }).ToArray()
                            ).ToArray();
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }
            return bitmap;
        }

        private static int FastMedianValueCl(List<List<byte>> imageBuf, int w, int h, int[] templt, int tw, int x, int y)
        {
            int k = 0;
            int px, py, mid = 0;
            int count_temp = 0;

            for (var i = 0; i < tw; i++)
            {
                py = y - tw / 2 + i;
                px = x + tw / 2 + 1;

                if (py >= 0 && py < h && px >= 0 && px < w)
                {
                    k = imageBuf[py][px];
                    templt[k]++;
                }

                px = x - tw / 2;

                if (py >= 0 && py < h && px >= 0 && px < w)
                {
                    k = imageBuf[py][px];
                    templt[k]--;
                }
            }
            for (var a = 0; a < 256; a++)
            {
                mid = a;
                count_temp += templt[a];
                if (count_temp > tw * tw / 2)
                {
                    break;
                }
            }

            return mid;
        }

        /// <summary>
        /// 应用中值滤波器
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static byte[][] ApplyMedianFilter(List<List<byte>> data, int size)
        {
            var h = data.Count;
            var w = data.First().Count;
            var result = new byte[h][];
            for (var i = 0; i < h; i++)
            {
                result[i] = new byte[w];
            }

            var templt = new int[256];

            int x, y, a;

            for (y = size / 2; y < h - size / 2; y++)
            {
                for (int i = 0; i < 256; i++)
                {
                    templt[i] = 0;
                }
                for (int i = y - size / 2; i < y + size / 2; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        int k = data[i][j];
                        templt[k]++;
                    }
                }
                for (x = size / 2 + 1; x < w - size / 2; x++)
                {
                    a = FastMedianValueCl(data, w, h, templt, size, x, y);

                    a = a > 255 ? 255 : a;
                    a = a < 0 ? 0 : a;
                    result[y][x] = (byte)a;
                }
            }

            return result;
        }

        /// <summary>
        /// 中值滤波
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Bitmap MedianFilter(this Bitmap source, int r)
        {
            var listR = new List<List<byte>>();
            var listG = new List<List<byte>>();
            var listB = new List<List<byte>>();
            switch (source.Type)
            {
                case 1:
                    foreach (var i in source.IndexData)
                    {
                        listR.Add(i.Select(j => source.Paletta[j].rgbRed).ToList());
                        listG.Add(i.Select(j => source.Paletta[j].rgbGreen).ToList());
                        listB.Add(i.Select(j => source.Paletta[j].rgbBlue).ToList());
                    }
                    break;
                case 2:
                    foreach (var i in source.RGBData)
                    {
                        listR.Add(i.Select(j => j.rgbtRed).ToList());
                        listG.Add(i.Select(j => j.rgbtGreen).ToList());
                        listB.Add(i.Select(j => j.rgbtBlue).ToList());
                    }
                    break;
                case 3:
                    foreach (var i in source.RGBAData)
                    {
                        listR.Add(i.Select(j => j.rgbRed).ToList());
                        listG.Add(i.Select(j => j.rgbGreen).ToList());
                        listB.Add(i.Select(j => j.rgbBlue).ToList());
                    }
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }

            // 对三通道进行中值滤波
            listR = ApplyMedianFilter(listR, r).Select(i => i.ToList()).ToList();
            listG = ApplyMedianFilter(listG, r).Select(i => i.ToList()).ToList();
            listB = ApplyMedianFilter(listB, r).Select(i => i.ToList()).ToList();

            if (!(source.Clone() is Bitmap bitmap))
            {
                return null;
            }

            switch (bitmap.Type)
            {
                case 1:
                    var rgbs = listR.Select(
                        (elem1, index1) =>
                            elem1.Select((elem2, index2) =>
                                (
                                    listR[index1][index2],
                                    listG[index1][index2],
                                    listB[index1][index2]
                                )).ToArray()
                            ).ToArray();

                    (bitmap.IndexData, bitmap.Paletta) = PalettaExtension.RGBArrayToPaletta(rgbs);
                    break;
                case 2:
                    bitmap.RGBData = listR.Select(
                        (elem1, index1) =>
                            elem1.Select((elem2, index2) =>
                                new RGBTriple
                                {
                                    rgbtRed = listR[index1][index2],
                                    rgbtGreen = listG[index1][index2],
                                    rgbtBlue = listB[index1][index2]
                                }).ToArray()
                            ).ToArray();
                    break;
                case 3:
                    bitmap.RGBAData = listR.Select(
                        (elem1, index1) =>
                            elem1.Select((elem2, index2) =>
                                new RGBQuad
                                {
                                    rgbRed = listR[index1][index2],
                                    rgbGreen = listG[index1][index2],
                                    rgbBlue = listB[index1][index2]
                                }).ToArray()
                            ).ToArray();
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }

            return bitmap;
        }
    }
}
