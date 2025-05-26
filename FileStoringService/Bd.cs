using System;
using System.Collections.Concurrent;

namespace FileStoringService.Data
{
    public class Bd
    {
        private readonly ConcurrentDictionary<Guid, string> _files 
            = new ConcurrentDictionary<Guid, string>();


        public void AddFile(Guid id, string path)
        {
            _files[id] = path;
        }
        public bool TryGetPath(Guid id, out string path)
        {
            return _files.TryGetValue(id, out path);
        }

        public bool RemoveFile(Guid id)
        {
            return _files.TryRemove(id, out _);
        }
    }
}