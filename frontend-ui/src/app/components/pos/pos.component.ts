import { Component, OnInit } from '@angular/core';
import { Product, ProductService } from '../../services/product.service';
import { SaleService, SaleProduct } from '../../services/sale.service';
import { CustomerService, Customer } from '../../services/customer.service';

@Component({
  selector: 'app-pos',
  templateUrl: './pos.component.html'
})
export class PosComponent implements OnInit {
  products: Product[] = [];
  customers: Customer[] = [];
  search = '';
  cart: { product: Product; quantity: number }[] = [];
  total = 0;
  errorMsg: string | null = null;
  successMsg: string | null = null;
  selectedCustomerId: number | null = null;
  paymentMethod: string = '';
  paymentMethods = ['Efectivo', 'Tarjeta', 'Paypal'];

  constructor(
    private productService: ProductService,
    private saleService: SaleService,
    private customerService: CustomerService
  ) {}

  ngOnInit() {
    this.loadProducts();
    this.customerService.getPaged(1, 100, '').subscribe(res => this.customers = res.customers);
  }

  loadProducts() {
    this.productService.getPaged(1, 100, this.search).subscribe(res => {
      this.products = res.products;
    });
  }

  onSearchChange(value: string) {
    this.search = value;
    this.loadProducts();
  }

  addToCart(product: Product) {
    const existing = this.cart.find(c => c.product.id === product.id);
    if (existing) {
      if (existing.quantity < product.stock) {
        existing.quantity++;
      }
    } else {
      if (product.stock > 0) {
        this.cart.push({ product, quantity: 1 });
      }
    }
    this.updateTotal();
  }

  removeFromCart(productId: number) {
    this.cart = this.cart.filter(c => c.product.id !== productId);
    this.updateTotal();
  }

  updateQuantity(productId: number, qty: number) {
    const item = this.cart.find(c => c.product.id === productId);
    if (item) {
      if (qty > 0 && qty <= item.product.stock) {
        item.quantity = qty;
      }
      this.updateTotal();
    }
  }

  updateTotal() {
    this.total = this.cart.reduce((sum, c) => sum + c.product.price * c.quantity, 0);
  }

  finalizeSale() {
    this.errorMsg = null;
    this.successMsg = null;
    if (!this.cart.length) {
      this.errorMsg = 'El carrito está vacío.';
      return;
    }
    if (!this.paymentMethod) {
      this.errorMsg = 'Selecciona un método de pago.';
      return;
    }
    const items: SaleProduct[] = this.cart.map(c => ({
      productId: c.product.id,
      quantity: c.quantity
    }));
    this.saleService.registerSale(items, this.selectedCustomerId, this.paymentMethod).subscribe({
      next: resp => {
        this.successMsg = 'Venta registrada correctamente.';
        this.cart = [];
        this.total = 0;
        this.paymentMethod = '';
        this.selectedCustomerId = null;
        this.loadProducts();
      },
      error: err => {
        this.errorMsg = err.error?.message || 'Error al registrar la venta.';
      }
    });
  }
}