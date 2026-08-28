using fase_01.application.dtos;
using fase_01.application.mappings;
using fase_01.domain.entities;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Mappings
{
    public class UserMappingExtensionsTests
    {
        [Fact]
        public void UserToDto_ShouldMapAllPropertiescorrectly()
        {
            // Arrange
            var entity = new User
            {
                Id = 1,
                FullName = "John Doe",
                NickName = "johnd",
                Email = "john@example.com",
                Admin = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            dto.Id.Should().Be(entity.Id);
            dto.FullName.Should().Be(entity.FullName);
            dto.NickName.Should().Be(entity.NickName);
            dto.Email.Should().Be(entity.Email);
            dto.Admin.Should().Be(entity.Admin);
            dto.CreatedAt.Should().Be(entity.CreatedAt);
        }

        [Fact]
        public void DtoToUser_ShouldMapAllPropertiescorrectly()
        {
            // Arrange
            var dto = new UserDto
            {
                Id = 1,
                FullName = "John Doe",
                NickName = "johnd",
                Email = "john@example.com",
                Admin = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            entity.FullName.Should().Be(dto.FullName);
            entity.NickName.Should().Be(dto.NickName);
            entity.Email.Should().Be(dto.Email);
            entity.Admin.Should().Be(dto.Admin);
            entity.CreatedAt.Should().Be(dto.CreatedAt);
        }

    }
}