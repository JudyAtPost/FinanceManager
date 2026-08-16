using PersonalFinance.Api.Extensions;
using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;

namespace PersonalFinance.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/api/transactions").WithTags("Transactions");

        group.MapGet("/", async (
                [AsParameters] TransactionFilter filter,
                TransactionService service,
                CancellationToken cancellationToken) =>
                await ResolveMonth(filter.Year, filter.Month).Match(async budgetMonth =>
                {
                    var query = new TransactionQuery(
                        budgetMonth, filter.CategoryId, filter.Type, filter.Page ?? 1, filter.PageSize ?? 50);
                    return Results.Ok(await service.ListAsync(query, cancellationToken));
                }))
            .WithName("ListTransactions")
            .WithSummary("Lists transactions, optionally filtered by month, category, and type.");

        group.MapGet("/{id:guid}", async (Guid id, TransactionService service, CancellationToken cancellationToken) =>
                (await service.GetAsync(id, cancellationToken)).Match(transaction => Results.Ok(transaction)))
            .WithName("GetTransaction")
            .WithSummary("Loads a single transaction.");

        group.MapPost("/", async (SaveTransactionRequest request, TransactionService service, CancellationToken cancellationToken) =>
                (await service.CreateAsync(request, cancellationToken))
                    .Match(created => Results.Created($"/api/transactions/{created.Id}", created)))
            .WithName("CreateTransaction")
            .WithSummary("Records an income or expense.");

        group.MapPut("/{id:guid}", async (Guid id, SaveTransactionRequest request, TransactionService service, CancellationToken cancellationToken) =>
                (await service.UpdateAsync(id, request, cancellationToken)).Match(transaction => Results.Ok(transaction)))
            .WithName("UpdateTransaction")
            .WithSummary("Updates a transaction.");

        group.MapDelete("/{id:guid}", async (Guid id, TransactionService service, CancellationToken cancellationToken) =>
                (await service.DeleteAsync(id, cancellationToken)).Match(() => Results.NoContent()))
            .WithName("DeleteTransaction")
            .WithSummary("Deletes a transaction.");

        return app;
    }

    private static Result<BudgetMonth?> ResolveMonth(int? year, int? month)
    {
        if (year is null || month is null)
        {
            return Result.Success<BudgetMonth?>(null);
        }

        return BudgetMonth.Create(year.Value, month.Value).Map(value => (BudgetMonth?)value);
    }

    private readonly record struct TransactionFilter(
        int? Year,
        int? Month,
        Guid? CategoryId,
        TransactionType? Type,
        int? Page,
        int? PageSize);
}
