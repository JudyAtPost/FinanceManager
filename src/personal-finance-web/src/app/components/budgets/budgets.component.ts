import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Budget, Category } from '../../api.models';
import { FinanceApiService } from '../../finance-api.service';

@Component({
  selector: 'pf-budgets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './budgets.component.html'
})
export class BudgetsComponent implements OnChanges {
  private readonly api = inject(FinanceApiService);

  @Input({ required: true }) year!: number;
  @Input({ required: true }) month!: number;
  @Input() categories: Category[] = [];
  @Output() readonly changed = new EventEmitter<void>();

  budgets: Budget[] = [];
  error = '';

  categoryId = '';
  limit: number | null = null;

  ngOnChanges(): void {
    this.reload();
  }

  expenseCategories(): Category[] {
    return this.categories.filter((category) => category.type === 'Expense');
  }

  canAdd(): boolean {
    return !!this.categoryId && (this.limit ?? 0) > 0;
  }

  reload(): void {
    this.api.listBudgets(this.year, this.month).subscribe({
      next: (budgets) => {
        this.budgets = budgets;
        this.error = '';
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not load budgets.')
    });
  }

  add(): void {
    if (!this.canAdd()) {
      return;
    }

    this.api
      .createBudget({
        categoryId: this.categoryId,
        year: this.year,
        month: this.month,
        limit: this.limit ?? 0
      })
      .subscribe({
        next: () => {
          this.limit = null;
          this.error = '';
          this.reload();
          this.changed.emit();
        },
        error: (err) => (this.error = err?.error?.detail ?? 'Could not save the budget.')
      });
  }

  remove(budget: Budget): void {
    this.api.deleteBudget(budget.id).subscribe({
      next: () => {
        this.reload();
        this.changed.emit();
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not delete the budget.')
    });
  }
}
