

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PDFChecker {
    class PdfImageProcessor {

        public List<PointF> ProcessPage(string pdfPath, int pageNo) {
            string tempDir = Path.GetTempPath();
            string imagePath = ExtractUsingGhostScript(pdfPath, 
                tempDir + "\\testPNG" + new Random().Next(100, 10000), pageNo - 1);

            if (imagePath != null && File.Exists(imagePath)) {
                try {
                    using (Bitmap bitmap = new Bitmap(imagePath)) {

                        var width = bitmap.Width;
                        var height = bitmap.Height;

                        //2480 = (190 - 400)

                        int startX = width * 190 / 2480;
                        int endX = width * 400 / 2480;

                        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                            ImageLockMode.ReadOnly, bitmap.PixelFormat);

                        Color whiteColor = Color.White;
                        int numLinesWithWhitePixels = 0;

                        List<PointF> rowPoints = new List<PointF>();
                        bool textStarted = false;

                        int textRegionWidth = endX - startX;
                        int textStartY = 0;
                        float heightFloat = height;

                        unsafe
                        {
                            int bitsPerPixel = GetBitsPerPixels(bitmap.PixelFormat);
                            byte* sourceBytes = (byte*)bmpData.Scan0;

                            int headerTableBottom = GetHeaderTableBottom(bitsPerPixel, bmpData);

                            int startY = height * headerTableBottom / 3508;
                            startY += 5;
                            int y = startY;

                            sourceBytes += y * width * bitsPerPixel;

                            while (y < height) {
                                int numNonWhitePixels = 0;

                                sourceBytes += startX * bitsPerPixel;
                                for (int i = 0; i < textRegionWidth; i++) {
                                    if (sourceBytes[0] < 250 || sourceBytes[1] < 250 || sourceBytes[2] < 250) {
                                        numNonWhitePixels++;
                                        if (numNonWhitePixels >= 10) {
                                            if (!textStarted) {
                                                textStartY = y - 2;
                                                textStarted = true;
                                            }
                                            sourceBytes += (textRegionWidth - i) * bitsPerPixel;
                                            break;
                                        }
                                    }

                                    sourceBytes += bitsPerPixel;
                                }
                                sourceBytes += (width - textRegionWidth - startX) * bitsPerPixel;

                                if (numNonWhitePixels < 5) {
                                    numLinesWithWhitePixels++;
                                    if (numLinesWithWhitePixels >= 8) {
                                        if (textStarted) {
                                            textStarted = false;
                                            rowPoints.Add(new PointF(textStartY / heightFloat,
                                                (y - 14) / heightFloat));
                                        }
                                    }
                                } else {
                                    numLinesWithWhitePixels = 0;
                                }

                                //skip 1 row
                                y += 2;
                                sourceBytes += width * bitsPerPixel;
                            }

                        }

                        bitmap.UnlockBits(bmpData);

                        return rowPoints;
                    }
                } finally {
                    File.Delete(imagePath);
                }
            }
            return null;

        }

        private unsafe int GetHeaderTableBottom(int bitsPerPixels, BitmapData bmpData) {
            int y = (int)(bmpData.Height * 0.35);
            int width = bmpData.Width;

            byte* sourceBytes = (byte*)bmpData.Scan0;
            //skip y lines
            sourceBytes += width * bitsPerPixels * y;

            int minBlackPixels = (int)(bmpData.Width * .7);

            while (y >= 700) {
                int numBlackPixels = 0;

                for (int i = 0; i < width; i++) {
                    //var pixel = bitmap.GetPixel(i, y);
                    if (sourceBytes[0] < 10 && sourceBytes[1] < 10 && sourceBytes[2] < 10) {
                        numBlackPixels++;
                    }
                    sourceBytes -= bitsPerPixels;
                }
                if (numBlackPixels >= 200) {
                    if (numBlackPixels >= minBlackPixels) {
                        return y;
                    }
                }
                y--;
            }

            return 725;
        }

        private string ExtractUsingGhostScript(string pdfPath, string imagePath, int pageNo = 0) {
            using (var rasterizer = new Ghostscript.NET.Rasterizer.GhostscriptRasterizer()) {
                rasterizer.Open(pdfPath);
                imagePath = imagePath + ".png";

                var img = rasterizer.GetPage(300, 300, pageNo + 1);
                img.Save(imagePath, ImageFormat.Png);

                return imagePath;
            }

        }

        private int GetBitsPerPixels(PixelFormat pixelFormat) {
            switch (pixelFormat) {
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;
            }

            return 0;
        }

    }
}