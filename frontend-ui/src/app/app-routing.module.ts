import { UserManagementComponent } from './components/user-management/user-management.component';
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
  // ... otras rutas ...
  { path: 'usuarios', component: UserManagementComponent, canActivate: [AuthGuard] }
];