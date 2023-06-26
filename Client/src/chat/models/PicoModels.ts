import { PorcupineKeyword } from '@picovoice/porcupine-web';

export class PicoModels {
  public static porcupineModel = {
    publicPath: 'assets/porcupine_params.pv',
    forceWrite: true
  }

  public static porcupineKeywords: PorcupineKeyword[] = [
    {
      label: 'Bumblebee',
      publicPath: 'assets/bumblebee_wasm.ppn',
      forceWrite: true,
      sensitivity: 1,
    },
  ]
}
