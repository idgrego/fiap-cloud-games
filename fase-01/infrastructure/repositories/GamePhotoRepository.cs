namespace fase_01.infrastructure.repositories
{
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;
    using fase_01.infrastructure.data;

    public class GamePhotoRepository
        : BaseRepository<GamePhoto, int>, IGamePhotoRepository
    {
        public GamePhotoRepository(AppDbContext context)
            : base(context) { }

        public override async Task UpSertAsync(GamePhoto entity)
        {
            var existingEntity = await GetByIdAsync(entity.Id);
            if (existingEntity != null)
                await this.UpdateAsync(entity);
            else
                await AddAsync(entity);
        }
    }
}