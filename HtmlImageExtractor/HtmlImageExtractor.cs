using HtmlAgilityPack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using System.Text.RegularExpressions;

namespace HtmlImageExtractor;

/// <summary>
/// A powerful utility class that extracts base64-encoded images from HTML content 
/// and converts them to physical file references for optimized web performance.
/// 
/// This library is perfect for:
/// - Content Management Systems (CMS) that handle rich text with embedded images
/// - Email processing systems that need to extract and serve images separately
/// - Web applications that optimize page load times by converting inline images
/// - Blog platforms and editorial systems
/// - Any application dealing with HTML content containing base64 images
/// </summary>
public class HtmlImageExtractor
{
    /// <summary>
    /// Extracts base64-encoded images from HTML content and converts them to file references.
    /// This method processes HTML content, finds all embedded base64 images, converts them to 
    /// the specified format, and updates the HTML to reference these files instead.
    /// 
    /// Benefits:
    /// - Reduces HTML file size significantly
    /// - Improves page load performance by enabling browser caching
    /// - Allows CDN distribution of images
    /// - Supports multiple output formats (PNG, JPEG, WebP)
    /// - Memory-efficient processing without disk I/O during extraction
    /// </summary>
    /// <param name="htmlContent">The HTML content containing base64-encoded images</param>
    /// <param name="baseUrl">The base URL where images will be served (default: "/images")</param>
    /// <param name="imageFormat">The output format for converted images (default: PNG)</param>
    /// <param name="imageNamePrefix">The prefix for generated filenames (default: "image")</param>
    /// <returns>A result object containing the modified HTML and extracted image files</returns>
    /// <exception cref="ArgumentException">Thrown when htmlContent is null or empty</exception>
    public static ExtractResult ExtractImagesFromHtml(
        string htmlContent, 
        string baseUrl = "/images", 
        ImageOutputFormat imageFormat = ImageOutputFormat.Png,
        string imageNamePrefix = "image")
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("HTML content cannot be null or empty", nameof(htmlContent));

        // Parse HTML content using HtmlAgilityPack for robust HTML processing
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        // Initialize collections for processing results
        var imageFiles = new List<ImageFile>();
        var imageCounter = 1;

        // Find all img elements with src attributes using XPath
        var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
        
        // Process each img element found in the HTML
        if (imgNodes != null)
        {
            foreach (var imgNode in imgNodes)
            {
                var srcAttribute = imgNode.GetAttributeValue("src", "");
                
                // Check if the src attribute contains a base64 data URL
                if (IsBase64DataUrl(srcAttribute))
                {
                    try
                    {
                        // Process the base64 image and convert it to the specified format
                        var imageFile = ProcessBase64Image(
                            srcAttribute, 
                            imageNamePrefix, 
                            imageCounter, 
                            imageFormat);

                        if (imageFile != null)
                        {
                            // Add the processed image to our collection
                            imageFiles.Add(imageFile);

                            // Update the HTML src attribute to point to the new file
                            var newSrc = $"{baseUrl.TrimEnd('/')}/{imageFile.FileName}";
                            imgNode.SetAttributeValue("src", newSrc);

                            // Increment counter for next image
                            imageCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log errors for debugging purposes, but continue processing other images
                        Console.WriteLine($"Error processing image: {ex.Message}");
                    }
                }
            }
        }

        // Return the complete result with modified HTML and extracted images
        return new ExtractResult
        {
            ModifiedHtml = htmlDoc.DocumentNode.OuterHtml,
            ImageFiles = imageFiles
        };
    }

