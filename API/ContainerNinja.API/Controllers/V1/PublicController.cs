using ContainerNinja.Contracts.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ContainerNinja.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PublicController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [MapToApiVersion("1.0")]
        [HttpGet("Music")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<FileResult> Music()
        {
            var downloads = Path.Combine(_webHostEnvironment.WebRootPath, "downloads");
            var testFilePath = Path.Combine(downloads, "music.mp3");
            byte[] readBytes;
            using (var fileStream = new FileStream(testFilePath, FileMode.Open))
            {
                readBytes = new byte[fileStream.Length];
                fileStream.Read(readBytes, 0, (int)fileStream.Length);
            }
            return File(readBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "music.mp3");
        }

    }
}
