import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { CategoryBreakdownItem, MonthlySummary } from '../../api.models';
import { categoryColor } from '../../category-color';

@Component({
  selector: 'pf-monthly-summary',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './monthly-summary.component.html'
})
export class MonthlySummaryComponent implements OnChanges {
  @Input({ required: true }) summary: MonthlySummary | null = null;
  @Input() previousSummary: MonthlySummary | null = null;

  expenseChartData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [{ data: [] }] };

  readonly chartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          boxWidth: 12,
          padding: 12,
          font: { size: 11 }
        }
      }
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
