using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;
using PersonalFinance.Infrastructure.Persistence;
using PersonalFinance.Infrastructure.Persistence.Repositories;

namespace PersonalFinance.Tests.Repositories;

[TestClass]
public sealed class TransactionRepositoryTests
{
    private static FinanceDbContext CreateContext()
    {
        DbContextOptions<FinanceDbContext> options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    private static Transaction CreateTransaction(decimal amount, DateOnly date, Guid categoryId) =>
        Transaction.Create($"Transaction {Guid.NewGuid()}", amount, date, categoryId).Value;

    [TestMethod]
    public async Task ListAsync_WhenFilteredByMonth_ReturnsOnlyTransactionsWithinThatMonth()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        context.Categories.Add(groceries);

        Transaction inMarch = CreateTransaction(50m, new DateOnly(2025, 3, 15), groceries.Id);
        Transaction inApril = CreateTransaction(60m, new DateOnly(2025, 4, 1), groceries.Id);
        context.Transactions.AddRange(inMarch, inApril);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);
        var query = new TransactionQuery(Month: BudgetMonth.Create(2025, 3).Value);

        PagedResult<Transaction> result = await sut.ListAsync(query, CancellationToken.None);

        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(inMarch.Id, result.Items.Single().Id);
    }

    [TestMethod]
    public async Task ListAsync_WhenFilteredByCategory_ReturnsOnlyMatchingTransactions()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        Category salary = Category.Create("Salary", TransactionType.Income).Value;
        context.Categories.AddRange(groceries, salary);

        Transaction groceryTransaction = CreateTransaction(50m, new DateOnly(2025, 3, 15), groceries.Id);
        Transaction salaryTransaction = CreateTransaction(2000m, new DateOnly(2025, 3, 1), salary.Id);
        context.Transactions.AddRange(groceryTransaction, salaryTransaction);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);
        var query = new TransactionQuery(CategoryId: groceries.Id);

        PagedResult<Transaction> result = await sut.ListAsync(query, CancellationToken.None);

        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(groceryTransaction.Id, result.Items.Single().Id);
    }

    [TestMethod]
    public async Task ListAsync_WhenFilteredByType_ReturnsOnlyTransactionsOfThatType()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        Category salary = Category.Create("Salary", TransactionType.Income).Value;
        context.Categories.AddRange(groceries, salary);

        Transaction groceryTransaction = CreateTransaction(50m, new DateOnly(2025, 3, 15), groceries.Id);
        Transaction salaryTransaction = CreateTransaction(2000m, new DateOnly(2025, 3, 1), salary.Id);
        context.Transactions.AddRange(groceryTransaction, salaryTransaction);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);
        var query = new TransactionQuery(Type: TransactionType.Income);

        PagedResult<Transaction> result = await sut.ListAsync(query, CancellationToken.None);

        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(salaryTransaction.Id, result.Items.Single().Id);
    }

    [TestMethod]
    public async Task ListAsync_OrdersByDateThenIdDescending_AndPagesResults()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        context.Categories.Add(groceries);

        Transaction oldest = CreateTransaction(10m, new DateOnly(2025, 1, 1), groceries.Id);
        Transaction middle = CreateTransaction(20m, new DateOnly(2025, 2, 1), groceries.Id);
        Transaction newest = CreateTransaction(30m, new DateOnly(2025, 3, 1), groceries.Id);
        context.Transactions.AddRange(oldest, middle, newest);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);
        var query = new TransactionQuery(Page: 1, PageSize: 2);

        PagedResult<Transaction> result = await sut.ListAsync(query, CancellationToken.None);

        Assert.AreEqual(3, result.TotalCount);
        Assert.HasCount(2, result.Items);
        Assert.AreEqual(newest.Id, result.Items[0].Id);
        Assert.AreEqual(middle.Id, result.Items[1].Id);
    }

    [TestMethod]
    public async Task GetAsync_WhenTransactionExists_ReturnsItWithCategoryLoaded()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        context.Categories.Add(groceries);
        Transaction transaction = CreateTransaction(50m, new DateOnly(2025, 3, 15), groceries.Id);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);

        Transaction? found = await sut.GetAsync(transaction.Id, CancellationToken.None);

        Assert.IsNotNull(found);
        Assert.IsNotNull(found!.Category);
        Assert.AreEqual(groceries.Id, found.Category!.Id);
    }

    [TestMethod]
    public async Task GetAsync_WhenTransactionDoesNotExist_ReturnsNull()
    {
        await using FinanceDbContext context = CreateContext();
        var sut = new TransactionRepository(context);

        Transaction? found = await sut.GetAsync(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task GetMonthlyTotalsByCategoryAsync_GroupsAndSumsAmountsWithinTheMonth()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        Category salary = Category.Create("Salary", TransactionType.Income).Value;
        context.Categories.AddRange(groceries, salary);

        context.Transactions.AddRange(
            CreateTransaction(50m, new DateOnly(2025, 3, 5), groceries.Id),
            CreateTransaction(25m, new DateOnly(2025, 3, 20), groceries.Id),
            CreateTransaction(2000m, new DateOnly(2025, 3, 1), salary.Id),
            CreateTransaction(999m, new DateOnly(2025, 4, 1), groceries.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);

        IReadOnlyList<CategoryTotal> totals = await sut.GetMonthlyTotalsByCategoryAsync(
            BudgetMonth.Create(2025, 3).Value,
            CancellationToken.None);

        Assert.HasCount(2, totals);
        CategoryTotal groceriesTotal = totals.Single(total => total.CategoryId == groceries.Id);
        Assert.AreEqual(75m, groceriesTotal.Total);
        Assert.AreEqual(TransactionType.Expense, groceriesTotal.Type);
        CategoryTotal salaryTotal = totals.Single(total => total.CategoryId == salary.Id);
        Assert.AreEqual(2000m, salaryTotal.Total);
    }

    [TestMethod]
    public async Task Add_ThenSaveChanges_PersistsTheTransaction()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        context.Categories.Add(groceries);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);
        Transaction transaction = CreateTransaction(50m, new DateOnly(2025, 3, 15), groceries.Id);

        sut.Add(transaction);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.IsTrue(await context.Transactions.AnyAsync(t => t.Id == transaction.Id));
    }

    [TestMethod]
    public async Task Remove_ThenSaveChanges_DeletesTheTransaction()
    {
        await using FinanceDbContext context = CreateContext();
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        context.Categories.Add(groceries);
        Transaction transaction = CreateTransaction(50m, new DateOnly(2025, 3, 15), groceries.Id);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(CancellationToken.None);

        var sut = new TransactionRepository(context);

        sut.Remove(transaction);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.IsFalse(await context.Transactions.AnyAsync(t => t.Id == transaction.Id));
    }
}
