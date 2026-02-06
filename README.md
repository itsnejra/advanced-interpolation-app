# Advanced Interpolation Suite

<div align="center">

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-0078D4?style=for-the-badge&logo=windows&logoColor=white)

**Professional numerical interpolation toolkit with audio processing capabilities**

Master's Thesis Project | Numerical Analysis | 2024-2025

</div>

---

## 🎯 Overview

Advanced Interpolation Suite is a comprehensive WPF desktop application designed for numerical analysis and audio signal processing. The application implements five classical interpolation algorithms with real-time visualization, error analysis, and practical audio denoising capabilities.

### Key Features

- **5 Interpolation Algorithms**: Lagrange, Newton, Cubic Spline, Hermite, Linear
- **Interactive Data Input**: Manual point entry with real-time plot updates
- **Function Sampling**: Generate samples from mathematical expressions with Chebyshev/uniform node distribution
- **Polynomial Degree Optimizer**: Automatically find optimal degree for target error threshold
- **Audio Processing**: Vinyl crackle removal using interpolation-based noise detection
- **Material Design UI**: Modern, professional interface with elegant visualizations

---

## 🚀 Quick Start

### Prerequisites

- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** or later
- **Visual Studio 2022** (recommended) or VS Code with C# extension

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/InterpolationMasterProject.git
   cd InterpolationMasterProject/InterpolationApp
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build --configuration Release
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

---

## 📚 Features in Detail

### 1️⃣ Manual Input Mode

Create custom datasets by manually entering data points or generating sample data.

**Features:**
- Add/remove individual points
- Generate random datasets
- Load sample data (sine wave with noise)
- Real-time interpolation visualization
- Algorithm comparison with performance metrics (RMSE, Max Error, Computation Time)

**Supported Algorithms:**
- **Lagrange Interpolation**: Global polynomial interpolation
- **Newton Interpolation**: Divided differences approach
- **Cubic Spline**: Piecewise cubic with C² continuity
- **Hermite Interpolation**: Matches derivatives at nodes
- **Linear Interpolation**: Simple piecewise linear

### 2️⃣ Function Sampling

Sample mathematical functions and analyze interpolation accuracy.

