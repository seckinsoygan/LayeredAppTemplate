using LayeredAppTemplate.Application.Common.Interfaces;
using LayeredAppTemplate.Application.DTOs.User;
using LayeredAppTemplate.Domain.Entities;

namespace LayeredAppTemplate.Application.Interfaces;
public interface IUserService : IService<UserDto, User>
{
    // Domain'e özel ek operasyonlar varsa buraya:
    Task<List<UserDto>> GetByEmailDomainAsync(string domain);
}
