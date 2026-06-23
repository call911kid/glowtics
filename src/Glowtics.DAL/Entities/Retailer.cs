using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.DAL.Enums;

namespace Glowtics.DAL.Entities
{
    public class Retailer
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Domain { get; set; }

        public RetailerStatus Status { get; set; }

        public string? ApiKeyHash { get; set; }

        public string? ApiKeyHint { get; set; }

        public string? ProductEndpoint { get; set; }

        public string? CartRedirectUrl { get; set; }

        public string MongoCollectionName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        

        public GlowticsUser User { get; set; }

        public ICollection<DiagnosticSession> DiagnosticSessions { get; set; }
        public ICollection<Product> Products { get; set; }

        public Retailer()
        {
            DiagnosticSessions = new HashSet<DiagnosticSession>();
            Products = new HashSet<Product>();
        }
    }
}
