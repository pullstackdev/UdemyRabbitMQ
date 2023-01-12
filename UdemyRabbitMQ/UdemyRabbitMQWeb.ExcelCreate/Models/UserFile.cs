using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UdemyRabbitMQWeb.ExcelCreate.Models
{
    public enum FileStatus 
    {
        Creating,
        Completed
    }

    public class UserFile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedDate { get; set; } //başta boş ondan nullable, excel oluştuğunda tarih basılacak
        public FileStatus FileStatus { get; set; } //worker servis excel oluşturunca completed olacak

        [NotMapped] //efcore'da mapleme yapma yani db'de olmasın
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() : "-"; //dto ilede yapılabilirdi ama uzamaması için yapıldı, created date'i string olarakta alıp tutabilmek için var
    }
}
