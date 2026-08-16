using PersonalFinance.Application.Budgets;
using PersonalFinance.Domain;

namespace PersonalFinance.Api.Endpoints;

public static class BudgetEndpoints
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapGet("/", async (int year, int month, BudgetService service, CancellationToken cancellationToken) =>
                await BudgetMonth.Create(year, month)
                    .Match(async budgetMonth => Results.Ok(await service.ListAsync(budgetMonth, cancellationToken))))
            .WithName("ListBudgets")
            .WithSummary("Lists the budgets defined for one month.");

        group.MapGet("/comparison", async (int year, int month, BudgetService service, CancellationToken cancellationToken) =>
                await BudgetMonth.Create(year, month)
                    .Match(async budgetMonth => Results.Ok(await service.CompareAsync(budgetMonth, cancellationToken))))
            .WithName("CompareBudgets")
            .WithSummary("Compares the budgets of one month against actual spending.");

        group.MapPost("/", async (CreateBudgetRequest request, BudgetService service, CancellationToken cancellationToken) =>
                (await service.CreateAsync(request, cancellationToken))
                    .Match(created => Results.Created($"/api/budgets/{created.Id}", created)))
            .WithName("CreateBudget")
            .WithSummary("Sets a spending limit for one category and month.");

        group.MapPut("/{id:guid}", async (Guid id, UpdateBudgetRequest request, BudgetService service, CancellationToken cancellationToken) =>
                (await service.UpdateAsync(id, request, cancellationToken)).Match(budget => Results.Ok(budget)))
            .WithName("UpdateBudget")
            .WithSummary("Changes the limit of an existing budget.");

        group.MapDelete("/{id:guid}", async (Guid id, BudgetService service, CancellationToken cancellationToken) =>
                (await service.DeleteAsync(id, cancellationToken)).Match(() => Results.NoContent()))
            .WithName("DeleteBudget")
            .WithSummary("Deletes a budget.");

        return app;
    }
}
