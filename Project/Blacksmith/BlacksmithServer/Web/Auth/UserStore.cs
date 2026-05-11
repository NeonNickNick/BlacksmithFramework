using System.Text.Json;

namespace BlacksmithServer.Web.Auth
{
    public sealed class UserStore
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly Dictionary<string, StoredUser> _users = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _filePath;

        public UserStore()
        {
            var directory = Path.Combine(AppContext.BaseDirectory, ".blacksmith-server");
            Directory.CreateDirectory(directory);
            _filePath = Path.Combine(directory, "users.json");
            LoadFromDisk();
        }

        public async Task<StoredUser?> FindAsync(string username)
        {
            await _gate.WaitAsync();
            try
            {
                return _users.TryGetValue(username, out var user) ? user : null;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<bool> TryAddAsync(StoredUser user)
        {
            await _gate.WaitAsync();
            try
            {
                if (_users.ContainsKey(user.Username))
                {
                    return false;
                }

                _users[user.Username] = user;
                await PersistAsync();
                return true;
            }
            finally
            {
                _gate.Release();
            }
        }

        private void LoadFromDisk()
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            try
            {
                var content = File.ReadAllText(_filePath);
                var users = JsonSerializer.Deserialize<List<StoredUser>>(content) ?? new();
                foreach (var user in users)
                {
                    _users[user.Username] = user;
                }
            }
            catch
            {
                Console.WriteLine($"Failed to load user store at {_filePath}. Starting with an empty store.");
            }
        }

        private async Task PersistAsync()
        {
            var content = JsonSerializer.Serialize(_users.Values.OrderBy(u => u.Username).ToList(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_filePath, content);
        }
    }

    public sealed class StoredUser
    {
        public required string Username { get; set; }
        public required string SaltBase64 { get; set; }
        public required string PasswordHashBase64 { get; set; }
        public required DateTime CreatedAtUtc { get; set; }
    }
}
