# LayeredAppTemplate

LayeredAppTemplate, kurumsal düzeyde, yeniden kullanılabilir ve genişletilebilir bir N-Layer mimari şablonudur. Bu proje, modern .NET uygulamaları için en iyi uygulamaları ve endüstri standartlarını içerir. Aşağıdaki modüller entegre edilmiştir:

- **ASP.NET Core Web API**
- **API Versiyonlaması**
- **JWT Authentication**
- **Swagger Dokümantasyonu (Versiyonlama & JWT Security)**
- **Global Exception Handling Middleware**
- **FluentValidation**
- **Serilog ile Loglama**
- **Caching (In-Memory)**
- **Rate Limiting (AspNetCoreRateLimit)**
- **Health Checks & Health Checks UI**

Proje, farklı uygulamalarda ortak kullanılabilecek, modüler, test edilebilir ve ölçeklenebilir bir altyapı sunar.

---

## Özellikler

- **Katmanlı Mimari:**  
  Domain, Application, Infrastructure, Persistence, UI (opsiyonel) ve SharedKernel gibi katmanlar.
- **API Versiyonlaması:**  
  Controller'larda `[ApiVersion]` ve `[Route("api/v{version:apiVersion}/[controller]")]` kullanılarak API sürümleri yönetilir.
- **JWT Authentication:**  
  Token tabanlı kimlik doğrulama ile güvenli erişim sağlanır.
- **Swagger Entegrasyonu:**  
  API dokümantasyonu, versiyon bilgisi ve JWT security desteği.
- **Global Exception Handling:**  
  Tüm hatalar merkezi olarak yakalanır ve loglanır.
- **FluentValidation:**  
  DTO'lar için esnek ve okunabilir validasyon kuralları.
- **Serilog:**  
  Konsol ve dosya üzerinden yapılandırılabilir loglama.
- **Caching:**  
  In-Memory cache ile performans artışı.
- **Rate Limiting:**  
  IP tabanlı istek sınırlandırması ile kötüye kullanımı önleme.
- **Health Checks & UI:**  
  Uygulamanın sağlık durumu, JSON endpoint (örneğin, `/health`) ve görsel arayüz (örneğin, `/health-ui`) üzerinden izlenir.

---

## Proje Yapısı

