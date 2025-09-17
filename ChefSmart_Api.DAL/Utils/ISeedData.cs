using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefSmart_Api.DAL.Utils
{
    public interface ISeedData
    {
        Task DataSeeding();
        Task IdentityDataSeeding();
    }
}
