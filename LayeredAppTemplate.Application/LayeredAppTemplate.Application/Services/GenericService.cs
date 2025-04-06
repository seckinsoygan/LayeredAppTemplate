using AutoMapper;
using LayeredAppTemplate.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LayeredAppTemplate.Application.Services
{
    public class GenericService<TDto, TEntity, TCreateDto, TUpdateDto> : IService<TDto, TEntity, TCreateDto, TUpdateDto>
        where TEntity : class, new()
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly ICacheService _cacheService;
        protected readonly ILogger<GenericService<TDto, TEntity, TCreateDto, TUpdateDto>> _logger;

        public GenericService(
            IRepository<TEntity> repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<GenericService<TDto, TEntity, TCreateDto, TUpdateDto>> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
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
                _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
                return cachedList;
            }

            _logger.LogDebug("Cache miss for {CacheKey}. Fetching from repository.", cacheKey);
            var entities = await _repository.GetAllAsync();
            var result = _mapper.Map<List<TDto>>(entities);

            // Cache'de varsayılan süre (ör. 5 dakika) ile saklayalım.
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async virtual Task<TDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = GetCacheKeyForItem(id);
            var cachedItem = await _cacheService.GetAsync<TDto>(cacheKey);
            if (cachedItem != null)
            {
                _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
                return cachedItem;
            }

            _logger.LogDebug("Cache miss for {CacheKey}. Fetching from repository.", cacheKey);
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
            await _unitOfWork.CommitAsync(); // Tüm değişiklikleri commit et

            // İlgili cache'leri temizleyelim.
            await _cacheService.RemoveAsync(GetCacheKeyForList());

            _logger.LogInformation("Created {EntityType} with Id {Id}", typeof(TEntity).Name, idProperty?.GetValue(entity));
            return (Guid)(idProperty?.GetValue(entity) ?? Guid.Empty);
        }

        public async virtual Task<bool> UpdateAsync(TUpdateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repository.UpdateAsync(entity);
            await _unitOfWork.CommitAsync(); // Güncellemeyi commit et

            // Güncelleme sonrası ilgili cache'i temizleyelim.
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null && idProperty.GetValue(entity) is Guid id)
            {
                await _cacheService.RemoveAsync(GetCacheKeyForItem(id));
            }
            await _cacheService.RemoveAsync(GetCacheKeyForList());

            _logger.LogInformation("Updated {EntityType}", typeof(TEntity).Name);
            return true;
        }

        public async virtual Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("{EntityType} with Id {Id} not found for deletion", typeof(TEntity).Name, id);
                return false;
            }
            await _repository.DeleteAsync(entity);
            await _unitOfWork.CommitAsync(); // Silme işlemini commit et

            // Silme sonrası ilgili cache'leri temizle.
            await _cacheService.RemoveAsync(GetCacheKeyForItem(id));
            await _cacheService.RemoveAsync(GetCacheKeyForList());

            _logger.LogInformation("Deleted {EntityType} with Id {Id}", typeof(TEntity).Name, id);
            return true;
        }
    }
}
