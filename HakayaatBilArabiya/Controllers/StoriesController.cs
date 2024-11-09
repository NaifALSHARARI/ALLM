using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HakayaatBilArabiya.Services;
using HakayaatBilArabiya.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;

namespace HakayaatBilArabiya.Controllers
{
    public class StoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WatsonAssistantClient _watsonAssistantClient;
        private readonly AllamSynonymsService _synonymsService;
        private readonly TextToSpeechService _textToSpeechService;

        public StoriesController(ApplicationDbContext context, WatsonAssistantClient watsonAssistantClient, AllamSynonymsService synonymsService, TextToSpeechService textToSpeechService)
        {
            _context = context;
            _watsonAssistantClient = watsonAssistantClient;
            _synonymsService = synonymsService;
            _textToSpeechService = textToSpeechService;
        }

        public async Task<IActionResult> Index()
        {
            var stories = await _context.Stories.ToListAsync();
            return View(stories);
        }

        [HttpGet]
        public IActionResult GenerateStory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateStory(StoryInputModel inputModel)
        {
            if (string.IsNullOrWhiteSpace(inputModel.ManualInput))
            {
                var selectedElements = new List<string>();

                if (!string.IsNullOrWhiteSpace(inputModel.Characters))
                    selectedElements.Add("الشخصيات");

                if (!string.IsNullOrWhiteSpace(inputModel.Animals))
                    selectedElements.Add("الحيوانات");

                if (!string.IsNullOrWhiteSpace(inputModel.Place))
                    selectedElements.Add("المكان");

                if (!string.IsNullOrWhiteSpace(inputModel.Season))
                    selectedElements.Add("الموسم");

                if (!string.IsNullOrWhiteSpace(inputModel.Time))
                    selectedElements.Add("الوقت");

                if (selectedElements.Count < 3)
                {
                    ViewBag.Error = "يجب اختيار ثلاثة عناصر على الأقل من (الشخصيات، الحيوانات، المكان، الوقت، والموسم) أو إدخال نص يدوي.";
                    return View();
                }
            }

            try
            {
                var generatedStory = await _watsonAssistantClient.GenerateTextAsync(inputModel);

                var title = "قصة مولدة";

                if (!string.IsNullOrWhiteSpace(inputModel.Characters))
                {
                    title = $"قصة: {inputModel.Characters}";
                }

                if (!string.IsNullOrWhiteSpace(inputModel.Animals))
                {
                    title += $" و {inputModel.Animals}";
                }
                if (!string.IsNullOrWhiteSpace(inputModel.Time))
                {
                    title += $" و {inputModel.Time}";
                }
                if (!string.IsNullOrWhiteSpace(inputModel.Season))
                {
                    title += $" و {inputModel.Season}";
                }
                if (!string.IsNullOrWhiteSpace(inputModel.Place))
                {
                    title += $" و {inputModel.Place}";
                }

                if (!string.IsNullOrWhiteSpace(inputModel.ManualInput))
                {
                    title = $"قصة : {inputModel.ManualInput.Substring(0, Math.Min(20, inputModel.ManualInput.Length))}...";
                }

                var story = new Story
                {
                    Title = title,
                    Content = generatedStory
                };

                var audioBytes = await _textToSpeechService.SynthesizeAsync(generatedStory);

                var audioFileName = $"{Guid.NewGuid()}.mp3";
                var audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio", audioFileName);

                var audioDirectory = Path.GetDirectoryName(audioPath);
                if (!Directory.Exists(audioDirectory))
                {
                    Directory.CreateDirectory(audioDirectory);
                }

                await System.IO.File.WriteAllBytesAsync(audioPath, audioBytes);

                story.AudioPath = $"/audio/{audioFileName}";

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"حدث خطأ أثناء توليد القصة: {ex.Message}";
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetSynonyms(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                ViewBag.Error = "يرجى إدخال كلمة للبحث عن مرادفاتها.";
                var stories = await _context.Stories.ToListAsync();
                return View("Index", stories);
            }

            try
            {
                var synonyms = await _synonymsService.GetSynonymsAsync(word);

                if (synonyms == null || synonyms.Count == 0)
                {
                    ViewBag.Error = "لم يتم العثور على مرادفات للكلمة المدخلة.";
                }
                else
                {
                    ViewBag.Word = word;
                    ViewBag.Synonyms = synonyms;
                }

                var stories = await _context.Stories.ToListAsync();

                return View("Index", stories);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"حدث خطأ أثناء الحصول على المرادفات: {ex.Message}";
                var stories = await _context.Stories.ToListAsync();
                return View("Index", stories);
            }
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var story = await _context.Stories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story != null)
            {
                _context.Stories.Remove(story);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}