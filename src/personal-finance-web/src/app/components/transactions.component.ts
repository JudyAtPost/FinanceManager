import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Category, PagedResult, Transaction, TransactionType } from '../api.models';
import { FinanceApiService } from '../finance-api.service';

@Component({
  selector: 'pf-transactions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="card">
      <h2>Transactions</h2>

      <div class="toolbar">
        <div class="field">
          <label class="label" for="filter-category">Category</label>
          <select id="filter-category" [(ngModel)]="filterCategoryId" (change)="reload()">
            <option value="">All categories</option>
            <option *ngFor="let category of categories" [value]="category.id">{{ category.name }}</option>
          </select>
        </div>
        <div class="field">
          <label class="label" for="filter-type">Type</label>
          <select id="filter-type" [(ngModel)]="filterType" (change)="reload()">
            <option value="">Income and expenses</option>
            <option value="Income">Income</option>
            <option value="Expense">Expense</option>
          </select>
        </div>
      </div>

      <form class="toolbar" (ngSubmit)="add()">
        <div class="field">
          <label class="label" for="new-description">Description</label>
          <input id="new-description" name="description" [(ngModel)]="description" required />
        </div>
        <div class="field">
          <label class="label" for="new-amount">Amount</label>
          <input id="new-amount" name="amount" type="number" step="0.01" min="0.01" [(ngModel)]="amount" required />
        </div>
        <div class="field">
          <label class="label" for="new-date">Date</label>
          <input id="new-date" name="date" type="date" [(ngModel)]="date" required />
        </div>
        <div class="field">
          <label class="label" for="new-category">Category</label>
          <select id="new-category" name="categoryId" [(ngModel)]="categoryId" required>
            <option *ngFor="let category of categories" [value]="category.id">
              {{ category.name }} ({{ category.type }})
            </option>
          </select>
        </div>
        <button type="submit" [disabled]="!canAdd()">Add transaction</button>
      </form>

      <p class="error" *ngIf="error">{{ error }}</p>

      <table *ngIf="page && page.items.length; else empty">
        <thead>
          <tr>
            <th>Date</th>
            <th>Description</th>
            <th>Category</th>
            <th>Type</th>
            <th class="amount">Amount</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let transaction of page.items">
            <td>{{ transaction.date }}</td>
            <td>{{ transaction.description }}</td>
            <td>{{ transaction.categoryName }}</td>
            <td><span class="badge">{{ transaction.type }}</span></td>
            <td class="amount">{{ transaction.amount | currency: 'EUR' }}</td>
            <td><button type="button" class="ghost" (click)="remove(transaction)">Delete</button></td>
          </tr>
        </tbody>
      </table>
      <ng-template #empty><p class="muted">No transactions for this filter.</p></ng-template>

      <div class="toolbar" *ngIf="page && page.totalPages > 1">
        <button type="button" (click)="goTo(page.page - 1)" [disabled]="page.page <= 1">Previous</button>
        <span class="muted">Page {{ page.page }} of {{ page.totalPages }} ({{ page.totalCount }} items)</span>
        <button type="button" (click)="goTo(page.page + 1)" [disabled]="page.page >= page.totalPages">Next</button>
      </div>
    </section>
  `
})
export class TransactionsComponent implements OnChanges {
  private readonly api = inject(FinanceApiService);

  @Input({ required: true }) year!: number;
  @Input({ required: true }) month!: number;
  @Input() categories: Category[] = [];
  @Output() readonly changed = new EventEmitter<void>();

  page: PagedResult<Transaction> | null = null;
  error = '';

  filterCategoryId = '';
  filterType: '' | TransactionType = '';

  description = '';
  amount: number | null = null;
  date = new Date().toISOString().slice(0, 10);
  categoryId = '';

  private currentPage = 1;

  ngOnChanges(): void {
    this.currentPage = 1;
    this.reload();
  }

  canAdd(): boolean {
    return this.description.trim().length > 0 && (this.amount ?? 0) > 0 && !!this.categoryId;
  }

  goTo(page: number): void {
    this.currentPage = page;
    this.reload();
  }

  reload(): void {
    this.api
      .listTransactions({
        year: this.year,
        month: this.month,
        categoryId: this.filterCategoryId || undefined,
        type: this.filterType || undefined,
        page: this.currentPage,
        pageSize: 20
      })
      .subscribe({
        next: (page) => {
          this.page = page;
          this.error = '';
        },
        error: (err) => (this.error = err?.error?.detail ?? 'Could not load transactions.')
      });
  }

  add(): void {
    if (!this.canAdd()) {
      return;
    }

    this.api
      .createTransaction({
        description: this.description.trim(),
        amount: this.amount ?? 0,
        date: this.date,
        categoryId: this.categoryId
      })
      .subscribe({
        next: () => {
          this.description = '';
          this.amount = null;
          this.error = '';
          this.reload();
          this.changed.emit();
        },
        error: (err) => (this.error = err?.error?.detail ?? 'Could not save the transaction.')
      });
  }

  remove(transaction: Transaction): void {
    this.api.deleteTransaction(transaction.id).subscribe({
      next: () => {
        this.reload();
        this.changed.emit();
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not delete the transaction.')
    });
  }
}
