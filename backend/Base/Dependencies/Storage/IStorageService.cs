using MiniAppGIBA.Base.Dependencies.Storage.DTOs;

namespace MiniAppGIBA.Base.Dependencies.Storage
{
    public interface IStorageService
    {
        Task<bool> DeleteFileAsync(string relativePath);
        Task<bool> DeleteFolderAsync(string? subFolder = null);
        Task<List<string>> GetMediaUrls(string hostUrl, string? subFolder = null);

        Task<StorageResult> SaveFileAsync(IFormFile file, string? subFolder = null);
        Task<List<StorageResult>> SaveFilesAsync(List<IFormFile> files, string? subFolder = null);
    }
}
