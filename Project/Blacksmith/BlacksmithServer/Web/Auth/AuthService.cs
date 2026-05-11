using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BlacksmithServer.Web.Auth
{
    public sealed class AuthService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        private static readonly Regex UsernamePattern = new("^[A-Za-z0-9_]{3,24}$", RegexOptions.Compiled);

        private readonly UserStore _userStore;
        private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();

        public AuthService(UserStore userStore)
        {
            _userStore = userStore;
        }

        public async Task<AuthResponse> RegisterAsync(string? username, string? password)
        {
            var normalizedUsername = NormalizeUsername(username);
            var passwordText = password?.Trim() ?? string.Empty;
            var validation = ValidateCredentials(normalizedUsername, passwordText);
            if (validation != null)
            {
                return validation;
            }

            if (await _userStore.FindAsync(normalizedUsername!) != null)
            {
                return new AuthResponse
                {
                    Ok = false,
                    Message = "Username already exists."
                };
            }

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = HashPassword(passwordText, salt);

            var added = await _userStore.TryAddAsync(new StoredUser
            {
                Username = normalizedUsername!,
                SaltBase64 = Convert.ToBase64String(salt),
                PasswordHashBase64 = Convert.ToBase64String(hash),
                CreatedAtUtc = DateTime.UtcNow
            });

            if (!added)
            {
                return new AuthResponse
                {
                    Ok = false,
                    Message = "Username already exists."
                };
            }

            return CreateSession(normalizedUsername!, "Registration successful.");
        }

        public async Task<AuthResponse> LoginAsync(string? username, string? password)
        {
            var normalizedUsername = NormalizeUsername(username);
            var passwordText = password?.Trim() ?? string.Empty;
            var validation = ValidateCredentials(normalizedUsername, passwordText);
            if (validation != null)
            {
                return validation;
            }

            var user = await _userStore.FindAsync(normalizedUsername!);
            if (user == null)
            {
                return new AuthResponse
                {
                    Ok = false,
                    Message = "Invalid username or password."
                };
            }

            var salt = Convert.FromBase64String(user.SaltBase64);
            var hash = HashPassword(passwordText, salt);
            var expected = Convert.FromBase64String(user.PasswordHashBase64);
            if (!CryptographicOperations.FixedTimeEquals(hash, expected))
            {
                return new AuthResponse
                {
                    Ok = false,
                    Message = "Invalid username or password."
                };
            }

            return CreateSession(user.Username, "Login successful.");
        }

        public bool TryGetUsername(string? token, out string username)
        {
            username = string.Empty;
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (_sessions.TryGetValue(token, out var session))
            {
                username = session.Username;
                return true;
            }

            return false;
        }

        public void Logout(string token)
        {
            _sessions.TryRemove(token, out _);
        }

        private static string? NormalizeUsername(string? username)
        {
            var trimmed = username?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static AuthResponse? ValidateCredentials(string? username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || !UsernamePattern.IsMatch(username))
            {
                return new AuthResponse
                {
                    Ok = false,
                    Message = "Username must be 3-24 characters and only contain letters, digits, or underscores."
                };
            }

            if (password.Length < 6 || password.Length > 128)
            {
                return new AuthResponse
                {
                    Ok = false,
                    Message = "Password must be between 6 and 128 characters."
                };
            }

            return null;
        }

        private AuthResponse CreateSession(string username, string message)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _sessions[token] = new SessionInfo(username, DateTime.UtcNow);

            return new AuthResponse
            {
                Ok = true,
                Message = message,
                Token = token,
                Username = username
            };
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        }

        private sealed record SessionInfo(string Username, DateTime CreatedAtUtc);
    }
}
