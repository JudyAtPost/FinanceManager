using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Application.Summaries;
using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Summaries;

[TestClass]
public sealed class MonthlySummaryServiceTests
{
    private static readonly BudgetMonth March2025 = new(2025, 3);

    private static readonly Guid SalaryId = Guid.CreateVersion7();
    private static readonly Guid RentId = Guid.CreateVersion7();
    private static readonly Guid GroceriesId = Guid.CreateVersion7();

    private static CategoryTotal[] SampleTotals =>
    [
        new(SalaryId, "Salary", TransactionType.Income, 2500m),
        new(RentId, "Rent", TransactionType.Expense, 900m),
        new(GroceriesId, "Groceries", TransactionType.Expense, 900m - 0.01m)
    ];

    [TestMethod]
    public void Build_SumsIncomeAndExpensesSeparately()
    {
        MonthlySummary summary = MonthlySummaryService.Build(March2025, SampleTotals, []);

        Assert.AreEqual(2500m, summary.TotalIncome);
        Assert.AreEqual(1799.99m, summary.TotalExpenses);
        Assert.AreEqual(700.01m, summary.Balance);
    }

    [TestMethod]
    public void Build_ReportsTheMonthItWasAskedFor()
    {
        MonthlySummary summary = MonthlySummaryService.Build(March2025, SampleTotals, []);

        Assert.AreEqual(2025, summary.Year);
        Assert.AreEqual(3, summary.Month);
    }

    [TestMethod]
    public void Build_PicksTheExpenseCategoryWithTheHighestTotal()
    {
        MonthlySummary summary = MonthlySummaryService.Build(March2025, SampleTotals, []);

        Assert.IsNotNull(summary.TopExpenseCategory);
        Assert.AreEqual(RentId, summary.TopExpenseCategory.CategoryId);
        Assert.AreEqual(900m, summary.TopExpenseCategory.Total);
    }

    [TestMethod]
    public void Build_NeverPicksAnIncomeCategoryAsTopSpending()
    {
        CategoryTotal[] totals =
        [
            new(SalaryId, "Salary", TransactionType.Income, 9999m),
            new(RentId, "Rent", TransactionType.Expense, 10m)
        ];

        MonthlySummary summary = MonthlySummaryService.Build(March2025, totals, []);

        Assert.IsNotNull(summary.TopExpenseCategory);
        Assert.AreEqual(RentId, summary.TopExpenseCategory.CategoryId);
    }

    [TestMethod]
    public void Build_WithoutExpenses_LeavesTheTopCategoryUnset()
    {
        CategoryTotal[] totals = [new(SalaryId, "Salary", TransactionType.Income, 2500m)];

        MonthlySummary summary = MonthlySummaryService.Build(March2025, totals, []);

        Assert.IsNull(summary.TopExpenseCategory);
        Assert.AreEqual(0m, summary.TotalExpenses);
    }

    [TestMethod]
    public void Build_WithoutAnyTransactions_ReturnsAnEmptySummary()
    {
        MonthlySummary summary = MonthlySummaryService.Build(March2025, [], []);

        Assert.AreEqual(0m, summary.TotalIncome);
        Assert.AreEqual(0m, summary.TotalExpenses);
        Assert.AreEqual(0m, summary.Balance);
        Assert.IsEmpty(summary.Breakdown);
        Assert.IsEmpty(summary.Budgets);
        Assert.IsNull(summary.TopExpenseCategory);
    }

    [TestMethod]
    public void Build_ExpressesEachCategoryAsAShareOfItsOwnTotal()
    {
        MonthlySummary summary = MonthlySummaryService.Build(March2025, SampleTotals, []);

        CategoryBreakdownItem salary = summary.Breakdown.Single(item => item.CategoryId == SalaryId);
        CategoryBreakdownItem rent = summary.Breakdown.Single(item => item.CategoryId == RentId);

        Assert.AreEqual(100m, salary.ShareOfTypeTotal);
        Assert.AreEqual(50m, rent.ShareOfTypeTotal);
    }

    [TestMethod]
    public void Build_OrdersBreakdownByTypeThenDescendingTotal()
    {
        MonthlySummary summary = MonthlySummaryService.Build(March2025, SampleTotals, []);

        CollectionAssert.AreEqual(
            new[] { SalaryId, RentId, GroceriesId },
            summary.Breakdown.Select(item => item.CategoryId).ToArray());
    }

    [TestMethod]
    public void Build_IncludesTheBudgetComparisonOfTheMonth()
    {
        Budget rentBudget = Budget.Create(RentId, March2025, 800m);

        MonthlySummary summary = MonthlySummaryService.Build(March2025, SampleTotals, [rentBudget]);

        BudgetComparison comparison = summary.Budgets.Single();
        Assert.AreEqual(RentId, comparison.CategoryId);
        Assert.AreEqual("Rent", comparison.CategoryName);
        Assert.IsTrue(comparison.IsOverBudget);
        Assert.AreEqual(100m, comparison.OverspentBy);
    }
}
