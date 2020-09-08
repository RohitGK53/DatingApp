using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        
        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

                var msgFromRepo = await _repo.GetMessage(id);

                if(msgFromRepo == null)
                    return NotFound();
                
            return Ok(msgFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId,[FromQuery] MessageParams messageParams)
        {
                if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                    return Unauthorized();
                
                messageParams.userId = userId;

                var msgsFromRepo =  await _repo.GetMessagesForUser(messageParams);

                var msgs = _mapper.Map<IEnumerable<MessageToReturnDto>>(msgsFromRepo);

                Response.AddPagintion(msgsFromRepo.CurrentPage, msgsFromRepo.PageSize, msgsFromRepo.TotalCount,msgsFromRepo.TotalPages);

                return Ok(msgs);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessagThread(int userId,int recipientId)
        {
             if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var msgFromRepo = await _repo.GetMessageThread(userId,recipientId);

            var msgThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(msgFromRepo);

            return Ok(msgThread);

        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);


            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if(recipient == null)
                return BadRequest("Could not find the user");
            
            var msg = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(msg);

            

            if(await _repo.SaveAll())
            {
                var msgToReturn = _mapper.Map<MessageToReturnDto>(msg);
                return CreatedAtRoute("GetMessage", new {userId, id = msg.Id}, msgToReturn);
            }
                
            
            throw new System.Exception("Creating the message failed on save");

        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id,int userId)
        {
                if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                    return Unauthorized();

                var msgFromRepo = await _repo.GetMessage(id);

                if(msgFromRepo.SenderId == userId)
                    msgFromRepo.SenderDeleted = true;

                if(msgFromRepo.RecipientId == userId)
                    msgFromRepo.RecipientDeleted = true;
                
                if(msgFromRepo.SenderDeleted && msgFromRepo.RecipientDeleted)
                {
                    _repo.Delete(msgFromRepo);
                }

                if(await _repo.SaveAll())
                    return NoContent();

                throw new System.Exception("Error deleting the msg");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId,int id)
        {
                if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                    return Unauthorized();
                
                var msg = await _repo.GetMessage(id);

                if(msg.RecipientId != userId)
                    return Unauthorized();
                
                msg.IsRead = true;
                msg.DateRead = System.DateTime.Now;

                await _repo.SaveAll();

                return NoContent();
        }

    }
}