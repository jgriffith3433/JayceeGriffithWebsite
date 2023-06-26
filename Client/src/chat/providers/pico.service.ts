import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { TokenService } from '../../app/providers/token.service';
import { PorcupineService } from '@picovoice/porcupine-angular';
import { PorcupineDetection } from '@picovoice/porcupine-web';
import { PicoModels } from '../models/PicoModels';

@Injectable({
  providedIn: 'root'
})
export class PicoService implements OnDestroy {
  public autoStart: boolean = false;
  public isLoaded: boolean = false;
  public isListening: boolean = false;
  private isLoadedSubscription: Subscription;
  private isListeningSubscription: Subscription;

  public get keywordDetectionListener(): Subject<PorcupineDetection> {
    return this.porcupineService.keywordDetection$;
  }

  public get errorListener(): Subject<Error | null> {
    return this.porcupineService.error$;
  }

  constructor(private http: HttpClient,
    private tokenService: TokenService,
    private porcupineService: PorcupineService,
  ) {

    this.isLoadedSubscription = this.porcupineService.isLoaded$.subscribe(isLoaded => {
      this.isLoaded = isLoaded;
      console.log('isLoaded: ' + this.isLoaded);
      console.log('autoStart: ' + this.autoStart);
      if (this.autoStart) {
        this.start();
      }
    });
    this.isListeningSubscription = this.porcupineService.isListening$.subscribe(isListening => {
      this.isListening = isListening;
      console.log('isListening: ' + this.isListening);
    });
  }

  public async start() {
    if (this.isListening) {
      await this.porcupineService.stop();
    }
    await this.porcupineService.start();
  }

  public async stop() {
    if (this.isListening) {
      await this.porcupineService.stop();
    }
  }

  public async load() {
    if (this.tokenService.IsPicoAuthenticated) {
      if (this.isLoaded) {
        await this.porcupineService.release();
      }
      if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
        console.log("This browser does not support the API yet");
      }

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
        navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
          if (hasMicrophone) {
            if (stream.getAudioTracks().length > 0) {
              try {
                this.porcupineService.init(
                  this.tokenService.getPico() as string,
                  PicoModels.porcupineKeywords[0],
                  PicoModels.porcupineModel
                );
              }
              catch (error: any) {
                if (error) {
                  console.error(error);
                }
              }
            }
          }
        }, error => {
          console.log(error);
        });
      });
    }
  }

  public unload() {
    this.isLoadedSubscription.unsubscribe();
    this.isListeningSubscription.unsubscribe();
    this.porcupineService.release();
  }

  ngOnDestroy(): void {
    this.unload();
  }
}
