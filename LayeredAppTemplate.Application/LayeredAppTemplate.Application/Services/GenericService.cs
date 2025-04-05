using AutoMapper;
using LayeredAppTemplate.Application.Common.Interfaces;

namespace LayeredAppTemplate.Application.Services
{
    public class GenericService<TDto, TEntity> : IService<TDto, TEntity>
        where TEntity : class, new()
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IMapper _mapper;

        public GenericService(IRepository<TEntity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<TDto>>(entities);
        }

        public async Task<TDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<TDto>(entity);
        }

        public async Task<Guid> CreateAsync(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            var idProperty = typeof(TEntity).GetProperty("Id");

            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return (Guid)(idProperty?.GetValue(entity) ?? Guid.Empty);
        }

        public async Task<bool> UpdateAsync(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
