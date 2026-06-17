using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;

namespace FactoryOpsApp.Infrastructure.Service
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file));

            string uploadsFolder = Path.Combine(_environment.WebRootPath, folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(folderName, uniqueFileName);
        }

        public async Task<byte[]> GetFileBytesAsync(string filePath)
        {
            string fullPath = Path.Combine(_environment.WebRootPath, filePath);
            return await File.ReadAllBytesAsync(fullPath);
        }
    }
}
