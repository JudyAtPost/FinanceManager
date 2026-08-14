using PersonalFinance.Application.Summaries;
using PersonalFinance.Domain;

namespace PersonalFinance.Api.Endpoints;

/// <summary>
/// Maps the monthly summary endpoint.
/// </summary>
public static class SummaryEndpoints
{
    /// <summary>Maps the monthly overview endpoint.</summary>
    /// <param name="app">The route builder to map onto.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/api/summary/{year:int}/{month:int}", (
                int year,
                int month,
                MonthlySummaryService service,
                CancellationToken cancellationToken) =>
                service.GetAsync(new BudgetMonth(year, month), cancellationToken))
            .WithTags("Summary")
            .WithName("GetMonthlySummary")
            .WithSummary("Returns totals, the category breakdown, budget comparison, and the top expense category of a month.");

        return app;
    }
}
