import { Component, TemplateRef, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CookedRecipeCalledIngredientDetailsDTO } from '../../../models/CookedRecipeCalledIngredientDetailsDTO';
import { CookedRecipeCalledIngredientDTO } from '../../../models/CookedRecipeCalledIngredientDTO';
import { CookedRecipeDTO } from '../../../models/CookedRecipeDTO';
import { CreateCookedRecipeCalledIngredientCommand } from '../../../models/CreateCookedRecipeCalledIngredientCommand';
import { CreateCookedRecipeCommand } from '../../../models/CreateCookedRecipeCommand';
import { RecipeDTO } from '../../../models/RecipeDTO';
import { RecipesOptionDTO } from '../../../models/RecipesOptionDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateCookedRecipeCalledIngredientCommand } from '../../../models/UpdateCookedRecipeCalledIngredientCommand';
import { UpdateCookedRecipeCalledIngredientDetailsCommand } from '../../../models/UpdateCookedRecipeCalledIngredientDetailsCommand';
import { CookedRecipesService } from '../../../providers/cooked-recipes.service';


@Component({
  selector: 'app-cooked-recipes-component',
  templateUrl: './cooked-recipes.component.html',
  styleUrls: ['./cooked-recipes.component.scss']
})
export class CookedRecipesComponent implements OnInit {
  debug = false;
  recipesOptions: RecipesOptionDTO[] | undefined;
  cookedRecipes: CookedRecipeDTO[] | undefined;
  kitchenUnitTypes: UnitTypeDTO[] | undefined;
  selectedCookedRecipe: CookedRecipeDTO | undefined;
  selectedCookedRecipeCalledIngredient: CookedRecipeCalledIngredientDetailsDTO | undefined;
  selectedCookedRecipeCalledIngredientDetails: CookedRecipeCalledIngredientDetailsDTO | undefined;
  newCookedRecipeEditor: any = {};
  cookedRecipeOptionsEditor: any = {};
  cookedRecipeCalledIngredientDetailsEditor: any = {};
  newCookedRecipeModalRef: BsModalRef;
  cookedRecipeOptionsModalRef: BsModalRef;
  deleteCookedRecipeModalRef: BsModalRef;
  cookedRecipeCalledIngredientDetailsModalRef: BsModalRef;


  constructor(
    private cookedRecipesService: CookedRecipesService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.cookedRecipesService.getAllCookedRecipes().subscribe(
      result => {
        this.cookedRecipes = result.cookedRecipes;
        this.kitchenUnitTypes = result.kitchenUnitTypes;
        this.recipesOptions = result.recipesOptions;
        if (this.cookedRecipes?.length) {
          this.selectedCookedRecipe = this.cookedRecipes[0];
        }
      },
      error => console.error(error)
    );
  }

  // CookedRecipes
  //remainingCalledIngredients(recipe: RecipeDto): number {
  //  return recipe.calledIngredients.filter(t => !t.name).length;
  //}

