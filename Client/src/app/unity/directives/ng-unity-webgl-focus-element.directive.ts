import { Directive, ElementRef, Input, HostListener } from '@angular/core';
import { NgUnityWebglManagerService } from '../providers/ng-unity-webgl-manager.service';

@Directive({
  selector: '[appNgUnityWebglFocusElement]'
})
export class NgUnityWebglFocusElementDirective {

  @Input() gameName: string;

  constructor(
    private el: ElementRef,
    private ngUnityWebglManagerService: NgUnityWebglManagerService
  ) {

  }
  @HostListener('focus')
  onFocus() {
    //var instance = this.ngUnityWebglManagerService.getInstance(this.gameName);
    //instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
    //  payloadType: 'BrowserInputSwitched',
    //  isUnity: false
    //}));
  }
  @HostListener('click')
  onClick() {
    //var instance = this.ngUnityWebglManagerService.getInstance(this.gameName);
    //instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
    //  payloadType: 'BrowserInputSwitched',
    //  isUnity: false
    //}));
  }

  @HostListener('focusout')
  onFocusout() {
    //var instance = this.ngUnityWebglManagerService.getInstance(this.gameName);
    //instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
    //  payloadType: 'BrowserInputSwitched',
    //  isUnity: true
    //}));
  }

  onClickCanvas(): void {
    //var instance = this.ngUnityWebglManagerService.getInstance(this.gameName);
    //instance.SendMessage('BrowserBridge', 'ProcessPacket', JSON.stringify({
    //  payloadType: 'BrowserInputSwitched',
    //  isUnity: true
    //}));
  }
}
