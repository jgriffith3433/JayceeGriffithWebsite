import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HowDoIService } from '../../../../chat/providers/how-do-I.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  constructor(
    private howDoIService: HowDoIService,
    private router: Router
  ) { }

  ngOnInit(): void {

  }

  sendHowDoI(howDoIStr: string, forceFunctionCall: string = 'auto', goToPage: string | undefined = undefined): void {
    if (goToPage) {
      this.router.navigate([goToPage]);
      setTimeout(() => this.howDoIService.send(howDoIStr, forceFunctionCall), 500);
    }
    else {
      this.howDoIService.send(howDoIStr, forceFunctionCall);
    }
  }
}
