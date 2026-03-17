using Microsoft.AspNetCore.Http;

namespace FlowCare.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveCustomerId(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png" };

            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(ext))
                throw new Exception("Invalid image type");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File too large");

            var folder = Path.Combine(_env.WebRootPath, "uploads/customer_ids");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        public async Task<string> SaveAppointmentAttachment(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(ext))
                throw new Exception("Invalid file type");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File too large");

            var folder = Path.Combine(_env.WebRootPath, "uploads/appointment_attachments");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
    }
}