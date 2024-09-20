using Microsoft.EntityFrameworkCore;
using ContractManagement.Models;

namespace ContractManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Contrato> Contratos { get; set; }
    }
}
