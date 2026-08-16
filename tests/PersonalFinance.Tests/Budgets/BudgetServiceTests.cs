using AutoFixture;
using FakeItEasy;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Budgets;

[TestClass]
public sealed class BudgetServiceTests
{
    private static readonly BudgetMonth March2025 = BudgetMonth.Create(2025, 3).Value;

    private readonly Fixture _fixture = new();
    private readonly IBudgetRepository _budgets = A.Fake<IBudgetRepository>();
    private readonly ICategoryRepository _categories = A.Fake<ICategoryRepository>();
    private readonly ITransactionRepository _transactions = A.Fake<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    private BudgetService CreateSut() => new(_budgets, _categories, _transactions, _unitOfWork);

    [TestMethod]
    public async Task CreateAsync_WhenCategoryIsUnknown_ReturnsNotFound()
    {
        A.CallTo(() => _categories.GetAsync(A<Guid>._, A<CancellationToken>._)).Returns<Category?>(null);

        var request = new CreateBudgetRequest(Guid.CreateVersion7(), 2025, 3, 300m);

        Result<BudgetDto> result = await CreateSut().CreateAsync(request, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.NotFound, result.Error!.Type);
    }

    [TestMethod]
    public async Task CreateAsync_ForAnIncomeCategory_ReturnsConflict()
    {
        Category salary = Category.Create("Salary", TransactionType.Income).Value;
        A.CallTo(() => _categories.GetAsync(salary.Id, A<CancellationToken>._)).Returns(salary);

        var request = new CreateBudgetRequest(salary.Id, 2025, 3, 300m);

        Result<BudgetDto> result = await CreateSut().CreateAsync(request, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Conflict, result.Error!.Type);
    }

    [TestMethod]
    public async Task CreateAsync_WhenABudgetAlreadyExistsForTheMonth_ReturnsConflict()
    {
        Category rent = Category.Create("Rent", TransactionType.Expense).Value;
        A.CallTo(() => _categories.GetAsync(rent.Id, A<CancellationToken>._)).Returns(rent);
        A.CallTo(() => _budgets.GetForCategoryAndMonthAsync(rent.Id, March2025, A<CancellationToken>._))
            .Returns(Budget.Create(rent.Id, March2025, 900m).Value);

        var request = new CreateBudgetRequest(rent.Id, 2025, 3, 300m);

        Result<BudgetDto> result = await CreateSut().CreateAsync(request, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Conflict, result.Error!.Type);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [TestMethod]
    public async Task CreateAsync_ForANewExpenseCategoryBudget_PersistsAndReturnsIt()
    {
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        A.CallTo(() => _categories.GetAsync(groceries.Id, A<CancellationToken>._)).Returns(groceries);
        A.CallTo(() => _budgets.GetForCategoryAndMonthAsync(groceries.Id, March2025, A<CancellationToken>._))
            .Returns<Budget?>(null);

        var request = new CreateBudgetRequest(groceries.Id, 2025, 3, 300m);

        BudgetDto created = (await CreateSut().CreateAsync(request, CancellationToken.None)).Value;

        Assert.AreEqual(groceries.Id, created.CategoryId);
        Assert.AreEqual("Groceries", created.CategoryName);
        Assert.AreEqual(2025, created.Year);
        Assert.AreEqual(3, created.Month);
        Assert.AreEqual(300m, created.Limit);

        A.CallTo(() => _budgets.Add(A<Budget>.That.Matches(budget => budget.CategoryId == groceries.Id)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task CreateAsync_WithANonPositiveLimit_IsRejectedByTheDomain()
    {
        Category rent = Category.Create("Rent", TransactionType.Expense).Value;
        A.CallTo(() => _categories.GetAsync(rent.Id, A<CancellationToken>._)).Returns(rent);
        A.CallTo(() => _budgets.GetForCategoryAndMonthAsync(rent.Id, March2025, A<CancellationToken>._))
            .Returns<Budget?>(null);

        var request = new CreateBudgetRequest(rent.Id, 2025, 3, 0m);

        Result<BudgetDto> result = await CreateSut().CreateAsync(request, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenTheBudgetIsUnknown_ReturnsNotFound()
    {
        A.CallTo(() => _budgets.GetAsync(A<Guid>._, A<CancellationToken>._)).Returns<Budget?>(null);

        Result<BudgetDto> result = await CreateSut()
            .UpdateAsync(Guid.CreateVersion7(), new UpdateBudgetRequest(100m), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.NotFound, result.Error!.Type);
    }

    [TestMethod]
    public async Task UpdateAsync_ChangesTheLimitAndSaves()
    {
        Budget budget = Budget.Create(Guid.CreateVersion7(), March2025, 300m).Value;
        A.CallTo(() => _budgets.GetAsync(budget.Id, A<CancellationToken>._)).Returns(budget);

        BudgetDto updated = (await CreateSut()
            .UpdateAsync(budget.Id, new UpdateBudgetRequest(450m), CancellationToken.None)).Value;

        Assert.AreEqual(450m, updated.Limit);
        Assert.AreEqual(450m, budget.Limit);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task CompareAsync_WithoutBudgets_SkipsTheTransactionAggregation()
    {
        A.CallTo(() => _budgets.ListForMonthAsync(March2025, A<CancellationToken>._)).Returns(Array.Empty<Budget>());

        IReadOnlyList<BudgetComparison> comparisons = await CreateSut()
            .CompareAsync(March2025, CancellationToken.None);

        Assert.IsEmpty(comparisons);
        A.CallTo(() => _transactions.GetMonthlyTotalsByCategoryAsync(A<BudgetMonth>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task CompareAsync_CombinesBudgetsWithTheActualSpendingOfTheMonth()
    {
        Guid categoryId = Guid.CreateVersion7();
        Budget budget = Budget.Create(categoryId, March2025, 300m).Value;

        A.CallTo(() => _budgets.ListForMonthAsync(March2025, A<CancellationToken>._)).Returns(new[] { budget });
        A.CallTo(() => _transactions.GetMonthlyTotalsByCategoryAsync(March2025, A<CancellationToken>._))
            .Returns(new[] { new CategoryTotal(categoryId, "Groceries", TransactionType.Expense, 310m) });

        BudgetComparison comparison = (await CreateSut().CompareAsync(March2025, CancellationToken.None)).Single();

        Assert.AreEqual("Groceries", comparison.CategoryName);
        Assert.AreEqual(10m, comparison.OverspentBy);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenTheBudgetIsUnknown_ReturnsNotFound()
    {
        A.CallTo(() => _budgets.GetAsync(A<Guid>._, A<CancellationToken>._)).Returns<Budget?>(null);

        Result result = await CreateSut().DeleteAsync(_fixture.Create<Guid>(), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.NotFound, result.Error!.Type);
    }
}
