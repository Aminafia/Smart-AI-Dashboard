import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { DocumentModel } from '../models/documents/document.model';
import { PagedResponse } from '../models/shared/paged-response.model';

@Injectable({
    providedIn: 'root'
})
export class DocumentService {

    private readonly apiUrl = `${environment.apiUrl}/Documents`;

    constructor(private readonly http: HttpClient) { }

    upload(file: File): Observable<DocumentModel> 
    {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<DocumentModel>(this.apiUrl, formData);
    }

    getDocuments(page: number, pageSize: number): Observable<PagedResponse<DocumentModel>> 
    {
        return this.http.get<PagedResponse<DocumentModel>>(
            `${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
    }

    download(id: string): Observable<Blob> 
    {
        return this.http.get(
            `${this.apiUrl}/${id}`,
            {
                responseType: 'blob'
            });
    }

    delete(id: string): Observable<void> 
    {
        return this.http.delete<void>(
            `${this.apiUrl}/${id}`);
    }
}