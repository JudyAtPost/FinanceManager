using PersonalFinance.Application.Summaries;
using PersonalFinance.Domain;

namespace PersonalFinance.Api.Endpoints;

public static class SummaryEndpoints
{
    public static IEndpointRouteBuilder MapSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/api/summary/{year:int}/{month:int}", async (
                int year,
                int month,
                MonthlySummaryService service,
                CancellationToken cancellationToken) =>
            {
                Result<BudgetMonth> budgetMonth = BudgetMonth.Create(year, month);
                return budgetMonth.IsFailure
                    ? budgetMonth.Error!.ToProblem()
                    : Results.Ok(await service.GetAsync(budgetMonth.Value, cancellationToken));
            })
            .WithTags("Summary")
            .WithName("GetMonthlySummary")
            .WithSummary("Returns totals, the category breakdown, budget comparison, and the top expense category of a month.");

        return app;
    }
}
