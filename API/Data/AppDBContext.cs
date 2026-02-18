using System;
using Microsoft.EntityFrameworkCore;
using API.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
    public class AppDBContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set;}
        public DbSet<Member> Members { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<MemberLike> MemberLikes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<MemberLike>()
                .HasKey(k => new { k.SourceMemberId, k.TargetMemberId });
            
            modelBuilder.Entity<MemberLike>()
                .HasOne(s => s.SourceMember)
                .WithMany(l => l.LikedMembers)
                .HasForeignKey(s => s.SourceMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MemberLike>()
                .HasOne(s => s.TargetMember)
                .WithMany(l => l.LikedByMembers)
                .HasForeignKey(s => s.TargetMemberId)
                .OnDelete(DeleteBehavior.NoAction);


            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                }
            }
        }
    }
}
