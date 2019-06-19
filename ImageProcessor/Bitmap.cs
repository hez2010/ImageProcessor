using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace ImageProcessor
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RGBTriple
    {
        public byte rgbtBlue;
        public byte rgbtGreen;
        public byte rgbtRed;

        public static RGBTriple operator -(RGBTriple l, RGBTriple r)
        {
            return new RGBTriple
            {
                rgbtBlue = (byte)Math.Abs(l.rgbtBlue - r.rgbtBlue),
                rgbtGreen = (byte)Math.Abs(l.rgbtGreen - r.rgbtGreen),
                rgbtRed = (byte)Math.Abs(l.rgbtRed - r.rgbtRed)
            };
        }

        public static RGBTriple operator +(RGBTriple l, RGBTriple r)
        {
            return new RGBTriple
            {
                rgbtBlue = (byte)(l.rgbtBlue + r.rgbtBlue),
                rgbtGreen = (byte)(l.rgbtGreen + r.rgbtGreen),
                rgbtRed = (byte)(l.rgbtRed + r.rgbtRed)
            };
        }

        public static RGBTriple operator /(RGBTriple t, int frac)
        {
            return new RGBTriple
            {
                rgbtBlue = (byte)(t.rgbtBlue / frac),
                rgbtGreen = (byte)(t.rgbtGreen / frac),
                rgbtRed = (byte)(t.rgbtRed / frac)
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RGBQuad
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;

        public static RGBQuad operator -(RGBQuad l, RGBQuad r)
        {
            return new RGBQuad
            {
                rgbBlue = (byte)Math.Abs(l.rgbBlue - r.rgbBlue),
                rgbGreen = (byte)Math.Abs(l.rgbGreen - r.rgbGreen),
                rgbRed = (byte)Math.Abs(l.rgbRed - r.rgbRed)
            };
        }

        public static RGBQuad operator +(RGBQuad l, RGBQuad r)
        {
            return new RGBQuad
            {
                rgbBlue = (byte)(l.rgbBlue + r.rgbBlue),
                rgbGreen = (byte)(l.rgbGreen + r.rgbGreen),
                rgbRed = (byte)(l.rgbRed + r.rgbRed)
            };
        }
    }

    /// <summary>
    /// BMP 信息头
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    /// <summary>
    /// BMP 文件头
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapFileHeader
    {
        public ushort bfType;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    public class Bitmap : IDisposable, ICloneable // BITMAP 数据
    {
        /// <summary>
        /// 文件头
        /// </summary>
        public BitmapFileHeader FileHeader;
        /// <summary>
        /// 信息头
        /// </summary>
        public BitmapInfoHeader InfoHeader;
        /// <summary>
        /// 调色板
        /// </summary>
        public RGBQuad[] Paletta;
        /// <summary>
        /// 4 bytes RGB 数据
        /// </summary>
        public RGBQuad[][] RGBAData;
        /// <summary>
        /// 3 bytes RGB 数据
        /// </summary>
        public RGBTriple[][] RGBData;
        /// <summary>
        /// 调色板索引数据
        /// </summary>
        public ushort[][] IndexData;
        /// <summary>
        /// BMP 类型，1 -- 1/2/4/8/16 位图，2 -- 24 位图，3 -- 32 位图
        /// </summary>
        public int Type;
        /// <summary>
        /// 对齐时的补充字节数
        /// </summary>
        public int AlignSkip;
        /// <summary>
        /// 是否为灰度图
        /// </summary>
        public bool IsGray;
        /// <summary>
        /// 是否反向存储
        /// </summary>
        public bool IsReversed;

        private T ReadData<T>(FileStream fileStream) where T : struct
        {
            var length = Marshal.SizeOf<T>();
            var ptr = Marshal.AllocHGlobal(length);
            try
            {
                var data = new byte[length];
                fileStream.Read(data, 0, length);
                Marshal.Copy(data, 0, ptr, length);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 从文件流中读取数据
        /// </summary>
        /// <param name="fileStream"></param>
        private void ReadFromStream(FileStream fileStream)
        {
            fileStream.Seek(0, SeekOrigin.Begin); FileHeader = ReadData<BitmapFileHeader>(fileStream);
            InfoHeader = ReadData<BitmapInfoHeader>(fileStream);
            AlignSkip = 4 - ((InfoHeader.biWidth * InfoHeader.biBitCount) >> 3) & 3;
            switch (InfoHeader.biBitCount)
            {
                case 1:
                    {
                        IsGray = true;
                        var palettas = new List<RGBQuad>();
                        while (fileStream.Position < FileHeader.bfOffBits)
                        {
                            palettas.Add(ReadData<RGBQuad>(fileStream));
                        }
                        Paletta = palettas.ToArray();
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<byte>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<byte>());
                            for (var j = 0; j < InfoHeader.biWidth / 8; j++)
                            {
                                var value = ReadData<byte>(fileStream);
                                data[i].Add((byte)((value & 0b10000000) >> 7));
                                data[i].Add((byte)((value & 0b01000000) >> 6));
                                data[i].Add((byte)((value & 0b00100000) >> 5));
                                data[i].Add((byte)((value & 0b00010000) >> 4));
                                data[i].Add((byte)((value & 0b00001000) >> 3));
                                data[i].Add((byte)((value & 0b00000100) >> 2));
                                data[i].Add((byte)((value & 0b00000010) >> 1));
                                data[i].Add((byte)(value & 0b00000001));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        IndexData = data.Select(i => i.Select(j => (ushort)j).ToArray()).ToArray();
                        Type = 1;
                        break;
                    }
                case 2:
                    {
                        var palettas = new List<RGBQuad>();
                        while (fileStream.Position < FileHeader.bfOffBits)
                        {
                            palettas.Add(ReadData<RGBQuad>(fileStream));
                        }
                        Paletta = palettas.ToArray();
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<byte>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<byte>());
                            for (var j = 0; j < InfoHeader.biWidth / 4; j++)
                            {
                                var value = ReadData<byte>(fileStream);
                                data[i].Add((byte)((value & 0b11000000) >> 6));
                                data[i].Add((byte)((value & 0b00110000) >> 4));
                                data[i].Add((byte)((value & 0b00001100) >> 2));
                                data[i].Add((byte)(value & 0b00000011));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        IndexData = data.Select(i => i.Select(j => (ushort)j).ToArray()).ToArray();
                        Type = 1;
                        foreach (var i in palettas)
                        {
                            if (i.rgbBlue == i.rgbGreen && i.rgbGreen == i.rgbRed && i.rgbRed == i.rgbBlue)
                            {
                                IsGray = true;
                            }
                            else
                            {
                                IsGray = false;
                                break;
                            }
                        }
                        break;
                    }
                case 4:
                    {
                        var palettas = new List<RGBQuad>();
                        while (fileStream.Position < FileHeader.bfOffBits)
                        {
                            palettas.Add(ReadData<RGBQuad>(fileStream));
                        }
                        Paletta = palettas.ToArray();
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<byte>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<byte>());
                            for (var j = 0; j < InfoHeader.biWidth / 2; j++)
                            {
                                var value = ReadData<byte>(fileStream);
                                data[i].Add((byte)((value & 0b11110000) >> 4));
                                data[i].Add((byte)(value & 0b00001111));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        IndexData = data.Select(i => i.Select(j => (ushort)j).ToArray()).ToArray();
                        Type = 1;
                        foreach (var i in palettas)
                        {
                            if (i.rgbBlue == i.rgbGreen && i.rgbGreen == i.rgbRed && i.rgbRed == i.rgbBlue)
                            {
                                IsGray = true;
                            }
                            else
                            {
                                IsGray = false;
                                break;
                            }
                        }
                        break;
                    }
                case 8:
                    {
                        var palettas = new List<RGBQuad>();
                        while (fileStream.Position < FileHeader.bfOffBits)
                        {
                            palettas.Add(ReadData<RGBQuad>(fileStream));
                        }
                        Paletta = palettas.ToArray();
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<byte>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<byte>());
                            for (var j = 0; j < InfoHeader.biWidth; j++)
                            {
                                data[i].Add(ReadData<byte>(fileStream));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        IndexData = data.Select(i => i.Select(j => (ushort)j).ToArray()).ToArray();
                        Type = 1;
                        foreach (var i in palettas)
                        {
                            if (i.rgbBlue == i.rgbGreen && i.rgbGreen == i.rgbRed && i.rgbRed == i.rgbBlue)
                            {
                                IsGray = true;
                            }
                            else
                            {
                                IsGray = false;
                                break;
                            }
                        }
                        break;
                    }
                case 16:
                    {
                        var palettas = new List<RGBQuad>();
                        while (fileStream.Position < FileHeader.bfOffBits)
                        {
                            palettas.Add(ReadData<RGBQuad>(fileStream));
                        }
                        Paletta = palettas.ToArray();
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<ushort>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<ushort>());
                            for (var j = 0; j < InfoHeader.biWidth; j++)
                            {
                                data[i].Add(ReadData<ushort>(fileStream));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        IndexData = data.Select(i => i.ToArray()).ToArray();
                        Type = 1;
                        foreach (var i in palettas)
                        {
                            if (i.rgbBlue == i.rgbGreen && i.rgbGreen == i.rgbRed && i.rgbRed == i.rgbBlue)
                            {
                                IsGray = true;
                            }
                            else
                            {
                                IsGray = false;
                                break;
                            }
                        }
                        break;
                    }
                case 24:
                    {
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<RGBTriple>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<RGBTriple>());
                            for (var j = 0; j < InfoHeader.biWidth; j++)
                            {
                                data[i].Add(ReadData<RGBTriple>(fileStream));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        RGBData = data.Select(i => i.ToArray()).ToArray();
                        Type = 2;
                        var flag = false;
                        foreach (var j in RGBData)
                        {
                            foreach (var i in j)
                            {
                                if (i.rgbtBlue == i.rgbtGreen && i.rgbtGreen == i.rgbtRed && i.rgbtRed == i.rgbtBlue)
                                {
                                    IsGray = true;
                                }
                                else
                                {
                                    IsGray = false;
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                        break;
                    }
                case 32:
                    {
                        fileStream.Seek(FileHeader.bfOffBits, SeekOrigin.Begin);
                        var data = new List<List<RGBQuad>>();
                        for (var i = 0; i < Math.Abs(InfoHeader.biHeight); i++)
                        {
                            data.Add(new List<RGBQuad>());
                            for (var j = 0; j < InfoHeader.biWidth; j++)
                            {
                                data[i].Add(ReadData<RGBQuad>(fileStream));
                            }
                            fileStream.Seek(AlignSkip, SeekOrigin.Current);
                        }
                        if (InfoHeader.biHeight > 0)
                        {
                            data.Reverse();
                            InfoHeader.biHeight = -InfoHeader.biHeight;
                            IsReversed = true;
                        }
                        RGBAData = data.Select(i => i.ToArray()).ToArray();
                        Type = 3;
                        var flag = false;
                        foreach (var j in RGBAData)
                        {
                            foreach (var i in j)
                            {
                                if (i.rgbBlue == i.rgbGreen && i.rgbGreen == i.rgbRed && i.rgbRed == i.rgbBlue)
                                {
                                    IsGray = true;
                                }
                                else
                                {
                                    IsGray = false;
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("不支持的图像类型");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Paletta = null;
            RGBAData = null;
            RGBData = null;
            IndexData = null;
            GC.Collect();
        }

        private byte[] GetBytesFromStructure<T>(T structure) where T : struct
        {
            var length = Marshal.SizeOf<T>();
            var data = new byte[length];
            var ptr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, data, 0, length);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return data;
        }

        /// <summary>
        /// 从 Bitmap 对象得到 BMP 原始文件数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBitmapFileData()
        {
            var data = new List<byte>();
            data.AddRange(GetBytesFromStructure(FileHeader));
            data.AddRange(GetBytesFromStructure(InfoHeader));
            if (Paletta != null)
            {
                foreach (var i in Paletta)
                {
                    data.AddRange(GetBytesFromStructure(i));
                }
            }
            data.AddRange(Enumerable.Repeat((byte)0, (int)(FileHeader.bfOffBits - data.Count)));
            switch (Type)
            {
                case 1:
                    switch (InfoHeader.biBitCount)
                    {
                        case 1:
                            for (var i = 0; i < -InfoHeader.biHeight; i++)
                            {
                                for (var j = 0; j < InfoHeader.biWidth / 8; j++)
                                {
                                    var data1 = IndexData[i][j * 8];
                                    var data2 = IndexData[i][j * 8 + 1];
                                    var data3 = IndexData[i][j * 8 + 2];
                                    var data4 = IndexData[i][j * 8 + 3];
                                    var data5 = IndexData[i][j * 8 + 4];
                                    var data6 = IndexData[i][j * 8 + 5];
                                    var data7 = IndexData[i][j * 8 + 6];
                                    var data8 = IndexData[i][j * 8 + 7];
                                    var dataResult = data1;
                                    dataResult <<= 1;
                                    dataResult |= data2;
                                    dataResult <<= 1;
                                    dataResult |= data3;
                                    dataResult <<= 1;
                                    dataResult |= data4;
                                    dataResult <<= 1;
                                    dataResult |= data5;
                                    dataResult <<= 1;
                                    dataResult |= data6;
                                    dataResult <<= 1;
                                    dataResult |= data7;
                                    dataResult <<= 1;
                                    dataResult |= data8;
                                    data.Add((byte)dataResult);
                                }
                                data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                            }
                            break;
                        case 2:
                            for (var i = 0; i < -InfoHeader.biHeight; i++)
                            {
                                for (var j = 0; j < InfoHeader.biWidth / 4; j++)
                                {
                                    var data1 = IndexData[i][j * 4];
                                    var data2 = IndexData[i][j * 4 + 1];
                                    var data3 = IndexData[i][j * 4 + 2];
                                    var data4 = IndexData[i][j * 4 + 3];
                                    var dataResult = data1;
                                    dataResult <<= 2;
                                    dataResult |= data2;
                                    dataResult <<= 2;
                                    dataResult |= data3;
                                    dataResult <<= 2;
                                    dataResult |= data4;
                                    data.Add((byte)dataResult);
                                }
                                data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                            }
                            break;
                        case 4:
                            for (var i = 0; i < -InfoHeader.biHeight; i++)
                            {
                                for (var j = 0; j < InfoHeader.biWidth / 2; j++)
                                {
                                    var data1 = IndexData[i][j * 2];
                                    var data2 = IndexData[i][j * 2 + 1];
                                    data.Add((byte)((data1 << 4) | data2));
                                }
                                data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                            }
                            break;
                        case 8:
                            for (var i = 0; i < -InfoHeader.biHeight; i++)
                            {
                                for (var j = 0; j < InfoHeader.biWidth; j++)
                                {
                                    data.Add((byte)IndexData[i][j]);
                                }
                                data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                            }
                            break;
                        case 16:
                            for (var i = 0; i < -InfoHeader.biHeight; i++)
                            {
                                for (var j = 0; j < InfoHeader.biWidth; j++)
                                {
                                    data.AddRange(GetBytesFromStructure(IndexData[i][j]));
                                }
                                data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                            }
                            break;
                        default:
                            throw new NotSupportedException("不支持的图像类型");
                    }
                    break;
                case 2:
                    for (var i = 0; i < -InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < InfoHeader.biWidth; j++)
                        {
                            data.AddRange(GetBytesFromStructure(RGBData[i][j]));
                        }
                        data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                    }
                    break;
                case 3:
                    for (var i = 0; i < -InfoHeader.biHeight; i++)
                    {
                        for (var j = 0; j < InfoHeader.biWidth; j++)
                        {
                            data.AddRange(GetBytesFromStructure(RGBAData[i][j]));
                        }
                        data.AddRange(Enumerable.Repeat((byte)0, AlignSkip));
                    }
                    break;
                default:
                    throw new Exception("不支持的图像类型");
            }
            return data.ToArray();
        }

        /// <summary>
        /// 克隆图像
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            if (!(MemberwiseClone() is Bitmap bitmap))
            {
                throw new NullReferenceException("图像不可为 null");
            }

            if (bitmap.RGBData != null)
            {
                bitmap.RGBData = new RGBTriple[-InfoHeader.biHeight][];
                RGBData.CopyTo(bitmap.RGBData, 0);
                for (var i = 0; i < RGBData.Length; i++)
                {
                    bitmap.RGBData[i] = new RGBTriple[InfoHeader.biWidth];
                    RGBData[i].CopyTo(bitmap.RGBData[i], 0);
                }
            }
            if (bitmap.RGBAData != null)
            {
                bitmap.RGBAData = new RGBQuad[-InfoHeader.biHeight][];
                RGBAData.CopyTo(bitmap.RGBAData, 0);
                for (var i = 0; i < RGBAData.Length; i++)
                {
                    bitmap.RGBAData[i] = new RGBQuad[InfoHeader.biWidth];
                    RGBAData[i].CopyTo(bitmap.RGBAData[i], 0);
                }
            }
            if (bitmap.IndexData != null)
            {
                bitmap.IndexData = new ushort[-InfoHeader.biHeight][];
                IndexData.CopyTo(bitmap.IndexData, 0);
                for (var i = 0; i < IndexData.Length; i++)
                {
                    bitmap.IndexData[i] = new ushort[InfoHeader.biWidth];
                    IndexData[i].CopyTo(bitmap.IndexData[i], 0);
                }
            }
            if (bitmap.Paletta != null)
            {
                bitmap.Paletta = new RGBQuad[bitmap.Paletta.Length];
                Paletta.CopyTo(bitmap.Paletta, 0);
            }
            return bitmap;
        }

        public Bitmap(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ReadFromStream(fileStream);
            }
        }

        public Bitmap(FileStream fileStream)
        {
            ReadFromStream(fileStream);
        }
    }
}
