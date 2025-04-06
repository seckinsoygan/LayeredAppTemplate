using LayeredAppTemplate.Application.Common.Interfaces;
using LayeredAppTemplate.Domain.Entities;
using LayeredAppTemplate.Persistence;
using LayeredAppTemplate.Persistence.Repositories;

namespace LayeredAppTemplate.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IRepository<User>? _userRepository;
        // Diğer repository'ler için benzer alanlar

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _userRepository ??= new GenericRepository<User>(_context);
        // Örneğin, eğer Product repository ekleyecekseniz:
        // public IRepository<Product> Products => _productRepository ??= new GenericRepository<Product>(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
