using System;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController(IMessageRepository messageRepository, IMemberRepository memberRepository) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var sender = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
            var recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
            if(recipient == null || sender == null || sender.Id == recipient.Id) return NotFound("Cannot send this message.");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content
            };
            messageRepository.AddMessage(message);
            if (await messageRepository.SaveAllAsync()) return message.ToDto();

            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer([FromQuery] MessageParams messageParams)
        {
            messageParams.MemberId = User.GetMemberId();
            var messages = await messageRepository.GetMessagesForMember(messageParams);
            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
        {
            var currentUserId = User.GetMemberId();
            var messages = await messageRepository.GetMessageThread(currentUserId, recipientId);
            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(string id)
        {
            var currentUserId = User.GetMemberId();
            var message = await messageRepository.GetMessage(id);
            if (message == null) return NotFound("Message not found.");
            if (message.SenderId != currentUserId && message.RecipientId != currentUserId) return BadRequest("You are not authorized to delete this message.");

            if(message.SenderId == currentUserId) message.SenderDeleted = true;
            if(message.RecipientId == currentUserId) message.RecipientDeleted = true;

            if(message is { SenderDeleted: true, RecipientDeleted: true })
            {
                messageRepository.DeleteMessage(message);
            }
            if (await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete message.");
        }
    }
}