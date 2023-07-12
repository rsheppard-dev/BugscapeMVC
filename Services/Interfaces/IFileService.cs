namespace BugscapeMVC.Services.Interfaces
{
    public interface IFileService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
        public string ConvertByteArrayToFile(byte[] fileData, string extension);
        public string GetFileIcon(string file);
        public string FormatFileSize(long bytes); 
    }
}