**Capabilities:**
- Parse mathematical expressions: `sin(x)`, `x^2`, `exp(-x^2)`, `1/(1+25*x^2)` (Runge's function)
- Chebyshev node distribution (minimizes Runge phenomenon)
- Uniform node distribution
- Compare interpolation error across all algorithms
- Visualize real function vs interpolated curves

**Supported Functions:**
- Trigonometric: `sin`, `cos`, `tan`
- Exponential: `exp`, `log`, `ln`
- Algebraic: `x^n`, `sqrt`, `abs`
- Constants: `pi`, `e`

### 3️⃣ Polynomial Degree Optimizer

Automatically determine the minimum polynomial degree required for desired accuracy.

**Workflow:**
1. Define target function and interval
2. Set target maximum error (ε)
3. Specify search range for degree (n_min, n_max)
4. Algorithm tests degrees incrementally and finds n_min where error ≤ ε

**Output:**
- Optimal degree n_min
- Actual achieved error
- Error vs degree plot (logarithmic scale)
- Detailed analysis table

### 4️⃣ Audio Processing (Vinyl Crackle Removal)

Remove noise artifacts from audio recordings using numerical interpolation.

**How It Works:**
1. **Detection**: Uses derivative-based and amplitude-based outlier detection
2. **Marking**: Identifies isolated spikes (clicks/pops) 2-5 samples wide
3. **Interpolation**: Replaces noisy samples with interpolated values from clean neighbors
4. **Reconstruction**: Preserves stereo/mono channel configuration

**Supported Formats:**
- WAV, MP3, FLAC, AIFF

**Parameters:**
- **Noise Threshold**: 1.5-5.0σ (lower = more aggressive)
- **Algorithm**: Cubic Spline recommended for smooth audio

---

## 🛠️ Technical Architecture

### Technology Stack

- **Framework**: .NET 8.0, WPF (Windows Presentation Foundation)
- **UI Library**: MaterialDesignInXAML Toolkit 5.1.0
- **Plotting**: OxyPlot.Wpf 2.2.0
- **Audio Processing**: NAudio 2.2.1
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.3.2

### Project Structure

```
InterpolationApp/
├── Algorithms/              # Core interpolation implementations
│   ├── IInterpolationAlgorithm.cs
│   ├── LagrangeInterpolation.cs
│   ├── NewtonInterpolation.cs
│   ├── CubicSplineInterpolation.cs
│   ├── HermiteInterpolation.cs
│   └── LinearInterpolation.cs
├── Models/                  # Data models
│   ├── DataPoint.cs
│   └── InterpolationResult.cs
├── Services/                # Business logic
│   ├── AudioProcessingService.cs
│   └── DataService.cs
├── Helpers/                 # Utility classes
│   ├── FunctionEvaluator.cs
│   ├── PolynomialDegreeOptimizer.cs
│   └── TestAudioGenerator.cs
├── ViewModels/              # MVVM ViewModels
│   ├── MainViewModel.cs
│   └── Converters.cs
├── Views/                   # WPF Views
│   └── MainWindow.xaml
└── App.xaml                 # Application entry point
```

### Key Design Patterns

- **MVVM (Model-View-ViewModel)**: Clean separation of concerns
- **Command Pattern**: User interactions via RelayCommand
- **Strategy Pattern**: Pluggable interpolation algorithms via interface
- **Observer Pattern**: Data binding with INotifyPropertyChanged

---

## 📊 Algorithm Complexity

| Algorithm | Time Complexity | Space Complexity | Smoothness |
|-----------|----------------|------------------|------------|
| Lagrange | O(n²) | O(n) | C⁰ (not smooth) |
| Newton | O(n²) | O(n) | C⁰ (not smooth) |
| Linear | O(n log n) | O(n) | C⁰ (piecewise) |
| Hermite | O(n) | O(n) | C¹ (smooth) |
| Cubic Spline | O(n) | O(n) | C² (very smooth) |

**Recommendations:**
- **Small datasets (<20 points)**: Lagrange or Newton
- **Smooth curves required**: Cubic Spline
- **Derivative matching**: Hermite
- **Real-time performance**: Linear

---

## 🧪 Testing & Examples

### Example 1: Runge's Function

```csharp
Function: 1/(1+25*x^2)
Interval: [-1, 1]
Degree: 20
Nodes: Chebyshev (recommended)

Result: 
- Uniform nodes → Runge phenomenon (oscillations)
- Chebyshev nodes → Stable approximation
```

### Example 2: Vinyl Crackle Removal

```
Input: vintage_recording.wav (44.1kHz stereo)
Algorithm: Cubic Spline
Threshold: 3.0σ
Detected: 1,247 clicks/pops (0.12% of samples)
Result: Clean audio with preserved dynamics
```

---

## 📖 Mathematical Background

### Lagrange Polynomial

$$
L(x) = \sum_{i=0}^{n} y_i \prod_{\substack{j=0 \\ j \neq i}}^{n} \frac{x - x_j}{x_i - x_j}
$$

### Newton Divided Differences

$$
N(x) = f[x_0] + \sum_{i=1}^{n} f[x_0, \ldots, x_i] \prod_{j=0}^{i-1} (x - x_j)
$$

### Cubic Spline Conditions

For interval $[x_i, x_{i+1}]$:
- $S(x_i) = y_i$ (interpolation)
- $S'(x_i^-)= S'(x_i^+)$ (C¹ continuity)
- $S''(x_i^-) = S''(x_i^+)$ (C² continuity)

---

## 🐛 Known Issues & Limitations

1. **Large Datasets**: Performance degrades with >1000 points (Lagrange/Newton)
2. **Runge Phenomenon**: High-degree polynomials oscillate near edges (use Chebyshev nodes)
3. **Audio Formats**: MP3/FLAC require external codecs on some systems
4. **Memory Usage**: Large audio files (>100MB) may cause slowdowns

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

**Code Style:**
- Follow C# conventions (PascalCase for public members)
- Add XML documentation comments
- Include unit tests for new algorithms

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🎓 Academic Context

This project was developed as part of a Master's thesis in Numerical Analysis at the Faculty of Electrical Engineering, University of Sarajevo.

**Thesis Title**: *Advanced Numerical Interpolation Methods with Applications in Audio Signal Processing*

**Supervisor**: [Supervisor Name]

**Year**: 2024-2025

---

## 📧 Contact

**Author**: Nejra Smajlović

**University**: PTF UNZE, Software Engineering

**Project Link**: [https://github.com/YOUR_USERNAME/InterpolationMasterProject](https://github.com/YOUR_USERNAME/InterpolationMasterProject)

---

## 🙏 Acknowledgments

- **MaterialDesignInXAML**: For the beautiful UI components
- **OxyPlot**: For powerful plotting capabilities
- **NAudio**: For robust audio processing
- **Faculty of Electrical Engineering**: For academic support

---

<div align="center">

**⭐ If you find this project useful, please consider giving it a star! ⭐**

Made with ❤️ for Numerical Analysis

</div>
