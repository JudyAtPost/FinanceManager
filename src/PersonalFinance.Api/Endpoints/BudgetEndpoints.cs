using PersonalFinance.Application.Budgets;
using PersonalFinance.Domain;

namespace PersonalFinance.Api.Endpoints;

/// <summary>
/// Maps the budget endpoints.
/// </summary>
public static class BudgetEndpoints
{
    /// <summary>Maps CRUD and comparison endpoints for monthly budgets.</summary>
    /// <param name="app">The route builder to map onto.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapGet("/", (int year, int month, BudgetService service, CancellationToken cancellationToken) =>
                service.ListAsync(new BudgetMonth(year, month), cancellationToken))
            .WithName("ListBudgets")
            .WithSummary("Lists the budgets defined for one month.");

        group.MapGet("/comparison", (int year, int month, BudgetService service, CancellationToken cancellationToken) =>
                service.CompareAsync(new BudgetMonth(year, month), cancellationToken))
            .WithName("CompareBudgets")
            .WithSummary("Compares the budgets of one month against actual spending.");

        group.MapPost("/", async (CreateBudgetRequest request, BudgetService service, CancellationToken cancellationToken) =>
            {
                BudgetDto created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/budgets/{created.Id}", created);
            })
            .WithName("CreateBudget")
            .WithSummary("Sets a spending limit for one category and month.");

        group.MapPut("/{id:guid}", (Guid id, UpdateBudgetRequest request, BudgetService service, CancellationToken cancellationToken) =>
                service.UpdateAsync(id, request, cancellationToken))
            .WithName("UpdateBudget")
            .WithSummary("Changes the limit of an existing budget.");

        group.MapDelete("/{id:guid}", async (Guid id, BudgetService service, CancellationToken cancellationToken) =>
            {
                await service.DeleteAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteBudget")
            .WithSummary("Deletes a budget.");

        return app;
    }
}
