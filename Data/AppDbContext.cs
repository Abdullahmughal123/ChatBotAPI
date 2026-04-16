using Microsoft.EntityFrameworkCore;
namespace ChatbotApi.Data
{
    using ChatbotApi.Models;
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
