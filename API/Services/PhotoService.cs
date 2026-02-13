using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using CloudinaryDotNet.Actions;
using API.Entities;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using System;
using Configuration;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        var result = new ImageUploadResult();
        try
        {
            if (file.Length > 0)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "bV-appPhotos"
                };

                result = await _cloudinary.UploadAsync(uploadParams);
            }
        }
        catch(Exception ex)
        {
            throw new Exception("Photo upload failed: " + ex.Message);
        }
        return result;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var result = new DeletionResult();
        try
        {
            var deleteParams = new DeletionParams(publicId);
            result = await _cloudinary.DestroyAsync(deleteParams);            
        }
        catch(Exception ex)
        {
            throw new Exception("Photo deletion failed: " + ex.Message);
        }
        return result;
    }
}
