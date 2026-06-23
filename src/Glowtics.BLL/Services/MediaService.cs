using System;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;

namespace Glowtics.BLL.Services
{
    public class MediaService : IMediaService
    {
        public string ConvertImageToBase64(byte[] fileData)
        {
            if (fileData == null || fileData.Length == 0)
            {
                throw new BusinessRuleViolationException("File data cannot be empty.");
            }

            return Convert.ToBase64String(fileData);        
        }       
    }
}
