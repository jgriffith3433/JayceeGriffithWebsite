import { Directive, ElementRef, Input, OnDestroy, OnInit, HostListener, AfterViewInit } from '@angular/core';
import { Renderer2, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { NgUnityWebglManagerService } from '../providers/ng-unity-webgl-manager.service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';

const EngineTrigger: any = require('../../../assets/js/game/EngineTrigger.js');
const unityLoader: any = require('../../../assets/js/game/unityloader.js');
import { environment } from '../../../environments/environment';


@Directive({
  selector: '[appNgUnityWebgl]'
})
export class NgUnityWebglDirective implements OnInit, AfterViewInit, OnDestroy {
  @Input() gameName: string;
  instance: any;
  hubConnection: HubConnection;

  constructor(
    private el: ElementRef,
    private renderer2: Renderer2,
    @Inject(DOCUMENT) private _document: any,
    private ngUnityWebglManagerService: NgUnityWebglManagerService
  ) {

  }

  public stopConnection() {
    this.hubConnection.start().then(() => {
      console.log("stopped");
    }).catch(err => console.log(err));
  }

  public sendCP(e: any, t: any) {
    //TODO: Read up about this
    //https://stackoverflow.com/questions/2176861/javascript-get-clipboard-data-on-paste-event-cross-browser
    e.preventDefault();
    var pastedText = undefined;
    //TODO: Can we get other data types and serialize them?
    if ((window as any)['clipboardData'] && (window as any)['clipboardData'].getData) {
      pastedText = (window as any)['clipboardData'].getData('Text');
    }
    else if (e.clipboardData && e.clipboardData.getData) {
      pastedText = e.clipboardData.getData('text/plain');
    }
    t.instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
      payloadType: 'BrowserPastedEventPayload',
      pastedText: pastedText
    }));
  }

  public clearCanvas() {
    var webGlVersion = this.instance.Module.SystemInfo.hasWebGL;
    if (webGlVersion == 1) {
      var webGlContext = this.el.nativeElement.getContext('webgl');
      if (webGlContext) {
        webGlContext.clearColor(0, 0, 0, 0)
        webGlContext.clear(webGlContext.COLOR_BUFFER_BIT);
      }
    }
    else {
      var webGlContext = this.el.nativeElement.getContext('webgl2');
      if (!webGlVersion) {
        webGlContext = this.el.nativeElement.getContext('experimental-webgl');
      }
      if (webGlContext) {
        webGlContext.clearColor(0, 0, 0, 0)
        webGlContext.clear(webGlContext.COLOR_BUFFER_BIT);
      }
    }
  }

  //@HostListener('click', ['$event']) onClick($event: any) {
  //  this.instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
  //    payloadType: 'BrowserInputSwitched',
  //    isUnity: true
  //  }));
  //}

  switchUI(elm: any): void {
    if (!this.instance) {
      return;
    }
    while (elm !== document.body && elm !== document) {
      switch (elm.tagName.toUpperCase()) {
        case "A":
        case "BUTTON":
        case "TEXTAREA":
          
          this.instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
            payloadType: 'BrowserInputSwitched',
            isUnity: false
          }));
          return;
      }
      elm = elm.parentNode;
    }
    //not captured by "ui" layer
    
    this.instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
      payloadType: 'BrowserInputSwitched',
      isUnity: true
    }));
  }

  ngOnInit() {

  }

  ngAfterViewInit() {
    this._document.addEventListener('mousedown', (e: any) => {
      this.switchUI(e.target);
    });
    this._document.addEventListener('touchstart', (e: any) => {
      this.switchUI(e.target);
    });

    

    EngineTrigger.on("clearCanvas", () => {
      this.clearCanvas();
    });
    function unityShowBanner(msg: any, type: any) {
      if (type == 'error') {
        console.log("error: " + msg);
      }
      else if (type == 'warning') {
        console.log("warning: " + msg);
      }
    }
    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
      // Mobile device style: fill the whole browser client area with the game canvas:

      //var meta = document.createElement('meta');
      //meta.name = 'viewport';
      //meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
      //document.getElementsByTagName('head')[0].appendChild(meta);
      //container.className = "unity-mobile";
      //canvas.className = "unity-mobile";

      // To lower canvas resolution on mobile devices to gain some
      // performance, uncomment the following line:
      // config.devicePixelRatio = 1;

      //unityShowBanner('WebGL builds are not supported on mobile devices.');
    } else {
      // Desktop style: Render the game canvas in a window that can be maximized to fullscreen:

      //canvas.style.width = "960px";
      //canvas.style.height = "600px";
    }
    var loadingBar = <HTMLElement>document.querySelector("#unity-loading-bar");
    var progressBarFull = <HTMLElement>document.querySelector("#unity-progress-bar-full");
    loadingBar.style.display = "block";

    unityLoader.createUnityInstance(this.el.nativeElement, {
      dataUrl: `/assets/games/${this.gameName}/${this.gameName}.data`,
      frameworkUrl: `/assets/games/${this.gameName}/${this.gameName}.framework.js`,
      codeUrl: `/assets/games/${this.gameName}/${this.gameName}.wasm`,
      streamingAssetsUrl: "StreamingAssets",
      companyName: "DefaultCompany",
      productName: "ProjectSnapWebgame",
      productVersion: "0.3",
      showBanner: unityShowBanner,
    }, (progress: any) => {
      progressBarFull.style.width = 100 * progress + "%";
    }).then((instance: any) => {
      loadingBar.style.display = "none";
      this.instance = instance;
      window.onbeforeunload = function (e) {
        /*instance.SendMessage("BrowserBridge", "ProcessPacket", JSON.stringify({
          payloadType: 'BrowserClosePayload',
          source: 'Window'
        }));*/
      };

      this.ngUnityWebglManagerService.setInstance(this.gameName, this.instance);
      this.instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
        payloadType: 'BrowserInitPayload',
        hubUrl: environment.production ? window.location.origin + environment.hubBaseUri : environment.hubBaseUri,
      }));
      var gamerTag = localStorage.getItem('gamerTag');
      if (gamerTag) {
        this.instance.SendMessage('BrowserBridge', 'SetGamerTag', gamerTag);
      }
      else {
        this.instance.SendMessage('BrowserBridge', 'SetGamerTag', 'User');
      }
      var t = this;
      document.onpaste = (e) => {
        this.sendCP(e, t)
      };
    }).catch((message: any) => {
      alert(message);
    });
  }

  ngOnDestroy(): void {
  }
}
