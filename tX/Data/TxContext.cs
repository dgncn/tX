using System.Drawing;
using Microsoft.EntityFrameworkCore;
using tX.Data.Entities;

namespace tX.Data
{
    public class TxContext : DbContext
    {
        public TxContext(DbContextOptions<TxContext> options)
            : base(options)
        {
        }

        public DbSet<TxEntity> Transactions { get; set; }
    }
}
