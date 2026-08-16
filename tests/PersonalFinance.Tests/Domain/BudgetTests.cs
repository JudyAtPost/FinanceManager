using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Domain;

/// <summary>
/// Verifies the invariants of the Budget entity.
/// </summary>
[TestClass]
public sealed class BudgetTests
{
    private static readonly Guid ValidCategoryId = Guid.CreateVersion7();
    private static readonly BudgetMonth ValidMonth = new(2025, 3);

    [TestMethod]
    public void Create_WithValidValues_Succeeds()
    {
        Budget budget = Budget.Create(ValidCategoryId, ValidMonth, 300m);

        Assert.AreEqual(ValidCategoryId, budget.CategoryId);
        Assert.AreEqual(ValidMonth, budget.Month);
        Assert.AreEqual(300m, budget.Limit);
        Assert.AreNotEqual(Guid.Empty, budget.Id);
    }

    [TestMethod]
    public void Create_WithAnEmptyCategoryId_ThrowsDomainValidationException()
    {
        Assert.ThrowsExactly<DomainValidationException>(
            () => Budget.Create(Guid.Empty, ValidMonth, 300m));
    }

    [TestMethod]
    public void Create_WithZeroLimit_ThrowsDomainValidationException()
    {
        Assert.ThrowsExactly<DomainValidationException>(
            () => Budget.Create(ValidCategoryId, ValidMonth, 0m));
    }

    [TestMethod]
    public void Create_WithNegativeOneLimit_ThrowsDomainValidationException()
    {
        Assert.ThrowsExactly<DomainValidationException>(
            () => Budget.Create(ValidCategoryId, ValidMonth, -1m));
    }

    [TestMethod]
    public void Create_WithANegativeDecimalLimit_ThrowsDomainValidationException()
    {
        decimal limit = -100.50m;

        Assert.ThrowsExactly<DomainValidationException>(
            () => Budget.Create(ValidCategoryId, ValidMonth, limit));
    }

    [TestMethod]
    public void Create_RoundsLimitToTwoDecimals()
    {
        Budget budget = Budget.Create(ValidCategoryId, ValidMonth, 300.555m);

        Assert.AreEqual(300.56m, budget.Limit);
    }

    [TestMethod]
    public void ChangeLimit_UpdatesTheLimit()
    {
        Budget budget = Budget.Create(ValidCategoryId, ValidMonth, 300m);

        budget.ChangeLimit(400m);

        Assert.AreEqual(400m, budget.Limit);
    }

    [TestMethod]
    public void ChangeLimit_WithANonPositiveLimit_ThrowsDomainValidationException()
    {
        Budget budget = Budget.Create(ValidCategoryId, ValidMonth, 300m);

        Assert.ThrowsExactly<DomainValidationException>(
            () => budget.ChangeLimit(0m));
    }
}

/// <summary>
/// Verifies the invariants of the BudgetMonth value object.
/// </summary>
[TestClass]
public sealed class BudgetMonthTests
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(2025)]
    [DataRow(9999)]
    public void Constructor_WithAValidYear_Succeeds(int year)
    {
        BudgetMonth month = new(year, 6);

        Assert.AreEqual(year, month.Year);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(10000)]
    public void Constructor_WithAnInvalidYear_ThrowsDomainValidationException(int year)
    {
        Assert.ThrowsExactly<DomainValidationException>(() => new BudgetMonth(year, 6));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(6)]
    [DataRow(12)]
    public void Constructor_WithAValidMonth_Succeeds(int month)
    {
        BudgetMonth budgetMonth = new(2025, month);

        Assert.AreEqual(month, budgetMonth.Month);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(13)]
    [DataRow(100)]
    public void Constructor_WithAnInvalidMonth_ThrowsDomainValidationException(int month)
    {
        Assert.ThrowsExactly<DomainValidationException>(() => new BudgetMonth(2025, month));
    }

    [TestMethod]
    public void FromDate_CreatesMonthFromDate()
    {
        BudgetMonth month = BudgetMonth.FromDate(new DateOnly(2025, 3, 14));

        Assert.AreEqual(2025, month.Year);
        Assert.AreEqual(3, month.Month);
    }

    [TestMethod]
    [DataRow(2025, 3, 14, true)]
    [DataRow(2025, 3, 1, true)]
    [DataRow(2025, 3, 31, true)]
    [DataRow(2025, 2, 1, false)]
    [DataRow(2025, 4, 1, false)]
    [DataRow(2026, 3, 1, false)]
    public void Contains_ChecksIfDateBelongsToMonth(int year, int month, int day, bool expected)
    {
        BudgetMonth budgetMonth = new(2025, 3);
        DateOnly date = new(year, month, day);

        Assert.AreEqual(expected, budgetMonth.Contains(date));
    }

    [TestMethod]
    public void FirstDay_ReturnsTheFirstDayOfTheMonth()
    {
        BudgetMonth month = new(2025, 3);

        Assert.AreEqual(new DateOnly(2025, 3, 1), month.FirstDay);
    }

    [TestMethod]
    public void LastDay_ReturnsTheLastDayOfTheMonth()
    {
        BudgetMonth month = new(2025, 3);

        Assert.AreEqual(new DateOnly(2025, 3, 31), month.LastDay);
    }

    [TestMethod]
    public void LastDay_HandlesFebruary()
    {
        BudgetMonth nonLeapYear = new(2025, 2);
        BudgetMonth leapYear = new(2024, 2);

        Assert.AreEqual(new DateOnly(2025, 2, 28), nonLeapYear.LastDay);
        Assert.AreEqual(new DateOnly(2024, 2, 29), leapYear.LastDay);
    }

    [TestMethod]
    public void ToString_FormatsAsYyyyMm()
    {
        BudgetMonth month = new(2025, 3);

        Assert.AreEqual("2025-03", month.ToString());
    }

#pragma warning disable MSTEST0037
    [TestMethod]
    public void CompareTo_OrdersMonthsChronologically()
    {
        BudgetMonth march2025 = new(2025, 3);
        BudgetMonth april2025 = new(2025, 4);
        BudgetMonth march2026 = new(2026, 3);

        Assert.IsTrue(march2025.CompareTo(april2025) < 0);
        Assert.IsTrue(april2025.CompareTo(march2025) > 0);
        Assert.IsTrue(march2025.CompareTo(march2026) < 0);
        Assert.AreEqual(0, march2025.CompareTo(new BudgetMonth(2025, 3)));
    }
#pragma warning restore MSTEST0037
}
