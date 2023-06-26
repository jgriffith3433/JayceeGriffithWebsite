import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CalledIngredientDTO } from '../../../models/CalledIngredientDTO';
import { CalledIngredientsService } from '../../../providers/called-ingredients.service';

@Component({
  selector: 'app-called-ingredients',
  templateUrl: './called-ingredients.component.html'
})
export class CalledIngredientsComponent implements OnInit {
  public calledIngredients: CalledIngredientDTO[] | undefined;

  constructor(
    private calledIngredientsService: CalledIngredientsService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.calledIngredientsService.getAll().subscribe(result => {
      this.calledIngredients = result.calledIngredients;
    }, error => console.error(error));
  }
}
