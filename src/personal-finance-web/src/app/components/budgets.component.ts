import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Budget, Category } from '../api.models';
import { FinanceApiService } from '../finance-api.service';

@Component({
  selector: 'pf-budgets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="card">
      <h2>Budgets</h2>

      <form class="toolbar" (ngSubmit)="add()">
        <div class="field">
          <label class="label" for="budget-category">Expense category</label>
          <select id="budget-category" name="categoryId" [(ngModel)]="categoryId" required>
            <option *ngFor="let category of expenseCategories()" [value]="category.id">{{ category.name }}</option>
          </select>
        </div>
        <div class="field">
          <label class="label" for="budget-limit">Monthly limit</label>
          <input id="budget-limit" name="limit" type="number" step="0.01" min="0.01" [(ngModel)]="limit" required />
        </div>
        <button type="submit" [disabled]="!canAdd()">Set budget</button>
      </form>

      <p class="error" *ngIf="error">{{ error }}</p>

      <table *ngIf="budgets.length; else empty">
        <thead>
          <tr>
            <th>Category</th>
            <th class="amount">Limit</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let budget of budgets">
            <td>{{ budget.categoryName }}</td>
            <td class="amount">{{ budget.limit | currency: 'EUR' }}</td>
            <td><button type="button" class="ghost" (click)="remove(budget)">Delete</button></td>
          </tr>
        </tbody>
      </table>
      <ng-template #empty><p class="muted">No budgets defined for this month.</p></ng-template>
    </section>
  `
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
