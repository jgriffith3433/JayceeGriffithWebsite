import { Component, OnDestroy, OnInit } from '@angular/core';
import { ChatToggledService } from '../../../chat/providers/chat-toggled.service';
import { AuthService } from '../../providers/auth.service';
import { TokenService } from '../../providers/token.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit, OnDestroy {
  constructor(
    private authService: AuthService,
    private tokenService: TokenService,
    private chatToggledService: ChatToggledService,
  ) {
  }

  chatVisible: boolean = false;
  chatStyle: string = 'floating';
  isExpanded: boolean = false;
  isAuthentiacated: boolean = false;

  ngOnInit() {
    this.authService.authListener.subscribe(authorized => this.isAuthentiacated = authorized);
    this.isAuthentiacated = this.tokenService.IsAuthenticated;

    this.chatToggledService.chatStyleListener.subscribe(chatStyle => this.chatStyle = chatStyle);
    this.chatToggledService.chatToggledListener.subscribe(chatToggled => this.chatVisible = chatToggled);
    //this.chatToggled = this.tokenService.IsAuthenticated;
  }

  ngOnDestroy() {
    this.authService.authListener.unsubscribe();
    this.chatToggledService.chatStyleListener.unsubscribe();
    this.chatToggledService.chatToggledListener.unsubscribe();
  }

  get chatObscuringView() {
    return this.chatStyle == 'floating' || this.chatStyle == 'docked';
  }

  collapse() {
    this.isExpanded = false;
  }

  unCollapse() {
    this.isExpanded = true;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logout() {
    this.authService.logout();
  }
}
