using System;
using Glowtics.DAL.Enums;

namespace Glowtics.BLL.Responses
{
    public class CreateRetailerProfileResponse
    {
        public Guid Id { get; set; }
        public string Domain { get; set; }
        public RetailerStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
