using Microsoft.EntityFrameworkCore;

namespace FileAnalisysService.Data
{
    public class FileAnalysisRecord
    {
        public Guid Id { get; set; }
        public int ParagraphCount { get; set; }
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public bool IsPlagiarized { get; set; }
        public string Hash { get; set; } = null!;
        public DateTime AnalyzedAt { get; set; }
    }
    public class AnalysisContext : DbContext
    {
        public AnalysisContext(DbContextOptions<AnalysisContext> options)
            : base(options) { }

        public DbSet<FileAnalysisRecord> Analyses { get; set; } = null!;
    }
}