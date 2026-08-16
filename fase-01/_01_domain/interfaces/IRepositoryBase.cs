namespace fase_01.domain.interfaces
{
    public interface IRepositoryBase<T, TKey> where T : class
    {
        Task UpSertAsync(T entity);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(TKey id);
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> ListAllAsync();
    }
}