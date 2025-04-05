using AutoMapper;
using LayeredAppTemplate.Application.Common.Interfaces;

namespace LayeredAppTemplate.Application.Services
{
    public class GenericService<TDto, TEntity, TCreateDto, TUpdateDto> : IService<TDto, TEntity, TCreateDto, TUpdateDto>
        where TEntity : class, new()
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ICacheService _cacheService;

        public GenericService(IRepository<TEntity> repository, IMapper mapper, ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        // Cache key oluştururken, entity tipinin adını kullanıyoruz.
        protected virtual string GetCacheKeyForList() => $"{typeof(TEntity).Name}_List";
        protected virtual string GetCacheKeyForItem(Guid id) => $"{typeof(TEntity).Name}_Id_{id}";

        public async virtual Task<List<TDto>> GetAllAsync()
        {
            var cacheKey = GetCacheKeyForList();
            var cachedList = await _cacheService.GetAsync<List<TDto>>(cacheKey);
            if (cachedList != null)
            {
                return cachedList;
            }

            var entities = await _repository.GetAllAsync();
            var result = _mapper.Map<List<TDto>>(entities);

            // Cache'de 5 dakika süreyle saklayalım.
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async virtual Task<TDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = GetCacheKeyForItem(id);
            var cachedItem = await _cacheService.GetAsync<TDto>(cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }

            var entity = await _repository.GetByIdAsync(id);
            var result = _mapper.Map<TDto>(entity);
            if (result != null)
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            }
            return result;
        }

        public async virtual Task<Guid> CreateAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            // Eğer entity'de Id property varsa, ona yeni bir Guid atayalım.
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Cache'leri temizlemek iyi bir uygulamadır:
            await _cacheService.RemoveAsync(GetCacheKeyForList());
            return (Guid)(idProperty?.GetValue(entity) ?? Guid.Empty);
        }

        public async virtual Task<bool> UpdateAsync(TUpdateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            // Güncelleme sonrası ilgili cache'i temizleyelim.
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null && idProperty.GetValue(entity) is Guid id)
            {
                await _cacheService.RemoveAsync(GetCacheKeyForItem(id));
            }
            await _cacheService.RemoveAsync(GetCacheKeyForList());
            return true;
        }

        public async virtual Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;
            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();

            // Silme sonrası ilgili cache'leri temizle.
            await _cacheService.RemoveAsync(GetCacheKeyForItem(id));
            await _cacheService.RemoveAsync(GetCacheKeyForList());
            return true;
        }
    }
}