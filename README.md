# Kütüphane Yönetim Sistemi

Kütüphane Yönetim Sistemi, kütüphanelerdeki kitapların takibi ve yönetimi için geliştirilmiş bir .NET Core MVC tabanlı web uygulamasıdır. Bu sistem sayesinde, kitapların kütüphanede olup olmadığı, ödünç verilmiş kitapların takibi, yeni kitap eklemeleri ve kitapların ödünç verilme süreçleri kolaylıkla yönetilebilir.

## Özellikler

- **Kitap Listeleme:** Uygulamanın ana sayfasında, kitaplar alfabetik sıraya göre listelenir. Her kitap için ad, yazar, resim ve mevcut durumu (kütüphanede veya ödünç verilmiş) görüntülenir.
  
- **Kitap Ödünç Verme:** Kullanıcılar, listelenen kitaplardan birini seçerek ödünç verebilirler. Ödünç verme işlemi sırasında, ödünç alan kişinin adı ve kitabın geri getirileceği tarih sisteme kaydedilir.

- **Ödünç Verilen Kitap Bilgileri:** Eğer bir kitap ödünç verilmişse, bu kitabın ödünç alan kişinin adı ve geri getireceği tarih kitap listesinde gösterilir.

- **Yeni Kitap Ekleme:** Kullanıcılar, sistemde yeni kitaplar ekleyebilir. Kitap ekleme işlemi için bir form aracılığıyla kitap adı, yazarı ve resmi yüklenebilir.

- **Ödünç Kitap Durumunu Güncelleme:** Kütüphane çalışanları, ödünç verilen kitapların durumunu güncelleyebilir ve kitapları sistemde tekrar müsait olarak işaretleyebilir.

## Teknik Detaylar

- **Controller:** `BookController` sınıfı, kitapların listelenmesi, ödünç verilmesi, yeni kitap eklenmesi ve ödünç kitap durumunun güncellenmesi gibi temel işlevleri yönetir.

- **Modeller:** `Book` ve `BorrowedBook` modelleri, kitapların ve ödünç kitap işlemlerinin veritabanında nasıl temsil edileceğini tanımlar.

- **ViewModel:** `BookViewModel` ve `BorrowBookViewModel`, kullanıcı arayüzünde kitap eklemek ve ödünç vermek için gerekli veri yapısını sağlar.

- **View:** `index.cshtml` ve `create.cshtml` gibi Razor View'ları, kullanıcı arayüzünün nasıl görüneceğini ve çalışacağını belirler.

## Kurulum ve Kullanım

Projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyin:

1. Projeyi GitHub'dan klonlayın veya indirin.
2. Projeyi .NET Core destekli bir IDE'de açın (örneğin Visual Studio).
3. Gerekli NuGet paketlerini yükleyin.
4. Uygulamayı başlatın ve `https://localhost:[port]/` adresinden erişin.

## Kullanılan Teknolojiler

- **Backend:** .NET Core MVC
- **Veritabanı:** MSSQL veya PostgreSQL
- **ORM:** Entity Framework
- **Frontend:** HTML5, Bootstrap, jQuery

## Geliştirme Notları

- **Exception Handling:** Uygulama içinde oluşabilecek her türlü exception yakalanır ve logger ile kaydedilir.

- **Güvenlik:** CSRF koruması, güvenli form gönderimleri için `ValidateAntiForgeryToken` özniteliği kullanılarak sağlanır.

- **Kullanılabilirlik:** Kullanıcı dostu arayüz, hızlı ve rahat navigasyon için tasarlanmıştır.

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.
