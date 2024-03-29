﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Web;
using System.Collections.Generic;
using Sedogo.BusinessObjects;

public class ImageHelper
{
    private const string Extensions = "*.jpg;*.gif";

    private static string _fileStoreFolder;

    private static string ImageCacheDirectory
    {
        get
        {
            if (string.IsNullOrEmpty(_fileStoreFolder))
            {
                var globalData = new GlobalData(string.Empty);
                _fileStoreFolder = globalData.GetStringValue("FileStoreFolder");
            }
            return _fileStoreFolder;
        }
    }

    /// <summary>
    /// Returns the Triplet{width, height, radius} with coordinates containnig respictive width and height of the image
    /// </summary>
    /// <returns>Triplet{width, height, radius}</returns>
    public static Triplet<int, int, int> GetImageSize(ImageType type)
    {
        GlobalData gd = new GlobalData("");
        int thumbnailSize = gd.GetIntegerValue("ThumbnailSize");
        int previewSize = gd.GetIntegerValue("PreviewSize");
        int thumbnailSizeSmall = gd.GetIntegerValue("ThumbnailSizeSmall");//110 gd.GetIntegerValue("ThumbnailSize");
        int thumbnailSizeSlideShow = gd.GetIntegerValue("ThumbnailSizeSlideShow");

        Triplet<int, int, int> dimensions;
        switch (type)
        {
            case ImageType.UserPreview: //1
                dimensions = new Triplet<int, int, int>(previewSize, previewSize, 0);
                break;
            case ImageType.UserThumbnail: //2
                dimensions = new Triplet<int, int, int>(thumbnailSize, thumbnailSize, 0);
                break;
            case ImageType.EventPreview: //3
                dimensions = new Triplet<int, int, int>(previewSize, previewSize, 0);
                break;
            case ImageType.EventThumbnail: //4
                dimensions = new Triplet<int, int, int>(thumbnailSize, thumbnailSize, 0);
                break;
            case ImageType.EventCommentPreview: //5
                dimensions = new Triplet<int, int, int>(previewSize, previewSize, 0);
                break;
            case ImageType.EventCommentThumbnail: //6
                dimensions = new Triplet<int, int, int>(thumbnailSize, thumbnailSize, 0);
                break;
            case ImageType.EventPicturePreview: //7
                dimensions = new Triplet<int, int, int>(previewSize, previewSize, 0);
                break;
            case ImageType.EventPictureThumbnail: //8
                dimensions = new Triplet<int, int, int>(thumbnailSize, thumbnailSize, 0);
                break;
            case ImageType.EventPictureThumbnailSmall: //13
                dimensions = new Triplet<int, int, int>(thumbnailSizeSmall, thumbnailSizeSmall, 0);
                break;
            case ImageType.EventPictureThumbnailSlideShow: //14
                dimensions = new Triplet<int, int, int>(thumbnailSizeSlideShow, thumbnailSizeSlideShow, 0);
                break;
            default:
                dimensions = new Triplet<int, int, int>();
                break;
        }
        return dimensions;
    }

    /// <summary>
    /// Gets the subdir.
    /// </summary>
    /// <param name="imageType">Type of the image.</param>
    /// <returns></returns>
    public static string GetSubdir(ImageType imageType)
    {
        switch (imageType)
        {
            case ImageType.UserOriginal: 
            case ImageType.UserPreview: 
            case ImageType.UserThumbnail:
                return "profilePics";
            case ImageType.EventCommentOriginal:
            case ImageType.EventOriginal:
            case ImageType.EventPictureOriginal:
            case ImageType.EventPreview:
            case ImageType.EventThumbnail:
            case ImageType.EventCommentPreview:
            case ImageType.EventCommentThumbnail:
            case ImageType.EventPicturePreview:
            case ImageType.EventPictureThumbnail:
            case ImageType.EventPictureThumbnailSmall:
            case ImageType.EventPictureThumbnailSlideShow:
                return "eventPics";
            default:
                return "default";
        }
    }

    /// <summary>
    /// Gets the relative image path.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="guid">The GUID.</param>
    /// <param name="imageType">Type of the image.</param>
    /// <param name="overrideImage">if set to <c>true</c> [override image].</param>
    /// <param name="resultWidth">Width of the result.</param>
    /// <param name="resultHeight">Height of the result.</param>
    /// <returns></returns>
    public static string GetRelativeImagePath(int id, string guid, ImageType imageType, bool overrideImage, out int resultWidth, out int resultHeight)
    {
        resultWidth = 1;
        resultHeight = 1;
        var dimensions = GetImageSize(imageType);
        var physPath = Path.Combine(Path.Combine(Path.Combine(ImageCacheDirectory, GetSubdir(imageType)), guid.Substring(0, 2)), guid.Substring(2, 2));
        var virtPath = "~/" + Path.Combine(Path.Combine(Path.Combine("assets", GetSubdir(imageType)), guid.Substring(0, 2)), guid.Substring(2, 2)).Replace("\\", "/");
        var imageSrc = GetRelativeImagePath(id, imageType, physPath, virtPath, dimensions.First, dimensions.Second, dimensions.Third, overrideImage, ref resultWidth, ref resultHeight);
        return imageSrc;
    }


