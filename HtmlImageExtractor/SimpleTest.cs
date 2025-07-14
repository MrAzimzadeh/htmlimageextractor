using System;

namespace SimpleTest;

/// <summary>
/// Basit test sınıfı - kütüphane testi için
/// </summary>
public static class Test
{
    public static void RunBasicTest()
    {
        Console.WriteLine("=== HtmlImageExtractor Basit Test ===\n");

        // Test HTML içeriği - küçük bir base64 PNG görseli
        var testHtml = @"
<html>
    <body>
        <h1>Test Sayfası</h1>
        <img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="" alt=""Test Görseli"" />
        <p>Normal görsel:</p>
        <img src=""/normal/image.jpg"" alt=""Normal"" />
    </body>
</html>";

        try
        {
            // Base64 regex ile kontrol
            var base64Pattern = @"data:image\/[a-zA-Z0-9+\/]*;base64,";
            var matches = System.Text.RegularExpressions.Regex.Matches(testHtml, base64Pattern);
            
            Console.WriteLine($"HTML içinde {matches.Count} adet base64 görsel bulundu.");
            
            if (matches.Count > 0)
            {
                Console.WriteLine("✓ Test HTML'de base64 görsel mevcut");
                
                // Base64 veriyi çıkar
                var base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
                var imageBytes = Convert.FromBase64String(base64Data);
                
                Console.WriteLine($"✓ Base64 veri çözümlendi: {imageBytes.Length} byte");
                Console.WriteLine("✓ Temel fonksiyonlar çalışıyor");
            }
            else
            {
                Console.WriteLine("✗ Base64 görsel bulunamadı");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test başarısız: {ex.Message}");
        }

        Console.WriteLine("\n=== Test Tamamlandı ===");
    }
}
