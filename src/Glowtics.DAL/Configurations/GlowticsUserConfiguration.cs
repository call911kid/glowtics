using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glowtics.DAL.Configurations
{
    public class GlowticsUserConfiguration:IEntityTypeConfiguration<GlowticsUser>
    {
        public void Configure(EntityTypeBuilder<GlowticsUser> builder)
        {
            
        }
    }
}
