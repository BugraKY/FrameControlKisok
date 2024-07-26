using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameControlKiosk.Models
{
    public class ReportMain
    {//Mantıksal olarak bir veri tabanında aynı anda 6 dan fazla kontrol sürecinde olamaz. yani 6 dan fazla Done False olmamalı.
        [Key]
        public long Id { get; set; }
        public int StationId { get; set; }//StationId kaç ise şuan o stationda işlemde demektir. 0 ise işlemde değil demektir.
        public long ComponentId { get; set; } //Parça adı ve parça no gibi bilgiler buraya kaydolacak. Rapor doldurmaya başlamadan önce buradaki id tanımlanmalı!
        public string? FrameNo { get; set; }//Frame no her parça kontrolünde değişiyor. Sabit değil ve buda rapor doldurmaya başlamadan önce tanımlanmalı.
        public DateTime StartingDate { get; set; }//Kontrol başlangıç zamanı
        public DateTime EndingDate { get; set; }//Kontrol bitiş zamanı Done true olduğu takdirde ikiside beraber doldurulmalı.
        public bool Done { get; set; } //Kontrol tamamen bittiği zaman true olacak. False olması durumunda kontrolün bitmediği ve süreçte olduğu anlamına gelir.
        public bool ProcessingDone { get; set; }//Burası true olduğunda o istasyondaki iş bitmiş olur. Yani bütün definition formlar tamamlanmış olduğu takdirde burası true olmalı
        public bool Cancelled { get; set; } //Kontrol iptal edilmesi halinde true olacak. Aynı zamanda hangi processline da iptal olduğu bilgiside bu şekilde kaydolmuş olacak
        public int ProcessedLine { get; set; }// Reportmain her bir istasyon bitrişinde güncellenerek hangi process te olduğu bilgisi buraya güncellenecek.
                                              //Eğer Done ve Cancelled true ise proje kontrolü aslen iptal edilmiş anlamına gelir.
                                              //Eğer EndingDate tarih girilirse, Done true olursa, Cancelled false olursa (Bu 3 koşul yerine getirilirse) kontrol başarılı bir şekilde tamamlanmış demektir.
                                              //Eğer ProcessingDone true ise processedline daki değer tamamlanmış demektir.
        [NotMapped]
        public List<ReportDetail>? ReportDetails { get; set; }
        [NotMapped]
        public List<CoordinateCheck>? CoordinateCheck { get; set; }
        [NotMapped]
        public Component? Component { get; set; }

    }
}
