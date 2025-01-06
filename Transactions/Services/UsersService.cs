using kursah_5semestr.Abstractions;
using kursah_5semestr.Contracts;
using Microsoft.EntityFrameworkCore;

namespace kursah_5semestr.Services
{
    public class UsersService : IUsersService
    {
        private AppDbContext _context;
        private ILogger _logger;

        public UsersService(AppDbContext context, ILogger<UsersService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created user {user.Id}");
            return user;
        }

        public User? GetUserByLogin(string login)
        {
            return _context.Users
                .Where(u => u.Login == login)
                .First();
        }
    }
}
