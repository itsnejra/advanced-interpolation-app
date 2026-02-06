using System;
using System.IO;
using System.Linq;
using NAudio.Wave;
using InterpolationApp.Algorithms;

namespace InterpolationApp.Services
{
    /// <summary>
    /// ENHANCED Audio processing service - VINYL CRACKLE MODE
    /// </summary>
    public class AudioProcessingService
    {
        /// <summary>
        /// Load audio file and extract samples - MONO conversion for processing
        /// </summary>
        public (float[] samples, int sampleRate, int channels) LoadAudioFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Audio file not found", filePath);

            using var reader = new AudioFileReader(filePath);
            
            int sampleRate = reader.WaveFormat.SampleRate;
            int channels = reader.WaveFormat.Channels;
            
            // Read ALL samples
            var buffer = new float[reader.Length / sizeof(float)];
            int samplesRead = reader.Read(buffer, 0, buffer.Length);
            
            if (samplesRead < buffer.Length)
            {
                Array.Resize(ref buffer, samplesRead);
            }

            // Convert to MONO for processing (average all channels)
            float[] monoSamples;
            
            if (channels == 1)
            {
                monoSamples = buffer;
            }
            else
            {
                // Stereo/Multi-channel → Mono
                int monoLength = buffer.Length / channels;
                monoSamples = new float[monoLength];
                
                for (int i = 0; i < monoLength; i++)
                {
                    float sum = 0;
                    for (int ch = 0; ch < channels; ch++)
                    {
                        sum += buffer[i * channels + ch];
                    }
                    monoSamples[i] = sum / channels;
                }
            }

            return (monoSamples, sampleRate, channels);
        }

        /// <summary>
        /// MOVING AVERAGE + OUTLIER INTERPOLATION
        /// Two-stage approach: smoothing + outlier removal
        /// </summary>
        public float[] RemoveNoiseInterpolation(float[] samples, IInterpolationAlgorithm algorithm, 
                                                 double thresholdMultiplier = 3.0, int windowSize = 50)
        {
            if (samples == null || samples.Length == 0)
                throw new ArgumentException("Samples array is empty");

            Console.WriteLine($"[NOISE REMOVAL] Starting with {samples.Length} samples");
            Console.WriteLine($"[NOISE REMOVAL] Threshold multiplier: {thresholdMultiplier}");
            
            // STAGE 1: Apply moving average filter to reduce Gaussian noise
            // Larger filter = more noise reduction (but slightly more "smoothing" of signal)
            int filterSize = Math.Max(15, (int)(windowSize / 2)); // AGGRESSIVE smoothing
            if (filterSize % 2 == 0) filterSize++; // Make it odd
            
            // Apply filter MULTIPLE times for better noise reduction
            float[] smoothed = samples;
            int numPasses = 3; // More passes = cleaner signal
            
            for (int pass = 1; pass <= numPasses; pass++)
            {
                smoothed = ApplyMovingAverage(smoothed, filterSize);
                Console.WriteLine($"[NOISE REMOVAL] Pass {pass}/{numPasses}: Applied moving average filter (size: {filterSize})");
            }
            
            // STAGE 2: Detect remaining outliers (spikes that survived smoothing)
            float[] result = (float[])smoothed.Clone();
            
            // Calculate statistics on smoothed signal
            double mean = smoothed.Average();
            double variance = smoothed.Select(s => Math.Pow(s - mean, 2)).Average();
            double stdDev = Math.Sqrt(variance);
            
            double threshold = Math.Abs(mean) + thresholdMultiplier * stdDev;
            
            Console.WriteLine($"[OUTLIER DETECTION] Mean: {mean:F6}, StdDev: {stdDev:F6}");
            Console.WriteLine($"[OUTLIER DETECTION] Threshold: ±{threshold:F6}");
            
            // Mark outliers
            bool[] isOutlier = new bool[smoothed.Length];
            int outlierCount = 0;
            
            for (int i = 0; i < smoothed.Length; i++)
            {
                if (Math.Abs(smoothed[i]) > threshold)
                {
                    isOutlier[i] = true;
                    outlierCount++;
                }
            }
            
            Console.WriteLine($"[OUTLIER DETECTION] Found {outlierCount} outliers ({100.0 * outlierCount / smoothed.Length:F2}%)");
            
            if (outlierCount == 0)
            {
                Console.WriteLine($"[NOISE REMOVAL] No outliers detected - returning smoothed signal");
                return result;
            }
            
            // STAGE 3: Interpolate outliers
            int processedCount = 0;
            
            for (int i = 0; i < smoothed.Length; i += windowSize)
            {
                int windowEnd = Math.Min(i + windowSize, smoothed.Length);
                int interpolated = ProcessWindow(result, isOutlier, i, windowEnd, algorithm);
                processedCount += interpolated;
            }

            Console.WriteLine($"[NOISE REMOVAL] Interpolated {processedCount} outlier samples");
            Console.WriteLine($"[NOISE REMOVAL] ✅ Processing complete!");

            return result;
        }
        
