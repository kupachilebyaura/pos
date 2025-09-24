import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ProductManagementComponent } from './components/product-management/product-management.component';
import { ProductService } from './services/product.service';

@NgModule({
  declarations: [
    ProductManagementComponent,
    // ...otros componentes
  ],
  imports: [
    ReactiveFormsModule,
    // ...otros módulos
  ],
  providers: [ProductService]
})
export class AppModule { }