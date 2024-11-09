using HakayaatBilArabiya.Models;
using Microsoft.AspNetCore.Mvc;

public class GamesController : Controller
{
    public IActionResult Index()
    {
        var games = new List<EducationalGame>
        {
            new EducationalGame
            {
                Id = 1,
                ImageUrl = "/images/giraffe.png",
                WordWithMissingLetter = "افــة",
                CorrectLetter = "ز",
                Choices = new List<string> { "ز", "غ" }
            },
            new EducationalGame
            {
                Id = 2,
                ImageUrl = "/images/airplane.png",
                WordWithMissingLetter = "طائــر",
                CorrectLetter = "ة",
                Choices = new List<string> { "أ", "ي", "ة" }
            },
            new EducationalGame
            {
                Id = 3,
                ImageUrl = "/images/bike.jpg",
                WordWithMissingLetter = "دراجــ",
                CorrectLetter = "ة",
                Choices = new List<string> { "ة", "ت", "ث" }
            },
            new EducationalGame
            {
                Id = 4,
                ImageUrl = "/images/fruits.jpg",
                WordWithMissingLetter = "فاكــه",
                CorrectLetter = "ة",
                Choices = new List<string> { "ة", "ح", "خ" }
            },
            new EducationalGame
            {
                Id = 5,
                ImageUrl = "/images/lion.jpg",
                WordWithMissingLetter = "أســ",
                CorrectLetter = "د",
                Choices = new List<string> { "د", "ذ", "ز" }
            },
            new EducationalGame
            {
                Id = 6,
                ImageUrl = "/images/apple.jpg",
                WordWithMissingLetter = "تـاح",
                CorrectLetter = "ف",
                Choices = new List<string> { "ف", "ث", "ب" }
            },
            new EducationalGame
            {
                Id = 7,
                ImageUrl = "/images/ball.jpg",
                WordWithMissingLetter = "كــر",
                CorrectLetter = "ة",
                Choices = new List<string> { "ر", "ة", "د" }
            },
            new EducationalGame
            {
                Id = 8,
                ImageUrl = "/images/camel.jpg",
                WordWithMissingLetter = "جمــ",
                CorrectLetter = "ل",
                Choices = new List<string> { "م", "ل", "ن" }
            },
            new EducationalGame
            {
                Id = 9,
                ImageUrl = "/images/cat.jpg",
                WordWithMissingLetter = "قــ ة",
                CorrectLetter = "ط",
                Choices = new List<string> { "ط", "ث", "ش" }
            },
            new EducationalGame
            {
                Id = 10,
                ImageUrl = "/images/house.jpg",
                WordWithMissingLetter = "منــ ل",
                CorrectLetter = "ز",
                Choices = new List<string> { "ز", "ر", "ذ" }
            },
            new EducationalGame
            {
                Id = 11,
                ImageUrl = "/images/tree.jpg",
                WordWithMissingLetter = "ش ــرة",
                CorrectLetter = "ج",
                Choices = new List<string> { "ج", "ح", "خ" }
            },
            new EducationalGame
            {
                Id = 12,
                ImageUrl = "/images/car.png",
                WordWithMissingLetter = "س ــارة",
                CorrectLetter = "ي",
                Choices = new List<string> { "أ", "ي", "ت" }
            }
        };

        return View(games);
    }
}
