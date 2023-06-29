import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './providers/auth.guard';
import { HomeComponent } from './components/pages/home/home.component';
import { LoginComponent } from './components/pages/login/login.component';
import { PortfolioComponent } from './components/pages/portfolio/portfolio.component';
import { HelpComponent } from './components/pages/help/help.component';

const routes: Routes = [
  {
    path: 'portfolio'/*, canActivate: [AuthGuard]*/, children: [
      { path: '', component: PortfolioComponent },
    ]
  },
  { path: 'login', component: LoginComponent },
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'help', component: HelpComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { onSameUrlNavigation: 'reload' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
