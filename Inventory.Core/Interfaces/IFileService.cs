using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        Task<string> SaveBase64ImageAsync(string base64String, string folderName);
        void DeleteFile(string relativePath);
    }
}
