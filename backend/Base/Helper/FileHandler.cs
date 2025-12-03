using Microsoft.AspNetCore.Http;

namespace MiniAppGIBA.Base.Helper;

public class FileHandler
{
    public static IEnumerable<string> GetMediaUrls(string webrootPath, string hostUrl, string folderPath)
    {
        var files = Path.Combine(webrootPath, folderPath);
        IEnumerable<string>? imageUrls = null;
        if (Directory.Exists(files))
        {
            var imageFiles = Directory.GetFiles(files);

            imageUrls = imageFiles.Select(file =>
                $"{hostUrl}/{folderPath}/{Path.GetFileName(file)}"
            );
        }

        return imageUrls ?? Enumerable.Empty<string>();
    }

    public static async Task<List<string>> SaveFiles(List<IFormFile> files, string uploadFolder)
    {
        var savedFileNames = new List<string>();
        
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        foreach (var file in files)
        {
            var newName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, newName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            
            savedFileNames.Add(newName);
        }

        return savedFileNames;
    }

    public static async Task<string> SaveFile(IFormFile file, string uploadFolder)
    {
        return await SaveFileWithName(file, uploadFolder, Guid.NewGuid().ToString());
    }

    public static async Task<string> SaveFileWithName(IFormFile file, string uploadFolder, string name)
    {
        try
        {
            var newName = $"{name}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, newName);

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return newName;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving file {file.FileName} to {uploadFolder}: {ex.Message}", ex);
        }
    }

    public static void DeleteFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
        }
    }

    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void DeleteAllFiles(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(folderPath))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    public static void RemoveFiles(List<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore errors when deleting files
                }
            }
        }
    }
}

