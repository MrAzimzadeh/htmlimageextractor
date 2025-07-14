# HtmlImageExtractor - Installation and Usage Guide

## NuGet Package Installation

The project is ready and the NuGet package has been created! For usage:

### 1. Local Testing
```bash
# To test the package locally
dotnet add package HtmlImageExtractor --source "c:\Users\LENOVO\Desktop\MyLib\HTML\HtmlImageExtractor\bin\Release\"
```

### 2. Publishing to NuGet.org
```bash
# To publish to NuGet.org (API key required)
dotnet nuget push "c:\Users\LENOVO\Desktop\MyLib\HTML\HtmlImageExtractor\bin\Release\HtmlImageExtractor.1.0.0.nupkg" --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
```

## Usage Examples

### Basic Usage
```csharp
using HtmlImageExtractor;

var htmlContent = @"
    <div>
        <img src=""data:image/png;base64,iVBORw0KGgo..."" alt=""Base64 Image"" />
        <img src=""/normal/image.jpg"" alt=""Normal Image"" />
    </div>";

// Extract base64 images from HTML
var result = HtmlImageExtractor.ExtractImagesFromHtml(htmlContent);

Console.WriteLine($"Processed images count: {result.ImageFiles.Count}");

// Extracted files
foreach (var imageFile in result.ImageFiles)
{
    Console.WriteLine($"File: {imageFile.FileName}");
    Console.WriteLine($"Size: {imageFile.FileSizeBytes} bytes");
    
    // To save the file physically:
    File.WriteAllBytes($"output/{imageFile.FileName}", imageFile.FileData);
}

// Updated HTML
Console.WriteLine(result.ModifiedHtml);
```

### Advanced Usage
```csharp
// With customized settings
var result = HtmlImageExtractor.ExtractImagesFromHtml(
    htmlContent: htmlContent,
    baseUrl: "https://mycdn.com/images",           
    imageFormat: ImageOutputFormat.Webp,          
    imageNamePrefix: "blog_img"              
);
```

## Features

✅ **Automatic Detection**: Finds all `data:image/...;base64,...` formatted images in HTML  
✅ **Format Conversion**: Converts to PNG, JPEG, WebP formats  
✅ **Memory-Based**: No files written to disk, all processed in RAM  
✅ **High Performance**: Uses HtmlAgilityPack and ImageSharp  
✅ **Customizable**: File names, URLs, formats can be customized  
✅ **Error Handling**: Invalid base64 data is silently skipped  

## API Documentation

### HtmlImageExtractor.ExtractImagesFromHtml()

**Parameters:**
- `htmlContent` _(string)_: HTML content to process
- `baseUrl` _(string, optional)_: Base URL where images will be displayed (default: "/images")
- `imageFormat` _(ImageOutputFormat, optional)_: Output format (default: Png)
- `imageNamePrefix` _(string, optional)_: File name prefix (default: "image")

**Return Value:** `ExtractResult`
- `ModifiedHtml` _(string)_: HTML with base64 references replaced by file paths
- `ImageFiles` _(List<ImageFile>)_: Image files created in memory

### ImageFile Class
- `FileName` _(string)_: File name (e.g., "image_1.png")
- `FileData` _(byte[])_: File data
- `OriginalMimeType` _(string)_: Original MIME type
- `OutputFormat` _(ImageOutputFormat)_: Output format
- `FileSizeBytes` _(long)_: File size in bytes

## Project Structure

```
HtmlImageExtractor/
├── HtmlImageExtractor.cs        # Main library class
├── Examples/
│   └── UsageExamples.cs         # Usage examples
├── Tests/
│   └── SimpleTests.cs           # Test classes
├── TestApp/
│   └── Program.cs               # Test application
├── HtmlImageExtractor.csproj    # Project file
├── README.md                    # Documentation
└── SimpleTest.cs               # Simple test class
```

## Dependencies

- .NET 9.0+
- HtmlAgilityPack 1.11.54
- SixLabors.ImageSharp 3.1.5

## License

MIT License

## Support

This library is ideal for HTML editors, blog systems, CMS platforms, and email processing systems.
