using PersonalFinance.Application.Categories;

namespace PersonalFinance.Api.Endpoints;

/// <summary>
/// Maps the category endpoints.
/// </summary>
public static class CategoryEndpoints
{
    /// <summary>Maps CRUD endpoints for categories.</summary>
    /// <param name="app">The route builder to map onto.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", (CategoryService service, CancellationToken cancellationToken) =>
                service.ListAsync(cancellationToken))
            .WithName("ListCategories")
            .WithSummary("Lists all categories.");

        group.MapGet("/{id:guid}", (Guid id, CategoryService service, CancellationToken cancellationToken) =>
                service.GetAsync(id, cancellationToken))
            .WithName("GetCategory")
            .WithSummary("Loads a single category.");

        group.MapPost("/", async (SaveCategoryRequest request, CategoryService service, CancellationToken cancellationToken) =>
            {
                CategoryDto created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/categories/{created.Id}", created);
            })
            .WithName("CreateCategory")
            .WithSummary("Creates a category.");

        group.MapPut("/{id:guid}", (Guid id, SaveCategoryRequest request, CategoryService service, CancellationToken cancellationToken) =>
                service.UpdateAsync(id, request, cancellationToken))
            .WithName("UpdateCategory")
            .WithSummary("Updates a category.");

        group.MapDelete("/{id:guid}", async (Guid id, CategoryService service, CancellationToken cancellationToken) =>
            {
                await service.DeleteAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteCategory")
            .WithSummary("Deletes a category that is no longer referenced.");

        return app;
    }
}