    /// <summary>
    /// Gets the relative image path.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="guid">The GUID.</param>
    /// <param name="imageType">Type of the image.</param>
    /// <returns></returns>
    public static string GetRelativeImagePath(int id, string guid, ImageType imageType)
    {
        return GetRelativeImagePath(id, guid, imageType, false);
    }

    /// <summary>
    /// Gets the relative image path.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="guid">The GUID.</param>
    /// <param name="imageType">Type of the image.</param>
    /// <param name="overrideImage">if set to <c>true</c> [override image].</param>
    /// <returns></returns>
    public static string GetRelativeImagePath(int id, string guid, ImageType imageType, bool overrideImage)
    {
        int w, h;
        return GetRelativeImagePath(id, guid, imageType, overrideImage, out w, out h);
    }


    private static string GetRelativeImagePath(int id, ImageType imageType, string physPath, string virtPath, int width, int height, bool overrideImage)
    {
        int w = 1, h = 1;
        return GetRelativeImagePath(id, imageType, physPath, virtPath, width, height, 0, overrideImage, ref w, ref h);
    }

    /// <summary>
    /// Gets the image SRC.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="imageType">Type of the image.</param>
    /// <param name="physPath">The subdir.</param>
    /// <param name="virtPath">The virt path.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="overrideImage">if set to <c>true</c> [override image].</param>
    /// <returns></returns>
    private static string GetRelativeImagePath(int id, ImageType imageType, string physPath, string virtPath, int width, int height, int radius, bool overrideImage)
    {
        try
        {
            int w = 1, h = 1;
            return GetRelativeImagePath(id, imageType, physPath, virtPath, width, height, radius, overrideImage, ref w, ref h);
        }
        catch (Exception ex)
        {
            var errorLog = new ErrorLog();
            errorLog.WriteLog("ImageHelper", "GetRelativeImagePath", ex.Message, logMessageLevel.errorMessage);
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns the path to the image that should be displayed when there is no image
    /// </summary>
    /// <returns></returns>
    private static string GetEmptyImageSrc()
    {
        return VirtualPathUtility.ToAbsolute("~/img/empty.png");
    }

    /// <summary>
    /// Returns the RELATIVE path to the image that should be displayed when there is no image
    /// </summary>
    /// <returns></returns>
    private static string GetEmptyImageSrc(ImageType type)
    {
        Triplet<int, int, int> dimensions = GetImageSize(type);
        string srcImageName;
        string filename = "empty.jpg";
        switch (type)
        {
            case ImageType.UserPreview:
            case ImageType.UserThumbnail:
                srcImageName = "img-profilepic-120x120.png";
                filename = string.Format("empty_{0}_{1}_{2}.jpg", "profilepic", dimensions.First, dimensions.Second);
                break;
            default:
                return VirtualPathUtility.ToAbsolute("~/img/" + filename);
        }

        if (File.Exists(HttpContext.Current.Server.MapPath("~/img/" + filename)))
            return VirtualPathUtility.ToAbsolute("~/img/" + filename);

        var newEmptyImage = Resize(HttpContext.Current.Server.MapPath("~/img/placeholders/" + srcImageName),
                                     dimensions.First, dimensions.Second, dimensions.Third, type != ImageType.EventPictureThumbnailSlideShow, type);

        newEmptyImage.Save(HttpContext.Current.Server.MapPath("~/img/" + filename), ImageFormat.Png);
        return VirtualPathUtility.ToAbsolute("~/img/" + filename);
    }
    /// <summary>
    /// Returns the real image dimensions as Point
    /// </summary>
    /// <returns>Point{x, y}</returns>
    public static Point GetImageDimensions(string path)
    {
        Point dimensions = new Point();
        string realPath = HttpContext.Current.Server.MapPath(path);
        if (File.Exists(realPath))
        {
            Image img = Image.FromFile(realPath);
            dimensions.X = img.Width;
            dimensions.Y = img.Height;
        }
        return dimensions;
    }
    /// <summary>
    /// Retrieve the real image dimensions as int, int
    /// </summary>
    /// <returns></returns>
    public static void GetImageDimensions(string path, ref int width, ref int height)
    {
        Point dimensions = GetImageDimensions(path);
        width = dimensions.X;
        height = dimensions.Y;
    }

    /// <summary>
    /// Returns the RELATIVE path to the icon that has the same file name as the extension of a document
    /// </summary>
    /// <param name="extension">extension</param>
    /// <returns>path</returns>
    public static string GetIconForFile(string extension)
    {
        extension = extension.Replace(".", string.Empty);
        const string iconsDirPath = "~/img/Icons";

        if (HttpContext.Current != null && HttpContext.Current.Server != null)
        {
            HttpServerUtility server = HttpContext.Current.Server;
            string iconsDir = server.MapPath(iconsDirPath);
            DirectoryInfo dir = new DirectoryInfo(iconsDir);
            if (dir.Exists)
            {
                List<FileInfo> files = new List<FileInfo>();
                files.AddRange(dir.GetFiles());
                foreach (FileInfo file in files)
                {
                    if (Path.GetFileNameWithoutExtension(file.Name).Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return iconsDirPath + "/" + file.Name;
                    }
                }
            }
        }
        return iconsDirPath + "/" + "GENERICO.PNG";
        //return null;
    }

    /// <summary>
    /// Returns the RELATIVE path to the image on the website
    /// </summary>
    /// <param name="id">The GUID.</param>
    /// <param name="subdir">The subdir.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="resultWidth">width of the resized image</param>
    /// <param name="resultHeight">Height of the resized image</param>
    /// <returns></returns>
    private static string GetRelativeImagePath(int id, ImageType imageType, string physPath, string virtPath, int width, int height, int radius, bool overrideImage, ref int resultWidth, ref int resultHeight)
    {
        switch (imageType)
        {
            case ImageType.EventCommentPreview:
            case ImageType.EventCommentThumbnail:
                physPath = Path.Combine(physPath, "comments");
                virtPath = string.Format("{0}/{1}", virtPath, "comments");
                break;
            case ImageType.EventPicturePreview:
            case ImageType.EventPictureThumbnail:
            case ImageType.EventPictureThumbnailSmall:
            case ImageType.EventPictureThumbnailSlideShow:
                physPath = Path.Combine(physPath, "pictures");
                virtPath = string.Format("{0}/{1}", virtPath, "pictures");
                break;
        }
        HttpServerUtility server = HttpContext.Current.Server;
        DirectoryInfo dir = new DirectoryInfo(physPath);
        if (!dir.Exists)
        {
            dir.Create();
        }
        var extensions = Extensions.Split(new[] {';'});
        var files = new List<FileInfo>();
        foreach (string s in extensions)
        {
            files.AddRange(dir.GetFiles(s));
        }

        var repository = new ImageRepository();
        string filename = repository.GetImagePath(id, imageType);
        string thmName = Path.GetFileNameWithoutExtension(filename) + "_" + width + "_" + height + ".jpg";
        string thmNameWithoutExtension=Path.GetFileNameWithoutExtension(thmName);


        if (!overrideImage)
        {
            foreach (var file in files)
            {

                {
                    if (Path.GetFileNameWithoutExtension(file.Name) == thmNameWithoutExtension)
                    {Image img = Image.FromFile(file.FullName);
                        resultWidth = img.Width;
                        resultHeight = img.Height;
                        return string.Format("{0}/{1}", virtPath, file.Name);
                    }
              }
            }
        }
        if (string.IsNullOrEmpty(filename))
        {
            return string.Format("{0}/{1}", virtPath, thmName);
        }
        switch (imageType)
        {
            case ImageType.UserOriginal:
            case ImageType.EventOriginal:
            case ImageType.EventCommentOriginal:
            case ImageType.EventPictureOriginal:
                return string.Format("{0}/{1}", Path.GetDirectoryName(Path.GetDirectoryName(virtPath)), filename);
        }
        string fullPath;
        if (!Path.IsPathRooted(physPath))
        {
            fullPath =
                server.MapPath("~\\" +
                               Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(physPath)), filename));
        }
        else
        {
            if (imageType == ImageType.EventCommentPreview ||
                imageType == ImageType.EventCommentThumbnail ||
                imageType == ImageType.EventPicturePreview ||
                imageType == ImageType.EventPictureThumbnail ||
                imageType == ImageType.EventPictureThumbnailSmall ||
                imageType == ImageType.EventPictureThumbnailSlideShow
                )
            {
                fullPath =
                    Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(physPath))),
                                 filename);
            }
            else
            {
                fullPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(physPath)), filename);
            }
        }
        if (!File.Exists(fullPath))
        {
            return string.Format("{0}/{1}", virtPath, thmName);
        }
        
        using (var sourceImage = Image.FromFile(fullPath))
        {
            using (var targetImage = Resize(sourceImage, width, height, radius, imageType != ImageType.EventPictureThumbnailSlideShow, imageType))
            {
                targetImage.Save(physPath + @"\" + thmName, ImageFormat.Jpeg); //save as jpg only
                resultWidth = targetImage.Width;
                resultHeight = targetImage.Height;
            }
        }
        return string.Format("{0}/{1}", virtPath, thmName);
    }

    private static string GetSuffix(ImageType imageType)
    {
        switch (imageType)
        {
            case ImageType.EventCommentPreview:
            case ImageType.EventCommentThumbnail:
                return "C";
            case ImageType.EventPicturePreview:
            case ImageType.EventPictureThumbnail:
            case ImageType.EventPictureThumbnailSmall:
            case ImageType.EventPictureThumbnailSlideShow:
                return "P";
            default:
                return string.Empty;
        }
    }

    private static bool IsImageWithSameDimensionsExists(Image img, string fileName, int height, int width)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        string[] nameParts = fileNameWithoutExtension.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        return img.Height == height && img.Width == width ||
               nameParts.Length >= 3 &&
               nameParts[nameParts.Length - 2] == width.ToString() &&
               nameParts[nameParts.Length - 1] == height.ToString();
    }

    /// <summary>
    /// Resize image to specified size and rounds corners.
    /// </summary>
    /// <param name="path">Path to Image</param>
    /// <param name="width">Required width</param>
    /// <param name="height">Required heght</param>
    /// <param name="radius">Radius of rounded corners</param>
    /// <returns>Image</returns>
    public static Image Resize(string path, int width, int height, int radius, bool crop, ImageType type)
    {
        if (File.Exists(path))
        {
            return Resize(Image.FromFile(path), width, height, radius, crop, type);
        }
        throw new ArgumentException("Wrong path. The image does not exists", "path");
    }

    /// <summary>Imape
    /// Resize image to specified size.
    /// </summary>
    /// <param name="image">Source image</param>
    /// <param name="width">Required width</param>
    /// <param name="height">Required heght</param>
    /// <returns>Image</returns>
    /// <summary>
    /// Resize image to specified size.
    /// </summary>
    /// <param name="image">Source image</param>
    /// <param name="width">Required width</param>
    /// <param name="height">Required heght</param>
    /// <returns>Image</returns>
    public static Image Resize(Image image, int width, int height, bool crop, ImageType type)
    {
        if (image == null) throw new ArgumentNullException("image");

        if (height == 0)
        {
            height = (int)(image.Height * width / (float)image.Width);
        }

        if (width == 0)
        {
            width = (int)(image.Width * height / (float)image.Height);
        }

        int sourceWidth = image.Width;
        int sourceHeight = image.Height;
        int destWidth, destHeight;
        if (sourceWidth > sourceHeight)                    // Landscape
        {
            destWidth = width;
            destHeight = (type==ImageType.EventPictureThumbnailSlideShow ? width : height) * sourceHeight / sourceWidth;
        }
        else                                          //Portrait
        {
            destWidth = type == ImageType.EventPictureThumbnailSlideShow ? width / 2 : width;
            destHeight = (type == ImageType.EventPictureThumbnailSlideShow ? width / 2 : height) * sourceHeight / sourceWidth;
        }
        Bitmap bitmap = new Bitmap(destWidth, destHeight,
                          PixelFormat.Format32bppArgb);
        
        bitmap.SetResolution(image.HorizontalResolution,
                         image.VerticalResolution);

        Graphics grPhoto = Graphics.FromImage(bitmap);
        grPhoto.Clear(Color.Transparent);
        grPhoto.InterpolationMode =
                InterpolationMode.HighQualityBicubic;
        
        if(sourceWidth>sourceHeight)                    // Landscape
            grPhoto.DrawImage(image,
                new Rectangle(0, 0, destWidth, destHeight),
                new Rectangle( 0, 0, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);
        else                                          //Portrait
            grPhoto.DrawImage(image,
                new Rectangle(0, 0, destWidth, destHeight),
                new Rectangle(0, 0, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);
        grPhoto.Dispose();
        return bitmap;
    }

    /// <summary>
    /// Resize image to specified size and rounds corners.
    /// </summary>
    /// <param name="image">Source image</param>
    /// <param name="width">Required width</param>
    /// <param name="height">Required heght</param>
    /// <param name="radius">Radius of rounded corners</param>
    /// <returns>Image</returns>
    public static Image Resize(Image image, int width, int height, int radius, bool crop, ImageType type)
    {
        image = Resize(image, width, height, crop, type);

        if (radius == 0)
        {
            return image;
        }

        Image bmp = MakeRoundedCorners(image, radius);

        //If you need white fillings uncomment next row:
        //bmp = RestoreToSquare(bmp);

        return bmp;
    }

    private static Image MakeRoundedCorners(Image image, int radius)
    {
        Bitmap bmp = new Bitmap(image, image.Width, image.Height);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            Brush brush = new SolidBrush(Color.White);

            for (int i = 0; i < 4; i++)
            {
                Point[] cornerUpLeft = new Point[3];

                cornerUpLeft[0].X = 0;
                cornerUpLeft[0].Y = 0;

                cornerUpLeft[1].X = radius;
                cornerUpLeft[1].Y = 0;

                cornerUpLeft[2].X = 0;
                cornerUpLeft[2].Y = radius;

                GraphicsPath pathCornerUpLeft = new GraphicsPath();

                pathCornerUpLeft.AddArc(cornerUpLeft[0].X, cornerUpLeft[0].Y,
                                        radius, radius, 180, 90);
                pathCornerUpLeft.AddLine(cornerUpLeft[0].X, cornerUpLeft[0].Y,
                                         cornerUpLeft[1].X, cornerUpLeft[1].Y);
                pathCornerUpLeft.AddLine(cornerUpLeft[0].X, cornerUpLeft[0].Y,
                                         cornerUpLeft[2].X, cornerUpLeft[2].Y);

                g.FillPath(brush, pathCornerUpLeft);
                pathCornerUpLeft.Dispose();

                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }

            brush.Dispose();
            g.Dispose();
        }
        return bmp;
    }

    /// <summary>
    /// Restore Image from rectangle  to square 
    /// </summary>
    /// <param name="image">Source image</param>
    /// <returns>Image</returns>
    private static Bitmap RestoreToSquare(Image image)
    {
        int sourceWidth = image.Width;
        int sourceHeight = image.Height;
        int destWidth = image.Width > image.Height ? image.Width : image.Height;
        int destHeight = image.Width > image.Height ? image.Width : image.Height;
        const int sourceX = 0;
        const int sourceY = 0;
        int destX = image.Width > image.Height ? 0 : (image.Height - image.Width) / 2;
        int destY = image.Width < image.Height ? 0 : (image.Width - image.Height) / 2;

        Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);
        bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        Graphics grPhoto = Graphics.FromImage(bmPhoto);
        grPhoto.Clear(Color.White);
        grPhoto.InterpolationMode =
            InterpolationMode.HighQualityBicubic;
        grPhoto.DrawImage(image,
                          new Rectangle(destX, destY, sourceWidth, sourceHeight),
                          new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                          GraphicsUnit.Pixel);

        grPhoto.Dispose();
        return bmPhoto;
    }

    public static void DeleteImage(int id, string guid, ImageType imageType)
    {
        var physPath =
            Path.Combine(Path.Combine(Path.Combine(ImageCacheDirectory, GetSubdir(imageType)), guid.Substring(0, 2)),
                         guid.Substring(2, 2));

        switch (imageType)
        {
            case ImageType.EventCommentPreview:
            case ImageType.EventCommentThumbnail:
                physPath = Path.Combine(physPath, "comments");
                break;
            case ImageType.EventPicturePreview:
            case ImageType.EventPictureThumbnail:
            case ImageType.EventPictureThumbnailSmall:
            case ImageType.EventPictureThumbnailSlideShow:
                physPath = Path.Combine(physPath, "pictures");
                break;
        }

        DirectoryInfo dir = new DirectoryInfo(physPath);
        if (!dir.Exists)
        {
            dir.Create();
        }
        var extensions = Extensions.Split(new[] {';'});
        var files = new List<FileInfo>();
        foreach (string s in extensions)
        {
            files.AddRange(dir.GetFiles(s));
        }

        ImageRepository repository = new ImageRepository();
        var imagePath = repository.GetImagePath(id, imageType);

        foreach (var file in files)
        {
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            filename = filename.Substring(0, filename.Substring(0, filename.LastIndexOf("_")).LastIndexOf("_"));
            if (filename == Path.GetFileNameWithoutExtension(imagePath))
            {
                file.Delete();
            }
        }

        var imageFullPath = Path.Combine(Path.Combine(ImageCacheDirectory, GetSubdir(imageType)), imagePath);
        if (File.Exists(imageFullPath))
        {
            File.Delete(imageFullPath);
        }
    }
}