import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl } from '@angular/forms';
import { Role, RoleService } from '../../services/role.service';

const ALL_PERMISSIONS: string[] = [
  'user:create', 'user:update', 'user:delete', 'product:create', 'product:update', 'product:delete',
  'sale:create', 'sale:view', 'customer:create', 'customer:update', 'customer:delete'
];

@Component({
  selector: 'app-role-management',
  templateUrl: './role-management.component.html'
})
export class RoleManagementComponent implements OnInit {
  roles: Role[] = [];
  form: FormGroup;
  isEditing = false;
  editingRoleId: number | null = null;
  errorMsg: string | null = null;
  successMsg: string | null = null;
  allPermissions = ALL_PERMISSIONS;

  constructor(
    private roleService: RoleService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      permissions: this.fb.array([])
    });
  }

  ngOnInit() {
    this.loadRoles();
  }

  loadRoles() {
    this.roleService.getAll().subscribe({
      next: roles => this.roles = roles,
      error: err => this.errorMsg = err.error?.message || 'Error al cargar roles.'
    });
  }

  permissionsArray(): FormArray {
    return this.form.get('permissions') as FormArray;
  }

  togglePermission(permission: string) {
    const arr = this.permissionsArray();
    const idx = arr.value.indexOf(permission);
    if (idx === -1) {
      arr.push(new FormControl(permission));
    } else {
      arr.removeAt(idx);
    }
  }

  hasPermission(permission: string): boolean {
    return this.permissionsArray().value.includes(permission);
  }

  submit() {
    this.errorMsg = null;
    this.successMsg = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMsg = 'Revisa los campos del formulario.';
      return;
    }

    const roleData = {
      name: this.form.value.name,
      permissions: this.form.value.permissions
    };

    if (this.isEditing && this.editingRoleId != null) {
      this.roleService.update(this.editingRoleId, roleData)
        .subscribe({
          next: () => {
            this.successMsg = 'Rol actualizado correctamente.';
            this.loadRoles();
            this.cancelEdit();
          },
          error: err => {
            this.errorMsg = err.error?.message || 'Error al actualizar rol.';
          }
        });
    } else {
      this.roleService.create(roleData)
        .subscribe({
          next: () => {
            this.successMsg = 'Rol creado correctamente.';
            this.loadRoles();
            this.form.reset();
            this.form.setControl('permissions', this.fb.array([]));
          },
          error: err => {
            this.errorMsg = err.error?.message || 'Error al crear rol.';
          }
        });
    }
  }

  edit(role: Role) {
    this.isEditing = true;
    this.editingRoleId = role.id;
    this.form.patchValue({ name: role.name });
    // Asignar permisos actuales
    const arr = this.fb.array(role.permissions.map(p => new FormControl(p)));
    this.form.setControl('permissions', arr);
    this.errorMsg = null;
    this.successMsg = null;
  }

  cancelEdit() {
    this.isEditing = false;
    this.editingRoleId = null;
    this.form.reset();
    this.form.setControl('permissions', this.fb.array([]));
    this.errorMsg = null;
    this.successMsg = null;
  }
}