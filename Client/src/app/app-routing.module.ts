import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './providers/auth.guard';
import { AddoreditComponent } from './components/pages/addoredit/addoredit.component';
import { HomeComponent } from './components/pages/home/home.component';
import { ItemsListComponent } from './components/pages/items-list/items-list.component';
import { TodoListsComponent } from './components/pages/todo-lists/todo-lists.component';
import { TodoListComponent } from './components/pages/todo-list/todo-list.component';
import { LoginComponent } from './components/pages/login/login.component';
import { ProductsComponent } from './components/pages/walmart-products/walmart-products.component';
import { CompletedOrdersComponent } from './components/pages/completed-orders/completed-orders.component';
import { KitchenProductsComponent } from './components/pages/kitchen-products/kitchen-products.component';
import { CalledIngredientsComponent } from './components/pages/called-ingredients/called-ingredients.component';
import { RecipesComponent } from './components/pages/recipes/recipes.component';
import { CookedRecipesComponent } from './components/pages/cooked-recipes/cooked-recipes.component';
import { PortfolioComponent } from './components/pages/portfolio/portfolio.component';
import { OrdersComponent } from './components/pages/orders/orders.component';

const routes: Routes = [
  {
    path: 'items'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: ItemsListComponent },
      { path: 'add', component: AddoreditComponent },
      { path: ':id', component: AddoreditComponent },
      { path: ':id/edit', component: AddoreditComponent }
    ]
  },
  {
    path: 'todo'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: TodoListsComponent },
      { path: 'add', component: TodoListComponent },
      { path: ':id', component: TodoListComponent },
      { path: ':id/edit', component: TodoListComponent },
    ]
  },
  {
    path: 'walmart-products'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: ProductsComponent },
    ]
  },
  {
    path: 'completed-orders'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: CompletedOrdersComponent },
    ]
  },
  {
    path: 'orders'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: OrdersComponent },
    ]
  },
  {
    path: 'kitchen-products'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: KitchenProductsComponent },
    ]
  },
  {
    path: 'called-ingredients'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: CalledIngredientsComponent },
    ]
  },
  {
    path: 'recipes'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: RecipesComponent },
    ]
  },
  {
    path: 'consumed-recipes'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: CookedRecipesComponent },
    ]
  },
  {
    path: 'portfolio'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: PortfolioComponent },
    ]
  },
  { path: 'login', component: LoginComponent },
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { onSameUrlNavigation: 'reload' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
