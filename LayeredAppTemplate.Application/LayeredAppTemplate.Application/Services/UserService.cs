using AutoMapper;
using LayeredAppTemplate.Application.Common.Interfaces;
using LayeredAppTemplate.Application.DTOs.User;
using LayeredAppTemplate.Application.Interfaces;
using LayeredAppTemplate.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LayeredAppTemplate.Application.Services
{
    public class UserService : GenericService<UserDto, User, CreateUserDto, UpdateUserDto>, IUserService
    {
        public UserService(IRepository<User> repository, IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, ILogger<GenericService<UserDto, User, CreateUserDto, UpdateUserDto>> logger) : base(repository, unitOfWork, mapper, cacheService, logger)
        {
        }

        public async Task<List<UserDto>> GetByEmailDomainAsync(string domain)
        {
            var users = await _repository.GetAllAsync();
            var filtered = users.Where(u => u.Email.EndsWith("@" + domain)).ToList();
            return _mapper.Map<List<UserDto>>(filtered);
        }
    }
}
