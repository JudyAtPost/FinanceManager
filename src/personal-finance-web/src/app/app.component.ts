import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Category, MonthlySummary } from './api.models';
import { BudgetsComponent } from './components/budgets.component';
import { MonthlySummaryComponent } from './components/monthly-summary.component';
import { TransactionsComponent } from './components/transactions.component';
import { FinanceApiService } from './finance-api.service';

@Component({
  selector: 'pf-root',
  standalone: true,
  imports: [CommonModule, FormsModule, MonthlySummaryComponent, TransactionsComponent, BudgetsComponent],
  template: `
    <main class="container">
      <h1>Personal Finance</h1>
      <p class="subtitle">Track income and expenses, set monthly budgets, and see where the money went.</p>

      <div class="card toolbar">
        <div class="field">
          <label class="label" for="month">Month</label>
          <input id="month" type="month" [ngModel]="monthValue" (ngModelChange)="onMonthChange($event)" />
        </div>
        <button type="button" (click)="refresh()">Refresh</button>
      </div>

      <p class="error" *ngIf="error">{{ error }}</p>

      <pf-monthly-summary [summary]="summary" [previousSummary]="previousSummary"></pf-monthly-summary>

      <pf-transactions
        #transactions
        [year]="year"
        [month]="month"
        [categories]="categories"
        (changed)="loadSummary()"
      ></pf-transactions>

      <pf-budgets
        #budgets
        [year]="year"
        [month]="month"
        [categories]="categories"
        (changed)="loadSummary()"
      ></pf-budgets>
    </main>
  `
})
export class AppComponent implements OnInit {
  private readonly api = inject(FinanceApiService);

  @ViewChild('transactions') private transactions?: TransactionsComponent;
  @ViewChild('budgets') private budgets?: BudgetsComponent;

  private readonly today = new Date();

  year = this.today.getFullYear();
  month = this.today.getMonth() + 1;

  categories: Category[] = [];
  summary: MonthlySummary | null = null;
  previousSummary: MonthlySummary | null = null;
  error = '';

  get monthValue(): string {
    return `${this.year}-${String(this.month).padStart(2, '0')}`;
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadSummary();
  }

  onMonthChange(value: string): void {
    if (!value) {
      return;
    }

    const [year, month] = value.split('-').map(Number);
    this.year = year;
    this.month = month;
    this.refresh();
  }

  refresh(): void {
    this.loadSummary();
    this.transactions?.reload();
    this.budgets?.reload();
  }

  loadSummary(): void {
    this.api.getSummary(this.year, this.month).subscribe({
      next: (summary) => {
        this.summary = summary;
        this.error = '';
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not load the monthly summary.')
    });

    const { year: previousYear, month: previousMonth } = this.previousMonth();
    this.api.getSummary(previousYear, previousMonth).subscribe({
      next: (summary) => (this.previousSummary = summary),
      error: () => (this.previousSummary = null)
    });
  }

  private previousMonth(): { year: number; month: number } {
    return this.month === 1 ? { year: this.year - 1, month: 12 } : { year: this.year, month: this.month - 1 };
  }

  private loadCategories(): void {
    this.api.listCategories().subscribe({
      next: (categories) => (this.categories = categories),
      error: (err) => (this.error = err?.error?.detail ?? 'Could not load categories.')
    });
  }
}
