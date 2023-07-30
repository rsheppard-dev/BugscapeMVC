using BugscapeMVC.Services.Interfaces;

namespace BugscapeMVC.Services
{
    public class FileService : IFileService
    {
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            try
            {
                string fileBase64Data = Convert.ToBase64String(fileData);

                return string.Format($"data:{extension};base64,{fileBase64Data}");
            }
            catch (Exception)
            {  
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                MemoryStream memoryStream = new();

                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                memoryStream.Close();
                memoryStream.Dispose();

                return byteFile;
            }
            catch (Exception)
            {                
                throw;
            }
        }

        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal fileSize = bytes;
            string[] suffixes = {"Bytes", "KB", "MB", "GB", "TB", "PB"};

            while(Math.Round(fileSize / 1024) >= 1)
            {
                fileSize /= 1024;
                counter++;
            }

            return string.Format("{0:n1}{1}", fileSize, suffixes[counter]);
        }

        public string GetFileIcon(string file)
        {
            string fileImage = "default";

            if (string.IsNullOrWhiteSpace(file)) return fileImage;

            fileImage = Path.GetExtension(file).ToLower().Replace(".", "");

            fileImage = fileImage == "jpeg" ? "jpg" : fileImage;

            return $"/images/contentType/{fileImage}.png";
        }
    }
}