import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HowDoIService } from '../../../../chat/providers/how-do-I.service';

@Component({
  selector: 'app-help',
  templateUrl: './help.component.html',
  styleUrls: ['./help.component.scss']
})
export class HelpComponent implements OnInit {
  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private howDoIService: HowDoIService,
  ) {
  }

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
