using Microsoft.EntityFrameworkCore;

namespace MIS_Project_API.Controllers
{
    public class AppDbContext : DbContext
    {
        internal async Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}