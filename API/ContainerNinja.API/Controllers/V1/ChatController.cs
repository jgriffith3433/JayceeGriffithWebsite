using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Handlers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ContainerNinja.Controllers.V1
{
    //[Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Admin}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAudioService _audioService;

        public ChatController(IMediator mediator, IWebHostEnvironment webHostEnvironment, IAudioService audioService)
        {
            _mediator = mediator;
            _webHostEnvironment = webHostEnvironment;
            _audioService = audioService;
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(ChatResponseVM), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ChatResponseVM>> Create(GetChatResponseVM getChatResponse)
        {
            var chatConversation = await _mediator.Send(new GetChatConversation
            {
                ChatConversationId = getChatResponse.ChatConversationId,
            });
            var response = await _mediator.Send(new GetChatResponseQuery
            {
                ChatMessages = getChatResponse.ChatMessages,
                CurrentUrl = getChatResponse.CurrentUrl,
                ChatConversation = chatConversation,
                ForceFunctionCall = getChatResponse.ForceFunctionCall,
            });
            if (response.Error)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("Normal")]
        [ProducesResponseType(typeof(ChatResponseVM), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ChatResponseVM>> CreateNormal(GetChatResponseVM getChatResponse)
        {
            var chatConversation = await _mediator.Send(new GetChatConversation
            {
                ChatConversationId = getChatResponse.ChatConversationId,
            });
            var response = await _mediator.Send(new GetNormalChatResponseQuery
            {
                ChatMessages = getChatResponse.ChatMessages,
                CurrentUrl = getChatResponse.CurrentUrl,
                ChatConversation = chatConversation,
            });
            if (response.Error)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("Speech")]
        [ProducesResponseType(typeof(GetChatTextFromSpeechVm), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetChatTextFromSpeechVm>> Speech([FromForm] IFormFile speech)
        {
            if (speech != null)
            {
                //var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                //if (!Directory.Exists(uploads))
                //{
                //    Directory.CreateDirectory(uploads);
                //}

                //var fileName = speech.FileName;
                //var filePathNew = Path.Combine(uploads, fileName + "_1.wav");
                //var filePath2nd = Path.Combine(uploads, fileName + "_2.wav");
                //var filePath3rd = Path.Combine(uploads, fileName + "_3.wav");

                //if (System.IO.File.Exists(filePath3rd))
                //{
                //    System.IO.File.Delete(filePath3rd);
                //}
                //if (System.IO.File.Exists(filePath2nd))
                //{
                //    System.IO.File.Move(filePath2nd, filePath3rd);
                //}
                //if (System.IO.File.Exists(filePathNew))
                //{
                //    System.IO.File.Move(filePathNew, filePath2nd);
                //}




                //var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                //if (!Directory.Exists(uploads))
                //{
                //    Directory.CreateDirectory(uploads);
                //}

                //var fileName = speech.FileName;
                //var filePathBefore = Path.Combine(uploads, fileName + "_before.wav");
                //var filePathAfter = Path.Combine(uploads, fileName + "_after.wav");

                //if (System.IO.File.Exists(filePathBefore))
                //{
                //    System.IO.File.Delete(filePathBefore);
                //}
                //if (System.IO.File.Exists(filePathAfter))
                //{
                //    System.IO.File.Delete(filePathAfter);
                //}

                //using (var fileStream = new FileStream(filePathBefore, FileMode.Create))
                //{
                //    await speech.CopyToAsync(fileStream);
                //}

                var audioData = ConvertToByteArrayContent(speech);
                var strippedAudio = _audioService.StripNoiseAndTrimSilence(audioData);

                //using (var fileStream = new FileStream(filePathAfter, FileMode.Create))
                //{
                //    fileStream.Write(strippedAudio, 0, strippedAudio.Length);
                //}

                if (strippedAudio.Length > 256)
                {
                    return await _mediator.Send(new GetChatTextFromSpeechQuery
                    {
                        Speech = strippedAudio
                    });
                }
            }
            return Ok(new GetChatTextFromSpeechVm
            {
                Text = ""
            });
        }


        [MapToApiVersion("1.0")]
        [HttpPost("TextToSpeech")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<FileResult> TextToSpeech(GetChatTextToSpeechQuery query)
        {
            var bytes = await _mediator.Send(query);
            var downloads = Path.Combine(_webHostEnvironment.WebRootPath, "downloads");

            if (!Directory.Exists(downloads))
            {
                Directory.CreateDirectory(downloads);
            }

            //var filePath = Path.Combine(downloads, "text-to-speech.mp3");
            //using (var fileStream = new FileStream(filePath, FileMode.Create))
            //{
            //    fileStream.Write(bytes, 0, bytes.Length);
            //}

            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "text-to-speech.mp3");
            //testing
            //var testFilePath = Path.Combine(downloads, "bassmusic.mp3");
            //byte[] readBytes;
            //using (var fileStream = new FileStream(testFilePath, FileMode.Open))
            //{
            //    readBytes = new byte[fileStream.Length];
            //    fileStream.Read(readBytes, 0, (int)fileStream.Length);
            //}
            //return File(readBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "bassmusic.mp3");
        }

        private byte[] ConvertToByteArrayContent(IFormFile audofile)
        {
            byte[] data;

            using (var br = new BinaryReader(audofile.OpenReadStream()))
            {
                data = br.ReadBytes((int)audofile.OpenReadStream().Length);
            }
            return data;
        }
    }
}