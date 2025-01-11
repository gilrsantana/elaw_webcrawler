import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResponseApi } from '../models/api/response-api';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
  
export class WebcrawlerService {

  baseUrl = `${environment.apiUrl}`;
  constructor(private http: HttpClient) { }

  getProxies(sourceAdd: string): Observable<ResponseApi> {
    return this.http.get<ResponseApi>(`${this.baseUrl}?url=${sourceAdd}`);
  }

}
