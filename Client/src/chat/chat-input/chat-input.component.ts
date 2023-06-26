import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, ViewEncapsulation } from '@angular/core'

@Component({
  selector: 'chat-input',
  template: `
    <textarea type="text" class="chat-input-text" placeholder="Type message..." #message (keydown.enter)="onSubmit()" (keyup.enter)="message.value = ''" (keyup.escape)="dismiss.emit()"></textarea>
    <canvas id="recordingVisualizer" class="visualizer" height="50" #recordingVisualizer></canvas>
    <canvas id="recordingFrequenciesVisualizer" class="visualizer" height="50" #recordingFrequenciesVisualizer></canvas>
    <canvas id="speechToTextVisualizer" class="visualizer" height="50" #speechToTextVisualizer></canvas>
    <!--<button type="submit" class="chat-input-submit" (click)="onSubmit()">
      {{buttonText}}
    </button>-->
  `,
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./chat-input.component.css'],
})
export class ChatInputComponent implements OnInit {
  @Input() public buttonText = '↩︎'
  @Input() public focus = new EventEmitter()
  @Output() public send = new EventEmitter()
  @Output() public dismiss = new EventEmitter()
  @ViewChild('recordingVisualizer') public recordingVisualizerRef: ElementRef;
  @ViewChild('recordingFrequenciesVisualizer') public recordingFrequenciesVisualizerRef: ElementRef;
  @ViewChild('speechToTextVisualizer') public textToSpeechVisualizerRef: ElementRef;
  @ViewChild('message', { static: true }) message: ElementRef

  ngOnInit() {
    this.focus.subscribe(() => this.focusMessage())
  }

  public focusMessage() {
    this.message.nativeElement.focus()
  }

  public getMessage() {
    return this.message.nativeElement.value
  }

  public toggleRecord() {

  }

  public setMessage(str: string) {
    this.message.nativeElement.value = str;
  }

  public clearMessage() {
    this.message.nativeElement.value = ''
  }

  onSubmit() {
    const message = this.getMessage()
    if (message.trim() === '') {
      return
    }
    this.send.emit({ message })
    this.clearMessage()
    this.focusMessage()
  }

}
