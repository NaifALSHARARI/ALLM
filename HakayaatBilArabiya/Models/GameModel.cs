namespace HakayaatBilArabiya.Models
{
    public class EducationalGame
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } 
        public string WordWithMissingLetter { get; set; }
        public string CorrectLetter { get; set; }
        public List<string> Choices { get; set; }
    }


}
