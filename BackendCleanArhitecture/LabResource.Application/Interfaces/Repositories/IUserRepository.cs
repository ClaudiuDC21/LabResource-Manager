using LabResource.Domain.Entities;

namespace LabResource.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}