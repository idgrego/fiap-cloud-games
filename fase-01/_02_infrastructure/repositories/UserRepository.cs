namespace fase_01.infrastructure.repositories
{
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;
    using fase_01.infrastructure.data;
    using Microsoft.EntityFrameworkCore;

    public class UserRepository
        : BaseRepository<User, int>, IUserRepository
    {
        public UserRepository(AppDbContext context)
            : base(context) { }

        /// <summary>
        /// Creates a new user account with the provided user information and hashed password.
        /// </summary>
        /// <param name="entity">The user information.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>The created user.</returns>
        public async Task<User> RegisterAsync(User entity, string hashedPassword)
        {
            entity.Account = new Account
            {
                PasswordHash = hashedPassword,
                Approved = true,
                FailedCounter = 0,
            };

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        /// <summary>
        /// Get a user by email address, including their account information.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>The user if found, otherwise null.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await this._context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public override async Task UpSertAsync(User entity)
        {
            var existingEntity = await GetByIdAsync(entity.Id);
            if (existingEntity != null)
                await this.UpdateAsync(entity);
            else
                await AddAsync(entity);
        }

        /// <summary>
        /// Informa se existe pelo menos 1 usuário no banco
        /// </summary>
        /// <returns></returns>
        public async Task<bool> hasAnyUser()
        {
            return await this._context.Users.AnyAsync();
        }
    }
}