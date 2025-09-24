import { Component, OnInit } from '@angular/core';
import { User, UserService } from '../../services/user.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html'
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  form: FormGroup;
  isEditing = false;
  editingUserId: number | null = null;
  roles = ['Admin', 'Seller'];
  errorMsg: string | null = null;
  successMsg: string | null = null;

  constructor(
    private userService: UserService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', []], // Required only on create
      role: ['', [Validators.required]],
      email: [''],
      fullName: ['']
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getAll().subscribe({
      next: users => this.users = users,
      error: err => this.errorMsg = err.error?.message || 'Error al cargar usuarios.'
    });
  }

  submit() {
    this.errorMsg = null;
    this.successMsg = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMsg = 'Revisa los campos del formulario.';
      return;
    }

    const userData = this.form.value;
    if (!this.isEditing && !userData.password) {
      this.errorMsg = 'La contraseña es obligatoria al crear usuario.';
      return;
    }

    if (this.isEditing && this.editingUserId != null) {
      // Si la contraseña está vacía, no la envía (no se actualiza)
      const payload = { ...userData };
      if (!payload.password) delete payload.password;
      this.userService.update(this.editingUserId, payload)
        .subscribe({
          next: () => {
            this.successMsg = 'Usuario actualizado correctamente.';
            this.loadUsers();
            this.cancelEdit();
          },
          error: err => {
            this.errorMsg = err.error?.message || 'Error al actualizar usuario.';
          }
        });
    } else {
      this.userService.create(userData)
        .subscribe({
          next: () => {
            this.successMsg = 'Usuario creado correctamente.';
            this.loadUsers();
            this.form.reset();
          },
          error: err => {
            this.errorMsg = err.error?.message || 'Error al crear usuario.';
          }
        });
    }
  }

  edit(user: User) {
    this.isEditing = true;
    this.editingUserId = user.id;
    this.form.patchValue({
      username: user.username,
      password: '',
      role: user.role,
      email: user.email,
      fullName: user.fullName
    });
    this.errorMsg = null;
    this.successMsg = null;
  }

  cancelEdit() {
    this.isEditing = false;
    this.editingUserId = null;
    this.form.reset();
    this.errorMsg = null;
    this.successMsg = null;
  }

  delete(id: number) {
    if (confirm('¿Seguro que deseas eliminar este usuario?')) {
      this.userService.delete(id).subscribe({
        next: () => {
          this.successMsg = 'Usuario eliminado correctamente.';
          this.loadUsers();
        },
        error: err => {
          this.errorMsg = err.error?.message || 'Error al eliminar usuario.';
        }
      });
    }
  }
}