import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { UserService, User } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  form: FormGroup;
  user: User | null = null;
  msg: string | null = null;

  constructor(private userService: UserService, private fb: FormBuilder) {
    this.form = this.fb.group({
      email: [''],
      fullName: [''],
      password: ['']
    });
  }

  ngOnInit() {
    this.userService.getOwnProfile().subscribe(u => {
      this.user = u;
      this.form.patchValue({ email: u.email, fullName: u.fullName });
    });
  }

  submit() {
    this.userService.updateOwnProfile(this.form.value).subscribe(() => {
      this.msg = 'Perfil actualizado';
    });
  }
}