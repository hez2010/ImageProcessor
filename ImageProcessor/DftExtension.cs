using OpenCvSharp;
using System;
using System.IO;

namespace ImageProcessor
{
    public static class DftExtension
    {
        /// <summary>
        /// 离散傅里叶变换
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap Dft(this Bitmap source)
        {
            var fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";
            File.WriteAllBytes(fileName, source.GetBitmapFileData());

            using (var img = Cv2.ImRead(fileName, ImreadModes.Grayscale))
            {
                fileName = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".bmp";

                var padded = new Mat();

                // 计算最佳 DFT 尺寸
                var m = Cv2.GetOptimalDFTSize(img.Rows);
                var n = Cv2.GetOptimalDFTSize(img.Cols);
                Cv2.CopyMakeBorder(img, padded, 0, m - img.Rows, 0, n - img.Cols, BorderTypes.Constant, Scalar.All(0));

                var paddedF32 = new Mat();
                padded.ConvertTo(paddedF32, MatType.CV_32F);
                Mat[] planes = { paddedF32, Mat.Zeros(padded.Size(), MatType.CV_32F) };
                var complex = new Mat();
                Cv2.Merge(planes, complex);

                // 执行 dft
                var dft = new Mat();
                Cv2.Dft(complex, dft);

                // log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
                Cv2.Split(dft, out var dftPlanes); // planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))

                // planes[0] = magnitude
                var magnitude = new Mat();
                Cv2.Magnitude(dftPlanes[0], dftPlanes[1], magnitude);

                magnitude += Scalar.All(1);
                Cv2.Log(magnitude, magnitude);

                var spectrum = magnitude[
                    new Rect(0, 0, magnitude.Cols & -2, magnitude.Rows & -2)];

                // 交换象限
                var cx = spectrum.Cols / 2;
                var cy = spectrum.Rows / 2;

                var q0 = new Mat(spectrum, new Rect(0, 0, cx, cy)); // 左上
                var q1 = new Mat(spectrum, new Rect(cx, 0, cx, cy)); // 右上
                var q2 = new Mat(spectrum, new Rect(0, cy, cx, cy)); // 左下
                var q3 = new Mat(spectrum, new Rect(cx, cy, cx, cy)); // 右下

                var tmp = new Mat();
                q0.CopyTo(tmp);
                q3.CopyTo(q0);
                tmp.CopyTo(q3);
                q1.CopyTo(tmp);
                q2.CopyTo(q1);
                tmp.CopyTo(q2);

                // 归一化到 0~255
                Cv2.Normalize(spectrum, spectrum, 0, 255, NormTypes.MinMax);
                var result = new Mat();
                spectrum.ConvertTo(result, MatType.CV_8U);

                result.SaveImage(fileName);

                return new Bitmap(fileName);
            }
        }
    }
}
