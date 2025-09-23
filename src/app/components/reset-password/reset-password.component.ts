import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent {
  form: FormGroup;
  errorMsg: string | null = null;
  successMsg: string | null = null;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      token: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });

    // Si el token viene por query param lo rellenamos
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.form.get('token')?.setValue(token);
    }
  }

  submit() {
    this.errorMsg = null;
    this.successMsg = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMsg = 'Revisa los campos del formulario.';
      return;
    }
    this.submitting = true;
    const { token, newPassword } = this.form.value;
    this.auth.resetPassword(token, newPassword).subscribe({
      next: () => {
        this.successMsg = 'Contraseña restablecida correctamente. Ahora puedes iniciar sesión.';
        setTimeout(() => this.router.navigate(['/login']), 2500);
      },
      error: err => {
        this.errorMsg = err.error?.message || 'Error al restablecer contraseña.';
        this.submitting = false;
      }
    });
  }
}