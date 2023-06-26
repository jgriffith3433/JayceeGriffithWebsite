import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { GetChatResponseQuery } from "../models/GetChatResponseQuery";
import { GetChatResponseVm } from "../models/GetChatResponseVm";
import { GetChatSpeechToTextVM } from '../models/GetChatSpeechToTextVM';
import { Config } from "./config";
import { DomSanitizer } from '@angular/platform-browser';
import { GetChatTextToSpeechVM } from "../models/GetChatTextToSpeechVM";

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  constructor(
    private http: HttpClient, private domSanitizer: DomSanitizer
  ) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getChatResponse(normal: boolean, query: GetChatResponseQuery): Observable<GetChatResponseVm> {
    if (normal) {
      return this.http.post(`${this.baseUri}/Chat/Normal`, query).pipe((map(x => <GetChatResponseVm>x)));
    }
    else {
      return this.http.post(`${this.baseUri}/Chat`, query).pipe((map(x => <GetChatResponseVm>x)));
    }
  }

  getChatSpeechToText(speech: any, lastMessage: string | undefined): Observable<GetChatSpeechToTextVM> {
    const formData = new FormData();
    formData.append("speech", speech);
    if (lastMessage) {
      formData.append("lastMessage", lastMessage);
    }
    return this.http.post(`${this.baseUri}/Chat/Speech`, formData).pipe((map(x => <GetChatSpeechToTextVM>x)));;
  }

  getChatTextToSpeech(text: string): Observable<string> {
    let body = {
      text: text
    } as GetChatTextToSpeechVM;

    return this.http.post<Blob>(`${this.baseUri}/Chat/TextToSpeech`, body, { 'responseType': 'blob' as 'json' }).pipe(map(blob => this.domSanitizer.bypassSecurityTrustUrl(window.URL.createObjectURL(blob)) as string));
  }
}
