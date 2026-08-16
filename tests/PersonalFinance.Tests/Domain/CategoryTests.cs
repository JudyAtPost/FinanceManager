using PersonalFinance.Domain;

namespace PersonalFinance.Tests.Domain;

/// <summary>
/// Verifies the invariants of the Category entity.
/// </summary>
[TestClass]
public sealed class CategoryTests
{
    [TestMethod]
    public void Create_WithAValidName_Succeeds()
    {
        Category category = Category.Create("Groceries", TransactionType.Expense).Value;

        Assert.AreEqual("Groceries", category.Name);
        Assert.AreEqual(TransactionType.Expense, category.Type);
        Assert.AreNotEqual(Guid.Empty, category.Id);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_WithABlankName_ReturnsValidationError(string? name)
    {
        Result<Category> result = Category.Create(name ?? "", TransactionType.Expense);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_WithANameExceedingMaxLength_ReturnsValidationError()
    {
        string tooLong = new('x', Category.MaxNameLength + 1);

        Result<Category> result = Category.Create(tooLong, TransactionType.Expense);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Create_TrimmsWhitespace()
    {
        Category category = Category.Create("  Groceries  ", TransactionType.Income).Value;

        Assert.AreEqual("Groceries", category.Name);
    }

    [TestMethod]
    public void Create_WithAnUndefinedTransactionType_ReturnsValidationError()
    {
        const TransactionType undefinedType = (TransactionType)999;

        Result<Category> result = Category.Create("Test", undefinedType);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }

    [TestMethod]
    public void Update_ChangesTheNameAndType()
    {
        Category category = Category.Create("Groceries", TransactionType.Expense).Value;

        category.Update("Food", TransactionType.Income);

        Assert.AreEqual("Food", category.Name);
        Assert.AreEqual(TransactionType.Income, category.Type);
    }

    [TestMethod]
    public void Update_WithABlankName_ReturnsValidationError()
    {
        Category category = Category.Create("Groceries", TransactionType.Expense).Value;

        Result result = category.Update("", TransactionType.Expense);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error!.Type);
    }
}
