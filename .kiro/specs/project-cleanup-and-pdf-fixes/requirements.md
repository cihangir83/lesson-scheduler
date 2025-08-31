# Requirements Document

## Introduction

Bu özellik, projedeki gereksiz dosyaları temizleme ve PDF çıktılarındaki sorunları düzeltme amacıyla geliştirilecektir. Kullanıcı deneyimini iyileştirmek ve projeyi daha temiz hale getirmek hedeflenmektedir.

## Requirements

### Requirement 1: Proje Temizliği

**User Story:** Geliştirici olarak, projemde gereksiz dosyaların olmamasını istiyorum, böylece proje daha temiz ve yönetilebilir olsun.

#### Acceptance Criteria

1. WHEN proje taranır THEN kullanılmayan test dosyaları tespit edilir ve silinir
2. WHEN test_basic.json dosyası kontrol edilir THEN artık kullanılmıyorsa silinir
3. WHEN eski test dosyaları kontrol edilir THEN gereksiz olanlar silinir
4. WHEN proje yapısı gözden geçirilir THEN sadece aktif kullanılan dosyalar kalır

### Requirement 2: PDF Türkçe Karakter Desteği

**User Story:** Kullanıcı olarak, PDF çıktılarında Türkçe karakterlerin doğru görünmesini istiyorum, böylece raporlarım düzgün okunabilir olsun.

#### Acceptance Criteria

1. WHEN PDF oluşturulur THEN Türkçe karakterler (ç, ğ, ı, ö, ş, ü) doğru görüntülenir
2. WHEN öğretmen isimleri PDF'e yazılır THEN Türkçe karakterler bozulmaz
3. WHEN ders isimleri PDF'e yazılır THEN Türkçe karakterler düzgün çıkar
4. WHEN sınıf isimleri PDF'e yazılır THEN Türkçe karakterler doğru görünür

### Requirement 3: PDF Başlık Düzenleme

**User Story:** Kullanıcı olarak, PDF çıktılarında gereksiz başlıkların çıkmamasını istiyorum, böylece daha temiz görünüm elde edeyim.

#### Acceptance Criteria

1. WHEN sınıf programı PDF'i oluşturulur THEN "Sınıf Programları" başlığı çıkmaz
2. WHEN öğretmen programı PDF'i oluşturulur THEN "Öğretmen Programları" başlığı çıkmaz
3. WHEN PDF ilk sayfası oluşturulur THEN sadece gerekli içerik görünür
4. WHEN PDF başlıkları kontrol edilir THEN sadece okul adı ve tarih bilgileri görünür

### Requirement 4: PDF Metin Düzeni Düzeltme

**User Story:** Kullanıcı olarak, PDF'deki metinlerin sayfadan taşmamasını istiyorum, böylece tüm bilgiler okunabilir olsun.

#### Acceptance Criteria

1. WHEN müdür adı PDF'e yazılır THEN metin sayfadan taşmaz
2. WHEN uzun isimler PDF'e yazılır THEN otomatik satır kaydırma yapılır
3. WHEN PDF düzeni kontrol edilir THEN tüm metinler sayfa sınırları içinde kalır
4. WHEN metin boyutu ayarlanır THEN içerik sayfaya sığacak şekilde ölçeklenir

### Requirement 5: Öğretmen Program PDF'inde İsim Ekleme

**User Story:** Kullanıcı olarak, öğretmen programı PDF'inin alt kısmında hangi öğretmenin programı olduğunu görmek istiyorum, böylece programın kime ait olduğunu bileyim.

#### Acceptance Criteria

1. WHEN öğretmen programı PDF'i oluşturulur THEN alt kısımda öğretmen adı görünür
2. WHEN PDF footer'ı oluşturulur THEN öğretmen adı müdür adının yanında yer alır
3. WHEN bireysel öğretmen PDF'i çıkarılır THEN ilgili öğretmenin adı belirtilir
4. WHEN PDF kimlik bilgileri kontrol edilir THEN hem okul hem öğretmen bilgileri görünür