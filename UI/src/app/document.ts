import { FileItem } from 'ng2-file-upload';

export class Document {
    id: number;
    name: string;
    isSelected: boolean;
    isUploading: boolean;
    isNew: boolean;
    totalTermDocCount: number;
    fileItem?: FileItem;
}