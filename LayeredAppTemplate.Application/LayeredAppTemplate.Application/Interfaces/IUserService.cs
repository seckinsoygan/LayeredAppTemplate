using LayeredAppTemplate.Application.Common.Interfaces;
using LayeredAppTemplate.Application.DTOs.User;
using LayeredAppTemplate.Domain.Entities;

namespace LayeredAppTemplate.Application.Interfaces;
public interface IUserService : IService<UserDto, User, CreateUserDto, UpdateUserDto>
{
    // Domain'e özel ek operasyonlar eklenebilir
    Task<List<UserDto>> GetByEmailDomainAsync(string domain);
}
