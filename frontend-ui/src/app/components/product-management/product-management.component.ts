import { Component, OnInit } from '@angular/core';
import { Product, ProductService } from '../../services/product.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-product-management',
  templateUrl: './product-management.component.html'
})
export class ProductManagementComponent implements OnInit {
  products: Product[] = [];
  form: FormGroup;
  isEditing = false;
  editingProductId: number | null = null;
  selectedImage: File | null = null;
  search = '';
  page = 1;
  pageSize = 10;
  total = 0;

  // Cloudinary
  cloudinaryUrl = 'https://api.cloudinary.com/v1_1/<tu_cloud_name>/image/upload';
  cloudinaryPreset = '<tu_upload_preset>';

  constructor(
    private productService: ProductService,
    private fb: FormBuilder,
    private http: HttpClient
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      description: [''],
      price: [0, [Validators.required, Validators.min(0.01)]],
      stock: [0, [Validators.required, Validators.min(0)]],
      code: ['']
    });
  }

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.productService.getPaged(this.page, this.pageSize, this.search)
      .subscribe(response => {
        this.products = response.products;
        this.total = response.total;
      });
  }

  onSearchChange(value: string) {
    this.search = value;
    this.page = 1;
    this.loadProducts();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadProducts();
  }

  onImageSelected(event: any) {
    this.selectedImage = event.target.files[0] ?? null;
  }

  async uploadImageDirect(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.cloudinaryPreset);

    const res: any = await this.http.post(this.cloudinaryUrl, formData).toPromise();
    // Extract just the filename from the secure_url
    const url: string = res.secure_url;
    const fileName = url.substring(url.lastIndexOf('/') + 1);
    return fileName;
  }

  async submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    let imageUrl = null;
    if (this.selectedImage) {
      imageUrl = await this.uploadImageDirect(this.selectedImage);
    }

    const productData = {
      ...this.form.value,
      imageUrl
    };

    if (this.isEditing && this.editingProductId != null) {
      this.productService.updateWithImageUrl(this.editingProductId, productData)
        .subscribe(() => {
          this.loadProducts();
          this.cancelEdit();
        });
    } else {
      this.productService.createWithImageUrl(productData)
        .subscribe(() => {
          this.loadProducts();
          this.form.reset();
          this.selectedImage = null;
        });
    }
  }

  edit(product: Product) {
    this.isEditing = true;
    this.editingProductId = product.id;
    this.form.patchValue(product);
    this.selectedImage = null;
  }

  cancelEdit() {
    this.isEditing = false;
    this.editingProductId = null;
    this.form.reset();
    this.selectedImage = null;
  }

  delete(id: number) {
    if (confirm('¿Seguro que deseas eliminar este producto?')) {
      this.productService.delete(id).subscribe(() => this.loadProducts());
    }
  }
}