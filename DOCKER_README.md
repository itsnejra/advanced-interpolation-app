# 🐳 Docker Setup - Advanced Interpolation Suite

## 📋 Pregled

Ovaj projekat sadrži WPF desktop aplikaciju koja je **djelimično dockerizovana**.

### ⚠️ Važno razumijevanje

**WPF aplikacije NISU idealne za Docker** jer zahtijevaju:
- Windows OS sa GUI
- Grafički display
- Interakciju sa korisnikom

**Kreirao sam 3 pristupa:**

1. **Windows Container** - Za native WPF (Dockerfile.windows)
2. **Linux Container** - Za headless/CLI mod (Dockerfile)  
3. **Web API** - Najbolji za produkciju (planiran)

---

## 🚀 QUICK START

### Opcija 1: Windows Container (ZAHTIJEVA Windows Docker)

```powershell
# 1. Switcha Docker Desktop na Windows containers
# Right-click Docker Desktop icon → "Switch to Windows containers..."

# 2. Build
docker build -f Dockerfile.windows -t interpolation-app:windows .

# 3. Run (zahtijeva display forwarding)
docker run -it --name interpolation-app interpolation-app:windows
```

**Napomena:** Ova opcija je komplikovana za GUI pristup. Zahtijeva RDP ili VNC setup.

---

### Opcija 2: Linux Container (EKSPERIMENTALNO)

```bash
# Build
docker build -t interpolation-app:linux .

# Run
docker run -it --rm --name interpolation-app interpolation-app:linux
```

**Napomena:** Ovo neće pokrenuti GUI, već CLI verziju (ako postoji).

---

### Opcija 3: Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

---

## 📁 Struktura Fajlova

```
.
├── Dockerfile                  # Linux container (Alpine-based)
├── Dockerfile.windows          # Windows container (Server Core)
├── docker-compose.yml          # Multi-service orchestration
├── .dockerignore              # Exclude files from build
├── DOCKER_GUIDE.md            # Detaljna dokumentacija
└── API_PLAN.md                # Plan za Web API verziju
```

---

## 🎯 Preporučeni Pristup Za Produkciju

### KREIRAJ WEB API VERZIJU

Najbolji pristup je kreirati **ASP.NET Core Web API** koji koristi iste algoritme:

**Prednosti:**
- ✅ Linux containers (manji, brži)
- ✅ Cloud-ready (Azure, AWS, GCP)
- ✅ REST API endpoints
- ✅ Swagger dokumentacija
- ✅ Horizontalno skalabilno
- ✅ CI/CD friendly

**Struktura:**
```
InterpolationApp.Core/          # Shared library sa algoritmima
InterpolationApp/               # WPF Desktop (lokalno)
InterpolationApi/               # ASP.NET Core Web API (Docker)
```

---

## 🔧 Development

### Lokal Development (bez Docker-a)

```powershell
# Restore packages
dotnet restore

# Build
dotnet build

# Run WPF app
dotnet run --project InterpolationApp
```

### Docker Development

```bash
# Build image
docker build -t interpolation-dev .

# Run sa volume mounting (za development)
docker run -it --rm \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/output:/app/output \
  interpolation-dev
```

---

## 📊 Image Sizes

| Image | Size | OS | Runtime |
|-------|------|-----|---------|
| `interpolation-app:linux` | ~150 MB | Alpine Linux | .NET 8 |
| `interpolation-app:windows` | ~5 GB | Windows Server Core | .NET 8 |

---

## 🐛 Troubleshooting

### Problem: "Switch to Windows containers" opcija nije dostupna

**Rješenje:** Docker Desktop mora biti instaliran sa Windows containers support.

### Problem: GUI se ne pojavljuje u Windows container

**Rješenje:** Trebaš RDP pristup ili VNC server u containeru.

### Problem: Build fails sa WPF errors

**Rješenje:** Koristi Dockerfile.windows jer Linux ne podržava WPF native.

---

## 📚 Dodatni Resursi

- [DOCKER_GUIDE.md](DOCKER_GUIDE.md) - Detaljna dokumentacija
- [API_PLAN.md](API_PLAN.md) - Plan za Web API verziju
- [MATEMATICKA_VERIFIKACIJA.md](MATEMATICKA_VERIFIKACIJA.md) - Verifikacija algoritama

---

## 🎓 Za Master Rad

**Preporuka:** Umjesto dockerizacije WPF aplikacije, razmotri:

1. **Zadrži WPF** za desktop GUI (za demonstraciju)
2. **Kreiraj Web API** sa istim algoritmima
3. **Dockerizuj Web API** (jednostavnije i produktivnije)
4. **Deploy na cloud** (Azure App Service, AWS ECS, Google Cloud Run)

Ovo ti omogućava da pokažeš:
- ✅ Desktop aplikaciju (WPF)
- ✅ Web API (REST)
- ✅ Docker containerization
- ✅ Cloud deployment
- ✅ Microservices architecture

---

## ⚡ Next Steps

Javi mi koju opciju želiš:

**A) Kreiraj Web API projekat** (preporučeno)
- Novi ASP.NET Core projekat
- REST endpoints za sve interpolacije
- Swagger dokumentacija
- Optimizovan Dockerfile
- Ready za deployment

**B) Nastavi sa Windows Container setup**
- RDP/VNC konfiguracija
- Display forwarding
- Windows-specific optimizacije

**C) Headless CLI verzija**
- Command-line interface
- Batch processing
- Simple Docker image

Javi mi šta biraš! 🚀
