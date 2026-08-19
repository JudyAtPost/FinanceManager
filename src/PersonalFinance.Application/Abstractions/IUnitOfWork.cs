using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken);
}
