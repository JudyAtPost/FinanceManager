using Microsoft.EntityFrameworkCore;
using Npgsql;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    /// <summary>PostgreSQL <c>unique_violation</c>.</summary>
    private const string UniqueViolation = "23505";

    /// <summary>PostgreSQL <c>foreign_key_violation</c>.</summary>
    private const string ForeignKeyViolation = "23503";

    /// <summary>PostgreSQL <c>check_violation</c>.</summary>
    private const string CheckViolation = "23514";

    /// <summary>PostgreSQL <c>not_null_violation</c>.</summary>
    private const string NotNullViolation = "23502";

    private readonly FinanceDbContext _context;

    public UnitOfWork(FinanceDbContext context) => _context = context;

    /// <inheritdoc />
    /// <remarks>
    /// The database — not an application-level pre-check — is the authority on uniqueness and referential
    /// integrity, because any check-then-act sequence has a window in which a concurrent request can slip
    /// through. Translating the violation here turns that race from a 500 into the correct 409.
    /// </remarks>
    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict(
                "The record was modified by another request. Reload it and retry with the current values.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres
            && TryTranslate(postgres, out Error? error))
        {
            return error!;
        }
    }

    private static bool TryTranslate(PostgresException postgres, out Error? error)
    {
        error = postgres.SqlState switch
        {
            UniqueViolation => Error.Conflict("A record with the same unique values already exists."),
            ForeignKeyViolation => Error.Conflict(
                "The record is still referenced by other records, or references one that no longer exists."),
            CheckViolation or NotNullViolation => Error.Validation("The record violates a database constraint."),
            _ => null
        };

        return error is not null;
    }
}
