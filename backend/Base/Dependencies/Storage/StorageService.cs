using Microsoft.Extensions.Options;
using MiniAppGIBA.Base.Dependencies.Storage.DTOs;

namespace MiniAppGIBA.Base.Dependencies.Storage
{
    public class StorageService : IStorageService
    {
        private readonly string _webRoot;
        private readonly string _rootUpload;

        public StorageService(IWebHostEnvironment env, IOptions<StorageOptions> opts)
        {
            _webRoot = env.WebRootPath;
            _rootUpload = Path.Combine(_webRoot, opts.Value.UploadFolder);

            // chưa có folder thì tạo
            if (!Directory.Exists(_rootUpload))
            {
                Directory.CreateDirectory(_rootUpload);
            }
        }

        public async Task<StorageResult> SaveFileAsync(IFormFile file, string? subFolder = null)
        {
            var folder = GetOrCreateFolder(subFolder);
            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, fileName);

            using var fs = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fs);

            var rel = Path.GetRelativePath(_webRoot, fullPath)
                         .Replace("\\", "/");

            return new StorageResult
            {
                FileName = fileName,
                RelativePath = rel,
                PublicUrl = string.Empty    // sẽ gán ở Controller
            };
        }

        public async Task<List<StorageResult>> SaveFilesAsync(List<IFormFile> files, string? subFolder = null)
        {
            var list = new List<StorageResult>();
            foreach (var f in files)
                list.Add(await SaveFileAsync(f, subFolder));
            return list;
        }

        public async Task<bool> DeleteFileAsync(string relativePath)
        {
            return await Task.Run(() =>
            {
                var full = Path.Combine(_webRoot, relativePath);
                if (!File.Exists(full)) return false;
                File.Delete(full);
                return true;
            });
        }

        public async Task<bool> DeleteFolderAsync(string? subFolder = null)
        {
            return await Task.Run(() =>
            {
                var folder = subFolder is null
                    ? _rootUpload
                    : Path.Combine(_rootUpload, subFolder);

                if (!Directory.Exists(folder)) return false;
                Directory.Delete(folder, true);
                return true;
            });
        }

        public async Task<List<string>> GetMediaUrls(string hostUrl, string? subFolder = null)
        {
            return await Task.Run(() =>
            {
                var folder = subFolder is null
                    ? _rootUpload
                    : Path.Combine(_rootUpload, subFolder);

                if (!Directory.Exists(folder)) return new List<string>();

                return Directory.GetFiles(folder)
                                .Select(f =>
                                {
                                    var rel = Path.GetRelativePath(_webRoot, f).Replace("\\", "/");
                                    return $"{hostUrl.TrimEnd('/')}/{rel}";
                                }).ToList();
            });
        }

        private string GetOrCreateFolder(string? sub)
        {
            var folder = sub is null
                ? _rootUpload
                : Path.Combine(_rootUpload, sub);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }
}
