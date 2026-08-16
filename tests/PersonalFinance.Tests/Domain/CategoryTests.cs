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
        Category category = Category.Create("Groceries", TransactionType.Expense);

        Assert.AreEqual("Groceries", category.Name);
        Assert.AreEqual(TransactionType.Expense, category.Type);
        Assert.AreNotEqual(Guid.Empty, category.Id);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_WithABlankName_ThrowsDomainValidationException(string? name)
    {
        Assert.ThrowsExactly<DomainValidationException>(
            () => Category.Create(name ?? "", TransactionType.Expense));
    }

    [TestMethod]
    public void Create_WithANameExceedingMaxLength_ThrowsDomainValidationException()
    {
        string tooLong = new('x', Category.MaxNameLength + 1);

        Assert.ThrowsExactly<DomainValidationException>(
            () => Category.Create(tooLong, TransactionType.Expense));
    }

    [TestMethod]
    public void Create_TrimmsWhitespace()
    {
        Category category = Category.Create("  Groceries  ", TransactionType.Income);

        Assert.AreEqual("Groceries", category.Name);
    }

    [TestMethod]
    public void Create_WithAnUndefinedTransactionType_ThrowsDomainValidationException()
    {
        const TransactionType undefinedType = (TransactionType)999;

        Assert.ThrowsExactly<DomainValidationException>(
            () => Category.Create("Test", undefinedType));
    }

    [TestMethod]
    public void Update_ChangesTheNameAndType()
    {
        Category category = Category.Create("Groceries", TransactionType.Expense);

        category.Update("Food", TransactionType.Income);

        Assert.AreEqual("Food", category.Name);
        Assert.AreEqual(TransactionType.Income, category.Type);
    }

    [TestMethod]
    public void Update_WithABlankName_ThrowsDomainValidationException()
    {
        Category category = Category.Create("Groceries", TransactionType.Expense);

        Assert.ThrowsExactly<DomainValidationException>(
            () => category.Update("", TransactionType.Expense));
    }
}
