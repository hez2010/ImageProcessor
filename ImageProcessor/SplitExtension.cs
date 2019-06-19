using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    public static class SplitExtension
    {
        /// <summary>
        /// 大津方法获取自动阈值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte GetAutomaticThreshold(this Bitmap source)
        {
            var bitmap = source.ConvertTo24Bitmap().RGB2Gray();
            var p = new double[256];
            double mn = -bitmap.InfoHeader.biHeight * bitmap.InfoHeader.biWidth;
            var rgbs = bitmap.RGBData;
            for (var i = 0; i < 256; i++)
            {
                p[i] = rgbs.Sum(x => x.Count(y => y.rgbtBlue == i)) / mn;
            }
            var w0 = new double[256];
            var N0 = new double[256];
            w0[0] = p[0];
            N0[0] = w0[0] * mn;
            for (var i = 1; i < 256; i++)
            {
                w0[i] = w0[i - 1] + p[i];
                N0[i] = w0[i] * mn;
            }

            var w1 = new double[256];
            var N1 = new double[256];
            w1[255] = p[255];
            N1[255] = w1[255] * mn;
            for (var i = 254; i >= 0; i--)
            {
                w1[i] = w1[i + 1] + p[i];
                N1[i] = w1[i] * mn;
            }

            var u0 = new double[256];
            for (var i = 0; i < 256; i++)
            {
                double ans = 0;
                for (var j = 0; j <= i; j++)
                {
                    ans += j * p[j] / w0[i];
                }
                u0[i] = ans;
            }
            var u1 = new double[256];
            for (var i = 255; i >= 0; i--)
            {
                double ans = 0;
                for (var j = 255; j > i; j--)
                {
                    ans += j * p[j] / w1[i];
                }
                u1[i] = ans;
            }

            var u = new double[256];
            for (var i = 0; i < 256; i++)
            {
                u[i] = w0[i] * u0[i] + w1[i] * u1[i];
            }

            var g = new (int Index, double Value)[256];
            for (var i = 0; i < 256; i++)
            {
                g[i] = (i, w0[i] * Math.Pow(u0[i] - u[i], 2) + w1[i] * Math.Pow(u1[i] - u[i], 2));
            }

            return Convert.ToByte(g.OrderByDescending(i => i.Value).First().Index);
        }
    }
}
