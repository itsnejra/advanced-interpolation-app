# 🚀 Advanced Interpolation Suite

> Numerical interpolation algorithms implementation with WPF GUI and Docker support

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Quick Start](#quick-start)
  - [Option 1: Docker (Recommended)](#option-1-docker-recommended)
  - [Option 2: Local Build](#option-2-local-build)
- [Algorithms](#algorithms)
- [Usage](#usage)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [License](#license)

---

## 🎯 Overview

Advanced Interpolation Suite is a comprehensive numerical analysis application featuring multiple interpolation algorithms with a modern WPF desktop interface.

**Key Features:**
- 5 interpolation algorithms (Linear, Lagrange, Newton, Hermite, Cubic Spline)
- Function sampling with Chebyshev nodes
- Polynomial degree optimizer
- Audio noise removal
- Real-time visualization with OxyPlot
- Material Design UI

---

## ✨ Features

### Interpolation Methods

1. **Linear Interpolation** - Fast piecewise linear
2. **Lagrange Interpolation** - Polynomial through all points
3. **Newton Divided Differences** - Efficient polynomial evaluation
4. **Hermite Interpolation** - C¹ continuous with derivatives
5. **Cubic Spline** - C² continuous, smoothest curves

### Additional Tools

- Function sampling from mathematical expressions
- Runge phenomenon demonstration
- Polynomial degree optimization
- Audio signal processing
- Data export (CSV, PNG)

---

## 🚀 Quick Start

### Option 1: Docker (Recommended)

**⚠️ Important:** This is a WPF desktop application. Docker support is **experimental** and has limitations for GUI applications.

#### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Windows 10/11 (for Windows containers)

#### Run with Docker

```bash
# Clone repository
git clone https://github.com/YOUR_USERNAME/advanced_interpolation_project.git
cd advanced_interpolation_project

# Build and run (Linux container - headless)
docker-compose up -d

# OR build manually
docker build -t interpolation-app .
docker run -it --rm interpolation-app
```

#### Windows Container (for full GUI support)

```powershell
# Switch Docker to Windows containers
# Right-click Docker Desktop icon → "Switch to Windows containers..."

# Build
docker build -f Dockerfile.windows -t interpolation-app:windows .

# Run
docker run -it --name interpolation-app interpolation-app:windows
```

**Note:** GUI access in Windows containers requires additional RDP/VNC setup. See [DOCKER_GUIDE.md](DOCKER_GUIDE.md) for details.

---

### Option 2: Local Build (Full GUI Experience)

#### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11
- Visual Studio 2022 (recommended) or VS Code

#### Steps

```powershell
# 1. Clone repository
git clone https://github.com/YOUR_USERNAME/advanced_interpolation_project.git
cd advanced_interpolation_project

# 2. Restore NuGet packages
dotnet restore

# 3. Build solution
dotnet build

# 4. Run application
dotnet run --project InterpolationApp

# OR open in Visual Studio
start InterpolationApp.sln
```

---

## 🧮 Algorithms

All algorithms are implemented from scratch without using external interpolation libraries.

### Linear Interpolation
```
y = y₀ + (y₁ - y₀) × (x - x₀) / (x₁ - x₀)
```
- **Complexity:** O(1) evaluation
- **Continuity:** C⁰
- **Best for:** Fast previews, simple data

### Lagrange Interpolation
```
P(x) = Σ yᵢ × Lᵢ(x)
where Lᵢ(x) = ∏(x - xⱼ) / (xᵢ - xⱼ) for j ≠ i
```
- **Complexity:** O(n²) evaluation
- **Continuity:** C∞
- **Best for:** Small datasets (n < 20)

### Newton Divided Differences
```
P(x) = f[x₀] + f[x₀,x₁](x-x₀) + f[x₀,x₁,x₂](x-x₀)(x-x₁) + ...
```
- **Complexity:** O(n²) preprocessing, O(n) evaluation
- **Continuity:** C∞
- **Best for:** Multiple evaluations

### Hermite Interpolation
```
H(t) = h₀₀(t)·y₀ + h₁₀(t)·h·m₀ + h₀₁(t)·y₁ + h₁₁(t)·h·m₁
```
- **Complexity:** O(1) per segment
- **Continuity:** C¹
- **Best for:** Smooth curves with derivative info

### Cubic Spline (Natural)
```
Sᵢ(x) = aᵢ + bᵢ(x-xᵢ) + cᵢ(x-xᵢ)² + dᵢ(x-xᵢ)³
```
- **Complexity:** O(n) preprocessing, O(log n) evaluation
- **Continuity:** C²
- **Best for:** Smoothest visualization

---

## 💻 Usage

### Manual Input Mode

1. Launch application
2. Navigate to **Manual Input** tab
3. Add data points manually or load sample data
4. Select interpolation algorithm
5. Click **INTERPOLATE**

### Function Sampling Mode

1. Navigate to **Function Sampling** tab
2. Enter mathematical expression (e.g., `sin(x)`, `1/(1+25*x^2)`)
3. Set interval `[xMin, xMax]`
4. Choose polynomial degree `n`
5. Enable Chebyshev nodes (optional)
6. Click **GENERATE & INTERPOLATE**

### Degree Optimizer

1. Navigate to **Degree Optimizer** tab
2. Enter function and interval
3. Set target maximum error `ε`
4. Specify search range
5. Click **FIND MINIMUM DEGREE**

### Audio Processing

1. Navigate to **Audio Processing** tab
2. Click **LOAD AUDIO FILE** (WAV, MP3, FLAC, AIFF)
3. Select interpolation algorithm
4. Adjust noise detection threshold
5. Click **REMOVE NOISE**
6. Click **SAVE PROCESSED AUDIO**

---

## 📚 Documentation

- [MATEMATICKA_VERIFIKACIJA.md](MATEMATICKA_VERIFIKACIJA.md) - Mathematical verification of algorithms
- [VERIFIKACIJA_OPISA.md](VERIFIKACIJA_OPISA.md) - Implementation description verification
- [DOCKER_README.md](DOCKER_README.md) - Docker quick start guide
- [DOCKER_GUIDE.md](DOCKER_GUIDE.md) - Detailed Docker documentation
- [API_PLAN.md](API_PLAN.md) - Web API implementation plan

---

## 🏗️ Project Structure

```
advanced_interpolation_project/
├── InterpolationApp/           # Main WPF application
│   ├── Algorithms/            # Interpolation implementations
│   ├── ViewModels/            # MVVM view models
│   ├── Views/                 # XAML views
│   ├── Models/                # Data models
│   ├── Services/              # Business logic
│   └── Helpers/               # Utility classes
├── AudioTestGenerator/         # Audio test file generator
├── Dockerfile                  # Linux container
├── Dockerfile.windows          # Windows container
├── docker-compose.yml          # Multi-service orchestration
└── docs/                       # Documentation

```

---

## 🛠️ Built With

- [.NET 8.0](https://dotnet.microsoft.com/) - Application framework
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) - UI framework
- [OxyPlot](https://oxyplot.github.io/) - Plotting library
- [Material Design](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - UI components
- [NAudio](https://github.com/naudio/NAudio) - Audio processing
- [MathNet.Numerics](https://numerics.mathdotnet.com/) - Mathematical functions

---

## 📊 NuGet Packages

```xml
<PackageReference Include="OxyPlot.Wpf" Version="2.1.2" />
<PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
<PackageReference Include="MaterialDesignColors" Version="2.1.4" />
<PackageReference Include="NAudio" Version="2.2.1" />
<PackageReference Include="MathNet.Numerics" Version="5.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.77" />
```

---

## 🎓 Academic Use

This project was developed as part of a Master's thesis in Numerical Analysis.

**Features for academic demonstration:**
- ✅ From-scratch implementation (no library interpolation functions)
- ✅ Mathematical verification included
- ✅ Algorithm comparison and benchmarking
- ✅ Real-world application (audio processing)
- ✅ Comprehensive documentation

---

## 🐛 Known Issues

### Docker Limitations

- **Linux containers:** Cannot run WPF GUI (headless mode only)
- **Windows containers:** Require RDP/VNC for GUI access
- **Recommendation:** Use local build for full GUI experience, or consider the Web API version (see [API_PLAN.md](API_PLAN.md))

### Runge Phenomenon

- Lagrange and Newton interpolations may exhibit oscillations for high-degree polynomials
- **Solution:** Use Chebyshev nodes or Cubic Spline

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Your Name**
- GitHub: itsnejra
- Email: nejra.smajlovic.22@size.ba

---

## 🙏 Acknowledgments

- Master's thesis advisor
- Numerical Analysis course materials
- .NET and WPF communities
- Material Design contributors



**Made with ❤️ for Numerical Analysis**
