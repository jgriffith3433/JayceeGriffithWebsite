import { Component, OnInit } from '@angular/core';
import { UntypedFormBuilder, UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginDTO } from 'src/app/models/LoginDTO';
import { AuthService } from 'src/app/providers/auth.service';
import { Location } from '@angular/common';
import { TokenService } from 'src/app/providers/token.service';
import { PicoService } from '../../../../chat/providers/pico.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  form: UntypedFormGroup;
  isError: boolean = false;
  errMsg: string = "Some error has occured!";

  constructor(
    private formBuilder: UntypedFormBuilder,
    private authService: AuthService,
    private router: Router,
    private picoService: PicoService,
    private location: Location, private tokenService: TokenService) {
    this.form = this.formBuilder.group({
      emailAddress: new UntypedFormControl([], [Validators.required, Validators.email]),
      password: new UntypedFormControl([], [Validators.required])
    })
  }

  ngOnInit(): void { 
    // clear off any tokens in cache
    // if user lands on login
    if (this.tokenService.IsAuthenticated) {
      this.authService.logout();
    }
  }

  login() {
    console.log(this.form.value);
    this.authService.login(<LoginDTO>this.form.value).subscribe({
      next: (response) => {
        this.router.navigate(['home']);
        if (!this.picoService.isLoaded) {
          this.picoService.autoStart = true;
          this.picoService.load();
        }
        else {
          this.picoService.start();
        }
      },
      error: (err) => {
        if (err.status === 401) {
          this.form.reset();
          this.errMsg = "Authentication Failed!";
        }
        this.isError = true;
      }
    });
  }

  back() {
    this.location.back();
  }

}
