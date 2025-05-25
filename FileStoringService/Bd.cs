using System;
using System.Collections.Concurrent;

namespace FileStoringService.Data
{
    /// <summary>
    /// Хранит сопоставление между идентификатором файла и путём к нему.
    /// </summary>
    public class Bd
    {
        // Потокобезопасный словарь для хранения (id → путь)
        private readonly ConcurrentDictionary<Guid, string> _files 
            = new ConcurrentDictionary<Guid, string>();

        /// <summary>
        /// Добавить или обновить запись с указанным id и путём.
        /// </summary>
        public void AddFile(Guid id, string path)
        {
            _files[id] = path;
        }

        /// <summary>
        /// Попытаться получить путь по заданному id.
        /// </summary>
        public bool TryGetPath(Guid id, out string path)
        {
            return _files.TryGetValue(id, out path);
        }

        /// <summary>
        /// Удалить запись по id.
        /// </summary>
        public bool RemoveFile(Guid id)
        {
            return _files.TryRemove(id, out _);
        }
    }
}