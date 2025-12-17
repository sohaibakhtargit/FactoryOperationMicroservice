using Microsoft.AspNetCore.Http;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        Task<byte[]> GetFileBytesAsync(string filePath);

        //Task DeleteFileAsync(string filePath);

        //Task<FileStream?> GetFileStreamAsync(string filePath);
    }
}
