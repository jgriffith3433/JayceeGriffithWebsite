import { Component, TemplateRef, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CalledIngredientDetailsDTO } from '../../../models/CalledIngredientDetailsDTO';
import { CalledIngredientDTO } from '../../../models/CalledIngredientDTO';
import { CreateCalledIngredientCommand } from '../../../models/CreateCalledIngredientCommand';
import { CreateRecipeCommand } from '../../../models/CreateRecipeCommand';
import { RecipeDTO } from '../../../models/RecipeDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateCalledIngredientCommand } from '../../../models/UpdateCalledIngredientCommand';
import { UpdateCalledIngredientDetailsCommand } from '../../../models/UpdateCalledIngredientDetailsCommand';
import { UpdateRecipeNameCommand } from '../../../models/UpdateRecipeNameCommand';
import { UpdateRecipeServesCommand } from '../../../models/UpdateRecipeServesCommand';
import { RecipesService } from '../../../providers/recipes.service';


@Component({
  selector: 'app-recipes-component',
  templateUrl: './recipes.component.html',
  styleUrls: ['./recipes.component.scss']
})
export class RecipesComponent implements OnInit {
  debug = false;
  recipes: RecipeDTO[] | undefined;
  kitchenUnitTypes: UnitTypeDTO[] | undefined;
  selectedRecipe: RecipeDTO | undefined;
  selectedCalledIngredient: CalledIngredientDTO | undefined;
  selectedCalledIngredientDetails: CalledIngredientDetailsDTO | undefined;
  newRecipeEditor: any = {};
  recipeOptionsEditor: any = {};
  calledIngredientDetailsEditor: any = {};
  newRecipeModalRef: BsModalRef;
  recipeOptionsModalRef: BsModalRef;
  deleteRecipeModalRef: BsModalRef;
  calledIngredientDetailsModalRef: BsModalRef;

  constructor(
    private recipesService: RecipesService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.recipesService.getAll().subscribe(
      result => {
        this.recipes = result.recipes;
        this.kitchenUnitTypes = result.kitchenUnitTypes;
        if (this.recipes?.length) {
          this.selectedRecipe = this.recipes[0];
        }
      },
      error => console.error(error)
    );
  }

  // Recipes
  remainingCalledIngredients(recipe: RecipeDTO): number | undefined {
    return recipe.calledIngredients?.filter(t => !t.name).length;
  }

