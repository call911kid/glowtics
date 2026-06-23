using System;
using System.Collections.Generic;

namespace Glowtics.DAL.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid RetailerId { get; set; }
        public string ExternalProductId { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> TargetConditions { get; set; }
        public List<string> ActiveIngredients { get; set; }
        public List<string> Conflicts { get; set; }
        public List<string> ImageUrls { get; set; }

        public Retailer Retailer { get; set; }
        public ICollection<DiagnosticSession> DiagnosticSessions { get; set; }

        public Product()
        {
            DiagnosticSessions = new HashSet<DiagnosticSession>();
            TargetConditions = new List<string>();
            ActiveIngredients = new List<string>();
            Conflicts = new List<string>();
            ImageUrls = new List<string>();
        }
    }
}
