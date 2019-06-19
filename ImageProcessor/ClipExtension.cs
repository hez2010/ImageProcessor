using OpenCvSharp;
using System;
using System.IO;

namespace ImageProcessor
{
    public static class ClipExtension
    {
        /// <summary>
        /// 图像裁剪
        /// </summary>
        /// <param name="source"></param>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap Clip(this Bitmap source, Point point, Size size)
        {
            string fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
            File.WriteAllBytes(fileName, source.GetBitmapFileData());

            using (var img = Cv2.ImRead(fileName))
            {
                fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
                img.Clone(new Rect(point, size)).SaveImage(fileName);
            }
            return new Bitmap(fileName);
        }
    }
}