    /// <summary>
    /// Determines whether a given src attribute contains a base64 data URL.
    /// Supports common image formats: PNG, JPEG, GIF, WebP, BMP, etc.
    /// </summary>
    /// <param name="src">The src attribute value to check</param>
    /// <returns>True if the src contains a base64 data URL, false otherwise</returns>
    private static bool IsBase64DataUrl(string src)
    {
        if (string.IsNullOrWhiteSpace(src))
            return false;

        // Regex pattern to match data URLs with base64 encoding
        var base64Pattern = @"^data:image\/[a-zA-Z0-9+\/]*;base64,";
        return Regex.IsMatch(src, base64Pattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Processes a base64 data URL and converts it to the specified image format.
    /// This method handles the complete conversion pipeline:
    /// 1. Parses the data URL to extract MIME type and base64 data
    /// 2. Decodes the base64 string to raw image bytes
    /// 3. Loads the image using ImageSharp for format conversion
    /// 4. Converts to the target format with optimal settings
    /// 5. Generates appropriate filename with correct extension
    /// </summary>
    /// <param name="base64DataUrl">The complete data URL containing base64 image data</param>
    /// <param name="namePrefix">Prefix for the generated filename</param>
    /// <param name="counter">Sequential number for unique filename generation</param>
    /// <param name="outputFormat">Target format for the converted image</param>
    /// <returns>ImageFile object containing the processed image, or null if processing fails</returns>
    private static ImageFile? ProcessBase64Image(
        string base64DataUrl, 
        string namePrefix, 
        int counter, 
        ImageOutputFormat outputFormat)
    {
        try
        {
            // Split the data URL into header and base64 data parts
            // Format: data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...
            var parts = base64DataUrl.Split(',');
            if (parts.Length != 2)
                return null;

            var headerPart = parts[0]; // data:image/png;base64
            var base64Data = parts[1];  // iVBORw0KGgoAAAANSUhEUgAA...

            // Extract MIME type from the header using regex
            var mimeMatch = Regex.Match(headerPart, @"data:image\/([a-zA-Z0-9+]+)", RegexOptions.IgnoreCase);
            var originalMimeType = mimeMatch.Success ? mimeMatch.Groups[1].Value.ToLower() : "png";

            // Convert base64 string to byte array
            var imageBytes = Convert.FromBase64String(base64Data);

            // Use ImageSharp to load and convert the image
            using var originalImage = Image.Load(imageBytes);
            using var outputStream = new MemoryStream();

            // Get encoder and file extension based on target format
            var (encoder, extension) = GetEncoderAndExtension(outputFormat);
            
            // Save the image in the target format to memory stream
            originalImage.Save(outputStream, encoder);

            // Generate unique filename with appropriate extension
            var fileName = $"{namePrefix}_{counter}.{extension}";
            var convertedImageBytes = outputStream.ToArray();

            // Create and return the processed image file object
            return new ImageFile
            {
                FileName = fileName,
                FileData = convertedImageBytes,
                OriginalMimeType = originalMimeType,
                OutputFormat = outputFormat,
                FileSizeBytes = convertedImageBytes.Length
            };
        }
        catch (Exception ex)
        {
            // Log detailed error for debugging
            Console.WriteLine($"Failed to process base64 image: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Returns the appropriate image encoder and file extension for the specified output format.
    /// Each format is configured with optimal settings for web usage:
    /// - PNG: Lossless compression, supports transparency
    /// - JPEG: High quality (90%) with good compression ratio
    /// - WebP: Modern format with excellent compression and quality
    /// </summary>
    /// <param name="format">The desired output format</param>
    /// <returns>A tuple containing the encoder and file extension</returns>
    private static (IImageEncoder encoder, string extension) GetEncoderAndExtension(ImageOutputFormat format)
    {
        return format switch
        {
            ImageOutputFormat.Png => (new PngEncoder(), "png"),
            ImageOutputFormat.Jpeg => (new JpegEncoder { Quality = 90 }, "jpg"),
            ImageOutputFormat.Webp => (new WebpEncoder(), "webp"),
            _ => (new PngEncoder(), "png") // Default fallback to PNG
        };
    }
}

/// <summary>
/// Represents the result of the image extraction process.
/// Contains both the modified HTML with updated image references 
/// and the collection of extracted image files.
/// </summary>
public class ExtractResult
{
    /// <summary>
    /// The HTML content with base64 image sources replaced by file references.
    /// All data:image/...;base64,... attributes are converted to regular src="/path/filename.ext" format.
    /// </summary>
    public string ModifiedHtml { get; set; } = string.Empty;

    /// <summary>
    /// Collection of image files extracted from the HTML content.
    /// Each file is stored in memory as a byte array and ready for saving to disk or uploading to storage.
    /// </summary>
    public List<ImageFile> ImageFiles { get; set; } = new();
}

/// <summary>
/// Represents a single image file extracted from HTML content.
/// Contains all necessary information for saving the file and tracking its properties.
/// </summary>
public class ImageFile
{
    /// <summary>
    /// The generated filename including extension (e.g., "image_1.png", "blog_img_2.jpg").
    /// This filename is used in the updated HTML src attributes.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The complete image file data as a byte array.
    /// This can be directly written to disk or uploaded to cloud storage.
    /// </summary>
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The original MIME type detected from the base64 data URL (e.g., "png", "jpeg", "webp").
    /// Useful for tracking the source format before conversion.
    /// </summary>
    public string OriginalMimeType { get; set; } = string.Empty;

    /// <summary>
    /// The output format that this image was converted to.
    /// Corresponds to the format specified in the extraction method call.
    /// </summary>
    public ImageOutputFormat OutputFormat { get; set; }

    /// <summary>
    /// The size of the converted image file in bytes.
    /// Useful for storage planning and performance monitoring.
    /// </summary>
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Enumeration of supported image output formats.
/// Each format has different characteristics suitable for various use cases.
/// </summary>
public enum ImageOutputFormat
{
    /// <summary>
    /// PNG format - Lossless compression with transparency support.
    /// Best for: Images with transparency, screenshots, diagrams, logos.
    /// Larger file size but perfect quality preservation.
    /// </summary>
    Png,

    /// <summary>
    /// JPEG format - Lossy compression with excellent compatibility.
    /// Best for: Photographs, images with many colors, general web content.
    /// Smaller file size with good quality (90% quality setting).
    /// </summary>
    Jpeg,

    /// <summary>
    /// WebP format - Modern format with superior compression and quality.
    /// Best for: Modern web applications prioritizing performance.
    /// Excellent compression ratio with high quality, but limited older browser support.
    /// </summary>
    Webp
}
