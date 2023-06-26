import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
@Injectable({
  providedIn: 'root'
})
export class ChatToggledService {
  public chatStyleListener: Subject<string>;
  public chatToggledListener: Subject<boolean>;

  constructor(
  ) {
    this.chatStyleListener = new Subject<string>();
    this.chatToggledListener = new Subject<boolean>();
  }
}
