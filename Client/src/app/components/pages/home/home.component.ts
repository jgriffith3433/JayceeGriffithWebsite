import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { YouTubePlayerModule } from '@angular/youtube-player';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  @ViewChild("youTubePlayer") youTubePlayer: ElementRef<HTMLDivElement>;
  videoId: string = "sZYHMFaWDu8";
  visible: boolean = true;
  videoHeight: number | undefined;
  videoWidth: number | undefined;
  closeButtonRightOffset: string = "0px";

  constructor(
    private router: Router,
    private changeDetectorRef: ChangeDetectorRef,
    private youTubePlayerModule: YouTubePlayerModule,
  ) { }

  closeIFrame(): void {
    this.visible = false;
  }

  ngAfterViewInit(): void {
    this.onResize();
    window.addEventListener("resize", this.onResize.bind(this));
  }

  onResize(): void {
    // you can remove this line if you want to have wider video player than 1200px
    this.videoWidth = Math.min(
      this.youTubePlayer.nativeElement.clientWidth,
      1600
    );
    this.closeButtonRightOffset = ((((window as any).innerWidth - (this.videoWidth)) / 2) - 5) + "px";
    //this.videoWidth = this.youTubePlayer.nativeElement.clientWidth;

    // so you keep the ratio
    this.videoHeight = this.videoWidth * 0.6;
    this.changeDetectorRef.detectChanges();
  }
}
