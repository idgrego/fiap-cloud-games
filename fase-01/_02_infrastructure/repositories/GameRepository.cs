namespace fase_01.infrastructure.repositories
{
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;
    using fase_01.infrastructure.data;

    public class GameRepository
        : BaseRepository<Game, int>, IGameRepository
    {
        public GameRepository(AppDbContext context)
            : base(context) { }

        public override async Task UpSertAsync(Game entity)
        {
            var existingEntity = await GetByIdAsync(entity.Id);
            if (existingEntity != null)
                await this.UpdateAsync(entity);
            else
                await AddAsync(entity);
        }
    }
}