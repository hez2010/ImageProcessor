using System.Collections.Generic;
using System.Linq;

namespace ImageProcessor
{
    public static class PalettaExtension
    {
        /// <summary>
        /// RGB 数据转调色板+索引数据
        /// </summary>
        /// <param name="rgbs"></param>
        /// <returns></returns>
        public static (ushort[][] Indexes, RGBQuad[] Palettas) RGBArrayToPaletta((byte, byte, byte)[][] rgbs)
        {
            var rgbList = new List<(byte, byte, byte)>();
            foreach (var i in rgbs)
            {
                rgbList.AddRange(i);
            }
            var disted = rgbList.Distinct().ToList();
            var dict = new Dictionary<(byte, byte, byte), int>();

            for (var i = 0; i < disted.Count; i++)
            {
                dict[disted[i]] = i;
            }

            var paletta = disted.Select(i => new RGBQuad { rgbRed = i.Item1, rgbGreen = i.Item2, rgbBlue = i.Item3 }).ToArray();
            var indexData = new ushort[rgbs.Length][];
            for (var i = 0; i < rgbs.Length; i++)
            {
                indexData[i] = new ushort[rgbs[i].Length];
            }

            for (var i = 0; i < rgbs.Length; i++)
            {
                for (var j = 0; j < rgbs[0].Length; j++)
                {
                    indexData[i][j] = (ushort)dict[rgbs[i][j]];
                }
            }

            return (indexData, paletta);
        }
    }
}
