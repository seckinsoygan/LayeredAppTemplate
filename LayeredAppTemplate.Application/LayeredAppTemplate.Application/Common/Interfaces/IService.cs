namespace LayeredAppTemplate.Application.Common.Interfaces;
public interface IService<TDto, TEntity, in TCreateDto, in TUpdateDto>
{
    Task<List<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TCreateDto dto);
    Task<bool> UpdateAsync(TUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
