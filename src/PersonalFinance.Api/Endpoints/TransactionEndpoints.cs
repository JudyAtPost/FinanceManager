using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;

namespace PersonalFinance.Api.Endpoints;

/// <summary>
/// Maps the transaction endpoints.
/// </summary>
public static class TransactionEndpoints
{
    /// <summary>Maps CRUD and filtering endpoints for transactions.</summary>
    /// <param name="app">The route builder to map onto.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/api/transactions").WithTags("Transactions");

        group.MapGet("/", (
                int? year,
                int? month,
                Guid? categoryId,
                TransactionType? type,
                int? page,
                int? pageSize,
                TransactionService service,
                CancellationToken cancellationToken) =>
            {
                BudgetMonth? budgetMonth = year is { } y && month is { } m ? new BudgetMonth(y, m) : null;
                var query = new TransactionQuery(budgetMonth, categoryId, type, page ?? 1, pageSize ?? 50);
                return service.ListAsync(query, cancellationToken);
            })
            .WithName("ListTransactions")
            .WithSummary("Lists transactions, optionally filtered by month, category, and type.");

        group.MapGet("/{id:guid}", (Guid id, TransactionService service, CancellationToken cancellationToken) =>
                service.GetAsync(id, cancellationToken))
            .WithName("GetTransaction")
            .WithSummary("Loads a single transaction.");

        group.MapPost("/", async (SaveTransactionRequest request, TransactionService service, CancellationToken cancellationToken) =>
            {
                TransactionDto created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/transactions/{created.Id}", created);
            })
            .WithName("CreateTransaction")
            .WithSummary("Records an income or expense.");

        group.MapPut("/{id:guid}", (Guid id, SaveTransactionRequest request, TransactionService service, CancellationToken cancellationToken) =>
                service.UpdateAsync(id, request, cancellationToken))
            .WithName("UpdateTransaction")
            .WithSummary("Updates a transaction.");

        group.MapDelete("/{id:guid}", async (Guid id, TransactionService service, CancellationToken cancellationToken) =>
            {
                await service.DeleteAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteTransaction")
            .WithSummary("Deletes a transaction.");

        return app;
    }
}
