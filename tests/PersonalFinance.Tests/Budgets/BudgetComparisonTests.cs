using AutoFixture;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Budgets;

[TestClass]
public sealed class BudgetComparisonTests
{
    private readonly Fixture _fixture = new();

    [TestMethod]
    public void Compare_WhenSpendingExceedsLimit_FlagsOverrunWithTheDifference()
    {
        Guid categoryId = Guid.CreateVersion7();
        Budget budget = Budget.Create(categoryId, BudgetMonth.Create(2025, 3).Value, 300m).Value;
        CategoryTotal total = new(categoryId, "Rent", TransactionType.Expense, 310m);

        BudgetComparison comparison = BudgetService.Compare([budget], [total]).Single();

        Assert.IsTrue(comparison.IsOverBudget);
        Assert.AreEqual(10m, comparison.OverspentBy);
        Assert.AreEqual(0m, comparison.Remaining);
        Assert.AreEqual(300m, comparison.Limit);
        Assert.AreEqual(310m, comparison.Spent);
    }

    [TestMethod]
    public void Compare_WhenSpendingStaysBelowLimit_ReportsRemainingBudget()
    {
        Guid categoryId = Guid.CreateVersion7();
        Budget budget = Budget.Create(categoryId, BudgetMonth.Create(2025, 3).Value, 400m).Value;
        CategoryTotal total = new(categoryId, "Groceries", TransactionType.Expense, 250m);

        BudgetComparison comparison = BudgetService.Compare([budget], [total]).Single();

        Assert.IsFalse(comparison.IsOverBudget);
        Assert.AreEqual(0m, comparison.OverspentBy);
        Assert.AreEqual(150m, comparison.Remaining);
        Assert.AreEqual(62.5m, comparison.UsagePercentage);
    }

    [TestMethod]
    public void Compare_WhenSpendingEqualsLimit_IsNotConsideredOverBudget()
    {
        Guid categoryId = Guid.CreateVersion7();
        Budget budget = Budget.Create(categoryId, BudgetMonth.Create(2025, 3).Value, 200m).Value;
        CategoryTotal total = new(categoryId, "Leisure", TransactionType.Expense, 200m);

        BudgetComparison comparison = BudgetService.Compare([budget], [total]).Single();

        Assert.IsFalse(comparison.IsOverBudget);
        Assert.AreEqual(0m, comparison.OverspentBy);
        Assert.AreEqual(0m, comparison.Remaining);
        Assert.AreEqual(100m, comparison.UsagePercentage);
    }

    [TestMethod]
    public void Compare_WhenCategoryHasNoTransactions_ReportsZeroSpending()
    {
        Budget budget = Budget.Create(Guid.CreateVersion7(), BudgetMonth.Create(2025, 3).Value, _fixture.Create<decimal>() + 1m).Value;

        BudgetComparison comparison = BudgetService.Compare([budget], []).Single();

        Assert.AreEqual(0m, comparison.Spent);
        Assert.IsFalse(comparison.IsOverBudget);
        Assert.AreEqual(budget.Limit, comparison.Remaining);
    }

    [TestMethod]
    public void Compare_IgnoresIncomeTotals()
    {
        Guid categoryId = Guid.CreateVersion7();
        Budget budget = Budget.Create(categoryId, BudgetMonth.Create(2025, 3).Value, 100m).Value;
        CategoryTotal income = new(categoryId, "Salary", TransactionType.Income, 2500m);

        BudgetComparison comparison = BudgetService.Compare([budget], [income]).Single();

        Assert.AreEqual(0m, comparison.Spent);
        Assert.IsFalse(comparison.IsOverBudget);
    }

    [TestMethod]
    public void Compare_OrdersOverspentCategoriesFirst()
    {
        Guid withinBudget = Guid.CreateVersion7();
        Guid slightlyOver = Guid.CreateVersion7();
        Guid heavilyOver = Guid.CreateVersion7();

        var month = BudgetMonth.Create(2025, 3).Value;
        Budget[] budgets =
        [
            Budget.Create(withinBudget, month, 500m).Value,
            Budget.Create(slightlyOver, month, 100m).Value,
            Budget.Create(heavilyOver, month, 100m).Value
        ];

        CategoryTotal[] totals =
        [
            new(withinBudget, "Groceries", TransactionType.Expense, 120m),
            new(slightlyOver, "Leisure", TransactionType.Expense, 110m),
            new(heavilyOver, "Rent", TransactionType.Expense, 900m)
        ];

        IReadOnlyList<BudgetComparison> comparisons = BudgetService.Compare(budgets, totals);

        CollectionAssert.AreEqual(
            new[] { heavilyOver, slightlyOver, withinBudget },
            comparisons.Select(comparison => comparison.CategoryId).ToArray());
    }

    [TestMethod]
    public void Compare_SumsMultipleTotalsOfTheSameCategory()
    {
        Guid categoryId = Guid.CreateVersion7();
        Budget budget = Budget.Create(categoryId, BudgetMonth.Create(2025, 3).Value, 100m).Value;

        CategoryTotal[] totals =
        [
            new(categoryId, "Groceries", TransactionType.Expense, 60m),
            new(categoryId, "Groceries", TransactionType.Expense, 55m)
        ];

        BudgetComparison comparison = BudgetService.Compare([budget], totals).Single();

        Assert.AreEqual(115m, comparison.Spent);
        Assert.AreEqual(15m, comparison.OverspentBy);
    }
}
