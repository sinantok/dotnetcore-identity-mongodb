using AspNetCore.Identity.MongoDbCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreIdentity.Models.Identity
{
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public string Name { get; set; }

        public string LastName { get; set; }
    }
}
