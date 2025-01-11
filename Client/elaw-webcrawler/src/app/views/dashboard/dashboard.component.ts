import { CommonModule, DatePipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Proxy } from '../../models/entities/proxy';
import { WebcrawlerService } from '../../services/webcrawler.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, tap } from 'rxjs';
import { ResponseApi } from '../../models/api/response-api';
import { MessageModel } from '../../models/api/message-model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {

  sourceAddress: string = 'https://proxyservers.pro/proxy/list/order/updated/order_dir/desc';
  proxie: Proxy = {} as Proxy;

  constructor(private service: WebcrawlerService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private http: HttpClient) { }

  search() {
    this.spinner.show();
    this.service.getProxies(this.sourceAddress).pipe(
      tap((response: ResponseApi) => {
        if (response.viewData) {
          this.proxie = response.viewData;
        } 
      }),
        catchError((error: HttpErrorResponse) => {
          this.spinner.hide();
          if(error.status == 0) 
            this.toastr.error(error.message);
          else if(error.error.messages)
            error.error.messages.forEach((message: MessageModel) => {
              this.toastr.error(message.text);
            });
          else
            this.toastr.error('Erro ao buscar os dados.');
          return [];
        })
      ).subscribe(() => {
        this.spinner.hide();
      } )
  }

  download(address: string) {
    this.spinner.show();
    this.http.get(address, { responseType: 'blob' }).subscribe(
      (response: Blob) => {
        const url = window.URL.createObjectURL(response);
        const a = document.createElement('a');
        a.href = url;
        a.download = address.split('/').pop() || 'download';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.spinner.hide();
      },
      (error) => {
        this.toastr.error('Erro ao baixar o arquivo.');
        this.spinner.hide();
      }
    );

  }

}
