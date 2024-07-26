using System.ComponentModel.DataAnnotations;

namespace FrameControlKiosk.Models
{
    public class Component
    {
        [Key]
        public long Id { get; set; }
        public int FrameID { get; set; }
        public string? Part { get; set; }//parça
        public string? PartNo { get; set; }//Parça No: 1108759
        public string? CustomerName { get; set; }//Müşteri Adı: SİRO
        public string? OperationArea { get; set; }//Operasyon Adı:COŞKUNÖZ
        public string? Reference { get; set; }//Referans No: 713001491
        public string? OperationTEL { get; set; }//Coşkunöz Tel: 
        public string? RevDate { get; set; }//Revizyon Tarihi
        public bool Active { get; set; }//Parça nın aktif olup olmadığı durumda kontrol başlangıcında gösterili veya gizlenir
    }
}
