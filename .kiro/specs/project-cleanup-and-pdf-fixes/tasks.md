# Implementation Plan

- [x] 1. Proje dosya analizi ve temizlik



  - Kullanılmayan dosyaları tespit et ve güvenli şekilde sil
  - test_basic.json ve diğer gereksiz test dosyalarını kontrol et
  - Sadece kesin olarak kullanılmayan dosyaları sil
  - _Requirements: 1.1, 1.2, 1.3, 1.4_



- [x] 2. PDF Türkçe karakter desteği ekleme

  - PdfExportService.cs'de font ayarlarını güncelle
  - Türkçe karakter encoding'ini düzelt
  - Font seçimini Türkçe karakterleri destekleyecek şekilde ayarla
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [x] 3. PDF başlık düzenleme

  - "Sınıf Programları" ve "Öğretmen Programları" başlıklarını kaldır
  - PDF başlık yapısını sadeleştir
  - Gereksiz başlık metinlerini temizle
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [x] 4. PDF metin düzeni düzeltme


  - Müdür adı ve uzun metinlerin sayfadan taşmasını önle
  - Otomatik metin boyutlandırma ekle
  - Sayfa sınırları içinde kalacak şekilde düzenle
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ] 5. PDF footer düzenleme
  - Öğretmen adını sol tarafa, müdür adını sağ tarafa yerleştir
  - Aynı hizada ama yan yana değil (imza alanları için)
  - Footer düzenini her iki PDF türü için ayarla
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [ ] 6. Bireysel PDF export fonksiyonlarını iyileştir
  - Mevcut "Ayrı Dosyalar" seçeneklerinin düzgün çalıştığını kontrol et
  - Her öğretmen için ayrı PDF'de sadece o öğretmenin programının çıktığını doğrula
  - Her sınıf için ayrı PDF'de sadece o sınıfın programının çıktığını doğrula
  - Bireysel PDF'lerde doğru başlık ve footer bilgilerinin olduğunu kontrol et
  - _Requirements: 5.1, 5.2, 5.3, 5.4_