namespace LayeredAppTemplate.Application.Common.Interfaces;
public interface IService<TDto, TEntity>
{
    Task<List<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TDto dto);
    Task<bool> UpdateAsync(TDto dto);
    Task<bool> DeleteAsync(Guid id);
}
