using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Prisma.Infrastructure.Services.FileService;

public class FileService(IWebHostEnvironment _webHostEnvironment) 
{
    public async Task<string> UploadFileAsync(IFormFile file, string subFolder, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null.");

        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);

        // إنشاء الفولدر لو مش موجود تلقائياً
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // توليد اسم فريد للملف باستخدام Guid لمنع تكرار الأسماء
        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // حفظ الملف على السيرفر
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        // إرجاع الـ URL النسبي اللي هيتحفظ في قاعدة البيانات
        return $"/uploads/{subFolder}/{uniqueFileName}";
    }
}