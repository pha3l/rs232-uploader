using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pha3l.RS232Uploader.Utilities;

namespace Pha3l.RS232Uploader.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IOptions<AppConfig> _appConfig;

        [BindProperty]
        public IFormFile UploadedFile { get; set; }
        
        [BindProperty]
        public Toast Toast { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, IOptions<AppConfig> appConfig)
        {
            _logger = logger;
            _environment = environment;
            _appConfig = appConfig;

            if (!Directory.Exists(GetBaseUploadPath()))
                Directory.CreateDirectory(GetBaseUploadPath());
        }

        public async Task OnPostAsync()
        {
            var filePath = Path.Combine(GetBaseUploadPath(), UploadedFile.FileName);
            
            _logger.LogInformation($"Saving file {UploadedFile.FileName} to {filePath}");
            
            await using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(fs);
            }

            Toast = new Toast
            {
                Message = $"Uploaded {UploadedFile.FileName}",
                Class = "alert-success"
            };
            
            // Call the code or external process to send file
            var output = $"ls -al {GetBaseUploadPath()}".ExecuteBash();
            
            _logger.LogInformation(output);
        }

        private string GetBaseUploadPath() => 
            _appConfig.Value.UploadsDirectory ?? Path.Combine(_environment.ContentRootPath, "uploads");
    }
}