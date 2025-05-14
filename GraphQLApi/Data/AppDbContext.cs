
using Microsoft.EntityFrameworkCore;
using GraphQLApi.Models;

namespace GraphQLApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Book> Books => Set<Book>();
    }
}