  showNewRecipeModal(template: TemplateRef<any>): void {
    this.newRecipeModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('inputRecipeName')?.focus(), 250);
  }

  newRecipeCancelled(): void {
    this.newRecipeModalRef.hide();
    this.newRecipeEditor = {};
  }

  addRecipe(): void {
    const recipe = {
      id: 0,
      userImport: this.newRecipeEditor.userImport,
      name: this.newRecipeEditor.name,
      serves: this.newRecipeEditor.serves,
      calledIngredients: []
    } as RecipeDTO;

    this.recipesService.createRecipe(this._cast(recipe, CreateRecipeCommand)).subscribe(
      result => {
        this.selectedRecipe = result;
        this.recipes?.push(recipe);
        this.newRecipeModalRef.hide();
        this.newRecipeEditor = {};
      },
      error => {
        this.newRecipeEditor.errorResponse = JSON.parse(error.response);
      }
    );
  }

  showRecipeOptionsModal(template: TemplateRef<any>) {
    if (this.selectedRecipe) {
      this.recipeOptionsEditor = {
        id: this.selectedRecipe.id,
        name: this.selectedRecipe.name,
        serves: this.selectedRecipe.serves
      };

      this.recipeOptionsModalRef = this.modalService.show(template);
    }
  }

  updateRecipeOptions() {
    let updateResponseSuccess = (result: any) => {
      if (this.recipes) {
        this.selectedRecipe = result;
        if (this.selectedRecipe) {
          for (var i = this.recipes.length - 1; i >= 0; i--) {
            if (this.recipes[i].id == this.selectedRecipe.id) {
              this.recipes[i] = this.selectedRecipe;
              break;
            }
          }
          this.recipeOptionsModalRef.hide();
          this.recipeOptionsEditor = {};
        }
      }
    }
    let updateResponseError = (error: any) => {
      this.recipeOptionsEditor.errorResponse = JSON.parse(error.response);
    }
    if (this.selectedRecipe) {
      if (this.selectedRecipe.name != this.recipeOptionsEditor.name) {
        this.recipesService.updateName(this.selectedRecipe.id, this._cast(this.recipeOptionsEditor, UpdateRecipeNameCommand)).subscribe(updateResponseSuccess, updateResponseError);
      }

      if (this.selectedRecipe.serves != this.recipeOptionsEditor.serves) {
        this.recipesService.updateServes(this.selectedRecipe.id, this._cast(this.recipeOptionsEditor, UpdateRecipeServesCommand)).subscribe(updateResponseSuccess, updateResponseError);
      }
    }
  }

  confirmDeleteRecipe(template: TemplateRef<any>) {
    this.recipeOptionsModalRef.hide();
    this.deleteRecipeModalRef = this.modalService.show(template);
  }

  deleteRecipeConfirmed(): void {
    if (this.selectedRecipe) {
      this.recipesService.deleteRecipe(this.selectedRecipe.id).subscribe(
        () => {
          this.deleteRecipeModalRef.hide();
          this.recipes = this.recipes?.filter(t => t.id !== this.selectedRecipe?.id);
          this.selectedRecipe = this.recipes?.length ? this.recipes[0] : undefined;
        },
        error => console.error(error)
      );
    }
  }

  // CalledIngredients

  isLowInventory(calledIngredient: CalledIngredientDTO | undefined): boolean {
    if (calledIngredient && calledIngredient.kitchenProduct && calledIngredient.kitchenProduct.amount && calledIngredient.amount) {
      return calledIngredient.kitchenProduct?.amount < calledIngredient.amount;
    }
    return true;
  }

  showCalledIngredientDetailsModal(template: TemplateRef<any>, calledIngredient: CalledIngredientDTO): void {
    this.recipesService.getCalledIngredientDetails(calledIngredient.id).subscribe(
      result => {
        this.selectedCalledIngredientDetails = result;
        if (this.selectedRecipe && this.selectedRecipe.calledIngredients) {
          for (var i = this.selectedRecipe.calledIngredients.length - 1; i >= 0; i--) {
            if (this.selectedRecipe.calledIngredients[i].id == this.selectedCalledIngredientDetails.id) {
              this.selectedRecipe.calledIngredients[i] = this.selectedCalledIngredientDetails;
              break;
            }
          }
          this.calledIngredientDetailsEditor = {
            ...this.selectedCalledIngredientDetails,
            search: this.selectedCalledIngredientDetails.name
          };

          this.calledIngredientDetailsModalRef = this.modalService.show(template);
        }
      },
      error => console.error(error)
    );
  }

  searchIngredientName(): void {
    this.recipesService.searchKitchenProductName(this.calledIngredientDetailsEditor.id, this.calledIngredientDetailsEditor.search).subscribe(
      result => {
        this.calledIngredientDetailsEditor.kitchenProductSearchItems = result.kitchenProductSearchItems;
      },
      error => {
        this.calledIngredientDetailsEditor.errorResponse = JSON.parse(error.response);
      }
    );
  }

  updateCalledIngredientDetails(): void {
    this.recipesService.updateCalledIngredientDetails(this.selectedCalledIngredientDetails?.id, this._cast(this.calledIngredientDetailsEditor, UpdateCalledIngredientDetailsCommand)).subscribe(
      result => {
        this.selectedCalledIngredientDetails = result;
        if (this.selectedRecipe && this.selectedRecipe.calledIngredients) {
          for (var i = this.selectedRecipe.calledIngredients.length - 1; i >= 0; i--) {
            if (this.selectedRecipe.calledIngredients[i].id == this.selectedCalledIngredientDetails.id) {
              this.selectedRecipe.calledIngredients[i] = this.selectedCalledIngredientDetails;
              break;
            }
          }

          this.calledIngredientDetailsModalRef.hide();
          this.calledIngredientDetailsEditor = {};
        }
      },
      error => console.error(error)
    );
  }

  addCalledIngredient() {
    if (this.kitchenUnitTypes) {
      const calledIngredient = {
        id: 0,
        name: '',
        kitchenUnitType: this.kitchenUnitTypes[0].value
      } as CalledIngredientDTO;
      if (this.selectedRecipe && this.selectedRecipe.calledIngredients) {
        this.selectedRecipe.calledIngredients.push(calledIngredient);
        const index = this.selectedRecipe.calledIngredients.length - 1;
        this.editCalledIngredient(calledIngredient, 'calledIngredientName' + index);
      }
    }
  }

  editCalledIngredient(calledIngredient: CalledIngredientDTO, inputId: string): void {
    this.selectedCalledIngredient = calledIngredient;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateCalledIngredient(calledIngredient: CalledIngredientDTO, pressedEnter: boolean = false): void {
              const isNewCalledIngredient = calledIngredient.id === 0;

    if (!calledIngredient.name?.trim()) {
      this.deleteCalledIngredient(calledIngredient);
      return;
    }

    if (calledIngredient.id === 0) {
      var createCalledIngredientCommand: CreateCalledIngredientCommand = {
        ...calledIngredient,
        recipeId: this.selectedRecipe?.id
      }
      this.recipesService
        .createCalledIngredient(createCalledIngredientCommand)
        .subscribe(
          result => {
            calledIngredient.id = result;
          },
          error => console.error(error)
        );
    } else {

      this.recipesService.updateCalledIngredient(calledIngredient.id, this._cast(calledIngredient, UpdateCalledIngredientCommand)).subscribe(
        () => console.log('Update succeeded.'),
        error => console.error(error)
      );
    }

    this.selectedCalledIngredient = undefined;
    this.selectedCalledIngredientDetails = undefined;

    if (isNewCalledIngredient && pressedEnter) {
      setTimeout(() => this.addCalledIngredient(), 250);
    }
  }

  deleteCalledIngredient(calledIngredient: CalledIngredientDTO | undefined) {
    if (calledIngredient && this.selectedRecipe && this.selectedRecipe.calledIngredients) {
      if (this.calledIngredientDetailsModalRef) {
        this.calledIngredientDetailsModalRef.hide();
      }

      if (calledIngredient.id === 0) {
        if (this.selectedCalledIngredient) {
          const calledIngredientIndex = this.selectedRecipe.calledIngredients.indexOf(this.selectedCalledIngredient);
          this.selectedRecipe.calledIngredients.splice(calledIngredientIndex, 1);
        }
      } else {
        this.recipesService.deleteRecipe(calledIngredient.id).subscribe(
          result => {
            if (this.selectedRecipe) {
              this.selectedRecipe.calledIngredients = this.selectedRecipe.calledIngredients?.filter(
                t => t.id !== calledIngredient.id
              );
            }
          },
          error => console.error(error)
        );
      }
    }
  }

  _cast<K extends T, T>(obj: K, tClass: { new(...args: any[]): K }): K {
    let returnObject: K = new tClass();
    for (let p in returnObject) {
      const value = obj[p] || undefined;
      if (value != undefined) {
        returnObject[p] = value;
      }
    }
    return returnObject;
  }
}
