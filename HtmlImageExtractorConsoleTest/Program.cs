
using HtmlImageExtractor;
using System.Text;

var htmlContent = Console.ReadLine();

var result = HtmlImageExtractor.HtmlImageExtractor.ExtractImagesFromHtml(htmlContent, "/img" , 
    imageFormat: ImageOutputFormat.Webp, 
    imageNamePrefix: "extracted_img"
);
Console.WriteLine(result.ModifiedHtml);

foreach (var image in result.ImageFiles)
{
    Console.WriteLine($"File: {image.FileName}, Size: {image.FileSizeBytes} byte, Format: {image.OutputFormat}");

    var outputPath = Path.Combine("wwwroot", image.FileName);
    Directory.CreateDirectory("wwwroot");
    File.WriteAllBytes(outputPath, image.FileData);
    Console.WriteLine($"Save: {outputPath}"); 
}
