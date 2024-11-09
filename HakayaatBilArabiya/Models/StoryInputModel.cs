using System.ComponentModel.DataAnnotations;

namespace HakayaatBilArabiya.Models
{
    public class StoryInputModel
    {
        [Display(Name = "المدخل اليدوي")]
        public string ManualInput { get; set; }

        [Display(Name = "الشخصيات")]
        public string Characters { get; set; }

        [Display(Name = "الحيوانات")]
        public string Animals { get; set; }

        [Display(Name = "المكان")]
        public string Place { get; set; }

        [Display(Name = "الموسم")]
        public string Season { get; set; }

        [Display(Name = "الوقت")]
        public string Time { get; set; }

        [Display(Name = "الجمهور المستهدف")]
        public string Audience { get; set; }

        [Display(Name = "الأسلوب")]
        public string Style { get; set; }

        [Display(Name = "القيمة الأخلاقية")]
        public string MoralValue { get; set; }
        public string StorySubject { get; set; }


    }

}
