using fase_01.application.dtos;
using fase_01.domain.entities;

namespace fase_01.application.mappings
{
    public static class UserMappingExtensions
    {
        /// <summary>
        /// Converte um objeto User para UserDto.
        /// </summary>
        /// <param name="user">Objeto User a ser convertido</param>
        /// <returns>Objeto UserDto</returns>
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                NickName = user.NickName,
                Email = user.Email,
                Admin = user.Admin,
                CreatedAt = user.CreatedAt,
                ValidatedAt = user.ValidatedAt
            };
        }

        /// <summary>
        /// Converte um objeto UserDto para User.
        /// </summary>
        /// <param name="dto">Objeto UserDto a ser convertido</param>
        /// <param name="existing">Objeto User existente para atualizar (opcional)</param>
        /// <returns>Objeto User convertido</returns>
        public static User ToEntity(this UserDto dto, User? existing = null)
        {
            var user = existing ?? new User();
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.NickName = dto.NickName;
            user.Admin = dto.Admin;
            user.CreatedAt = dto.CreatedAt;
            user.ValidatedAt = dto.ValidatedAt;
            return user;
        }

        /// <summary>
        /// Converte um objeto RegisterDto para User.
        /// </summary>
        /// <param name="dto">Objeto RegisterDto a ser convertido</param>
        /// <returns>Objeto User convertido</returns>
        public static User ToEntity(this RegisterDto dto)
        {
            var user = new User();
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.NickName = dto.NickName;
            user.Admin = false;
            user.CreatedAt = DateTime.UtcNow;
            user.ValidatedAt = null;
            return user;
        }
    }
}