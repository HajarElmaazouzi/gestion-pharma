using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace gestion_pharma.Data
{
    
        public class ApplicationIdentityDbContext: IdentityDbContext<Userpers>
        {
            public ApplicationIdentityDbContext(
                DbContextOptions<ApplicationIdentityDbContext> options)
                : base(options)
            {
            }
        }
    }

