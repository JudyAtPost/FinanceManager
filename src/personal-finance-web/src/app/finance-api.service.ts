import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { resolveApiBaseUrl } from './api-base-url';
import {
  Budget,
  Category,
  CreateBudgetRequest,
  MonthlySummary,
  PagedResult,
  SaveCategoryRequest,
  SaveTransactionRequest,
  Transaction,
  TransactionFilter
} from './api.models';

@Injectable({ providedIn: 'root' })
export class FinanceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = resolveApiBaseUrl();

  listCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/api/categories`);
  }

  createCategory(request: SaveCategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/api/categories`, request);
  }

  updateCategory(id: string, request: SaveCategoryRequest): Observable<Category> {
    return this.http.put<Category>(`${this.baseUrl}/api/categories/${id}`, request);
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/categories/${id}`);
  }

  listTransactions(filter: TransactionFilter): Observable<PagedResult<Transaction>> {
    let params = new HttpParams();
    if (filter.year !== undefined && filter.month !== undefined) {
      params = params.set('year', filter.year).set('month', filter.month);
    }
    if (filter.categoryId) {
      params = params.set('categoryId', filter.categoryId);
    }
    if (filter.type) {
      params = params.set('type', filter.type);
    }
    params = params.set('page', filter.page ?? 1).set('pageSize', filter.pageSize ?? 20);

    return this.http.get<PagedResult<Transaction>>(`${this.baseUrl}/api/transactions`, { params });
  }

  createTransaction(request: SaveTransactionRequest): Observable<Transaction> {
    return this.http.post<Transaction>(`${this.baseUrl}/api/transactions`, request);
  }

  deleteTransaction(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/transactions/${id}`);
  }

  listBudgets(year: number, month: number): Observable<Budget[]> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<Budget[]>(`${this.baseUrl}/api/budgets`, { params });
  }

  createBudget(request: CreateBudgetRequest): Observable<Budget> {
    return this.http.post<Budget>(`${this.baseUrl}/api/budgets`, request);
  }

  deleteBudget(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/budgets/${id}`);
  }

  getSummary(year: number, month: number): Observable<MonthlySummary> {
    return this.http.get<MonthlySummary>(`${this.baseUrl}/api/summary/${year}/${month}`);
  }
}
