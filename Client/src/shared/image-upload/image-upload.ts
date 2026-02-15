import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.css',
})
export class ImageUpload {
  protected imageSrc = signal<string | ArrayBuffer | null | undefined>(null);
  protected isDragging = false;
  private fileToUpload: File | null = null;
  uploadFile = output<File>();
  loading = input<boolean>(false);

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }
  
  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    if (event.dataTransfer?.files.length) {
      const file = event.dataTransfer.files[0];
      this.previewFile(file);
      this.fileToUpload = file;
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0] ?? null;
    if (!file) return;
    this.previewFile(file);
    this.fileToUpload = file;
    if (input) {
      input.value = '';
    }
  }

  onUploadFile() {
    if(this.fileToUpload){
      this.uploadFile.emit(this.fileToUpload);
    }
  }

  onCancel() {
    this.imageSrc.set(null);
    this.fileToUpload = null;
  }


  private previewFile(file: File) {
    const reader = new FileReader();
    reader.onload = (e) => {
      this.imageSrc.set(e.target?.result);
    };
    reader.readAsDataURL(file);
  }
}
