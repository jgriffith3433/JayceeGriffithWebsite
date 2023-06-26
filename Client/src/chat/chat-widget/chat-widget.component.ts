import { AfterViewInit, Component, ElementRef, HostListener, Input, OnDestroy, OnInit, ViewChild } from '@angular/core'
import { fadeIn, fadeInOut } from '../animations'
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, Subject, throwError as _observableThrow, of as _observableOf, Subscription, BehaviorSubject } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { ChatService } from '../providers/chat.service'

interface RecommendedVoices {
  [key: string]: boolean;
}

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

import { ChatMessageVm } from '../models/ChatMessageVm'
import { GetChatResponseQuery } from '../models/GetChatResponseQuery'
import { TokenService } from '../../app/providers/token.service';
import { PicoService } from '../providers/pico.service';
import { GetChatResponseVm } from '../models/GetChatResponseVm';
import { AuthService } from '../../app/providers/auth.service';
import { ChatInputComponent } from '../chat-input/chat-input.component';
import { HowDoIService } from '../providers/how-do-I.service';
import { ChatToggledService } from '../providers/chat-toggled.service';

//read online that maybe you want to use this instead?
//import { Blob } from 'buffer';


const rand = (max: number) => Math.floor(Math.random() * max)

@Component({
  selector: 'chat-widget',
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.css'],
  animations: [fadeInOut, fadeIn],
})
export class ChatWidgetComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('bottom') bottom: ElementRef;
  @ViewChild('audio') audioPlayerRef: ElementRef;
  @ViewChild('chatInput') chatInputRef: ChatInputComponent;
  @Input() public theme: 'blue' | 'grey' | 'red' = 'blue';
  public _visible = false;
  public _refreshing = false;
  public _botNavigating = false;
  public _chatStyle = 'floating';
  greeting: string = 'Kitchy';
  _previousScrollPosition = 0;
  _chatConversationId: number = -1;
  _forceFunctionCall: string | undefined = undefined;
  chatMessages: ChatMessageVm[] = [];
  private http: HttpClient;
  private baseUrl: string;
  protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;
  mediaRecorder: MediaRecorder | undefined;
  stoppingMediaRecorder: boolean = false;
  keepRecording: boolean = false;
  audioBlobsToSend: Blob[] = [];
  recordingVisualizerCanvasCtx: any;
  recordingFrequenciesVisualizerCanvasCtx: any;
  textToSpeechVisualizerCanvasCtx: any;
  audioContext: AudioContext;
  audioReady: boolean = false;
  ensureAudioContextPromise: Promise<AudioContext>;
  recordingBufferSource: AudioBufferSourceNode;
  recordingAudioStreamDestination: MediaStreamAudioDestinationNode;
  soundDetectorAudioStreamDestination: MediaStreamAudioDestinationNode;
  audioPlayer: HTMLAudioElement;
  textToSpeechAudioStreamSource: MediaElementAudioSourceNode;
  textToSpeechVisualAnalyser: AnalyserNode;
  textToSpeechVisualAnalyserDataArray: Uint8Array;
  textToSpeechVisualAnalyserBufferLength: number;
  textToSpeechVisualAnalyserAnimationFrameHandle: number | undefined;
  textToSpeechPlaying: boolean = false;
  synthvolume: number = 1;
  musicPlaying: boolean = false;
  finalRecordingAudioSourceNode: MediaStreamAudioSourceNode;
  recordingVisualAnalyserAudioStreamSource: MediaStreamAudioSourceNode;
  recordingVisualAnalyser: AnalyserNode;
  recordingVisualAnalyserDataArray: Uint8Array;
  recordingVisualAnalyserBufferLength: number;
  recordingVisualAnalyserAnimationFrameHandle: number | undefined;
  recordingVisualFrequenciesAnalyserAudioStreamSource: MediaStreamAudioSourceNode;
  recordingVisualFrequenciesAnalyser: AnalyserNode;
  recordingVisualFrequenciesAnalyserDataArray: Uint8Array;
  recordingVisualFrequenciesAnalyserBufferLength: number;
  recordingVisualFrequenciesAnalyserAnimationFrameHandle: number | undefined;
  recordingSoundAnalyserAudioStreamSource: MediaStreamAudioSourceNode;
  recordingCompressor: DynamicsCompressorNode;
  recordingHighPassCreateBiquadFilter: BiquadFilterNode;
  recordingBandPassCreateBiquadFilter: BiquadFilterNode;
  recordingSoundAnalyser: AnalyserNode;
  recordingSoundAnalyserDataArray: Uint8Array;
  recordingSoundAnalyserBufferLength: number;
  recordingSoundAnalyserAnimationFrameHandle: number | undefined;
  numberOfGainNodesToApply = 3;
  recordingAudioHeadGainNode: GainNode;
  recordingAudioGainNodes: GainNode[] = [];
  gainValue = 0;
  foundRoomVolume: boolean = false;
  foundRoomVolumeBug: boolean = false;
  sendingSpeech: boolean = false;
  sendingSpeechPromise: Promise<boolean>;
  chatTextFromSpeechToText: boolean = false;
  lastTimeSpeechSent = performance.now();
  speechToTextMessage: string | undefined = undefined;
  micVolumeOverTimeStart = performance.now();
  micVolumeOverTimeLowest: number = Number.POSITIVE_INFINITY;
  micVolumeOverTimeLowAverage: number = 0;
  micVolumeOverTimeLowCount = 0;
  micVolumeOverTimeLowTotal = 0;
  micVolumeOverTimeAverage: number = 0;
  micVolumeOverTimeAverageCount = 0;
  micVolumeOverTimeAverageTotal = 0;
  micVolumeOverTimeHighest: number = Number.NEGATIVE_INFINITY;
  micVolumeOverTimeHighAverage: number = 0;
  micVolumeOverTimeHighCount = 0;
  micVolumeOverTimeHighTotal = 0;


  SPEECH_TO_TEXT_INTERVAL_TIME = 5;
  MIC_VOLUME_DETECTION_TIME = 2;
  ROOM_DETECTION_VOLUME = 10;
  SPEECH_DETECTION_VOLUME = 200;
  SPEECH_DETECTION_ENV = 150;
  SPEECH_CLIPPING_VOLUME = 300;
  MIN_SILENCE_DURATION = 0.5;

  gainChanged(event: any) {
    this.gainValue = event.target.value;
  }

  MIN_DECIBELS = -35;
  badWordAndWakeWordRegex: RegExp = new RegExp("^bumble *bee*|[a@][s\$][s\$]$|[a@][s\$][s\$]h[o0][l1][e3][s\$]?|b[a@][s\$][t\+][a@]rd|b[e3][a@][s\$][t\+][i1][a@]?[l1]([i1][t\+]y)?|b[e3][a@][s\$][t\+][i1][l1][i1][t\+]y|b[e3][s\$][t\+][i1][a@][l1]([i1][t\+]y)?|b[i1][t\+]ch[s\$]?|b[i1][t\+]ch[e3]r[s\$]?|b[i1][t\+]ch[e3][s\$]|b[i1][t\+]ch[i1]ng?|b[l1][o0]wj[o0]b[s\$]?|c[l1][i1][t\+]|^(c|k|ck|q)[o0](c|k|ck|q)[s\$]?$|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[e3]d|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[e3]r|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[i1]ng|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[s\$]|^cum[s\$]?$|cumm??[e3]r|cumm?[i1]ngcock|(c|k|ck|q)um[s\$]h[o0][t\+]|(c|k|ck|q)un[i1][l1][i1]ngu[s\$]|(c|k|ck|q)un[i1][l1][l1][i1]ngu[s\$]|(c|k|ck|q)unn[i1][l1][i1]ngu[s\$]|(c|k|ck|q)un[t\+][s\$]?|(c|k|ck|q)un[t\+][l1][i1](c|k|ck|q)|(c|k|ck|q)un[t\+][l1][i1](c|k|ck|q)[e3]r|(c|k|ck|q)un[t\+][l1][i1](c|k|ck|q)[i1]ng|cyb[e3]r(ph|f)u(c|k|ck|q)|d[a@]mn|d[i1]ck|d[i1][l1]d[o0]|d[i1][l1]d[o0][s\$]|d[i1]n(c|k|ck|q)|d[i1]n(c|k|ck|q)[s\$]|[e3]j[a@]cu[l1]|(ph|f)[a@]g[s\$]?|(ph|f)[a@]gg[i1]ng|(ph|f)[a@]gg?[o0][t\+][s\$]?|(ph|f)[a@]gg[s\$]|(ph|f)[e3][l1][l1]?[a@][t\+][i1][o0]|(ph|f)u(c|k|ck|q)|(ph|f)u(c|k|ck|q)[s\$]?|g[a@]ngb[a@]ng[s\$]?|g[a@]ngb[a@]ng[e3]d|g[a@]y|h[o0]m?m[o0]|h[o0]rny|j[a@](c|k|ck|q)\-?[o0](ph|f)(ph|f)?|j[e3]rk\-?[o0](ph|f)(ph|f)?|j[i1][s\$z][s\$z]?m?|[ck][o0]ndum[s\$]?|mast(e|ur)b(8|ait|ate)|n+[i1]+[gq]+[e3]*r+[s\$]*|[o0]rg[a@][s\$][i1]m[s\$]?|[o0]rg[a@][s\$]m[s\$]?|p[e3]nn?[i1][s\$]|p[i1][s\$][s\$]|p[i1][s\$][s\$][o0](ph|f)(ph|f)|p[o0]rn|p[o0]rn[o0][s\$]?|p[o0]rn[o0]gr[a@]phy|pr[i1]ck[s\$]?|pu[s\$][s\$][i1][e3][s\$]|pu[s\$][s\$]y[s\$]?|[s\$][e3]x|[s\$]h[i1][t\+][s\$]?|[s\$][l1]u[t\+][s\$]?|[s\$]mu[t\+][s\$]?|[s\$]punk[s\$]?|[t\+]w[a@][t\+][s\$]?");
  //endSilenceDetected: boolean = false;
  public speechSynthesisOn: boolean = true;
  public normalConversation: boolean = false;
  public authenticated: boolean = false;
  public autoRecordOnChatToggle: boolean = false;
  audioSource = '';
  private currentWakeWord: string;
  private keywordSubscription: Subscription;
  private errorSubscription: Subscription;
  private authSubscription: Subscription;
  private howDoISubscription: Subscription;

  constructor(
    private chatService: ChatService,
    private router: Router,
    private tokenService: TokenService,
    private picoService: PicoService,
    private authService: AuthService,
    private howDoIService: HowDoIService,
    private chatToggleService: ChatToggledService,
    @Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    this.http = http;
    this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    this.router.events.forEach((event: any) => {
      // NavigationCancel
      // NavigationError
      // RoutesRecognized
      if (event instanceof NavigationStart) {
        if (!this._refreshing) {
          if (event.url.toLowerCase().indexOf('login') != -1) {
            this.greeting = 'Logging in.';
          }
          else {
            if (!this._botNavigating) {
              this.greeting = 'Navigating to the ' + (event.url.split('/').join(' ').trim() || 'home') + ' page.';
            }
          }
        }
      }
      if (event instanceof NavigationEnd) {
        this.greeting = 'Kitchy';
        if (this._refreshing) {
          this._refreshing = false;
          //window.scrollTo({ top: this._previousScrollPosition, behavior: 'instant' });
          setTimeout(() => {
            window.scrollTo({ top: this._previousScrollPosition, behavior: 'auto' });
          }, 1000);
        }
        else {
          //setTimeout(() => {
          //  this._chatConversationId = -1;
          //  if (event.url.toLowerCase().indexOf('login') == -1) {
          //    while (this.messages.length > 0) {
          //      this.messages.pop();
          //    }
          //    while (this.chatMessages.length > 0) {
          //      this.chatMessages.pop();
          //    }
          //    //this.greeting = 'How can I help you manage your ' + this.getCurrentPageName();
          //    this.greeting = 'Try saying: bumblebee';
          //  }
          //  if (this._botNavigating) {
          //    this._botNavigating = false;
          //  }
          //}, 500);
        }
      }
    });
    this.howDoISubscription = this.howDoIService.promise.subscribe(how => this.howDoICallback(how));
  }

  howDoICallback(howDoI: any) {
    if (!this.visible) {
      this.userToggleChat();
    }
    this._forceFunctionCall = howDoI.forceFunctionCall;
    this.sendMessage(howDoI.how, false);
  }

  public get visible() {
    return this._visible
  }

  @Input() public set visible(visible) {
    this._visible = visible
    this.chatToggleService.chatToggledListener.next(this._visible);
    if (this._visible) {
      setTimeout(() => {
        this.scrollToBottom()
        //this.focusMessage()
      }, 0)
    }
  }

  public get chatStyle(): string {
    return this._chatStyle;
  }

  @Input() public set chatStyle(chatStyle: string) {
    this._chatStyle = chatStyle;
    this.chatToggleService.chatStyleListener.next(this._chatStyle);
    if (this._visible) {
      setTimeout(() => {
        this.scrollToBottom()
        //this.focusMessage()
      }, 0)
    }
  }

  public get recording() {
    return this.mediaRecorder && this.mediaRecorder.state == "recording";
  }

  public get recordingPaused() {
    return this.mediaRecorder && this.mediaRecorder.state == "paused";
  }

  /*public focus = new Subject()*/

  public assistant = {
    name: 'assistant',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/women/${rand(100)}.jpg`,
  }

  public system = {
    name: 'system',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/women/${rand(100)}.jpg`,
  }

  public user = {
    name: 'user',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/men/${rand(100)}.jpg`,
  }

  public getAvatarForRole(role: string | undefined): string {
    if (role == 'user') {
      return this.user.avatar;
    }
    else if (role == 'system') {
      return this.system.avatar;
    }
    else if (role == 'function') {
      return this.system.avatar;
    }
    else if (role == 'assistant') {
      return this.assistant.avatar;
    }
    return "";
  }

  public addMessage(newChatMessage: ChatMessageVm) {
    this.chatMessages.push(newChatMessage);
  }

  public scrollToBottom() {
    if (this.bottom !== undefined) {
      this.bottom.nativeElement.scrollIntoView()
    }
  }

  ensureAudioContextCreated() {
    if (!this.ensureAudioContextPromise) {
      this.ensureAudioContextPromise = new Promise((resolve, reject) => {
        if (this.audioReady) {
          resolve(this.audioContext);
        }
        else {
          this.audioContext = new AudioContext(this.audioContextOptions);
          this.recordingBufferSource = this.audioContext.createBufferSource();
          this.recordingAudioStreamDestination = this.audioContext.createMediaStreamDestination();
          this.soundDetectorAudioStreamDestination = this.audioContext.createMediaStreamDestination();

          this.recordingAudioHeadGainNode = this.audioContext.createGain();
          for (let i = 0; i < this.numberOfGainNodesToApply; i++) {
            let thisGainNode = this.audioContext.createGain();
            if (this.recordingAudioGainNodes.length == 0) {
              this.recordingAudioHeadGainNode.connect(thisGainNode);
              this.recordingAudioGainNodes.push(thisGainNode);
            }
            else {
              this.recordingAudioGainNodes[i - 1].connect(thisGainNode);
              this.recordingAudioGainNodes.push(thisGainNode);
            }
          }

          this.recordingVisualAnalyserAudioStreamSource = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);
          this.recordingVisualAnalyser = this.audioContext.createAnalyser();
          this.recordingVisualAnalyser.fftSize = 2048;
          this.recordingVisualAnalyserBufferLength = this.recordingVisualAnalyser.frequencyBinCount;
          this.recordingVisualAnalyserDataArray = new Uint8Array(this.recordingVisualAnalyserBufferLength);
          this.recordingVisualAnalyserAudioStreamSource.connect(this.recordingVisualAnalyser);

          this.recordingVisualFrequenciesAnalyserAudioStreamSource = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);
          this.recordingVisualFrequenciesAnalyser = this.audioContext.createAnalyser();
          this.recordingVisualFrequenciesAnalyser.fftSize = 256;
          this.recordingVisualFrequenciesAnalyserBufferLength = this.recordingVisualFrequenciesAnalyser.frequencyBinCount;
          this.recordingVisualFrequenciesAnalyserDataArray = new Uint8Array(this.recordingVisualFrequenciesAnalyserBufferLength);
          this.recordingVisualFrequenciesAnalyserAudioStreamSource.connect(this.recordingVisualFrequenciesAnalyser);

          this.recordingSoundAnalyserAudioStreamSource = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);
          //this.recordingSoundAnalyserAudioStreamSource = this.audioContext.createMediaStreamSource(this.soundDetectorAudioStreamDestination.stream);
          this.recordingSoundAnalyser = this.audioContext.createAnalyser();
          this.recordingSoundAnalyser.fftSize = 256;
          //this.recordingSoundAnalyser.minDecibels = this.MIN_DECIBELS;
          this.recordingSoundAnalyserBufferLength = this.recordingSoundAnalyser.frequencyBinCount;
          this.recordingSoundAnalyserDataArray = new Uint8Array(this.recordingSoundAnalyserBufferLength);
          this.recordingSoundAnalyserAudioStreamSource.connect(this.recordingSoundAnalyser);


          //narrow band
          //https://en.wikipedia.org/wiki/Voice_frequency
          //	300–3,400
          //let q = 1550 / (3400 - 300);
          //this.recordingHighPassCreateBiquadFilter = new BiquadFilterNode(this.audioContext, {
          //  type: "highpass",
          //  //middle of narrow band
          //  frequency: 1550,
          //  //https://en.wikipedia.org/wiki/Q_factor
          //  Q: q,
          //  gain: -10,
          //} as BiquadFilterOptions);

          this.recordingBandPassCreateBiquadFilter = this.audioContext.createBiquadFilter();
          this.recordingBandPassCreateBiquadFilter.type = "bandpass";
          this.recordingBandPassCreateBiquadFilter.frequency.value = 1000;
          this.recordingBandPassCreateBiquadFilter.Q.value = 4;


          this.recordingHighPassCreateBiquadFilter = this.audioContext.createBiquadFilter();
          this.recordingHighPassCreateBiquadFilter.type = "highpass";
          this.recordingHighPassCreateBiquadFilter.frequency.value = 1000;
          this.recordingHighPassCreateBiquadFilter.Q.value = 20;

          this.recordingCompressor = this.audioContext.createDynamicsCompressor();

          this.recordingCompressor.threshold.setValueAtTime(-20, this.audioContext.currentTime);
          this.recordingCompressor.knee.setValueAtTime(0, this.audioContext.currentTime);
          this.recordingCompressor.ratio.setValueAtTime(20, this.audioContext.currentTime);
          this.recordingCompressor.attack.setValueAtTime(0, this.audioContext.currentTime);
          this.recordingCompressor.release.setValueAtTime(0.25, this.audioContext.currentTime);


          this.textToSpeechAudioStreamSource = this.audioContext.createMediaElementSource(this.audioPlayer);
          this.textToSpeechVisualAnalyser = this.audioContext.createAnalyser();
          this.textToSpeechVisualAnalyser.fftSize = 2048;
          this.textToSpeechVisualAnalyserBufferLength = this.textToSpeechVisualAnalyser.frequencyBinCount;
          this.textToSpeechVisualAnalyserDataArray = new Uint8Array(this.textToSpeechVisualAnalyserBufferLength);
          this.textToSpeechAudioStreamSource.connect(this.textToSpeechVisualAnalyser);


          this.source2 = this.textToSpeechAudioStreamSource;
          //this.source2 = this.audioContext.createMediaElementSource(this.audioPlayer);
          this.textToSpeechDestination = this.audioContext.createMediaStreamDestination();
          this.textToSpeechAudioGainNode = this.audioContext.createGain();
          this.source2.connect(this.textToSpeechAudioGainNode);
          this.textToSpeechAudioGainNode.connect(this.textToSpeechDestination);
          let mediaStream = this.audioContext.createMediaStreamSource(this.textToSpeechDestination.stream);
          mediaStream.connect(this.audioContext.destination);



          let hasMicrophone = false;
          navigator.mediaDevices.enumerateDevices().then((devices) => {
            devices.forEach((device) => {
              if (device.kind == 'audioinput') {
                hasMicrophone = true;
              }
            });
          }).catch(function (err) {
            console.log(err.name + ": " + err.message);
          }).then(() => {
            if (hasMicrophone) {
              navigator.mediaDevices.getUserMedia({ audio: true }).then(checkStream => {
                if (checkStream.getAudioTracks().length > 0) {
                  navigator.mediaDevices.getUserMedia(this.mediaStreamConstraints).then(stream => {
                    console.log('audio tracks: ' + stream.getAudioTracks().length);
                    if (stream.getAudioTracks().length > 0) {
                      let track = stream.getAudioTracks()[0];
                      let constraints = track.getConstraints();
                      console.log('Track Constraints:');
                      console.log(constraints);

                      let mediaTrackSettings = track.getSettings();
                      console.log('Track Settings:');
                      console.log(mediaTrackSettings);

                      this.source = this.audioContext.createMediaStreamSource(stream);
                      this.source.connect(this.recordingAudioHeadGainNode);
                      this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].connect(this.soundDetectorAudioStreamDestination);

                      //send straight to recording destination
                      //this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].connect(this.recordingAudioStreamDestination);
                      //or
                      //filter and compress
                      this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].connect(this.recordingHighPassCreateBiquadFilter);
                      this.recordingHighPassCreateBiquadFilter.connect(this.recordingCompressor.threshold);
                      this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].connect(this.recordingCompressor);
                      this.recordingCompressor.connect(this.recordingAudioStreamDestination);

                      if (this.recordingVisualAnalyserAnimationFrameHandle) {
                        window.cancelAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandle);
                        this.recordingVisualAnalyserAnimationFrameHandle = undefined;
                      }
                      this.recordingVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandler);

                      if (this.recordingVisualFrequenciesAnalyserAnimationFrameHandle) {
                        window.cancelAnimationFrame(this.recordingVisualFrequenciesAnalyserAnimationFrameHandle);
                        this.recordingVisualFrequenciesAnalyserAnimationFrameHandle = undefined;
                      }
                      this.recordingVisualFrequenciesAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingVisualFrequenciesAnalyserAnimationFrameHandler);

                      if (this.recordingSoundAnalyserAnimationFrameHandle) {
                        window.cancelAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandle);
                        this.recordingSoundAnalyserAnimationFrameHandle = undefined;
                      }
                      this.recordingSoundAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandler);

                      if (this.textToSpeechVisualAnalyserAnimationFrameHandle) {
                        window.cancelAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandle);
                        this.textToSpeechVisualAnalyserAnimationFrameHandle = undefined;
                      }
                      this.textToSpeechVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandler);



                      let waitAndCheckGain = () => {
                        if (this.foundRoomVolume) {
                          this.audioReady = true;
                          resolve(this.audioContext);
                        }
                        else {
                          setTimeout(waitAndCheckGain, 100);
                        }
                      };
                      waitAndCheckGain();
                    }
                  });
                }
              });
            }
          });
        }
      });
    }
    return this.ensureAudioContextPromise;
  }

  disconnectMicrophone() {
    //this.finalRecordingAudioSourceNode.disconnect(this.audioContext.destination);
    this.source.disconnect(this.recordingAudioHeadGainNode);
  }

  textToSpeechAudioGainNode: GainNode;
  //public focusMessage() {
  //  this.focus.next(true)
  //}

  ngAfterViewInit() {
    this.audioPlayer = this.audioPlayerRef.nativeElement as HTMLAudioElement;
    this.recordingVisualizerCanvasCtx = this.chatInputRef.recordingVisualizerRef.nativeElement.getContext("2d");
    this.textToSpeechVisualizerCanvasCtx = this.chatInputRef.textToSpeechVisualizerRef.nativeElement.getContext("2d");
    this.recordingFrequenciesVisualizerCanvasCtx = this.chatInputRef.recordingFrequenciesVisualizerRef.nativeElement.getContext("2d");

    let messageInput = this.chatInputRef.message.nativeElement as HTMLInputElement;

    messageInput.onkeyup = e => {
      if (this.recording) {
        this.stopRecording();
      }
    }

    messageInput.onclick = e => {
      if (this.recording) {
        this.stopRecording();
      }
    }

    messageInput.onpaste = e => {
      if (this.recording) {
        this.stopRecording();
      }
    }

    this.audioPlayer.crossOrigin = "anonymous";
    this.audioPlayer.oncanplaythrough = e => {
      this.textToSpeechPlaying = true;
      this.audioPlayer.play();
      if (this.textToSpeechVisualAnalyserAnimationFrameHandle) {
        window.cancelAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandle);
        this.textToSpeechVisualAnalyserAnimationFrameHandle = undefined;
      }
      this.textToSpeechVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandler);
    };
    this.audioPlayer.onended = e => {
      this.textToSpeechPlaying = false;
      if (this.chatTextFromSpeechToText && this.visible && !this.recording) {
        this.ensureAudioContextCreated().then(() => {
          if (this.sendingSpeech) {
            this.sendingSpeechPromise = this.sendingSpeechPromise.finally(() => this.record(false));
          }
          else {
            this.record(false);
          }
        });
      }
    }
    this.audioPlayer.onpause = e => {
      this.textToSpeechPlaying = false;
    }
  }
  mediaStreamConstraints: MediaStreamConstraints;
  audioContextOptions: AudioContextOptions;

  async ngOnInit() {
    navigator.mediaDevices.getUserMedia({ audio: true }).then(checkStream => {
      if (checkStream.getAudioTracks().length > 0) {
        let track = checkStream.getAudioTracks()[0];
        let capabilities = track.getCapabilities();
        console.log('Track Capabilities:');
        console.log(track.getCapabilities());

        this.mediaStreamConstraints = {
          audio: {
            noiseSuppression: false,
            suppressLocalAudioPlayback: true,
            advanced: [{
              noiseSuppression: false,
              sampleRate: capabilities.sampleRate?.max,
              sampleSize: capabilities.sampleSize?.max,
              suppressLocalAudioPlayback: true,
            }]
          }
        } as MediaStreamConstraints;

        this.audioContextOptions = {
          audio: {
            channelCount: 1,
            sampleRate: capabilities.sampleRate?.max,
            sampleSize: capabilities.sampleSize?.max,
            volume: 1,
          },
          noiseSuppression: false,
          sampleRate: capabilities.sampleRate?.max
        } as AudioContextOptions;
        this.ensureAudioContextCreated();
      }
    });

    window.onresize = () => {
      //this.visualizerRef.nativeElement.width = mainSection.offsetWidth;
    }

    this.authenticated = this.tokenService.IsAuthenticated;
    this.authSubscription = this.authService.authListener.subscribe(authenticated => {
      this.authenticated = authenticated;
      this.scrollToBottom();
      this.sendUnsentMessages();
    });
    this.keywordSubscription = this.picoService.keywordDetectionListener.subscribe(porcupineSubscription => {
      this.currentWakeWord = porcupineSubscription.label;
      if (this.textToSpeechPlaying) {
        this.audioPlayer.pause();
      }
      if (!this.visible) {
        this.toggleChat();
        if (!this.autoRecordOnChatToggle) {
          if (!this.recording) {
            this.record(true);
          }
        }
      }
      else {
        if (!this.recording) {
          this.ensureAudioContextCreated().then(() => {
            if (this.sendingSpeech) {
              this.sendingSpeechPromise = this.sendingSpeechPromise.finally(() => this.record(false));
            }
            else {
              this.record(false);
            }
          });
        }
      }
    });
    this.errorSubscription = this.picoService.errorListener.subscribe(error => {
      if (error) {
        console.error(error);
      }
    });
    this.picoService.autoStart = true;
    this.picoService.load();
  }

  ngOnDestroy(): void {
    this.howDoISubscription.unsubscribe();
    this.authSubscription.unsubscribe();
    this.keywordSubscription.unsubscribe();
    this.errorSubscription.unsubscribe();
    if (this.textToSpeechVisualAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandle);
      this.textToSpeechVisualAnalyserAnimationFrameHandle = undefined;
    }

    if (this.recordingVisualAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandle);
      this.recordingVisualAnalyserAnimationFrameHandle = undefined;
    }

    if (this.recordingVisualFrequenciesAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.recordingVisualFrequenciesAnalyserAnimationFrameHandle);
      this.recordingVisualFrequenciesAnalyserAnimationFrameHandle = undefined;
    }

    if (this.recordingSoundAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandle);
      this.recordingSoundAnalyserAnimationFrameHandle = undefined;
    }
  }

  //public detectRoomVolume() {
  //  this.ensureAudioContextCreated().then(ac => {
  //    this.gainValue = 0;
  //    this.soundDetectedLastSecond = false;
  //    this.source.connect(this.recordingAudioHeadGainNode);
  //    this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].connect(this.soundDetectorAudioStreamDestination);
  //    let increaseGainVolume = () => {
  //      if (this.soundDetectedLastSecond == false) {
  //        if (this.gainValue < 100) {
  //          this.gainValue += .05;
  //          setTimeout(increaseGainVolume, 50);
  //        }
  //      }
  //      else {
  //        console.log('room sound detected at: ' + this.gainValue);
  //        this.source.disconnect(this.recordingAudioHeadGainNode);
  //        this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].disconnect(this.soundDetectorAudioStreamDestination);
  //      }
  //    };
  //    increaseGainVolume();
  //  });
  //}

  //public detectSpeechVolume() {
  //  this.ensureAudioContextCreated().then(ac => {
  //    this.gainValue = 0;
  //    this.source.connect(this.recordingAudioHeadGainNode);
  //    this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].connect(this.soundDetectorAudioStreamDestination);
  //    let micVolumeHighCount = 0;
  //    let micVolumeHighTotal = 0;
  //    let checkMicLevel = () => {
  //      micVolumeHighTotal += this.micVolumeHigh;
  //      micVolumeHighCount++;
  //      if (micVolumeHighCount == 20) {
  //        let averageHigh = micVolumeHighTotal / micVolumeHighCount;
  //        console.log('averageHigh: ' + averageHigh);
  //        if (averageHigh >= this.SPEECH_DETECTION_VOLUME) {
  //          console.log('Speech sound detected at: ' + this.gainValue);
  //          this.source.disconnect(this.recordingAudioHeadGainNode);
  //          this.recordingAudioGainNodes[this.recordingAudioGainNodes.length - 1].disconnect(this.soundDetectorAudioStreamDestination);
  //        }
  //        else {
  //          if (this.gainValue < 100) {
  //            this.gainValue += 5;
  //            micVolumeHighTotal = 0;
  //            micVolumeHighCount = 0;
  //            alert('Two blue fish swam in the tank.');
  //            setTimeout(checkMicLevel, 100);
  //          }
  //          else {
  //            console.log('No speech sound detected.');
  //          }
  //        }
  //      }
  //      else {
  //        setTimeout(checkMicLevel, 100);
  //      }
  //    }
  //    checkMicLevel();
  //  });
  //}

  //public detectRoomAndSpeechVolume() {
  //  alert('todo');
  //}

  public userToggleChat() {
    this.normalConversation = (window as any).normalConversation;
    this.toggleChat();
  }

  public userToggleFloatingChat() {
    this.chatStyle = this.chatStyle == 'floating' ? 'docked' : 'floating';
  }

  public userToggleMinimizedChat() {
    if (this.chatStyle == 'minimized') {
      this.visible = false;
    }
    else {
      this.chatStyle = 'minimized';
    }
  }

  private toggleChat() {
    if (!this.tokenService.IsAuthenticated) {
      this.router.navigate(['login']);
      return;
    }
    this.visible = !this.visible;
    if (this.visible) {
      if (this.tokenService.IsPicoAuthenticated) {
        if (!this.picoService.isLoaded) {
          this.picoService.autoStart = true;
          this.picoService.load();
        }
        else {
          if (!this.picoService.isListening) {
            this.picoService.start();
          }
        }
      }
      if (this.autoRecordOnChatToggle && !this.recording) {
        this.ensureAudioContextCreated().then(() => {
          if (this.sendingSpeech) {
            this.sendingSpeechPromise = this.sendingSpeechPromise.finally(() => this.record(false));
          }
          else {
            this.record(false);
          }
        });
      }
    }
    else {
      if (this.recording) {
        this.stopRecording();
      }
    }
  }

  getCurrentPageName() {
    return this.router.url.split('/').join(' ').trim() || 'home';
  }

  public userSendMessage({ message }: any) {
    this._forceFunctionCall = "auto";
    this.sendMessage(message, false);
  }

  public sendMessage(message: string, textFromSpeechToText: boolean) {
    if (message.trim() === '') {
      return;
    }

    this.chatTextFromSpeechToText = textFromSpeechToText;
    var to = this.assistant.name;
    var previousMessage: ChatMessageVm | undefined;
    if (this.chatMessages.length > 0) {
      previousMessage = this.chatMessages[this.chatMessages.length - 1];
    }
    if (previousMessage && previousMessage.from == this.system.name && previousMessage.to == this.user.name) {
      to = this.system.name;
    }
    this.addMessage({
      content: message,
      rawContent: message,
      from: this.user.name,
      name: this.user.name,
      to: this.assistant.name,
      received: false,
    } as ChatMessageVm);

    this.scrollToBottom();
    this.sendUnsentMessages();
  }

  sendUnsentMessages() {
    let unsentMessages = false;
    var overloaded = false;
    var maxTokens = false;
    for (var i = 0; i < this.chatMessages.length; i++) {
      if (!this.chatMessages[i].received) {
        unsentMessages = true;
      }
      if (i == this.chatMessages.length - 1) {
        if (this.chatMessages[i].content && this.chatMessages[i].content?.indexOf('That model is currently overloaded with other requests.') != -1) {
          overloaded = true;
          if (this.chatMessages.length >= i) {
            this.chatMessages[i - 1].received = false;
          }
          unsentMessages = true;
        }
        if (this.chatMessages[i].content && this.chatMessages[i].content?.indexOf("This model's maximum context length is ") != -1) {
          maxTokens = true;
          if (this.chatMessages.length >= i) {
            this.chatMessages[i - 1].received = false;
          }
          unsentMessages = true;
        }
      }
    }
    if (overloaded) {
      this.chatMessages.pop();
    }
    if (maxTokens) {
      var lengthOfChatMessages = this.chatMessages.length;
      for (var i = 0; i < lengthOfChatMessages / 2; i++) {
        this.chatMessages.shift();
      }
    }
    if (!unsentMessages) {
      return;
    }
    let query: GetChatResponseQuery = {
      chatMessages: this.chatMessages,
      chatConversationId: this._chatConversationId,
      forceFunctionCall: this._forceFunctionCall,
      currentUrl: this.getCurrentPageName(),
    };

    this.chatService.getChatResponse(this.normalConversation, query).subscribe(
      result => this.receiveChatResponse(result, true),
      error => {
        setTimeout(() => {
          if (this.getCurrentPageName() != 'login') {
            if (error.errors) {
              for (let e in error.errors) {
                for (let i in error.errors[e]) {
                  this.addMessage({
                    content: error.errors[e][i],
                    rawContent: error.errors[e][i],
                    from: this.system.name,
                    name: this.system.name,
                    to: this.user.name,
                    received: true
                  } as ChatMessageVm);
                }
                console.error(error.errors[e]);
              }
            }
            else if (error.message) {
              this.addMessage({
                content: error.message,
                rawContent: error.message,
                from: this.system.name,
                name: this.system.name,
                to: this.user.name,
                received: true
              } as ChatMessageVm);
            }
            this.scrollToBottom();
          }
        }, 500);
      }
    );
  }

  getSystemGreeting() {
    return this.greeting;
  }

  public get debug() {
    return (window as any).debug;
  }

  receiveChatResponse(response: GetChatResponseVm, speak: boolean) {
    let newChatMessages: ChatMessageVm[] = [];
    if (response.chatMessages) {
      //get new messages
      for (let i = this.chatMessages.length; i < response.chatMessages.length; i++) {
        if (response.chatMessages[i].to == this.user.name) {
          response.chatMessages[i].received = true;
        }
        newChatMessages.push(response.chatMessages[i]);
      }
    }
    if (newChatMessages.length > 0) {
      let mostRecentMessage = newChatMessages[newChatMessages.length - 1];

      if (this.speechSynthesisOn && speak && !this.recording) {
        let textToSpeak = "";

        if (mostRecentMessage.content && mostRecentMessage.from == this.assistant.name) {
          textToSpeak = mostRecentMessage.content;
        }

        if (textToSpeak && textToSpeak.indexOf('Rate limit reached') == -1 && textToSpeak.indexOf('That model is currently overloaded with other requests.') == -1) {
          this.chatService.getChatTextToSpeech(textToSpeak).subscribe(result => {
            this.audioSource = result;
          });
        }
      }
    }
    if (response.createNewChat) {
      if (response.error) {
        setTimeout(() => {
          this._chatConversationId = -1;
          this._forceFunctionCall = undefined;
          while (this.chatMessages.length > 0) {
            this.chatMessages.pop();
          }
          this.router.navigateByUrl(this.router.url);
        }, 2000);
      }
      else {
        this._botNavigating = true;
        for (let newChatMessage of newChatMessages) {
          this.addMessage(newChatMessage);
        }
        this.scrollToBottom();
        setTimeout(() => {
          this._chatConversationId = -1;
          this._forceFunctionCall = undefined;
          while (this.chatMessages.length > 0) {
            this.chatMessages.pop();
          }
          this.router.navigateByUrl(response.navigateToPage || '/');
        }, 2000);
      }
    }
    else {
      this._chatConversationId = response.chatConversationId || -1;
      this._forceFunctionCall = response.forceFunctionCall || undefined;
      if (response.chatMessages) {
        this.chatMessages = response.chatMessages;
      }
      this.scrollToBottom();
      if (response.navigateToPage) {
        this.router.navigateByUrl(response.navigateToPage);
      }
      else if (response.dirty) {
        this._refreshing = true;
        this._previousScrollPosition = window.scrollY || document.getElementsByTagName("html")[0].scrollTop;
        this.router.navigateByUrl(this.router.url);
      }
    }
    this.sendUnsentMessages();
  }

  @HostListener('document:keypress', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === '/') {
      //this.focusMessage()
    }
    if (event.key === '?' && !this._visible) {
      this.toggleChat();
    }
  }

  toggleSpeechSynthesis() {
    this.speechSynthesisOn = !this.speechSynthesisOn;
  }

  toggleNormalConversation() {
    this.normalConversation = !this.normalConversation;
  }

  _addVoicesList = (voices: any[]) => {
    const list = window.document.createElement("div");
    let html =
      '<h2>Available Voices</h2><select id="languages"><option value="">autodetect language</option>';
    voices.forEach(voice => {
      html += `<option value="${voice.lang}" data-name="${voice.name}">${voice.name
        } (${voice.lang})</option>`;
    });
    list.innerHTML = html;
    window.document.body.appendChild(list);
  };

  stopRecording() {
    if (this.mediaRecorder) {
      this.stoppingMediaRecorder = true;
      this.mediaRecorder.stop();
    }
  }

  source: MediaStreamAudioSourceNode;
  source2: MediaElementAudioSourceNode;
  textToSpeechDestination: MediaStreamAudioDestinationNode;

  userToggleRecord() {
    if (this.recording) {
      this.audioBlobsToSend = [];
      this.stopRecording();
    }
    else {
      this.ensureAudioContextCreated().then(() => this.record());
    }
  }

  record(respond: boolean = false) {
    if (this.recording) {
      console.log("Cannot record: recording already in progress");
      return;
    }
    if (this.mediaRecorder) {
      console.log("Cannot record: media recorder not cleaned up");
      return;
    }
    if (this.sendingSpeech) {
      console.log("Cannot record: still sending speech");
      return;
    }


    if (respond && this.chatMessages.length == 0) {
      var chatResponse = {
        chatConversationId: this._chatConversationId,
        forceFunctionCall: this._forceFunctionCall,
        chatMessages: [...this.chatMessages, {
          content: 'How can I help you manage your ' + this.getCurrentPageName(),
          rawContent: 'How can I help you manage your ' + this.getCurrentPageName(),
          from: this.assistant.name,
          name: this.assistant.name,
          to: this.user.name,
          received: true
        }],
        createNewChat: false,
        dirty: false,
        error: false,
        navigateToPage: undefined
      } as GetChatResponseVm;
      this.receiveChatResponse(chatResponse, false);
    }
    if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
      console.log("This browser does not support the API yet");
    }

    console.log('recording audio tracks: ' + this.recordingAudioStreamDestination.stream.getAudioTracks().length);
    if (this.recordingAudioStreamDestination.stream.getAudioTracks().length > 0) {
      let recordingTrack = this.recordingAudioStreamDestination.stream.getAudioTracks()[0];

      this.finalRecordingAudioSourceNode = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);

      this.mediaRecorder = new MediaRecorder(this.finalRecordingAudioSourceNode.mediaStream);

      this.mediaRecorder.ondataavailable = (event: { data: Blob; }) => {
        if (this.stoppingMediaRecorder) {
          return;
        }
        if (this.mediaRecorder) {
          this.audioBlobsToSend.push(event.data);
          // wait for more data after beginning of file
          if (this.audioBlobsToSend.length > 1) {
            if (this.sendingSpeech) {
              this.lastTimeSpeechSent = performance.now();
            }
            else {
              if (performance.now() - this.lastTimeSpeechSent >= this.SPEECH_TO_TEXT_INTERVAL_TIME && !this.sendingSpeech) {
                this.sendSpeech(true, false, false);
              }
            }
          }
        }
      };

      this.mediaRecorder.onerror = e => {
        if (this.mediaRecorder) {
          this.mediaRecorder.ondataavailable = null;
          this.mediaRecorder = undefined;
        }
      }

      this.mediaRecorder.onstart = e => {
        console.log("recording");
        if (this.textToSpeechPlaying) {
          this.audioPlayer.pause();
        }
      }

      this.mediaRecorder.onstop = e => {
        console.log("Recording ended.");
        this.audioBlobsToSend = [];
        if (this.mediaRecorder) {
          this.mediaRecorder.ondataavailable = null;
          this.mediaRecorder.onerror = null;
          this.mediaRecorder.onstart = null;
          this.mediaRecorder.onstop = null;
          this.mediaRecorder = undefined;
        }
        this.stoppingMediaRecorder = false;
      };

      this.mediaRecorder.start(2000);
    }
  }

  textToSpeechVisualAnalyserAnimationFrameHandler = () => {
    this.visualizeSound(this.textToSpeechVisualAnalyser, this.textToSpeechVisualAnalyserDataArray, this.textToSpeechVisualAnalyserBufferLength, this.chatInputRef.textToSpeechVisualizerRef, this.textToSpeechVisualizerCanvasCtx);
    if (this.recordingAudioHeadGainNode) {
      this.unduckRecordingAudio(this.textToSpeechVisualAnalyser, this.textToSpeechVisualAnalyserDataArray, this.textToSpeechVisualAnalyserBufferLength);
    }
    this.textToSpeechVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandler);
  }

  unduckRecordingAudio(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number) {
    visualAnalyser.getByteTimeDomainData(visualAnalyserDataArray);

    let sliceWidth = visualAnalyserBufferLength / 100;
    let x = 0;
    let heighestV = 0;
    for (let i = 0; i < visualAnalyserBufferLength; i++) {
      let v = visualAnalyserDataArray[i] / 128.0;
      if (v > heighestV) {
        heighestV = v;
      }
      x += sliceWidth;
    }
    let audioGain = heighestV;
    audioGain = audioGain - 1;
    if (audioGain > 1) {
      audioGain = 1;
    }
    if (audioGain < 0) {
      audioGain = 0;
    }
    //TODO: don't wanna blow my ears out
    return;
    if (1 + (audioGain * 2.5) > this.recordingAudioHeadGainNode.gain.value) {
      this.recordingAudioHeadGainNode.gain.value += .1;
      if (this.recordingAudioHeadGainNode.gain.value > 3) {
        this.recordingAudioHeadGainNode.gain.value = 3;
      }
    }
    else if (1 + (audioGain * 2.5) < this.recordingAudioHeadGainNode.gain.value) {
      this.recordingAudioHeadGainNode.gain.value -= .005;
      if (this.recordingAudioHeadGainNode.gain.value < 1) {
        this.recordingAudioHeadGainNode.gain.value = 1;
      }
    }
    for (var i = 0; i < this.recordingAudioGainNodes.length; i++) {
      this.recordingAudioGainNodes[i].gain.value = this.recordingAudioHeadGainNode.gain.value;
    }
  }

  recordingVisualAnalyserAnimationFrameHandler = () => {
    this.visualizeSound(this.recordingVisualAnalyser, this.recordingVisualAnalyserDataArray, this.recordingVisualAnalyserBufferLength, this.chatInputRef.recordingVisualizerRef, this.recordingVisualizerCanvasCtx);
    this.duckTextToSpeechAudio(this.recordingVisualAnalyser, this.recordingVisualAnalyserDataArray, this.recordingVisualAnalyserBufferLength);
    this.recordingVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandler);
  }

  recordingVisualFrequenciesAnalyserAnimationFrameHandler = () => {
    this.visualizeSoundFrequencies(this.recordingVisualFrequenciesAnalyser, this.recordingVisualFrequenciesAnalyserDataArray, this.recordingVisualFrequenciesAnalyserBufferLength, this.chatInputRef.recordingFrequenciesVisualizerRef, this.recordingFrequenciesVisualizerCanvasCtx);
    this.recordingVisualFrequenciesAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingVisualFrequenciesAnalyserAnimationFrameHandler);
  }

  duckTextToSpeechAudio(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number) {
    visualAnalyser.getByteTimeDomainData(visualAnalyserDataArray);

    let sliceWidth = visualAnalyserBufferLength / 100;
    let x = 0;
    let highestV = 0;
    for (let i = 0; i < visualAnalyserBufferLength; i++) {
      let v = visualAnalyserDataArray[i] / 128.0;
      if (v > highestV) {
        highestV = v;
      }
      x += sliceWidth;
    }
    let audioGain = highestV;
    audioGain = audioGain - 1;
    if (audioGain > 1) {
      audioGain = 1;
    }
    if (audioGain < 0) {
      audioGain = 0;
    }
    if (1 - (audioGain * 25) > this.textToSpeechAudioGainNode.gain.value) {
      this.textToSpeechAudioGainNode.gain.value += .0005;
      if (this.textToSpeechAudioGainNode.gain.value > 1) {
        this.textToSpeechAudioGainNode.gain.value = 1;
      }
    }
    else if (1 - (audioGain * 25) < this.textToSpeechAudioGainNode.gain.value) {
      this.textToSpeechAudioGainNode.gain.value -= .05;
      if (this.textToSpeechAudioGainNode.gain.value < .3) {
        this.textToSpeechAudioGainNode.gain.value = .3;
      }
    }
    this.synthvolume = this.textToSpeechAudioGainNode.gain.value;
  }

  visualizeSound(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number, visualizerRef: ElementRef, canvasCtx: any) {
    const WIDTH = visualizerRef.nativeElement.width
    const HEIGHT = visualizerRef.nativeElement.height;

    visualAnalyser.getByteTimeDomainData(visualAnalyserDataArray);

    canvasCtx.fillStyle = 'rgb(25, 118, 210)';
    canvasCtx.fillRect(0, 0, WIDTH, HEIGHT);
    canvasCtx.lineWidth = 2;
    canvasCtx.strokeStyle = 'rgb(0, 0, 0)';
    canvasCtx.beginPath();

    let sliceWidth = WIDTH * 1.0 / visualAnalyserBufferLength;
    let x = 0;

    for (let i = 0; i < visualAnalyserBufferLength; i++) {

      let v = visualAnalyserDataArray[i] / 128.0;
      let y = v * HEIGHT / 2;

      if (i === 0) {
        canvasCtx.moveTo(x, y);
      } else {
        canvasCtx.lineTo(x, y);
      }

      x += sliceWidth;
    }

    canvasCtx.lineTo(visualizerRef.nativeElement.width, visualizerRef.nativeElement.height / 2);
    canvasCtx.stroke();
  }

  visualizeSoundFrequencies(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number, visualizerRef: ElementRef, canvasCtx: any) {
    const WIDTH = visualizerRef.nativeElement.width
    const HEIGHT = visualizerRef.nativeElement.height;

    visualAnalyser.getByteFrequencyData(visualAnalyserDataArray);

    canvasCtx.fillStyle = 'rgb(25, 118, 210)';
    canvasCtx.fillRect(0, 0, WIDTH, HEIGHT);
    canvasCtx.lineWidth = 2;
    canvasCtx.strokeStyle = 'rgb(0, 0, 0)';
    canvasCtx.beginPath();
    canvasCtx.globalAlpha = 0.1;

    const barWidth = (WIDTH / visualAnalyserBufferLength) * 2.5;
    let barHeight;
    let x = 0;

    for (let i = 0; i < visualAnalyserBufferLength; i++) {
      barHeight = visualAnalyserDataArray[i] / 4;

      canvasCtx.fillStyle = `rgb(${barHeight + 100}, 50, 50)`;
      canvasCtx.fillRect(x, HEIGHT - barHeight / 2, barWidth, barHeight);

      x += barWidth + 1;
    }
  }

  recordingSoundAnalyserAnimationFrameHandler = () => {
    this.detectSound(this.recordingSoundAnalyser, this.recordingSoundAnalyserDataArray, this.recordingSoundAnalyserBufferLength);
    this.recordingSoundAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandler);
  }

  detectSound(soundAnalyser: AnalyserNode, soundDataArray: Uint8Array, soundAnalyserBufferLength: number) {
    //if (!this.recording) {
    //  return;
    //}
    //console.log('compression: ' + this.recordingCompressor.reduction);
    //https://stackoverflow.com/questions/24083349/understanding-getbytetimedomaindata-and-getbytefrequencydata-in-web-audio
    //https://decibelpro.app/blog/how-many-decibels-does-a-human-speak-normally/

    soundAnalyser.getByteFrequencyData(soundDataArray);

    //https://developer.mozilla.org/en-US/docs/Web/API/AnalyserNode/getFloatFrequencyData
    //0 - 24000 hz
    //narrow band
    //https://en.wikipedia.org/wiki/Voice_frequency
    //300–3,400
    let minFreq = 300;
    let maxFreq = 3400;
    let currentFreq = 0;
    let freqPerDataPoint = 24000 / soundAnalyserBufferLength;
    let numOfDataPointsProcessed = 0;

    let sum = 1;
    for (let i = 0; i < soundAnalyserBufferLength; i++) {
      currentFreq = i * freqPerDataPoint;
      if (currentFreq >= minFreq && currentFreq <= maxFreq) {
        let amplitude = soundDataArray[i];
        sum += amplitude;
        numOfDataPointsProcessed++;
      }
    }
    let micVolumeAverage = sum / numOfDataPointsProcessed;

    let micVolumeLowest = Number.POSITIVE_INFINITY;
    let micVolumeHighest = Number.NEGATIVE_INFINITY;
    for (const amplitude of soundDataArray) {
      if (amplitude < micVolumeLowest) {
        micVolumeLowest = amplitude;
      }
      if (amplitude > micVolumeHighest) {
        micVolumeHighest = amplitude;
      }
    }

    this.micVolumeOverTimeLowCount++;
    this.micVolumeOverTimeLowTotal += micVolumeLowest;
    this.micVolumeOverTimeLowAverage = this.micVolumeOverTimeLowTotal / this.micVolumeOverTimeLowCount;

    this.micVolumeOverTimeAverageCount++;
    this.micVolumeOverTimeAverageTotal += micVolumeAverage;
    this.micVolumeOverTimeAverage = this.micVolumeOverTimeAverageTotal / this.micVolumeOverTimeAverageCount;

    this.micVolumeOverTimeHighCount++;
    this.micVolumeOverTimeHighTotal += micVolumeHighest;
    this.micVolumeOverTimeHighAverage = this.micVolumeOverTimeHighTotal / this.micVolumeOverTimeHighCount;

    if (micVolumeHighest > this.micVolumeOverTimeHighest) {
      this.micVolumeOverTimeHighest = micVolumeHighest;
    }

    if (micVolumeLowest < this.micVolumeOverTimeLowest) {
      this.micVolumeOverTimeLowest = micVolumeLowest;
    }

    if (this.micVolumeOverTimeHighAverage < this.SPEECH_DETECTION_VOLUME && !this.foundRoomVolumeBug) {
      if (this.foundRoomVolume && this.recording) {
        //lets not adjust the gain while we are recording
        return;
      }
      this.gainValue += .5;
      if (this.gainValue > 100) {
        this.gainValue = 50;
        this.foundRoomVolumeBug = true;
      }
    }
    else {
      if (!this.foundRoomVolume) {
        //record just over room volume
        this.foundRoomVolume = true;
      }
    }

    if (Number.isFinite(this.gainValue)) {
      this.recordingAudioHeadGainNode.gain.value = this.gainValue * .03;
      for (var i = 0; i < this.recordingAudioGainNodes.length; i++) {
        this.recordingAudioGainNodes[i].gain.value = this.recordingAudioHeadGainNode.gain.value;
      }
    }

    if (performance.now() - this.micVolumeOverTimeStart >= this.MIC_VOLUME_DETECTION_TIME) {
      this.micVolumeOverTimeStart = performance.now();
      this.micVolumeOverTimeLowest = micVolumeLowest;
      this.micVolumeOverTimeLowCount = 1;
      this.micVolumeOverTimeLowTotal = this.micVolumeOverTimeLowAverage;
      this.micVolumeOverTimeAverageCount = 1;
      this.micVolumeOverTimeAverageTotal = this.micVolumeOverTimeAverage;
      this.micVolumeOverTimeHighest = micVolumeHighest;
      this.micVolumeOverTimeHighCount = 1;
      this.micVolumeOverTimeHighTotal = this.micVolumeOverTimeHighAverage;
    }
  }

  combineAudioBlobs(audioBlobs: Blob[]): Promise<Blob> {
    return new Promise((resolve, reject) => {
      const combinedBlobParts: BlobPart[] = [];

      for (const blob of audioBlobs) {
        const fileReader = new FileReader();

        fileReader.onload = () => {
          const arrayBuffer = fileReader.result as ArrayBuffer;
          const blobPart = new Uint8Array(arrayBuffer);
          combinedBlobParts.push(blobPart);

          if (audioBlobs.length === combinedBlobParts.length) {
            const combinedBlob = new Blob(combinedBlobParts, { type: audioBlobs[0].type });
            resolve(combinedBlob);
          }
        };

        fileReader.onerror = () => {
          reject(new Error('Failed to read audio blobs.'));
        };

        fileReader.readAsArrayBuffer(blob);
      }
    });
  }

  sendSpeech(add: boolean, compress: boolean, trim: boolean): Promise<boolean> {
    this.lastTimeSpeechSent = performance.now();
    if (this.sendingSpeech) {
      this.sendingSpeechPromise = this.sendingSpeechPromise.finally(() => {
        this.sendingSpeech = false;
        this.sendSpeech(add, compress, trim)
      });
    }
    else {
      this.sendingSpeechPromise = new Promise((resolve, reject) => {
        if (this.sendingSpeech) {
          reject();
        }
        else {
          this.sendingSpeech = true;
          if (this.textToSpeechPlaying) {
            this.audioPlayer.pause();
          }
          let clone = [...this.audioBlobsToSend];
          if (!add) {
            this.audioBlobsToSend = [];
          }

          this.combineAudioBlobs(clone).then(blobToSend => {
            blobToSend.arrayBuffer().then(arrayBuffer => {
              if (arrayBuffer.byteLength == 0) {
                console.log('byte length 0');
                reject();
                return;
              }
              if (this.audioContextOptions.sampleRate) {
                const audioContext = new AudioContext({
                  sampleRate: this.audioContextOptions.sampleRate
                });
                audioContext.decodeAudioData(arrayBuffer).then(audioBuffer => {
                  if (audioBuffer.duration <= .25) {
                    console.log('audioBuffer duration less than .25');
                    resolve(false);
                    return;
                  }
                  /*
                //compress and trim
                var buffer = audioBuffer;
                if (trim) {
                  let threshold = this.calculateSilenceThresholdRMS(audioBuffer, 4);
                  if (threshold > .1) {
                    threshold = .1;
                  }
                  const frameSize = 2048;
                  const hopSize = 1024;
                  buffer = this.trimSilenceWithRoom(audioBuffer, threshold, hopSize, frameSize, audioContext, 1, 1);
                  console.log('duration after trim: ' + buffer.duration);
                }
                  if (this.audioContextOptions.sampleRate) {
                    var offlineAudioCtx = new OfflineAudioContext({
                      numberOfChannels: 1,
                      length: this.audioContextOptions.sampleRate * buffer.duration,
                      sampleRate: this.audioContextOptions.sampleRate,
                    });

                    let soundSource = offlineAudioCtx.createBufferSource();
                    soundSource.buffer = buffer;

                    if (compress) {
                      // Create Compressor Node
                      var compressor = offlineAudioCtx.createDynamicsCompressor();
                      compressor.threshold.setValueAtTime(-50, offlineAudioCtx.currentTime);
                      compressor.knee.setValueAtTime(40, offlineAudioCtx.currentTime);
                      compressor.ratio.setValueAtTime(12, offlineAudioCtx.currentTime);
                      compressor.attack.setValueAtTime(0, offlineAudioCtx.currentTime);
                      compressor.release.setValueAtTime(1, offlineAudioCtx.currentTime);

                      // Connect nodes to destination
                      soundSource.connect(compressor);
                      compressor.connect(offlineAudioCtx.destination);
                    }
                    else {
                      soundSource.connect(offlineAudioCtx.destination);
                    }

                    offlineAudioCtx.startRendering().then((renderedBuffer) => {

                      soundSource.loop = false;
                    }).catch((error) => {
                      console.error(error);
                      reject();
                    });

                    soundSource.start(0);
                  }
                  */
                  var duration = audioBuffer.duration;
                  console.log("duration: " + duration);
                  let rate = audioBuffer.sampleRate;
                  let offset = 0;
                  let waveBlob = this.bufferToWave(audioBuffer, audioBuffer.length)
                  //var waveFile = URL.createObjectURL(waveBlob);
                  if (duration >= .25) {
                    let lastMessage: string | undefined = undefined;
                    for (let i = this.chatMessages.length - 1; i >= 0; i--) {
                      if (this.chatMessages[i].from == this.user.name) {
                        lastMessage = this.chatMessages[i].content;
                        break;
                      }
                    }
                    this.chatService.getChatSpeechToText(waveBlob, lastMessage).subscribe(
                      result => {
                        let speechText = "";
                        if (result.text) {
                          //remove bad words and wake words
                          let sentence = "";
                          if (result.text.indexOf(" ") != - 1) {
                            for (let word of result.text.split(" ")) {
                              if (this.badWordAndWakeWordRegex.test(word.toLowerCase())) {
                                word = "";
                              }
                              else {
                                sentence += word + " ";
                              }
                            }
                          }
                          else {
                            if (!this.badWordAndWakeWordRegex.test(result.text)) {
                              sentence = result.text;
                            }
                          }
                          speechText = sentence.trim();
                          if (speechText.toLowerCase() == 'stop' || speechText.toLowerCase() == 'stop.') {
                            console.log('stop command');
                            speechText = "";
                            if (this.visible) {
                              this.toggleChat();
                            }
                          }
                          if (speechText.toLowerCase().indexOf('That model is currently overloaded with other requests') != -1) {
                            speechText = "";
                          }
                          if (speechText.toLowerCase() == 'you') {
                            speechText = "";
                          }
                          if (speechText.toLowerCase().indexOf('thanks for watching') != -1 ||
                            speechText.toLowerCase().indexOf('thank you for watching') != -1 ||
                            speechText.toLowerCase().indexOf('please subscribe to my channel.') != -1
                          ) {
                            //the openai must have been trained on video data? thanks for watching usually comes back when it's just noise
                            speechText = "";
                          }
                          this.musicPlaying = speechText == '🎶';

                          if (this.musicPlaying) {
                            this.stopRecording();
                          }
                        }
                        if (add) {
                          if ((this.speechToTextMessage == "" && speechText == "") || (this.speechToTextMessage && this.speechToTextMessage.toLowerCase().replace(/[^\x41-\x7F]/g, "") == speechText.toLowerCase().replace(/[^\x41-\x7F]/g, ""))) {
                            this.chatInputRef.clearMessage();
                            this._forceFunctionCall = "auto";
                            this.sendMessage(speechText, true);
                            this.stopRecording();
                            this.speechToTextMessage = undefined;
                          }
                          else {
                            this.chatInputRef.setMessage(speechText);
                            this.speechToTextMessage = speechText;
                          }
                        }
                        else {
                          if (speechText) {
                            this.chatInputRef.clearMessage();
                            this._forceFunctionCall = "auto";
                            this.sendMessage(speechText, true);
                            this.stopRecording();
                            this.speechToTextMessage = undefined;
                          }
                          else {
                            speechText = "";
                            this.speechToTextMessage = speechText;
                          }
                        }
                        resolve(true);
                      },
                      error => {
                        if (error.ok === false) {
                          switch (error.status) {
                            case 400:
                              this.addMessage({
                                content: 'An error while transcribing audio.',
                                rawContent: 'An error while transcribing audio.',
                                from: this.system.name,
                                name: this.system.name,
                                to: this.user.name,
                                received: false
                              } as ChatMessageVm);
                              break;
                            case 401:
                              this.addMessage({
                                content: 'You must be logged in.',
                                rawContent: 'You must be logged in.',
                                from: this.system.name,
                                name: this.system.name,
                                to: this.user.name,
                                received: false
                              } as ChatMessageVm);
                              break;
                            default:
                              this.addMessage({
                                content: 'An error while transcribing audio.',
                                rawContent: 'An error while transcribing audio.',
                                from: this.system.name,
                                name: this.system.name,
                                to: this.user.name,
                                received: false
                              } as ChatMessageVm);
                              break;
                          }
                        }
                        reject();
                      }
                    );
                  }
                  else {
                    resolve(false);
                  }
                }).catch(err => {
                  console.log(err);
                  reject();
                });
              }
              else {
                reject();
              }
            }).catch(error => {
              console.error(error);
              reject();
            });
          }).catch(error => {
            console.error(error);
            reject();
          });
        }
      }).then(() => this.sendingSpeech = false);
    }
    return this.sendingSpeechPromise;
  }

  audioBufferToBuffer(audioBuffer: AudioBuffer): ArrayBuffer {
    const numberOfChannels = audioBuffer.numberOfChannels;
    const length = audioBuffer.length;
    const sampleRate = audioBuffer.sampleRate;

    const buffer = new ArrayBuffer(length * numberOfChannels * 2); // 2 bytes per sample (16-bit audio)
    const view = new DataView(buffer);

    for (let channel = 0; channel < numberOfChannels; channel++) {
      const channelData = audioBuffer.getChannelData(channel);
      const offset = channel * length;

      for (let i = 0; i < length; i++) {
        const sample = channelData[i];
        const sampleValue = Math.max(-1, Math.min(1, sample));
        const sample16 = sampleValue < 0 ? sampleValue * 0x8000 : sampleValue * 0x7FFF;

        view.setInt16((offset + i) * 2, sample16, true); // true for little-endian
      }
    }

    return buffer;
  }

  bufferToWave(abuffer: AudioBuffer, len: number) {
    var numOfChan = abuffer.numberOfChannels,
      length = len * numOfChan * 2 + 44,
      buffer = new ArrayBuffer(length),
      view = new DataView(buffer),
      channels = [], i, sample,
      offset = 0,
      pos = 0;

    // write WAVE header
    setUint32(0x46464952);                         // "RIFF"
    setUint32(length - 8);                         // file length - 8
    setUint32(0x45564157);                         // "WAVE"

    setUint32(0x20746d66);                         // "fmt " chunk
    setUint32(16);                                 // length = 16
    setUint16(1);                                  // PCM (uncompressed)
    setUint16(numOfChan);
    setUint32(abuffer.sampleRate);
    setUint32(abuffer.sampleRate * 2 * numOfChan); // avg. bytes/sec
    setUint16(numOfChan * 2);                      // block-align
    setUint16(16);                                 // 16-bit (hardcoded in this demo)

    setUint32(0x61746164);                         // "data" - chunk
    setUint32(length - pos - 4);                   // chunk length

    // write interleaved data
    for (i = 0; i < abuffer.numberOfChannels; i++)
      channels.push(abuffer.getChannelData(i));

    while (pos < length) {
      for (i = 0; i < numOfChan; i++) {             // interleave channels
        sample = Math.max(-1, Math.min(1, channels[i][offset])); // clamp
        sample = (0.5 + sample < 0 ? sample * 32768 : sample * 32767) | 0; // scale to 16-bit signed int
        view.setInt16(pos, sample, true);          // write 16-bit sample
        pos += 2;
      }
      offset++                                     // next source sample
    }

    // create Blob
    return new Blob([buffer], { type: "audio/wav" });

    function setUint16(data: number) {
      view.setUint16(pos, data, true);
      pos += 2;
    }

    function setUint32(data: number) {
      view.setUint32(pos, data, true);
      pos += 4;
    }
  }

  trimSilence(audioBuffer: AudioBuffer, threshold: number, hopSize: number, frameSize: number, audioContext: AudioContext): AudioBuffer | undefined {
    const channelData = audioBuffer.getChannelData(0); // Assuming mono audio
    const sampleRate = audioBuffer.sampleRate;

    const numFrames = Math.ceil(channelData.length / hopSize);
    const nonSilentFrames = [];

    let lastFrameWasSilent = true;
    // Analyze the audio data in frames and detect non silent sections
    for (let i = 0; i < numFrames; i++) {
      const startSample = i * hopSize;
      const endSample = Math.min(startSample + frameSize, channelData.length);

      let isSilent = true;

      for (let j = startSample; j < endSample; j++) {
        if (Math.abs(channelData[j]) > threshold) { // Adjust the threshold value as needed
          isSilent = false;
          break;
        }
      }

      //make sure not to cut off any audio !lastFrameWasSilent
      if (!isSilent || !lastFrameWasSilent) {
        nonSilentFrames.push({
          start: startSample / sampleRate,
          end: endSample / sampleRate
        });
      }
      lastFrameWasSilent = isSilent;
    }

    // Merge consecutive non silent sections that are shorter than the minimum duration
    const mergedSections = [];
    let currentSection = null;

    for (const nonSilentSection of nonSilentFrames) {
      if (!currentSection) {
        currentSection = nonSilentSection;
      } else if (nonSilentSection.start - currentSection.end <= this.MIN_SILENCE_DURATION) {
        currentSection.end = nonSilentSection.end;
      } else {
        mergedSections.push(currentSection);
        currentSection = nonSilentSection;
      }
    }

    if (currentSection) {
      mergedSections.push(currentSection);
    }

    // Create a new audio buffer with the non-silent audio data
    const newNonSilentDuration = mergedSections.reduce((duration, section) => {
      return duration + (section.end - section.start);
    }, 0);

    if (newNonSilentDuration > 0) {

      const newBuffer = audioContext.createBuffer(1, Math.floor(newNonSilentDuration * sampleRate), sampleRate);

      let newBufferOffset = 0;

      for (const section of mergedSections) {
        const startSample = Math.floor(section.start * sampleRate);
        const endSample = Math.ceil(section.end * sampleRate);
        const sectionDuration = section.end - section.start;

        for (let channel = 0; channel < newBuffer.numberOfChannels; channel++) {
          const channelData = audioBuffer.getChannelData(channel);
          const newChannelData = newBuffer.getChannelData(channel);

          for (let i = startSample; i < endSample; i++) {
            newChannelData[newBufferOffset + i - startSample] = channelData[i];
          }
        }

        newBufferOffset += Math.floor(sectionDuration * sampleRate);
      }
      return newBuffer;
    }
    else {
      return undefined;
    }
  }


  trimSilenceWithRoom(audioBuffer: AudioBuffer, threshold: number, hopSize: number, frameSize: number, audioContext: AudioContext, preCaptureSeconds: number, postCaptureSeconds: number): AudioBuffer | undefined {
    const channelData = audioBuffer.getChannelData(0); // Assuming mono audio
    const sampleRate = audioBuffer.sampleRate;


    const numFrames = Math.ceil(channelData.length / hopSize);
    const numFramesCapturePre = parseInt((preCaptureSeconds * sampleRate / hopSize).toFixed(0));
    const numFramesCapturePost = parseInt((postCaptureSeconds * sampleRate / hopSize).toFixed(0));



    const nonSilentFrames: any = [];

    let lastFrameWasSilent = true;
    let previousSilentFrames: any = [];
    let silentFramesIncluded = 0;

    // Analyze the audio data in frames and detect non silent sections
    for (let i = 0; i < numFrames; i++) {
      const startSample = i * hopSize;
      const endSample = Math.min(startSample + frameSize, channelData.length);

      let isSilent = true;
      for (let j = startSample; j < endSample; j++) {
        if (Math.abs(channelData[j]) > threshold) { // Adjust the threshold value as needed
          isSilent = false;
          break;
        }
      }

      let frame = {
        start: startSample / sampleRate,
        end: endSample / sampleRate
      };
      if (isSilent) {
        //include audio just after
        if (lastFrameWasSilent == false) {
          silentFramesIncluded++;
          if (silentFramesIncluded <= numFramesCapturePost) {
            nonSilentFrames.push(frame);
            lastFrameWasSilent = false;
          }
          else {
            silentFramesIncluded = 0;
            lastFrameWasSilent = true;
          }
        }
        else {
          silentFramesIncluded = 0;
          lastFrameWasSilent = true;
        }
        //this frame was silent, add into previous buffer
        previousSilentFrames.push(frame);
        if (previousSilentFrames.length > numFramesCapturePre) {
          previousSilentFrames.shift();
        }
      }
      else {
        silentFramesIncluded = 0;
        //include audio just before
        for (var f of previousSilentFrames) {
          nonSilentFrames.push(f);
        }
        previousSilentFrames = [];
        //then include this non silent frame
        nonSilentFrames.push(frame);
        lastFrameWasSilent = false;
      }
    }

    // Merge consecutive non silent sections that are shorter than the minimum duration
    const mergedSections = [];
    let currentSection = null;

    for (const nonSilentSection of nonSilentFrames) {
      if (!currentSection) {
        currentSection = nonSilentSection;
      } else if (nonSilentSection.start - currentSection.end <= this.MIN_SILENCE_DURATION) {
        currentSection.end = nonSilentSection.end;
      } else {
        mergedSections.push(currentSection);
        currentSection = nonSilentSection;
      }
    }

    if (currentSection) {
      mergedSections.push(currentSection);
    }

    // Create a new audio buffer with the non-silent audio data
    const newNonSilentDuration = mergedSections.reduce((duration, section) => {
      return duration + (section.end - section.start);
    }, 0);

    if (newNonSilentDuration > 0) {
      const newBuffer = audioContext.createBuffer(1, Math.floor(newNonSilentDuration * sampleRate), sampleRate);

      let newBufferOffset = 0;

      for (const section of mergedSections) {
        const startSample = Math.floor(section.start * sampleRate);
        const endSample = Math.ceil(section.end * sampleRate);
        const sectionDuration = section.end - section.start;

        for (let channel = 0; channel < newBuffer.numberOfChannels; channel++) {
          const channelData = audioBuffer.getChannelData(channel);
          const newChannelData = newBuffer.getChannelData(channel);

          for (let i = startSample; i < endSample; i++) {
            newChannelData[newBufferOffset + i - startSample] = channelData[i];
          }
        }

        newBufferOffset += Math.floor(sectionDuration * sampleRate);
      }

      return newBuffer;
    }
    else {
      return undefined;
    }
  }


  calculateSilenceThreshold(audioBuffer: AudioBuffer, adjustmentFactor: number) {
    const channelData = audioBuffer.getChannelData(0);

    let sum = 0;
    for (let i = 0; i < channelData.length; i++) {
      sum += Math.abs(channelData[i]);
    }

    const averageAmplitude = sum / channelData.length;
    const threshold = averageAmplitude * adjustmentFactor;
    return threshold;
  }

  calculateSilenceThresholdRMS(audioBuffer: AudioBuffer, adjustmentFactor: number) {
    const channelData = audioBuffer.getChannelData(0);

    let sumOfSquares = 0;
    for (let i = 0; i < channelData.length; i++) {
      sumOfSquares += channelData[i] * channelData[i];
    }

    const meanSquare = sumOfSquares / channelData.length;
    const rmsAmplitude = Math.sqrt(meanSquare);
    const threshold = rmsAmplitude * adjustmentFactor;

    return threshold;
  }


}
//declare class MediaStreamRecorder {
//  constructor(stream);
//  mimeType: string;
//  ondataavailable(blob);
//  start(time);
//};

//interface Navigator {
//  getUserMedia(
//    options: { video?: boolean; audio?: boolean; },
//    success: (stream: any) => void,
//    error?: (error: string) => void
//  ): void;
//}
