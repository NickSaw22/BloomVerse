using System;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UnitOfWork(AppDBContext context) : IUnitOfWork
    {
        private IMemberRepository? _memberRepository;
        private ILikesRepository? _likesRepository;
        private IMessageRepository? _messageRepository;
        private IPhotoRepository? _photoRepository;

        public IMemberRepository MemberRepository => 
            _memberRepository ??= new MemberRepository(context);

        public ILikesRepository LikesRepository => 
            _likesRepository ??= new LikesRepository(context);

        public IMessageRepository MessageRepository => 
            _messageRepository ??= new MessageRepository(context);

        public IPhotoRepository PhotoRepository =>
            _photoRepository ??= new PhotoRepository(context);

        public async Task<bool> Complete()
        {
            try{
                return await context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                // Handle the exception or log it
                throw new Exception("An error occurred while saving changes to the database.", ex)  ;
            }
        }

        public bool HasChanges()
        {
            return context.ChangeTracker.HasChanges();
        }
    }
}