using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Data
{
    public class FileRecord
    {
        public Guid Id { get; set; }
        public string Path { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
    public class FileStoreContext : DbContext
    {
        public FileStoreContext(DbContextOptions<FileStoreContext> options)
            : base(options)
        { }

        public DbSet<FileRecord> Files { get; set; } = null!;
    }
}