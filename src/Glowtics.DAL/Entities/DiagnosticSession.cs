using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glowtics.DAL.Entities
{
    public class DiagnosticSession
    {
        public Guid Id { get; set; }

        public Guid RetailerId { get; set; }

        public string SkinProfileResult { get; set; }

        public string? ExternalUserId { get; set; }

        /// <summary>SHA-256 of the uploaded photo — lets a repeat (same user + same image) return the cached result.</summary>
        public string? ImageHash { get; set; }

        public string? Feedback { get; set; }

        public ICollection<Product> RecommendedProducts { get; set; }

        public DateTime CreatedAt { get; set; }


        public Retailer Retailer { get; set; }
    }
}
