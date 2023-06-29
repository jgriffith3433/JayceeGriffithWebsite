import { AfterContentInit, AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ChatWidgetComponent } from '../chat';
import { NgUnityWebglManagerService } from './unity/providers/ng-unity-webgl-manager.service';
const EngineTrigger: any = require('../assets/js/game/EngineTrigger.js');
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('chatWidget') chatWidgetRef: ChatWidgetComponent;
  @ViewChild('gameWrapper') gameWrapperRef: ElementRef;
  @ViewChild('game') gameCanvasRef: ElementRef;
  @ViewChild('app') app: ElementRef;
  title = 'JC';
  public theme = 'blue';
  startGame: boolean = false;

  constructor(
    private el: ElementRef,
    private ngUnityWebglManagerService: NgUnityWebglManagerService
  ) {

  }

  ngOnInit(): void {
    EngineTrigger.on("browser_packet", (packet: any) => this.onReceivePacket(packet));
  }

  ngOnDestroy(): void {
    //EngineTrigger.off("browser_packet", this.onReceivePacket);
  }



  ngAfterViewInit(): void {
    for (var key in this.app.nativeElement) {
      if (key.search('on') === 0) {
        this.app.nativeElement.addEventListener(key.slice(2), (e: any) => {
          if (!this.gameCanvasRef) {
            return;
          }
          var clone = new e.constructor(e.type, e);
          this.gameCanvasRef.nativeElement.dispatchEvent(clone);
          //if (e.preventDefault) {
          //  e.preventDefault();
          //}
        });
      }
    }
    // all were doing here is making sure unity is loaded after the chat widget has grabbed it's audio context
    setTimeout(() => {
      this.chatWidgetRef.ensureAudioContextCreated().finally(() => {
        this.startGame = true;
      });
    }, 500);
  }

  onReceivePacket(packet: any) {
    switch (packet.payloadType) {
      case 'BrowserInputSwitched':
        this.receiveBrowserInputSwitchedPayload(packet.payload);
        break;
    }
  }

  receiveBrowserInputSwitchedPayload(payload: any) {
    this.switchCanvasUI(payload.isUnity);
  }

  switchCanvasUI(isUnity: boolean) {
    console.log("switchCanvasUI: " + isUnity);
    if (isUnity) {

    }
    else {

    }
  }

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

  onResized(event: any) {
    //this.width = event.newWidth;
    //this.height = event.newHeight;
  }
}
