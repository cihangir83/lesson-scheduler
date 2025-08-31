# Design Document

## Overview

Bu tasarım, mevcut çalışan sistemi bozmadan proje temizliği ve PDF iyileştirmeleri yapmayı hedefler. Tüm değişiklikler geriye dönük uyumlu olacak ve mevcut fonksiyonalite korunacaktır.

## Architecture

### Güvenli Yaklaşım
- Önce dosya analizi yapılacak, hangi dosyaların kullanıldığı tespit edilecek
- PDF değişiklikleri sadece PdfExportService.cs dosyasında yapılacak
- Hiçbir core fonksiyonalite değiştirilmeyecek
- Test edilmiş çalışan kodlara dokunulmayacak

## Components and Interfaces

### 1. Proje Temizlik Komponenti
- **Amaç**: Kullanılmayan dosyaları güvenli şekilde tespit etme
- **Yaklaşım**: Sadece kesin olarak kullanılmayan dosyaları silme
- **Güvenlik**: Şüpheli durumda dosyayı silmeme

### 2. PDF Font ve Encoding Düzeltmesi
- **Mevcut Durum**: PdfExportService.cs'de font ayarları
- **Hedef**: Türkçe karakter desteği ekleme
- **Yöntem**: Font ayarlarını güncelleme, encoding düzeltme

### 3. PDF Layout Düzeltmesi
- **Mevcut Durum**: Başlıklar ve metin düzeni
- **Hedef**: Gereksiz başlıkları kaldırma, metin taşmasını önleme
- **Yöntem**: PDF oluşturma kodlarını güncelleme

## Data Models

Mevcut veri modelleri değiştirilmeyecek. Sadece PDF çıktı formatı iyileştirilecek.

## Error Handling

- Dosya silme işlemlerinde hata durumunda işlem durdurulacak
- PDF oluşturma hatalarında fallback mekanizması korunacak
- Mevcut hata yönetimi sistemi değiştirilmeyecek

## Testing Strategy

- Her değişiklik sonrası mevcut test verilerinin çalıştığı doğrulanacak
- PDF çıktıları test edilecek
- Türkçe karakterler kontrol edilecek
- Çözüm bulma fonksiyonalitesi test edilecek