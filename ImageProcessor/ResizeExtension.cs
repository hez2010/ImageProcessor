using OpenCvSharp;
using System;
using System.IO;

namespace ImageProcessor
{
    public static class ResizeExtension
    {
        public static Bitmap Resize(this Bitmap source, Size size)
        {
            string fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
            File.WriteAllBytes(fileName, source.GetBitmapFileData());

            using (var img = Cv2.ImRead(fileName))
            {
                fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
                img.Resize(size).SaveImage(fileName);
            }
            return new Bitmap(fileName);
        }
    }
}
