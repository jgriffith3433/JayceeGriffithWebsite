import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NgUnityWebglManagerService {
  unityInstances: any

  constructor() {
    this.unityInstances = {};
  }

  getInstance(instanceName: string): any {
    return this.unityInstances[instanceName];
  }

  setInstance(instanceName: string, instance: any): any {
    this.unityInstances[instanceName] = instance;
  }
}
