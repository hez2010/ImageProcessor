using OpenCvSharp;
using System;
using System.IO;

namespace ImageProcessor
{
    public static class MorphExtension
    {
        /// <summary>
        /// 腐蚀
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size">核大小</param>
        /// <returns></returns>
        public static Bitmap Ercode(this Bitmap source, Size size)
        {
            string fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
            File.WriteAllBytes(fileName, source.GetBitmapFileData());
            using (var img = Cv2.ImRead(fileName))
            {
                var erodeElement = Cv2.GetStructuringElement(MorphShapes.Rect, size);
                fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
                img.Erode(erodeElement).SaveImage(fileName);
            }
            return new Bitmap(fileName);
        }

        /// <summary>
        /// 膨胀
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size">核大小</param>
        /// <returns></returns>
        public static Bitmap Dilate(this Bitmap source, Size size)
        {
            string fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
            File.WriteAllBytes(fileName, source.GetBitmapFileData());
            using (var img = Cv2.ImRead(fileName))
            {
                var dilateElement = Cv2.GetStructuringElement(MorphShapes.Rect, size);
                fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
                img.Dilate(dilateElement).SaveImage(fileName);
            }
            return new Bitmap(fileName);
        }
    }
}
