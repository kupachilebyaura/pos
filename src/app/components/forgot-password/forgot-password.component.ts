import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
  email = '';
  msg: string | null = null;
  errorMsg: string | null = null;

  constructor(private auth: AuthService) {}

  submit() {
    this.auth.forgotPassword(this.email).subscribe({
      next: () => this.msg = 'Si el email existe, hemos enviado instrucciones.',
      error: err => this.errorMsg = err.error?.message || 'Error al solicitar recuperación.'
    });
  }
}