  showNewCookedRecipeModal(template: TemplateRef<any>): void {
    this.newCookedRecipeModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('inputCookedRecipeName')?.focus(), 250);
  }

  newCookedRecipeCancelled(): void {
    this.newCookedRecipeModalRef.hide();
    this.newCookedRecipeEditor = {};
  }

  addCookedRecipe(): void {
    this.cookedRecipesService.createCookedRecipe(this._cast(this.newCookedRecipeEditor, CreateCookedRecipeCommand)).subscribe(
      result => {
        this.selectedCookedRecipe = result;
        this.cookedRecipes?.push(this.selectedCookedRecipe);
        this.newCookedRecipeModalRef.hide();
        this.newCookedRecipeEditor = {};
      },
      error => {
        this.newCookedRecipeEditor.errorResponse = JSON.parse(error.response);
      }
    );
  }

  showCookedRecipeOptionsModal(template: TemplateRef<any>) {
    if (this.selectedCookedRecipe) {
      this.cookedRecipeOptionsEditor = {
        id: this.selectedCookedRecipe.id,
        recipeId: this.selectedCookedRecipe.recipeId
      };

      this.cookedRecipeOptionsModalRef = this.modalService.show(template);
    }
  }

  confirmDeleteCookedRecipe(template: TemplateRef<any>) {
    this.cookedRecipeOptionsModalRef.hide();
    this.deleteCookedRecipeModalRef = this.modalService.show(template);
  }

  deleteCookedRecipeConfirmed(): void {
    this.cookedRecipesService.deleteCookedRecipe(this.selectedCookedRecipe?.id).subscribe(
      result => {
        this.deleteCookedRecipeModalRef.hide();
        this.cookedRecipes = this.cookedRecipes?.filter(t => t.id !== this.selectedCookedRecipe?.id);
        this.selectedCookedRecipe = this.cookedRecipes?.length ? this.cookedRecipes[0] : undefined;
      },
      error => console.error(error)
    );
  }

  //CookedRecipeCalledIngredients

  showCookedRecipeCalledIngredientDetailsModal(template: TemplateRef<any>, cookedRecipeCalledIngredient: CookedRecipeCalledIngredientDTO): void {
    this.cookedRecipesService.getCookedRecipeCalledIngredientDetails(cookedRecipeCalledIngredient.id).subscribe(
      result => {
        if (this.selectedCookedRecipe && this.selectedCookedRecipe.cookedRecipeCalledIngredients) {
          this.selectedCookedRecipeCalledIngredientDetails = result;
          for (var i = this.selectedCookedRecipe.cookedRecipeCalledIngredients.length - 1; i >= 0; i--) {
            if (this.selectedCookedRecipe.cookedRecipeCalledIngredients[i].id == this.selectedCookedRecipeCalledIngredientDetails.id) {
              this.selectedCookedRecipe.cookedRecipeCalledIngredients[i] = this.selectedCookedRecipeCalledIngredientDetails;
              break;
            }
          }
          this.cookedRecipeCalledIngredientDetailsEditor = {
            ...this.selectedCookedRecipeCalledIngredientDetails,
            search: this.selectedCookedRecipeCalledIngredientDetails.name
          };

          this.cookedRecipeCalledIngredientDetailsModalRef = this.modalService.show(template);
        }
      },
      error => console.error(error)
    );
  }

  searchIngredientName(): void {
    this.cookedRecipesService.searchKitchenProductName(this.cookedRecipeCalledIngredientDetailsEditor.id, this.cookedRecipeCalledIngredientDetailsEditor.search).subscribe(
      result => {
        this.cookedRecipeCalledIngredientDetailsEditor.kitchenProductSearchItems = result.kitchenProductSearchItems;
        this.cookedRecipeCalledIngredientDetailsEditor.kitchenProductSearchItems.unshift({name: '', value: -1})
      },
      error => {
        this.cookedRecipeCalledIngredientDetailsEditor.errorResponse = JSON.parse(error.response);
      }
    );
  }

  updateCookedRecipeCalledIngredientDetails(): void {
    this.cookedRecipesService.updateCookedRecipeCalledIngredientDetails(this.selectedCookedRecipeCalledIngredientDetails?.id, this._cast(this.cookedRecipeCalledIngredientDetailsEditor, UpdateCookedRecipeCalledIngredientDetailsCommand)).subscribe(
      result => {
        this.selectedCookedRecipeCalledIngredientDetails = result;
        if (this.selectedCookedRecipe && this.selectedCookedRecipe.cookedRecipeCalledIngredients) {
          for (var i = this.selectedCookedRecipe.cookedRecipeCalledIngredients.length - 1; i >= 0; i--) {
            if (this.selectedCookedRecipe.cookedRecipeCalledIngredients[i].id == this.selectedCookedRecipeCalledIngredientDetails.id) {
              this.selectedCookedRecipe.cookedRecipeCalledIngredients[i] = this.selectedCookedRecipeCalledIngredientDetails;
              break;
            }
          }

          this.cookedRecipeCalledIngredientDetailsModalRef.hide();
          this.cookedRecipeCalledIngredientDetailsEditor = {};
        }
      },
      error => console.error(error)
    );
  }

  addCookedRecipeCalledIngredient() {
    if (this.kitchenUnitTypes && this.selectedCookedRecipe && this.selectedCookedRecipe.cookedRecipeCalledIngredients) {
      const cookedRecipeCalledIngredient = {
        id: 0,
        name: '',
        kitchenUnitType: this.kitchenUnitTypes[0].value,
        amount: 0,
        cookedRecipeId: this.selectedCookedRecipe.id,
      } as CookedRecipeCalledIngredientDetailsDTO;

      this.selectedCookedRecipe.cookedRecipeCalledIngredients.push(cookedRecipeCalledIngredient);
      const index = this.selectedCookedRecipe.cookedRecipeCalledIngredients.length - 1;
      this.editCookedRecipeCalledIngredient(cookedRecipeCalledIngredient, 'cookedRecipeCalledIngredientName' + index);
    }
  }

  editCookedRecipeCalledIngredient(cookedRecipeCalledIngredient: CookedRecipeCalledIngredientDetailsDTO, inputId: string): void {
    this.selectedCookedRecipeCalledIngredient = cookedRecipeCalledIngredient;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateCookedRecipeCalledIngredient(cookedRecipeCalledIngredient: CookedRecipeCalledIngredientDetailsDTO, pressedEnter: boolean = false): void {
    if (this.selectedCookedRecipe) {
      const isNewCalledIngredient = cookedRecipeCalledIngredient.id === 0;

      if (!cookedRecipeCalledIngredient.name?.trim()) {
        this.deleteCookedRecipeCalledIngredient(cookedRecipeCalledIngredient);
        return;
      }

      if (cookedRecipeCalledIngredient.id === 0) {
        var createCookedRecipeCalledIngredientCommand = this._cast(cookedRecipeCalledIngredient, CreateCookedRecipeCalledIngredientCommand);
        createCookedRecipeCalledIngredientCommand.cookedRecipeId = this.selectedCookedRecipe.id;

        this.cookedRecipesService.createCookedRecipeCalledIngredient(createCookedRecipeCalledIngredientCommand)
          .subscribe(
            result => {
              cookedRecipeCalledIngredient.id = result;
            },
            error => console.error(error)
          );
      } else {
        var updateCookedRecipeCalledIngredientCommand = this._cast(cookedRecipeCalledIngredient, UpdateCookedRecipeCalledIngredientCommand);
        this.cookedRecipesService.updateCookedRecipeCalledIngredient(cookedRecipeCalledIngredient.id, updateCookedRecipeCalledIngredientCommand).subscribe(
          () => console.log('Update succeeded.'),
          error => console.error(error)
        );
      }

      this.selectedCookedRecipeCalledIngredient = undefined;
      this.selectedCookedRecipeCalledIngredientDetails = undefined;

      if (isNewCalledIngredient && pressedEnter) {
        setTimeout(() => this.addCookedRecipeCalledIngredient(), 250);
      }
    }
  }

  deleteCookedRecipeCalledIngredient(cookedRecipeCalledIngredient: CookedRecipeCalledIngredientDTO | undefined) {
    if (cookedRecipeCalledIngredient && this.selectedCookedRecipe && this.selectedCookedRecipe.cookedRecipeCalledIngredients && this.selectedCookedRecipeCalledIngredient) {
      if (this.cookedRecipeCalledIngredientDetailsModalRef) {
        this.cookedRecipeCalledIngredientDetailsModalRef.hide();
      }

      if (cookedRecipeCalledIngredient.id === 0) {
        const cookedRecipeCalledIngredientIndex = this.selectedCookedRecipe.cookedRecipeCalledIngredients.indexOf(this.selectedCookedRecipeCalledIngredient);
        this.selectedCookedRecipe.cookedRecipeCalledIngredients.splice(cookedRecipeCalledIngredientIndex, 1);
      } else {
        this.cookedRecipesService.deleteCookedRecipeCalledIngredient(cookedRecipeCalledIngredient.id).subscribe(
          result => {
            if (this.selectedCookedRecipe && this.selectedCookedRecipe.cookedRecipeCalledIngredients) {
              this.selectedCookedRecipe.cookedRecipeCalledIngredients = this.selectedCookedRecipe.cookedRecipeCalledIngredients.filter(
                t => t.id !== cookedRecipeCalledIngredient.id
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
