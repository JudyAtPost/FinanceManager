using FakeItEasy;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Summaries;
using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Summaries;

[TestClass]
public sealed class MonthlySummaryOrchestrationTests
{
    private static readonly BudgetMonth March2025 = BudgetMonth.Create(2025, 3).Value;

    private readonly ITransactionRepository _transactions = A.Fake<ITransactionRepository>();
    private readonly IBudgetRepository _budgets = A.Fake<IBudgetRepository>();

    [TestMethod]
    public async Task GetAsync_QueriesTotalsAndBudgetsForTheRequestedMonth()
    {
        Guid rentId = Guid.CreateVersion7();

        A.CallTo(() => _transactions.GetMonthlyTotalsByCategoryAsync(March2025, A<CancellationToken>._))
            .Returns(new[]
            {
                new CategoryTotal(Guid.CreateVersion7(), "Salary", TransactionType.Income, 2500m),
                new CategoryTotal(rentId, "Rent", TransactionType.Expense, 900m)
            });

        A.CallTo(() => _budgets.ListForMonthAsync(March2025, A<CancellationToken>._))
            .Returns(new[] { Budget.Create(rentId, March2025, 800m).Value });

        var service = new MonthlySummaryService(_transactions, _budgets);

        MonthlySummary summary = await service.GetAsync(March2025, CancellationToken.None);

        Assert.AreEqual(2500m, summary.TotalIncome);
        Assert.AreEqual(900m, summary.TotalExpenses);
        Assert.AreEqual(1600m, summary.Balance);
        Assert.IsNotNull(summary.TopExpenseCategory);
        Assert.AreEqual("Rent", summary.TopExpenseCategory.CategoryName);
        Assert.IsTrue(summary.Budgets.Single().IsOverBudget);

        A.CallTo(() => _transactions.GetMonthlyTotalsByCategoryAsync(March2025, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _budgets.ListForMonthAsync(March2025, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
