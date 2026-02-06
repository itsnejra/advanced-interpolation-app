using System;
using System.IO;
using NAudio.Wave;

namespace AudioTestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║        🎵 TEST AUDIO GENERATOR - Interpolation Project 🎵   ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Output directory
            string outputDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "TestAudioSamples"
            );
            
            Directory.CreateDirectory(outputDir);

            Console.WriteLine($"📁 Output Directory: {outputDir}");
            Console.WriteLine();
            Console.WriteLine("Generating test audio files...");
            Console.WriteLine();

            // 1. Clean sine wave (baseline)
            Console.Write("[1/6] Creating clean_440hz.wav... ");
            GenerateSineWave(
                Path.Combine(outputDir, "clean_440hz.wav"),
                frequency: 440.0,
                durationSeconds: 5.0,
                noiseLevel: 0.0
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // 2. Light noise (subtle)
            Console.Write("[2/6] Creating light_noise.wav... ");
            GenerateSineWave(
                Path.Combine(outputDir, "light_noise.wav"),
                frequency: 440.0,
                durationSeconds: 5.0,
                noiseLevel: 0.08
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // 3. Medium noise (good for testing)
            Console.Write("[3/6] Creating medium_noise.wav... ");
            GenerateSineWave(
                Path.Combine(outputDir, "medium_noise.wav"),
                frequency: 440.0,
                durationSeconds: 5.0,
                noiseLevel: 0.15
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // 4. Heavy noise (aggressive test)
            Console.Write("[4/6] Creating heavy_noise.wav... ");
            GenerateSineWave(
                Path.Combine(outputDir, "heavy_noise.wav"),
                frequency: 440.0,
                durationSeconds: 5.0,
                noiseLevel: 0.25
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // 5. Audio with clicks/pops
            Console.Write("[5/6] Creating with_clicks.wav... ");
            GenerateAudioWithClicks(
                Path.Combine(outputDir, "with_clicks.wav"),
                baseFrequency: 440.0,
                durationSeconds: 5.0,
                numberOfClicks: 30
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // 6. Multi-frequency (complex signal)
            Console.Write("[6/6] Creating multi_frequency.wav... ");
            GenerateMultiFrequency(
                Path.Combine(outputDir, "multi_frequency.wav"),
                frequencies: new[] { 220.0, 440.0, 880.0 },
                durationSeconds: 5.0,
                noiseLevel: 0.1
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ✅ SUCCESS! ✅                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("📦 Generated 6 test audio files:");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  1. clean_440hz.wav       - Baseline (no noise)");
            Console.WriteLine("  2. light_noise.wav       - Subtle noise (8%)");
            Console.WriteLine("  3. medium_noise.wav      - Medium noise (15%) ⭐ START HERE");
            Console.WriteLine("  4. heavy_noise.wav       - Heavy noise (25%)");
            Console.WriteLine("  5. with_clicks.wav       - Click/pop artifacts");
            Console.WriteLine("  6. multi_frequency.wav   - Complex signal");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🎯 RECOMMENDED TEST:");
            Console.WriteLine("   1. Load: medium_noise.wav");
            Console.WriteLine("   2. Algorithm: Cubic Spline");
            Console.WriteLine("   3. Threshold: 3.0");
            Console.WriteLine("   4. Process & Save");
            Console.WriteLine("   5. Compare before/after!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void GenerateSineWave(string outputPath, double frequency, double durationSeconds, double noiseLevel)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];
            var random = new Random();

            for (int i = 0; i < totalSamples; i++)
            {
                double time = i / (double)sampleRate;
                double sineValue = 0.7 * Math.Sin(2.0 * Math.PI * frequency * time);
                double noise = noiseLevel * (random.NextDouble() * 2.0 - 1.0);
                samples[i] = (float)(sineValue + noise);
            }

            SaveAudioFile(outputPath, samples, sampleRate);
        }

        static void GenerateAudioWithClicks(string outputPath, double baseFrequency, double durationSeconds, int numberOfClicks)
        {
            int sampleRate = 44100;
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

        static void GenerateMultiFrequency(string outputPath, double[] frequencies, double durationSeconds, double noiseLevel)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];
            var random = new Random();

            for (int i = 0; i < totalSamples; i++)
            {
                double time = i / (double)sampleRate;
                double value = 0;

                foreach (var freq in frequencies)
                {
                    value += Math.Sin(2.0 * Math.PI * freq * time);
                }

                value /= frequencies.Length;
                value += noiseLevel * (random.NextDouble() * 2.0 - 1.0);

                samples[i] = (float)(0.7 * value);
            }

            SaveAudioFile(outputPath, samples, sampleRate);
        }

        static void SaveAudioFile(string filePath, float[] samples, int sampleRate)
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            using var writer = new WaveFileWriter(filePath, waveFormat);
            writer.WriteSamples(samples, 0, samples.Length);
        }
    }
}
