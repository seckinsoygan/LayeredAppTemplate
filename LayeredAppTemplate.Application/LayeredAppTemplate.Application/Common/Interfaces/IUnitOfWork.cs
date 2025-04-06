using LayeredAppTemplate.Domain.Entities;

namespace LayeredAppTemplate.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        // Diğer repository'ler de eklenebilir, örneğin:
        // IRepository<Product> Products { get; }

        Task<int> CommitAsync();
    }
}