        /// <summary>
        /// Apply simple moving average filter (low-pass filter)
        /// This reduces high-frequency noise
        /// </summary>
        private float[] ApplyMovingAverage(float[] samples, int windowSize)
        {
            float[] result = new float[samples.Length];
            int halfWindow = windowSize / 2;
            
            for (int i = 0; i < samples.Length; i++)
            {
                int start = Math.Max(0, i - halfWindow);
                int end = Math.Min(samples.Length - 1, i + halfWindow);
                
                float sum = 0;
                int count = 0;
                
                for (int j = start; j <= end; j++)
                {
                    sum += samples[j];
                    count++;
                }
                
                result[i] = sum / count;
            }
            
            return result;
        }

        private int ProcessWindow(float[] samples, bool[] isNoise, int start, int end, 
                                    IInterpolationAlgorithm algorithm)
        {
            // Find clean points in extended window
            int expandStart = Math.Max(0, start - 100);
            int expandEnd = Math.Min(samples.Length, end + 100);
            
            var cleanPoints = new System.Collections.Generic.List<(int index, float value)>();
            
            for (int i = expandStart; i < expandEnd; i++)
            {
                if (!isNoise[i])
                {
                    cleanPoints.Add((i, samples[i]));
                }
            }

            if (cleanPoints.Count < 3)
            {
                // Not enough clean points - use median of neighbors
                for (int i = start; i < end; i++)
                {
                    if (isNoise[i])
                    {
                        // Simple median filter fallback
                        int left = Math.Max(0, i - 2);
                        int right = Math.Min(samples.Length - 1, i + 2);
                        
                        var neighbors = new System.Collections.Generic.List<float>();
                        for (int n = left; n <= right; n++)
                        {
                            if (!isNoise[n] && n != i)
                            {
                                neighbors.Add(samples[n]);
                            }
                        }
                        
                        if (neighbors.Count > 0)
                        {
                            neighbors.Sort();
                            samples[i] = neighbors[neighbors.Count / 2]; // Median
                        }
                    }
                }
                return end - start;
            }

            // Prepare interpolation data
            double[] xPoints = cleanPoints.Select(p => (double)p.index).ToArray();
            double[] yPoints = cleanPoints.Select(p => (double)p.value).ToArray();

            try
            {
                algorithm.SetData(xPoints, yPoints);

                // Interpolate noisy points in current window
                int interpolatedCount = 0;
                for (int i = start; i < end; i++)
                {
                    if (isNoise[i])
                    {
                        samples[i] = (float)algorithm.Interpolate(i);
                        interpolatedCount++;
                    }
                }

                return interpolatedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOISE DETECTION] Window [{start},{end}] interpolation failed: {ex.Message}");
                
                // Fallback to median filter
                for (int i = start; i < end; i++)
                {
                    if (isNoise[i])
                    {
                        int left = Math.Max(0, i - 2);
                        int right = Math.Min(samples.Length - 1, i + 2);
                        
                        var neighbors = new System.Collections.Generic.List<float>();
                        for (int n = left; n <= right; n++)
                        {
                            if (!isNoise[n] && n != i)
                            {
                                neighbors.Add(samples[n]);
                            }
                        }
                        
                        if (neighbors.Count > 0)
                        {
                            neighbors.Sort();
                            samples[i] = neighbors[neighbors.Count / 2];
                        }
                    }
                }
                return end - start;
            }
        }

        /// <summary>
        /// Save processed audio - PROPERLY handles mono/stereo conversion
        /// </summary>
        public void SaveAudioFile(string filePath, float[] samples, int sampleRate, int originalChannels)
        {
            // Preserve original channel count
            int channels = Math.Max(1, originalChannels);
            
            float[] outputSamples;
            
            if (channels == 1)
            {
                // Mono - direct save
                outputSamples = samples;
            }
            else
            {
                // Mono → Stereo/Multi-channel
                outputSamples = new float[samples.Length * channels];
                
                for (int i = 0; i < samples.Length; i++)
                {
                    // Duplicate mono sample to all channels
                    for (int ch = 0; ch < channels; ch++)
                    {
                        outputSamples[i * channels + ch] = samples[i];
                    }
                }
            }

            // Create proper wave format
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            
            using var writer = new WaveFileWriter(filePath, waveFormat);
            writer.WriteSamples(outputSamples, 0, outputSamples.Length);
            
            Console.WriteLine($"[AUDIO SAVE] Saved: {outputSamples.Length} samples, {sampleRate}Hz, {channels} channel(s)");
        }
    }
}
