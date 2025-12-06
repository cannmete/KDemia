using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace KDemia.Extensions  // Bu Extension' un var olma amacı NETCore.Encrypt.Extensions' un uygun bir sürümü bulunamadığı içindir.
{
    public static class EncryptExtensions
    {
        public static string ToSHA256(this string password)
        {
            if (string.IsNullOrEmpty(password)) return "";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}

//Harika bir soru. Mülakatlarda hocalar genellikle "Neden Extension kullandın?" ve "Bu kod satırı ne işe yarıyor?" diye sorarlar.

//Sana mülakatta bu sınıfı açtırdığında, parmağınla satırları göstererek şunları söylemelisin. İşte senin için hazır mülakat senaryosu:

//1.Genel Giriş(İlk Cümle)
//"Hocam, projede güvenlik için dışarıdan hazır bir paket bulamayınca, .NET'in kendi kütüphanesini (System.Security.Cryptography) kullanarak bir Extension Method (Genişletme Metodu) yazdım. Bu metod, girdiğimiz şifreyi SHA-256 algoritmasıyla şifreliyor."

//2.Satır Satır Teknik Açıklama
//Hoca "Peki kodları anlat" derse şu sırayla git:

//public static class EncryptExtensions

//Sen: "Extension metodlar yazabilmek için sınıfın static olması zorunludur. O yüzden static yaptım."

//public static string ToSHA256(this string password)

//Sen: "Buradaki this ifadesi çok önemli. Bu sayede projenin herhangi bir yerinde string bir ifadenin sonuna nokta koyup sifre.ToSHA256() diyerek bu metodu çağırabiliyorum. Yani string veri tipine yeni bir özellik kazandırmış oldum."

//using (SHA256 sha256 = SHA256.Create())

//Sen: "Burada SHA256 algoritmasının bir örneğini oluşturuyorum. using bloğu içine aldım ki, şifreleme işlemi bitince bu nesne RAM'den (bellekten) hemen silinsin, performans artsın."

//byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

//Sen: "Bilgisayarlar ve şifreleme algoritmaları harflerle değil, sayılarla (byte) çalışır.

//Önce GetBytes ile gelen şifreyi (string) byte dizisine çevirdim.

//Sonra ComputeHash ile bu veriyi şifreledim."

//StringBuilder builder = new StringBuilder();

//Sen: "Şifrelenen veri elimde anlamsız byte'lar olarak duruyor. Bunları yan yana ekleyip tekrar okunabilir bir metin haline getirmek için StringBuilder kullandım. (Normal string birleştirme işlemi belleği yorar, StringBuilder daha performanslıdır.)"

//builder.Append(bytes[i].ToString("x2")); (EN KRİTİK YER BURASI)

//Sen: "Hocam burası püf noktası. Elimdeki byte'ları Hexadecimal (16'lık taban) formata çeviriyorum.

//x2: Her bir byte'ı 2 haneli bir hex koduna çevir demektir. Böylece şifremiz veritabanında saklanabilir, düzgün bir metne dönüşüyor."

//3. Hoca "Neden MD5 kullanmadın?" derse:
//(Eğer hoca kendi kodunu sorarsa) Cevabın: "Hocam araştırdım, MD5 algoritması artık eski kabul ediliyor ve kırılması daha kolaymış. SHA-256 günümüz standartlarında çok daha güvenli olduğu için bunu tercih ettim."

//Özetle Mülakat Taktikleri:
//Extension Method: "Var olan tipe (string) yeni özellik eklemek."

//Using Bloğu: "Bellek yönetimi (Garbage Collection) için."

//StringBuilder: "String birleştirme performansını artırmak için."

//x2: "Byte verisini 16'lık sayı sistemine (Hexadecimal) çevirmek için."

//Bu açıklamalarla hocadan tam puan alırsın. Çok profesyonel duruyor.