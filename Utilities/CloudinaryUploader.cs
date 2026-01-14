using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Utilities
{
    public class CloudinaryUploader
    {
        private readonly CloudinarySettings _settings;
        private Cloudinary cloudinary;

        public CloudinaryUploader(IOptions<CloudinarySettings> settings, IConfiguration configuration)
        {
            _settings = settings.Value;
            cloudinary = new Cloudinary(_settings.CloudinaryUrl);
            cloudinary.Api.Secure = true;
        }

        public async Task<string?> UploadMediaAsync(IFormFile file)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            //var videoExtensions = new[] { ".mp4", ".mov", ".avi", ".webm" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!imageExtensions.Contains(extension))
            {
                return "Unsupported file type";
            }

            var fileDesc = new FileDescription(file.FileName, file.OpenReadStream());

            if (imageExtensions.Contains(extension))
            {
                var imageParams = new ImageUploadParams
                {
                    File = fileDesc,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await cloudinary.UploadAsync(imageParams);
                return uploadResult.Error == null ? uploadResult.Url?.AbsoluteUri : null;
            }
            /*else if (videoExtensions.Contains(extension))
            {
                var videoParams = new VideoUploadParams
                {
                    File = fileDesc,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await cloudinary.UploadAsync(videoParams);
                return uploadResult.Error == null ? uploadResult.Url?.AbsoluteUri : null;
            }*/

            return null;
        }

        public async Task<string> UploadMultiMediaAsync(List<IFormFile> files, bool? checkValid = true)
        {
            if (checkValid == true && files.Count == 0) return "No file";

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            //var videoExtensions = new[] { ".mp4", ".mov", ".avi", ".webm" };
            List<string> result = new();

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();

                if (checkValid == true && !imageExtensions.Contains(ext))
                {
                    return "Wrong extension: " + ext;
                }

                var url = await UploadMediaAsync(file);
                if (url != null)
                {
                    result.Add(url);
                }
            }

            return string.Join(",", result);
        }
    }
}
