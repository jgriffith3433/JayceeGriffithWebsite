using ContainerNinja.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using NAudio.Wave;
using System;
using System.Diagnostics;
using static AutoMapper.Internal.ExpressionFactory;

namespace ContainerNinja.Core.Services
{
    public class AudioService : IAudioService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AudioService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public byte[] StripNoiseAndTrimSilence(byte[] data)
        {
            //TODO: figure out what's doing wrong with trimming
            return data;
            using (var readerStream = new MemoryStream(data))
            {
                using (var reader = new WaveFileReader(readerStream))
                {
                    var thresholdRMS = CalculateSilenceThresholdRMS(reader, 1);
                    Debug.WriteLine("threshold dB: " + thresholdRMS);
                    using (var writerStream = new MemoryStream())
                    {
                        using (var writer = new WaveFileWriter(writerStream, reader.WaveFormat))
                        {
                            WriteNonSilentTimesToWaveFile(reader, writer, thresholdRMS);
                            if (writer.TotalTime.TotalSeconds > 0.25)
                            {
                                return writerStream.GetBuffer();
                            }
                            else
                            {
                                return new byte[0];
                            }
                        }
                    }
                }
            }
        }

        private double CalculateSilenceThresholdRMS(WaveFileReader reader, double factor)
        {
            bool eof = false;
            var buffer = new byte[reader.WaveFormat.SampleRate / 8 * reader.WaveFormat.BlockAlign];
            double totalSquared = 0;
            int decibelsRead = 0;
            double totalHighestDb = double.NegativeInfinity;
            double totalLowestDb = double.PositiveInfinity;

            while (!eof)
            {
                int samplesRead = reader.Read(buffer, 0, buffer.Length);
                if (samplesRead == 0)
                {
                    eof = true;
                    break;
                }
                decibelsRead++;
                double sampleSumOfSquares = 0;
                for (int n = 0; n < samplesRead; n += 2)
                {
                    double b = BitConverter.ToInt16(buffer, n) / 32768.0;
                    var db = 20 * Math.Log10(b);
                    if (double.IsNaN(db) || !double.IsFinite(db))
                    {
                        db = 0;
                    }
                    sampleSumOfSquares += b * b;
                    if (db > totalHighestDb)
                    {
                        totalHighestDb = db;
                    }
                    if (db < totalLowestDb)
                    {
                        totalLowestDb = db;
                    }
                }
                var sampleMeanSquare = sampleSumOfSquares / samplesRead / 2;
                var sampleRms = Math.Sqrt(sampleMeanSquare);

                totalSquared += sampleRms * sampleRms;
            }
            var meanSquare = totalSquared / decibelsRead;
            var rmsDb = 20 * Math.Log10(Math.Sqrt(meanSquare));

            Debug.WriteLine("Highest dB: " + totalHighestDb);
            Debug.WriteLine("Lowest dB: " + totalLowestDb);
            Debug.WriteLine("RMS dB: " + rmsDb);
            reader.Position = 0;
            return rmsDb * factor;
        }

        private void WriteNonSilentTimesToWaveFile(WaveFileReader reader, WaveFileWriter writer, double silenceThreshold)
        {
            bool eof = false;
            var buffer = new byte[reader.WaveFormat.SampleRate / 8 * reader.WaveFormat.BlockAlign];

            while (!eof)
            {
                int samplesRead = reader.Read(buffer, 0, buffer.Length);
                if (samplesRead == 0)
                {
                    eof = true;
                    break;
                }
                double sampleSumOfSquares = 0;
                for (int n = 0; n < samplesRead; n += 2)
                {
                    double b = BitConverter.ToInt16(buffer, n) / 32768.0;
                    sampleSumOfSquares += b * b;
                }
                var sampleMeanSquare = sampleSumOfSquares / samplesRead / 2;
                var sampleDb = 20 * Math.Log10(Math.Sqrt(sampleMeanSquare));

                Debug.WriteLine("Sample dB: " + sampleDb);

                if (!IsSilence(sampleDb, silenceThreshold))
                {
                    writer.Write(buffer, 0, samplesRead);
                }
            }
            reader.Position = 0;
        }

        private static bool IsSilence(double decibel, double threshold)
        {
            return decibel < threshold;
        }

        //private static int GetdB(float amplitude)
        //{
        //    return 20 * (int)Math.Log10(Math.Abs(amplitude));
        //}
    }
}
