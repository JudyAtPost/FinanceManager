import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Category, PagedResult, Transaction, TransactionType } from '../../api.models';
import { categoryColor } from '../../category-color';
import { FinanceApiService } from '../../finance-api.service';

@Component({
  selector: 'pf-transactions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transactions.component.html'
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

  dotColor(categoryName: string): string {
    return categoryColor(categoryName);
  }

  initials(categoryName: string): string {
    return categoryName
      .split(' ')
      .filter((part) => part.length > 0)
      .slice(0, 2)
      .map((part) => part[0].toUpperCase())
      .join('');
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
