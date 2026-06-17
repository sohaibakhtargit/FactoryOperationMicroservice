using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.Common
{
    /*  public class FileStorageService : IFileStorageService
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
      }*/



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

                // Ensure WebRootPath exists
                if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
                {
                    _environment.WebRootPath =
                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                // Ensure wwwroot exists
                if (!Directory.Exists(_environment.WebRootPath))
                {
                    Directory.CreateDirectory(_environment.WebRootPath);
                }

                // Ensure target folder exists
                var uploadsFolder = Path.Combine(_environment.WebRootPath, folderName);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName =
                    $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Path.GetFileName(file.FileName)}";

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Always return relative path
                return Path.Combine(folderName, uniqueFileName).Replace("\\", "/");
            }

            public async Task<byte[]> GetFileBytesAsync(string filePath)
            {
                if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
                {
                    _environment.WebRootPath =
                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var fullPath = Path.Combine(_environment.WebRootPath, filePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("File not found", fullPath);

                return await File.ReadAllBytesAsync(fullPath);
            }
        }
    }

