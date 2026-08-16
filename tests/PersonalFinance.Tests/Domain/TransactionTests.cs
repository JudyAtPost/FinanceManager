using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Domain;

/// <summary>
/// Verifies the invariants of the Transaction entity.
/// </summary>
[TestClass]
public sealed class TransactionTests
{
    private static readonly Guid ValidCategoryId = Guid.CreateVersion7();

    [TestMethod]
    public void Create_WithValidValues_Succeeds()
    {
        Transaction transaction = Transaction.Create("Lunch at a cafe", 12.50m, new DateOnly(2025, 3, 14), ValidCategoryId).Value;

        Assert.AreEqual("Lunch at a cafe", transaction.Description);
        Assert.AreEqual(12.50m, transaction.Amount);
        Assert.AreEqual(new DateOnly(2025, 3, 14), transaction.Date);
        Assert.AreEqual(ValidCategoryId, transaction.CategoryId);
        Assert.AreNotEqual(Guid.Empty, transaction.Id);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_WithABlankDescription_ReturnsValidationError(string? description)
    {
        Result<Transaction> result = Transaction.Create(description ?? "", 10m, new DateOnly(2025, 3, 1), ValidCategoryId);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_WithADescriptionExceedingMaxLength_ReturnsValidationError()
    {
        string tooLong = new('x', Transaction.MaxDescriptionLength + 1);

        Result<Transaction> result = Transaction.Create(tooLong, 10m, new DateOnly(2025, 3, 1), ValidCategoryId);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_TrimmsWhitespace()
    {
        Transaction transaction = Transaction.Create("  Lunch  ", 10m, new DateOnly(2025, 3, 1), ValidCategoryId).Value;

        Assert.AreEqual("Lunch", transaction.Description);
    }

    [TestMethod]
    public void Create_WithZeroAmount_ReturnsValidationError()
    {
        Result<Transaction> result = Transaction.Create("Test", 0m, new DateOnly(2025, 3, 1), ValidCategoryId);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_WithNegativeOneAmount_ReturnsValidationError()
    {
        Result<Transaction> result = Transaction.Create("Test", -1m, new DateOnly(2025, 3, 1), ValidCategoryId);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_WithANegativeDecimalAmount_ReturnsValidationError()
    {
        decimal amount = -100.50m;

        Result<Transaction> result = Transaction.Create("Test", amount, new DateOnly(2025, 3, 1), ValidCategoryId);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_RoundsAmountToTwoDecimals()
    {
        Transaction transaction = Transaction.Create("Test", 10.555m, new DateOnly(2025, 3, 1), ValidCategoryId).Value;

        Assert.AreEqual(10.56m, transaction.Amount);
    }

    [TestMethod]
    public void Create_WithAnEmptyCategoryId_ReturnsValidationError()
    {
        Result<Transaction> result = Transaction.Create("Test", 10m, new DateOnly(2025, 3, 1), Guid.Empty);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Update_ChangesAllValues()
    {
        Transaction transaction = Transaction.Create("Lunch", 10m, new DateOnly(2025, 3, 1), ValidCategoryId).Value;
        Guid newCategoryId = Guid.CreateVersion7();

        transaction.Update("Dinner", 20m, new DateOnly(2025, 3, 2), newCategoryId);

        Assert.AreEqual("Dinner", transaction.Description);
        Assert.AreEqual(20m, transaction.Amount);
        Assert.AreEqual(new DateOnly(2025, 3, 2), transaction.Date);
        Assert.AreEqual(newCategoryId, transaction.CategoryId);
    }

    [TestMethod]
    public void Update_ClearsTheCategoryNavigationWhenIdChanges()
    {
        TransactionReflection.SetCategoryForTesting(out Transaction transaction, out _);
        Guid newCategoryId = Guid.CreateVersion7();

        transaction.Update("Test", 10m, new DateOnly(2025, 3, 1), newCategoryId);

        Assert.IsNull(transaction.Category);
    }
}

/// <summary>
/// Reflection helper to test navigation property behavior without affecting normal code paths.
/// </summary>
internal static class TransactionReflection
{
    public static void SetCategoryForTesting(out Transaction transaction, out Category category)
    {
        Guid categoryId = Guid.CreateVersion7();
        transaction = Transaction.Create("Test", 10m, new DateOnly(2025, 3, 1), categoryId).Value;
        category = Category.Create("Test", TransactionType.Expense).Value;

        var categoryProperty = typeof(Transaction).GetProperty(nameof(Transaction.Category));
        categoryProperty?.SetValue(transaction, category);
    }
}
