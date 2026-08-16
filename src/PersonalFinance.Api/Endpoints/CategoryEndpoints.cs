using PersonalFinance.Application.Categories;

namespace PersonalFinance.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", (CategoryService service, CancellationToken cancellationToken) =>
                service.ListAsync(cancellationToken))
            .WithName("ListCategories")
            .WithSummary("Lists all categories.");

        group.MapGet("/{id:guid}", async (Guid id, CategoryService service, CancellationToken cancellationToken) =>
                (await service.GetAsync(id, cancellationToken)).Match(category => Results.Ok(category)))
            .WithName("GetCategory")
            .WithSummary("Loads a single category.");

        group.MapPost("/", async (SaveCategoryRequest request, CategoryService service, CancellationToken cancellationToken) =>
                (await service.CreateAsync(request, cancellationToken))
                    .Match(created => Results.Created($"/api/categories/{created.Id}", created)))
            .WithName("CreateCategory")
            .WithSummary("Creates a category.");

        group.MapPut("/{id:guid}", async (Guid id, SaveCategoryRequest request, CategoryService service, CancellationToken cancellationToken) =>
                (await service.UpdateAsync(id, request, cancellationToken)).Match(category => Results.Ok(category)))
            .WithName("UpdateCategory")
            .WithSummary("Updates a category.");

        group.MapDelete("/{id:guid}", async (Guid id, CategoryService service, CancellationToken cancellationToken) =>
                (await service.DeleteAsync(id, cancellationToken)).Match(() => Results.NoContent()))
            .WithName("DeleteCategory")
            .WithSummary("Deletes a category that is no longer referenced.");

        return app;
    }
}
