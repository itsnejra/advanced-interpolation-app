using System;
using System.IO;
using NAudio.Wave;

namespace InterpolationApp.Helpers
{
    /// <summary>
    /// Helper class for generating test audio files with controlled noise
    /// Useful for testing audio interpolation algorithms
    /// </summary>
    public class TestAudioGenerator
    {
        /// <summary>
        /// Generate a test audio file with sine wave and optional noise
        /// </summary>
        /// <param name="outputPath">Output WAV file path</param>
        /// <param name="frequency">Sine wave frequency in Hz</param>
        /// <param name="durationSeconds">Duration in seconds</param>
        /// <param name="sampleRate">Sample rate (default 44100 Hz)</param>
        /// <param name="amplitude">Amplitude (0.0 to 1.0)</param>
        /// <param name="noiseLevel">Noise level (0.0 to 1.0, 0 = no noise)</param>
        public static void GenerateSineWaveWithNoise(
            string outputPath,
            double frequency = 440.0,
            double durationSeconds = 3.0,
            int sampleRate = 44100,
            double amplitude = 0.7,
            double noiseLevel = 0.0)
        {
            int totalSamples = (int)(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];
            var random = new Random();

            for (int i = 0; i < totalSamples; i++)
            {
                // Generate sine wave
                double time = i / (double)sampleRate;
                double sineValue = amplitude * Math.Sin(2.0 * Math.PI * frequency * time);

                // Add noise
                double noise = noiseLevel * (random.NextDouble() * 2.0 - 1.0);
                
                samples[i] = (float)(sineValue + noise);
            }

            SaveAudioFile(outputPath, samples, sampleRate);
        }

        /// <summary>
        /// Generate test audio with random clicks/pops (simulated noise)
        /// </summary>
        public static void GenerateAudioWithClicks(
            string outputPath,
            double baseFrequency = 440.0,
            double durationSeconds = 3.0,
            int sampleRate = 44100,
            int numberOfClicks = 20)
        {
            int totalSamples = (int)(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];
            var random = new Random();

            // Generate clean sine wave
            for (int i = 0; i < totalSamples; i++)
            {
                double time = i / (double)sampleRate;
                samples[i] = (float)(0.7 * Math.Sin(2.0 * Math.PI * baseFrequency * time));
            }

            // Add random clicks
            for (int i = 0; i < numberOfClicks; i++)
            {
                int clickPosition = random.Next(0, totalSamples);
                int clickWidth = random.Next(5, 20);
                
                for (int j = 0; j < clickWidth && clickPosition + j < totalSamples; j++)
                {
                    samples[clickPosition + j] = (float)(random.NextDouble() * 2.0 - 1.0);
                }
            }

            SaveAudioFile(outputPath, samples, sampleRate);
        }

        /// <summary>
        /// Generate audio with gaps (simulated packet loss)
        /// </summary>
        public static void GenerateAudioWithGaps(
            string outputPath,
            double baseFrequency = 440.0,
            double durationSeconds = 3.0,
            int sampleRate = 44100,
            int numberOfGaps = 10)
        {
            int totalSamples = (int)(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];
            var random = new Random();

            // Generate clean sine wave
            for (int i = 0; i < totalSamples; i++)
            {
                double time = i / (double)sampleRate;
                samples[i] = (float)(0.7 * Math.Sin(2.0 * Math.PI * baseFrequency * time));
            }

            // Create gaps (set to zero)
            for (int i = 0; i < numberOfGaps; i++)
            {
                int gapStart = random.Next(0, totalSamples - 100);
                int gapLength = random.Next(10, 100);
                
                for (int j = 0; j < gapLength && gapStart + j < totalSamples; j++)
                {
                    samples[gapStart + j] = 0.0f;
                }
            }

            SaveAudioFile(outputPath, samples, sampleRate);
        }

        /// <summary>
        /// Generate multi-frequency test signal
        /// </summary>
        public static void GenerateMultiFrequency(
            string outputPath,
            double[] frequencies,
            double durationSeconds = 3.0,
            int sampleRate = 44100,
            double noiseLevel = 0.05)
        {
            int totalSamples = (int)(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];
            var random = new Random();

            for (int i = 0; i < totalSamples; i++)
            {
                double time = i / (double)sampleRate;
                double value = 0;

                // Sum all frequencies
                foreach (var freq in frequencies)
                {
                    value += Math.Sin(2.0 * Math.PI * freq * time);
                }

                // Normalize by number of frequencies
                value /= frequencies.Length;

                // Add noise
                value += noiseLevel * (random.NextDouble() * 2.0 - 1.0);

                samples[i] = (float)(0.7 * value);
            }

            SaveAudioFile(outputPath, samples, sampleRate);
        }

        private static void SaveAudioFile(string filePath, float[] samples, int sampleRate)
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            
            using var writer = new WaveFileWriter(filePath, waveFormat);
            writer.WriteSamples(samples, 0, samples.Length);
        }

        /// <summary>
        /// Generate all test samples in a directory
        /// </summary>
        public static void GenerateAllTestSamples(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            Console.WriteLine("Generating test audio samples...");

            GenerateSineWaveWithNoise(
                Path.Combine(outputDirectory, "test_clean_440hz.wav"),
                frequency: 440.0,
                noiseLevel: 0.0);

            GenerateSineWaveWithNoise(
                Path.Combine(outputDirectory, "test_light_noise.wav"),
                frequency: 440.0,
                noiseLevel: 0.05);

            GenerateSineWaveWithNoise(
                Path.Combine(outputDirectory, "test_heavy_noise.wav"),
                frequency: 440.0,
                noiseLevel: 0.2);

            GenerateAudioWithClicks(
                Path.Combine(outputDirectory, "test_with_clicks.wav"),
                numberOfClicks: 30);

            GenerateAudioWithGaps(
                Path.Combine(outputDirectory, "test_with_gaps.wav"),
                numberOfGaps: 15);

            GenerateMultiFrequency(
                Path.Combine(outputDirectory, "test_multi_freq.wav"),
                new[] { 220.0, 440.0, 880.0 });

            Console.WriteLine($"Test samples generated in: {outputDirectory}");
        }
    }
}
