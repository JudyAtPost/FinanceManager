import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { CategoryBreakdownItem, MonthlySummary } from '../api.models';
import { categoryColor } from '../category-color';

@Component({
  selector: 'pf-monthly-summary',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <section *ngIf="summary as s">
      <div class="kpi-grid">
        <div class="kpi-card income">
          <div class="kpi-icon">💰</div>
          <div class="kpi-body">
            <div class="label">Income</div>
            <div class="kpi">{{ s.totalIncome | currency: 'EUR' }}</div>
            <div class="kpi-delta" *ngIf="incomeDelta() as delta" [class.up]="delta >= 0" [class.down]="delta < 0">
              {{ delta >= 0 ? '▲' : '▼' }} {{ absPercent(delta) | number: '1.0-1' }}% vs. last month
            </div>
          </div>
        </div>
        <div class="kpi-card expense">
          <div class="kpi-icon">📉</div>
          <div class="kpi-body">
            <div class="label">Expenses</div>
            <div class="kpi">{{ s.totalExpenses | currency: 'EUR' }}</div>
            <div class="kpi-delta" *ngIf="expensesDelta() as delta" [class.up]="delta <= 0" [class.down]="delta > 0">
              {{ delta >= 0 ? '▲' : '▼' }} {{ absPercent(delta) | number: '1.0-1' }}% vs. last month
            </div>
          </div>
        </div>
        <div class="kpi-card">
          <div class="kpi-icon">⚖️</div>
          <div class="kpi-body">
            <div class="label">Balance</div>
            <div class="kpi">{{ s.balance | currency: 'EUR' }}</div>
            <div class="kpi-delta" [class.up]="s.balance >= 0" [class.down]="s.balance < 0">
              {{ savingsRate() | number: '1.0-1' }}% savings rate
            </div>
          </div>
        </div>
        <div class="kpi-card top">
          <div class="kpi-icon">🏆</div>
          <div class="kpi-body">
            <div class="label">Biggest category</div>
            <div class="kpi" *ngIf="s.topExpenseCategory as top; else noTop">
              {{ top.categoryName }} ({{ top.total | currency: 'EUR' }})
            </div>
            <ng-template #noTop><div class="muted">No expenses recorded</div></ng-template>
          </div>
        </div>
      </div>

      <div class="dashboard-grid" style="margin-top:20px">
        <div class="card">
          <h2>Expenses by category</h2>
          <div class="chart-wrap" *ngIf="expenseChartData.labels?.length; else noBreakdown">
            <canvas baseChart [data]="expenseChartData" [options]="chartOptions" type="doughnut"></canvas>
          </div>
          <ng-template #noBreakdown><p class="muted">Nothing recorded for this month.</p></ng-template>
        </div>

        <div class="card">
          <h2>Budget vs. actual</h2>
          <ng-container *ngIf="s.budgets.length; else noBudgets">
            <div class="budget-item" *ngFor="let budget of s.budgets">
              <div class="budget-item-header">
                <span class="name">{{ budget.categoryName }}</span>
                <span class="amounts">
                  {{ budget.spent | currency: 'EUR' }} / {{ budget.limit | currency: 'EUR' }}
                </span>
              </div>
              <div class="progress-track">
                <div
                  class="progress-fill"
                  [class.warning]="!budget.isOverBudget && budget.usagePercentage >= 80"
                  [class.over]="budget.isOverBudget"
                  [style.width.%]="progressWidth(budget.usagePercentage)"
                ></div>
              </div>
            </div>
          </ng-container>
          <ng-template #noBudgets><p class="muted">No budgets defined for this month.</p></ng-template>
        </div>
      </div>

      <h2 style="margin-top:20px">Breakdown by category</h2>
      <table class="card" *ngIf="s.breakdown.length; else noTable">
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
      <ng-template #noTable><p class="muted">Nothing recorded for this month.</p></ng-template>
    </section>
  `
})
export class MonthlySummaryComponent implements OnChanges {
  @Input({ required: true }) summary: MonthlySummary | null = null;
  @Input() previousSummary: MonthlySummary | null = null;

  expenseChartData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [{ data: [] }] };

  readonly chartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' }
    }
  };

  ngOnChanges(): void {
    this.expenseChartData = this.buildExpenseChartData(this.summary?.breakdown ?? []);
  }

  savingsRate(): number {
    if (!this.summary || this.summary.totalIncome <= 0) {
      return 0;
    }

    return (this.summary.balance / this.summary.totalIncome) * 100;
  }

  incomeDelta(): number | null {
    return this.percentChange(this.summary?.totalIncome, this.previousSummary?.totalIncome);
  }

  expensesDelta(): number | null {
    return this.percentChange(this.summary?.totalExpenses, this.previousSummary?.totalExpenses);
  }

  absPercent(value: number): number {
    return Math.abs(value);
  }

  private percentChange(current: number | undefined, previous: number | undefined): number | null {
    if (current === undefined || previous === undefined || previous <= 0) {
      return null;
    }

    return ((current - previous) / previous) * 100;
  }

  progressWidth(usagePercentage: number): number {
    return Math.min(100, Math.max(0, usagePercentage));
  }

  private buildExpenseChartData(breakdown: CategoryBreakdownItem[]): ChartConfiguration<'doughnut'>['data'] {
    const expenses = breakdown.filter((item) => item.type === 'Expense');

    return {
      labels: expenses.map((item) => item.categoryName),
      datasets: [
        {
          data: expenses.map((item) => item.total),
          backgroundColor: expenses.map((item) => categoryColor(item.categoryName)),
          borderWidth: 0
        }
      ]
    };
  }
}
