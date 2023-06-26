import { Component, ViewChild } from '@angular/core';
import { ChatWidgetComponent } from '../chat';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  @ViewChild('chatWidget') chatWidgetRef: ChatWidgetComponent;
  title = 'WOF';
  public theme = 'blue';

  public get chatVisible() {
    if (!this.chatWidgetRef) {
      return false;
    }
    return this.chatWidgetRef.visible;
  }

  public get chatFloating() {
    if (!this.chatWidgetRef) {
      return false;
    }
    return this.chatWidgetRef.chatStyle == 'floating';
  }

  public get chatMinimized() {
    if (!this.chatWidgetRef) {
      return true;
    }
    return this.chatWidgetRef.chatStyle == 'minimized';
  }

  public get chatDocked() {
    if (!this.chatWidgetRef) {
      return false;
    }
    return this.chatWidgetRef.chatStyle == 'docked';
  }
}
