using fase_01.application.dtos;
using fase_01.domain.entities;

namespace fase_01.application.mappings
{
    public static class GameMappingExtensions
    {
        /// <summary>
        /// Converte um objeto Game para GameDto.
        /// </summary>
        /// <param name="game">Objeto Game a ser convertido</param>
        /// <returns>Objeto GameDto</returns>
        public static GameDto ToDto(this Game game)
        {
            return new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                Manufacturer = game.Manufacturer,
                Description = game.Description,
                Online = game.Online,
                Multiplayer = game.Multiplayer,
                CategoryId = game.CategoryId,
                CategoryName = game.Category.Name,
                UrlGame = game.UrlGame,
                UrlVideo = game.UrlVideo,
                CreatedAt = game.CreatedAt
            };
        }

        /// <summary>
        /// Converte um objeto GameDto para Game.
        /// </summary>
        /// <param name="dto">Objeto GameDto a ser convertido</param>
        /// <param name="existing">Objeto Game existente para atualizar (opcional)</param>
        /// <returns>Objeto Game convertido</returns>
        public static Game ToEntity(this GameDto dto, Game? existing = null)
        {
            var game = existing ?? new Game();
            game.Name = dto.Name;
            game.Manufacturer = dto.Manufacturer;
            game.Description = dto.Description;
            game.Online = dto.Online;
            game.Multiplayer = dto.Multiplayer;
            game.CategoryId = dto.CategoryId;
            game.UrlGame = dto.UrlGame;
            game.UrlVideo = dto.UrlVideo;
            return game;
        }
    }
}