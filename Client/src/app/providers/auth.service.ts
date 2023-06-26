import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Observer, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthToken } from '../models/AuthToken';
import { LoginDTO } from '../models/LoginDTO';
import { Config } from './config';
import { TokenService } from './token.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  authListener: BehaviorSubject<boolean> = new BehaviorSubject(false);

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private router: Router
  ) {
    this.authListener.next(this.tokenService.IsAuthenticated);
  }

  login(loginDto: LoginDTO) {
    return this.http.post(`${Config.api}/Auth/token`, loginDto).pipe(map((response) => {
      let token = <AuthToken>response;
      this.tokenService.setToken(token);
      this.authListener.next(true);
      return token;
    }));
  }
  logout() {
    this.tokenService.clearToken();
    this.authListener.next(false);
    this.router.navigate(['/']);
  }
}
