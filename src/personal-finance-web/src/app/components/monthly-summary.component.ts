import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MonthlySummary } from '../api.models';

@Component({
  selector: 'pf-monthly-summary',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="card" *ngIf="summary as s">
      <h2>Monthly summary</h2>

      <div class="grid">
        <div>
          <div class="label">Income</div>
          <div class="kpi income">{{ s.totalIncome | currency: 'EUR' }}</div>
        </div>
        <div>
          <div class="label">Expenses</div>
          <div class="kpi expense">{{ s.totalExpenses | currency: 'EUR' }}</div>
        </div>
        <div>
          <div class="label">Balance</div>
          <div class="kpi">{{ s.balance | currency: 'EUR' }}</div>
        </div>
        <div>
          <div class="label">Biggest category</div>
          <div class="kpi" *ngIf="s.topExpenseCategory as top; else noTop">
            {{ top.categoryName }} ({{ top.total | currency: 'EUR' }})
          </div>
          <ng-template #noTop><div class="muted">No expenses recorded</div></ng-template>
        </div>
      </div>

      <h2 style="margin-top:24px">Breakdown by category</h2>
      <table *ngIf="s.breakdown.length; else noBreakdown">
        <thead>
          <tr>
            <th>Category</th>
            <th>Type</th>
            <th class="amount">Total</th>
            <th class="amount">Share</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let item of s.breakdown">
            <td>{{ item.categoryName }}</td>
            <td><span class="badge">{{ item.type }}</span></td>
            <td class="amount">{{ item.total | currency: 'EUR' }}</td>
            <td class="amount">{{ item.shareOfTypeTotal | number: '1.0-2' }}%</td>
          </tr>
        </tbody>
      </table>
      <ng-template #noBreakdown><p class="muted">Nothing recorded for this month.</p></ng-template>

      <h2 style="margin-top:24px">Budget vs. actual</h2>
      <table *ngIf="s.budgets.length; else noBudgets">
        <thead>
          <tr>
            <th>Category</th>
            <th class="amount">Budget</th>
            <th class="amount">Spent</th>
            <th class="amount">Difference</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let budget of s.budgets">
            <td>{{ budget.categoryName }}</td>
            <td class="amount">{{ budget.limit | currency: 'EUR' }}</td>
            <td class="amount">{{ budget.spent | currency: 'EUR' }}</td>
            <td class="amount">
              {{ (budget.isOverBudget ? budget.overspentBy : budget.remaining) | currency: 'EUR' }}
            </td>
            <td>
              <span class="badge" [class.over]="budget.isOverBudget" [class.ok]="!budget.isOverBudget">
                {{ budget.isOverBudget ? 'Over by ' + (budget.overspentBy | currency: 'EUR') : 'Within budget' }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
      <ng-template #noBudgets><p class="muted">No budgets defined for this month.</p></ng-template>
    </section>
  `
})
export class MonthlySummaryComponent {
  @Input({ required: true }) summary: MonthlySummary | null = null;
}
