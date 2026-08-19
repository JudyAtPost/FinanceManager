import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Category, TransactionType } from '../../api.models';
import { FinanceApiService } from '../../finance-api.service';

@Component({
  selector: 'pf-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories.component.html'
})
export class CategoriesComponent implements OnInit {
  private readonly api = inject(FinanceApiService);

  @Output() readonly changed = new EventEmitter<void>();

  categories: Category[] = [];
  error = '';

  name = '';
  type: TransactionType = 'Expense';

  editingId: string | null = null;
  editName = '';
  editType: TransactionType = 'Expense';

  ngOnInit(): void {
    this.reload();
  }

  canAdd(): boolean {
    return this.name.trim().length > 0;
  }

  canSaveEdit(): boolean {
    return this.editName.trim().length > 0;
  }

  reload(): void {
    this.api.listCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
        this.error = '';
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not load categories.')
    });
  }

  add(): void {
    if (!this.canAdd()) {
      return;
    }

    this.api.createCategory({ name: this.name.trim(), type: this.type }).subscribe({
      next: () => {
        this.name = '';
        this.error = '';
        this.reload();
        this.changed.emit();
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not save the category.')
    });
  }

  startEdit(category: Category): void {
    this.editingId = category.id;
    this.editName = category.name;
    this.editType = category.type;
  }

  cancelEdit(): void {
    this.editingId = null;
    this.editName = '';
  }

  saveEdit(category: Category): void {
    if (!this.canSaveEdit()) {
      return;
    }

    this.api.updateCategory(category.id, { name: this.editName.trim(), type: this.editType }).subscribe({
      next: () => {
        this.error = '';
        this.cancelEdit();
        this.reload();
        this.changed.emit();
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not update the category.')
    });
  }

  remove(category: Category): void {
    this.api.deleteCategory(category.id).subscribe({
      next: () => {
        this.error = '';
        this.reload();
        this.changed.emit();
      },
      error: (err) => (this.error = err?.error?.detail ?? 'Could not delete the category.')
    });
  }
}
