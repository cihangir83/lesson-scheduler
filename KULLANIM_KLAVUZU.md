# 🎓 Akıllı Ders Dağıtım Programı - Kullanım Klavuzu

## 📋 Program Hakkında
Bu program, okullarda ders programlarını otomatik olarak oluşturan akıllı bir sistemdir. Öğretmen uygunlukları, ders saatleri ve okul kısıtlamalarını dikkate alarak en optimal ders dağılımını yapar.

**Geliştirici:** Mahir Cihangir Saraç

---

## 🚀 Kurulum ve İlk Çalıştırma

### Sistem Gereksinimleri
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (otomatik kurulur)
- Visual C++ Redistributable 2022 (otomatik kurulur)

### Kurulum Adımları
1. `LessonScheduler_Setup.exe` dosyasını çalıştırın
2. Kurulum sihirbazını takip edin
3. Gerekli bileşenler otomatik olarak kontrol edilip kurulacaktır
4. Kurulum tamamlandığında program otomatik başlayacaktır

---

## 🎯 Program Arayüzü

Program açıldığında karşınıza çıkan ana pencerede 7 sekme bulunur:

### 📑 Sekmeler
1. **⚙️ Ayarlar** - Okul bilgileri ve günlük ders saati ayarları
2. **📚 1. Tanımlamalar** - Dersler, öğretmenler ve sınıflar
3. **📝 2. Ders Atamaları** - Öğretmen-ders ilişkileri ve saatler
4. **⏰ 3. Kısıtlamalar** - Öğretmen uygunlukları
5. **🏫 Sonuç: Sınıf Programları** - Oluşturulan sınıf programları
6. **👨‍🏫 Sonuç: Öğretmen Programları** - Öğretmen programları
7. **📊 Sonuç: Kalite Raporu** - Çözüm kalitesi ve istatistikler

---

## 📖 Adım Adım Kullanım

### 1️⃣ ADIM 1: Ayarlar (⚙️ Ayarlar Sekmesi)

#### Okul Bilgileri
- **Okul Adı:** PDF çıktılarında görünecek okul adını girin
- **Müdür Adı:** PDF çıktılarında görünecek müdür adını girin

#### Günlük Ders Saati Ayarları
- Her gün için farklı ders saati sayısı belirleyebilirsiniz
- Kaydırıcıları kullanarak günlük ders saatlerini ayarlayın
- Varsayılan: Pazartesi-Cuma 8 saat, Cumartesi 6 saat

**💡 İpucu:** Ayarları değiştirdikten sonra "Kaydet" butonuna basmayı unutmayın!

### 2️⃣ ADIM 2: Tanımlamalar (📚 1. Tanımlamalar Sekmesi)

Bu sekmede 3 ana bölüm vardır:

#### 📚 Dersler Bölümü
- **➕ Ekle:** Yeni ders eklemek için
- **✏️ Düzenle:** Seçili dersi düzenlemek için
- **🗑️ Sil:** Seçili dersi silmek için

**Örnek Dersler:**
- Matematik, Türkçe, Fen Bilgisi
- İngilizce, Sosyal Bilgiler
- Beden Eğitimi, Müzik, Görsel Sanatlar

#### 👨‍🏫 Öğretmenler Bölümü
- **➕ Ekle:** Yeni öğretmen eklemek için
- **✏️ Düzenle:** Seçili öğretmeni düzenlemek için
- **🗑️ Sil:** Seçili öğretmeni silmek için

#### 🏫 Sınıflar Bölümü
- **➕ Ekle:** Yeni sınıf eklemek için
- **✏️ Düzenle:** Seçili sınıfı düzenlemek için
- **🗑️ Sil:** Seçili sınıfı silmek için

### 3️⃣ ADIM 3: Ders Atamaları (📝 2. Ders Atamaları Sekmesi)

#### Sınıf Seçimi
- Üst kısımdan ders atamalarını yapacağınız sınıfı seçin

#### Ders Atama Tablosu
Her satırda şu bilgiler bulunur:
- **Ders:** Hangi ders
- **Öğretmen:** Dersi verecek öğretmen
- **Toplam Saat:** Haftalık toplam ders saati
- **Blok Yapısı:** Derslerin nasıl bölüneceği (örn: "2,2,1" = 2+2+1 saatlik bloklar)

#### Ders Atama Butonları
- **➕ Yeni Atama:** Sınıfa yeni ders ataması ekler
- **✏️ Düzenle:** Seçili atamayı düzenler
- **🗑️ Sil:** Seçili atamayı siler

**💡 Blok Yapısı Örnekleri:**
- "2,2,1" → 2 saatlik 2 blok + 1 saatlik 1 blok
- "3,2" → 3 saatlik 1 blok + 2 saatlik 1 blok
- "1,1,1,1" → 4 adet 1 saatlik blok

