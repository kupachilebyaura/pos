import { Component, OnInit } from '@angular/core';
import { Customer, CustomerService } from '../../services/customer.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-customer-management',
  templateUrl: './customer-management.component.html'
})
export class CustomerManagementComponent implements OnInit {
  customers: Customer[] = [];
  form: FormGroup;
  isEditing = false;
  editingCustomerId: number | null = null;
  search = '';
  page = 1;
  pageSize = 10;
  total = 0;
  errorMsg: string | null = null;
  successMsg: string | null = null;

  constructor(
    private customerService: CustomerService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.email]],
      phone: ['', [Validators.pattern(/^[\d\s\+\-]+$/)]]
    });
  }

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.customerService.getPaged(this.page, this.pageSize, this.search)
      .subscribe({
        next: response => {
          this.customers = response.customers;
          this.total = response.total;
        },
        error: err => {
          this.errorMsg = err.error?.message || 'Error al cargar los clientes.';
        }
      });
  }

  onSearchChange(value: string) {
    this.search = value;
    this.page = 1;
    this.loadCustomers();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadCustomers();
  }

  submit() {
    this.errorMsg = null;
    this.successMsg = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMsg = 'Revisa los campos del formulario.';
      return;
    }

    const customerData = this.form.value;

    if (this.isEditing && this.editingCustomerId != null) {
      this.customerService.update(this.editingCustomerId, customerData)
        .subscribe({
          next: () => {
            this.successMsg = 'Cliente actualizado correctamente.';
            this.loadCustomers();
            this.cancelEdit();
          },
          error: err => {
            this.errorMsg = err.error?.message || 'Error al actualizar el cliente.';
          }
        });
    } else {
      this.customerService.create(customerData)
        .subscribe({
          next: () => {
            this.successMsg = 'Cliente creado correctamente.';
            this.loadCustomers();
            this.form.reset();
          },
          error: err => {
            this.errorMsg = err.error?.message || 'Error al crear el cliente.';
          }
        });
    }
  }

  edit(customer: Customer) {
    this.isEditing = true;
    this.editingCustomerId = customer.id;
    this.form.patchValue(customer);
    this.errorMsg = null;
    this.successMsg = null;
  }

  cancelEdit() {
    this.isEditing = false;
    this.editingCustomerId = null;
    this.form.reset();
    this.errorMsg = null;
    this.successMsg = null;
  }

  delete(id: number) {
    if (confirm('¿Seguro que deseas eliminar este cliente?')) {
      this.customerService.delete(id).subscribe({
        next: () => {
          this.successMsg = 'Cliente eliminado correctamente.';
          this.loadCustomers();
        },
        error: err => {
          this.errorMsg = err.error?.message || 'Error al eliminar el cliente.';
        }
      });
    }
  }
}