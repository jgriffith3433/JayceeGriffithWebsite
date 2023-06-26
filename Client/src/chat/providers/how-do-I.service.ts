import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
@Injectable({
  providedIn: 'root'
})
export class HowDoIService {
  public promise: Subject<any>;

  constructor(
  ) {
    this.promise = new Subject<any>();
  }

  public send(how: string, forceFunctionCall: string = "auto") {
    this.promise.next({
      how: how,
      forceFunctionCall: forceFunctionCall
    });
  }
}
