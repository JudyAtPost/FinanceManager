export type TransactionType = 'Income' | 'Expense';

export interface Category {
  id: string;
  name: string;
  type: TransactionType;
}

export interface SaveCategoryRequest {
  name: string;
  type: TransactionType;
}

export interface Transaction {
  id: string;
  description: string;
  amount: number;
  date: string;
  categoryId: string;
  categoryName: string;
  type: TransactionType;
}

export interface SaveTransactionRequest {
  description: string;
  amount: number;
  date: string;
  categoryId: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Budget {
  id: string;
  categoryId: string;
  categoryName: string;
  year: number;
  month: number;
  limit: number;
}

export interface CreateBudgetRequest {
  categoryId: string;
  year: number;
  month: number;
  limit: number;
}

export interface BudgetComparison {
  categoryId: string;
  categoryName: string;
  limit: number;
  spent: number;
  remaining: number;
  overspentBy: number;
  isOverBudget: boolean;
  usagePercentage: number;
}

export interface CategoryBreakdownItem {
  categoryId: string;
  categoryName: string;
  type: TransactionType;
  total: number;
  shareOfTypeTotal: number;
}

export interface MonthlySummary {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  breakdown: CategoryBreakdownItem[];
  budgets: BudgetComparison[];
  topExpenseCategory: CategoryBreakdownItem | null;
}

export interface TransactionFilter {
  year?: number;
  month?: number;
  categoryId?: string;
  type?: TransactionType;
  page?: number;
  pageSize?: number;
}
