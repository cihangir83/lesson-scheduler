# 🎓 Akıllı Ders Dağıtım Programı

Okullarda ders programlarını otomatik olarak oluşturan, öğretmen uygunlukları ve kısıtlamaları dikkate alan akıllı sistem.

## 📋 Program Hakkında

Bu program, Google OR-Tools optimizasyon kütüphanesi kullanarak okullarda en optimal ders dağılımını otomatik olarak hesaplar. Öğretmen uygunlukları, ders saatleri ve okul kısıtlamalarını dikkate alarak çakışmasız ders programları oluşturur.

**Geliştirici:** Mahir Cihangir Saraç

## ✨ Özellikler

- 🤖 **Akıllı Algoritma** - Google OR-Tools ile optimizasyon
- ⏰ **Zaman Tasarrufu** - Manuel süreçleri otomatikleştirir
- 👥 **Öğretmen Uygunlukları** - Gün/saat bazında kısıtlamalar
- 📄 **PDF Çıktı** - Profesyonel program raporları
- ⚙️ **Esnek Ayarlar** - Okul saatleri ve ders blokları
- 🔒 **Güvenli ve Ücretsiz** - Veriler sadece bilgisayarınızda kalır

## 🚀 Hızlı Başlangıç

### İndirme
**Seçenek 1:** [GitHub Releases](https://github.com/cihangir83/lesson-scheduler/releases) - Resmi indirme  
**Seçenek 2:** [Google Drive](https://drive.google.com/uc?export=download&id=1fEctjsM7svzqzY1ZDbJydn82KcISwmos) - Alternatif indirme

### Kurulum
1. `LessonScheduler_Setup.exe` dosyasını çalıştırın
2. Kurulum sihirbazını takip edin
3. Gerekli bileşenler otomatik kurulacaktır

### Sistem Gereksinimleri
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (otomatik kurulur)
- Visual C++ Redistributable 2022 (otomatik kurulur)

## 📖 Kullanım

### Temel Adımlar
1. **⚙️ Ayarlar** - Okul bilgileri ve günlük ders saatleri
2. **📚 Tanımlamalar** - Dersler, öğretmenler ve sınıflar
3. **📝 Ders Atamaları** - Öğretmen-ders ilişkileri
4. **⏰ Kısıtlamalar** - Öğretmen uygunlukları
5. **🚀 Çözüm Üret** - "Dağıtımı Başlat" ile otomatik program
6. **📄 PDF Çıktı** - Programları kaydet

### Detaylı Kullanım Klavuzu
Kapsamlı kullanım rehberi için [KULLANIM_KLAVUZU.md](KULLANIM_KLAVUZU.md) dosyasını inceleyin.

## 🌐 Tanıtım Sitesi

Program hakkında detaylı bilgi ve canlı demo için:
**https://cihangir83.github.io/lesson-scheduler-website/**

## 🛠️ Teknik Detaylar

### Kullanılan Teknolojiler
- **.NET 8.0** - Ana framework
- **WPF** - Kullanıcı arayüzü
- **Google OR-Tools** - Optimizasyon motoru
- **iTextSharp** - PDF oluşturma
- **Newtonsoft.Json** - Veri işleme

### Proje Yapısı
```
LessonScheduler/
├── Models/           # Veri modelleri
├── Services/         # İş mantığı servisleri
├── ViewModels/       # MVVM view modelleri
├── Views/           # WPF arayüz dosyaları
├── Utilities/       # Yardımcı sınıflar
├── Resources/       # Kaynaklar (ikon, vb.)
└── Themes/          # UI temaları
```

## 📊 Özellik Listesi

### ✅ Mevcut Özellikler
- [x] Otomatik ders dağıtımı
- [x] Öğretmen uygunluk kontrolü
- [x] Sınıf ve öğretmen programları
- [x] PDF export (toplu/ayrı)
- [x] JSON veri kaydetme/yükleme
- [x] Kalite raporu ve istatistikler
- [x] Esnek okul saati ayarları
- [x] Ders blok yapılandırması

### 🔄 Gelecek Özellikler
- [ ] Excel import/export
- [ ] Çoklu okul desteği
- [ ] Web tabanlı arayüz
- [ ] Mobil uygulama
- [ ] Bulut senkronizasyonu

## 🤝 Katkıda Bulunma

Bu proje açık kaynak kodludur ve katkılarınızı memnuniyetle karşılarız!

### Nasıl Katkıda Bulunabilirsiniz?
1. Repository'yi fork edin
2. Yeni bir branch oluşturun (`git checkout -b feature/yeni-ozellik`)
3. Değişikliklerinizi commit edin (`git commit -am 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/yeni-ozellik`)
5. Pull Request oluşturun

### Geliştirme Ortamı
- Visual Studio 2022 veya VS Code
- .NET 8.0 SDK
- Git

## 📝 Lisans

Bu proje MIT lisansı altında yayınlanmıştır. Detaylar için [LICENSE](LICENSE) dosyasını inceleyin.

## 📞 İletişim ve Destek

- **Issues:** Hata raporları ve özellik istekleri için GitHub Issues kullanın
- **Discussions:** Genel sorular ve tartışmalar için GitHub Discussions
- **Email:** Özel konular için geliştirici ile iletişime geçin

## 🏆 Teşekkürler

- **Google OR-Tools** ekibine optimizasyon kütüphanesi için
- **Microsoft** .NET ekibine platform desteği için
- Tüm test kullanıcılarına geri bildirimler için

---

## 📈 İstatistikler

![GitHub release (latest by date)](https://img.shields.io/github/v/release/cihangir83/lesson-scheduler)
![GitHub all releases](https://img.shields.io/github/downloads/cihangir83/lesson-scheduler/total)
![GitHub](https://img.shields.io/github/license/cihangir83/lesson-scheduler)
![GitHub last commit](https://img.shields.io/github/last-commit/cihangir83/lesson-scheduler)

**⭐ Projeyi beğendiyseniz yıldız vermeyi unutmayın!**