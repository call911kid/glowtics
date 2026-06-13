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

        public string RecommendedProducts { get; set; }

        public DateTime CreatedAt { get; set; }


        public Retailer Retailer { get; set; }
    }
}
