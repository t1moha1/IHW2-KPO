using System;
using System.Collections.Concurrent;

namespace FileAnalisysService.Data
{
    public class FileAnalysisInfo
    {
        public int ParagraphCount { get; set; }
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public bool IsPlagiarized { get; set; }
        public string? Hash { get; set; }

        public FileAnalysisInfo() { }

        public FileAnalysisInfo(int paragraphCount, int wordCount, int characterCount, bool isPlagiarized, string hash)
        {
            ParagraphCount = paragraphCount;
            WordCount = wordCount;
            CharacterCount = characterCount;
            IsPlagiarized = isPlagiarized;
            Hash = hash;
        }
    }

    public class Db
    {
        private readonly ConcurrentDictionary<Guid, FileAnalysisInfo> _files
            = new ConcurrentDictionary<Guid, FileAnalysisInfo>();

        public void AddFile(Guid id, FileAnalysisInfo fileInfo)
        {
            _files[id] = fileInfo;
        }

        public bool TryGetInfo(Guid id, out FileAnalysisInfo fileInfo)
        {
            return _files.TryGetValue(id, out fileInfo);
        }

        public bool RemoveFile(Guid id)
        {
            return _files.TryRemove(id, out _);
        }
        public bool HasHash(string hash)
        {
            return _files.Values.Any(file => file.Hash == hash);
        }

    }
}
