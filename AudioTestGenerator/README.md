# Audio Test Generator

Simple utility to generate test audio files with vinyl crackle noise for testing the interpolation-based noise removal.

## Usage

Run the generator executable:

```bash
cd AudioTestGenerator
dotnet run
```

This will create:
- `test_audio_clean.wav` - Clean sine wave
- `test_audio_noisy.wav` - Same wave with vinyl crackle added

## Test Files

Use `test_audio_noisy.wav` with the main application's Audio Processing tab to test noise removal capabilities.

## Building

```bash
cd AudioTestGenerator
dotnet build
```
