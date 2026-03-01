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
    public class MessagesController(IUnitOfWork _unitOfWork) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var sender = await _unitOfWork.MemberRepository.GetMemberByIdAsync(User.GetMemberId());
            var recipient = await _unitOfWork.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
            if(recipient == null || sender == null || sender.Id == recipient.Id) return NotFound("Cannot send this message.");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content
            };
            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete()) return message.ToDto();

            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer([FromQuery] MessageParams messageParams)
        {
            messageParams.MemberId = User.GetMemberId();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForMember(messageParams);
            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
        {
            var currentUserId = User.GetMemberId();
            var messages = await _unitOfWork.MessageRepository.GetMessageThread(currentUserId, recipientId);
            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(string id)
        {
            var currentUserId = User.GetMemberId();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            if (message == null) return NotFound("Message not found.");
            if (message.SenderId != currentUserId && message.RecipientId != currentUserId) return BadRequest("You are not authorized to delete this message.");

            if(message.SenderId == currentUserId) message.SenderDeleted = true;
            if(message.RecipientId == currentUserId) message.RecipientDeleted = true;

            if(message is { SenderDeleted: true, RecipientDeleted: true })
            {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }
            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete message.");
        }
    }
}