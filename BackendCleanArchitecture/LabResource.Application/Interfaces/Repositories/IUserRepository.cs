using LabResource.Domain.Entities;

namespace LabResource.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    Task<IEnumerable<User>> GetAllActiveAsync();

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task SaveChangesAsync();
}