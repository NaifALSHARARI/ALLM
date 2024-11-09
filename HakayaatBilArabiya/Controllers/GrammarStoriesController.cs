using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HakayaatBilArabiya.Services;

namespace HakayaatBilArabiya.Controllers
{
    public class GrammarStoriesController : Controller
    {
        private readonly AllamGrammarService _allamGrammarService;

        public GrammarStoriesController(AllamGrammarService allamGrammarService)
        {
            _allamGrammarService = allamGrammarService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                ViewBag.Error = "يرجى تحديد الموضوع النحوي.";
                return View();
            }

            try
            {
                var story = await _allamGrammarService.GenerateGrammarStoryAsync(topic);
                ViewBag.Story = story;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"حدث خطأ: {ex.Message}";
            }

            return View();
        }
    }
}