### 4️⃣ ADIM 4: Kısıtlamalar (⏰ 3. Kısıtlamalar Sekmesi)

#### Öğretmen Uygunluk Tablosu
- Her öğretmen için hangi gün ve saatlerde uygun olduğunu belirleyin
- ✅ Yeşil: Uygun
- ❌ Kırmızı: Uygun değil
- Hücrelere tıklayarak durumu değiştirebilirsiniz

#### Günler ve Saatler
- **Günler:** Pazartesi'den Cumartesi'ye
- **Saatler:** 1. saatten 8. saate kadar (ayarlara göre değişir)

### 5️⃣ ADIM 5: Çözüm Üretme

#### Menü → İşlemler → Dağıtımı Başlat
1. Üst menüden "İşlemler" → "Dağıtımı Başlat" seçin
2. Program otomatik olarak en optimal çözümü arayacaktır
3. İşlem tamamlandığında sonuç sekmelerinde programlar görünecektir

#### Alternatif: Test Verisi
- Hızlı test için "Dosya" → "Test Verisini Yükle" kullanabilirsiniz

---

## 📊 Sonuçları İnceleme

### 🏫 Sınıf Programları Sekmesi
- Her sınıfın haftalık ders programını görüntüler
- Tablo formatında gün/saat bazında düzenlenmiştir

### 👨‍🏫 Öğretmen Programları Sekmesi
- Her öğretmenin haftalık ders programını görüntüler
- Hangi saatte hangi sınıfta ders vereceğini gösterir

### 📊 Kalite Raporu Sekmesi
- Çözümün kalitesi hakkında detaylı bilgi
- Kısıtlamaların ne kadar karşılandığı
- Optimizasyon istatistikleri

---

## 📄 PDF Çıktı Alma

### Menü → PDF Export
Program 4 farklı PDF çıktı seçeneği sunar:

1. **📄 Sınıf Programları (Toplu)** - Tüm sınıflar tek dosyada
2. **📄 Sınıf Programları (Ayrı Dosyalar)** - Her sınıf ayrı dosya
3. **👨‍🏫 Öğretmen Programları (Toplu)** - Tüm öğretmenler tek dosyada
4. **👨‍🏫 Öğretmen Programları (Ayrı Dosyalar)** - Her öğretmen ayrı dosya

### PDF İçeriği
- Okul adı ve müdür adı (Ayarlar'dan alınır)
- Tarih bilgisi
- Düzenli tablo formatında program
- Profesyonel görünüm

---

## 💾 Veri Kaydetme ve Yükleme

### Dosya Menüsü
- **JSON Yükle:** Önceden kaydedilmiş veriyi yükler
- **JSON Kaydet:** Mevcut veriyi dosyaya kaydeder
- **Test Verisini Yükle:** Örnek veri seti yükler

### Otomatik Kaydetme
- Program ayarları otomatik olarak kaydedilir
- Çıkış yaparken veriler korunur

---

## ⚠️ Önemli Notlar ve İpuçları

### ✅ Başarılı Çözüm İçin
1. **Dengeli Atama:** Öğretmen başına düşen ders saatlerini dengeli dağıtın
2. **Yeterli Uygunluk:** Öğretmenlerin çoğu saatte uygun olmasını sağlayın
3. **Gerçekçi Saatler:** Toplam ders saatlerini günlük kapasiteye uygun tutun
4. **Blok Yapısı:** Uzun dersleri 2-3 saatlik bloklara bölün

### ❌ Yaygın Hatalar
- Öğretmen uygunluklarını çok kısıtlı tutmak
- Toplam ders saatlerinin günlük kapasiteyi aşması
- Aynı öğretmene çok fazla ders yüklemek
- Blok yapısını yanlış tanımlamak

### 🔧 Sorun Giderme
- **Çözüm bulunamıyor:** Kısıtlamaları gevşetin
- **Program donuyor:** Veri miktarını azaltın
- **PDF oluşmuyor:** Çözüm üretildiğinden emin olun

---

## 🆘 Teknik Destek

### Sistem Gereksinimleri Kontrolü
Program otomatik olarak gerekli bileşenleri kontrol eder ve kurar:
- .NET 8.0 Runtime
- Visual C++ Redistributable 2022

### Performans İpuçları
- Büyük okullar için (20+ sınıf) işlem süresi uzayabilir
- RAM kullanımı veri boyutuna göre değişir
- SSD kullanımı performansı artırır

---

## 📞 İletişim

**Geliştirici:** Mahir Cihangir Saraç

Program hakkında sorularınız için geliştirici ile iletişime geçebilirsiniz.

---

*Bu klavuz, Akıllı Ders Dağıtım Programı v4.0 için hazırlanmıştır.*