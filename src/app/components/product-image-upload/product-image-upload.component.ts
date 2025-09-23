import { Component } from '@angular/core';
import { HttpClient, HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-product-image-upload',
  templateUrl: './product-image-upload.component.html'
})
export class ProductImageUploadComponent {
  selectedFile: File | null = null;
  uploadProgress: number = 0;
  imageUrl: string | null = null;

  constructor(private http: HttpClient) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0] ?? null;
  }

  upload() {
    if (!this.selectedFile) {
      alert("Selecciona una imagen primero.");
      return;
    }
    const formData = new FormData();
    formData.append('file', this.selectedFile, this.selectedFile.name);

    this.http.post<{ imageUrl: string }>('http://localhost:5000/api/files/upload-image', formData, {
      reportProgress: true,
      observe: 'events'
    }).subscribe(event => {
      if (event.type === HttpEventType.UploadProgress && event.total) {
        this.uploadProgress = Math.round(100 * (event.loaded / event.total));
      } else if (event.type === HttpEventType.Response) {
        this.imageUrl = event.body?.imageUrl ?? null;
        this.uploadProgress = 0;
      }
    }, error => {
      alert('Error al subir la imagen');
      this.uploadProgress = 0;
    });
  }
}