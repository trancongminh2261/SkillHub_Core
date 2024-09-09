using LMSCore.LMS;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.AspNetCore.Components.Forms;
using Org.BouncyCastle.Utilities.Zlib;
using LMSCore.Models;
using static LMSCore.Models.lmsEnum;
using System.Net.Http;
using System.IO.Compression;


namespace LMSCore.Utilities
{
    public class UploadModel
    {
        public bool Success { get; set; } = false;
        public string FileName { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Message { get; set; }
        public string LinkResize { get; set; } = string.Empty;
        public string LinkReadFile { get; set; } = string.Empty;
    }
    public class UploadConfig
    {
        public const long Quality = 358400; // 350kb
        public static UploadModel UploadFile(IFormFile file, string baseUrl, string linkFolder, UploadType type = UploadType.Document, string fileName = "")
        {
            string link = string.Empty;
            string linkReSize = string.Empty;
            string linkReadFile = string.Empty;
            string fileNameOrigin = string.Empty;
            if (baseUrl.IndexOf("https") == -1)
                baseUrl = baseUrl.Replace("http", "https");
            var data = new UploadModel();
            try
            {
                if (file != null)
                {
                    fileNameOrigin = file.FileName;
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    if (string.IsNullOrEmpty(fileName))
                        fileName = Guid.NewGuid() + ext;
                    else
                        fileName += ext;
                    string fileNameReSize = Guid.NewGuid() + "_ReSize" + ext;
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    var result = false;
                    if (type == UploadType.Audio)
                        result = AssetCRM.IsValidAudio(ext);
                    else if (type == UploadType.Image)
                        result = AssetCRM.IsValidImage(ext);
                    else
                        result = AssetCRM.IsValidDocument(ext);
                    if (result)
                    {
                        var path = Path.Combine(linkFolder, fileName);
                        if (fileName.Contains(".zip"))
                        {
                            link = $"{baseUrl}/{linkFolder}{fileName}";
                            linkReadFile = $"{baseUrl}/{linkFolder}{fileName.Replace(".zip", "/") + "index.html"}";
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            //Tạo file lưu
                            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), linkFolder, fileName.Replace(".zip", ""));

                            Directory.CreateDirectory(folderPath);
                            //Mở file zip
                            string sourceArchive = path;
                            string destinaltion = folderPath;
                            ZipFile.ExtractToDirectory(sourceArchive, destinaltion);
                        }
                        else
                        {
                            link = $"{baseUrl}/{linkFolder}{fileName}";
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            if (AssetCRM.IsValidImage(ext))
                            {
                                if (file.Length < Quality)
                                    linkReSize = link;
                                else
                                {
                                    using (var stream = file.OpenReadStream())
                                    {
                                        using (var image = Image.FromStream(stream))
                                        {
                                            var compressedImage = CompressImageToTargetSize(image, Quality);
                                            // Save compressed image to disk or further process it
                                            var pathReSize = Path.Combine(linkFolder, fileNameReSize);
                                            compressedImage.Save(pathReSize, ImageFormat.Jpeg);
                                            linkReSize = $"{baseUrl}/{linkFolder}{fileNameReSize}";
                                        }
                                    }
                                }
                            }
                        }
                        data.Success = true;
                        data.FileName = fileNameOrigin;
                        data.Link = link;
                        data.LinkResize = linkReSize;
                        data.LinkReadFile = linkReadFile;
                        data.Message = ApiMessage.SAVE_SUCCESS;
                        return data;
                    }
                    else
                    {
                        data.Message = ApiMessage.INVALID_FILE;
                        return data;
                    }
                }
                else
                {
                    data.Message = ApiMessage.NOT_FOUND;
                    return data;
                }
            }
            catch (Exception ex)
            {
                data.Message = ex.Message;
                return data;
            }
        }
        private static Image CompressImageToTargetSize(Image image, long targetSize)
        {
            int width = image.Width;
            int height = image.Height;
            long quality = 100L;

            while (true)
            {
                using (var bmp = new Bitmap(image, width, height))
                {
                    var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    var myEncoder = Encoder.Quality;
                    var myEncoderParameters = new EncoderParameters(1);

                    var myEncoderParameter = new EncoderParameter(myEncoder, quality);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, jpgEncoder, myEncoderParameters);

                        if (ms.Length <= targetSize || quality <= 10L)
                        {
                            return Image.FromStream(new MemoryStream(ms.ToArray()));
                        }
                    }
                }

                // Giảm kích thước ảnh hoặc chất lượng để giảm dung lượng
                if (width > 100 && height > 100)
                {
                    width = (int)(width * 0.9);
                    height = (int)(height * 0.9);
                }
                else
                {
                    quality -= 10L;
                    if (quality <= 10L) break; // Nếu chất lượng đã giảm tới mức tối thiểu
                }
            }

            return null; // Không thể nén ảnh dưới 350KB
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static UploadModel UploadResizeImage(IFormFile file, string baseUrl, string linkFolder, string linkFolderReSize, int widthReSize, int heightReSize)
        {
            string link = string.Empty;
            string linkReSize = string.Empty;
            if (baseUrl.IndexOf("https") == -1)
                baseUrl = baseUrl.Replace("http", "https");
            var data = new UploadModel();
            try
            {
                if (file != null)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    string fileName = Guid.NewGuid() + ext;
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    var result = AssetCRM.IsValidImage(ext);
                    if (result)
                    {
                        fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(linkFolder, fileName);
                        link = $"{baseUrl}/{linkFolder}{fileName}";
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        var pathReSize = Path.Combine(linkFolderReSize, fileName);
                        linkReSize = $"{baseUrl}/{linkFolderReSize}{fileName}";
                        using (var inputStream = new FileStream(path, FileMode.Open))
                        using (var outputStream = new FileStream(pathReSize, FileMode.Create))
                        {
                            ResizeImage(inputStream, outputStream, widthReSize, heightReSize);
                        }
                        data.Success = true;
                        data.Link = link;
                        data.LinkResize = linkReSize;
                        data.Message = ApiMessage.SAVE_SUCCESS;
                        return data;
                    }
                    else
                    {
                        data.Message = ApiMessage.INVALID_IMAGE;
                        return data;
                    }
                }
                else
                {
                    data.Message = ApiMessage.NOT_FOUND;
                    return data;
                }
            }
            catch (Exception ex)
            {
                data.Message = ex.Message;
                return data;
            }
        }
        public static void ResizeImage(Stream input, Stream output, int width, int height)
        {
            using (var image = Image.FromStream(input))
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                destImage.Save(output, ImageFormat.Jpeg);
            }
        }
    }
}