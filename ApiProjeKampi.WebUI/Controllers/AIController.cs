using Microsoft.AspNetCore.Mvc;      // Controller ve IActionResult için


namespace ApiProjeKampi.WebUI.Controllers
{
    public class AIController : Controller
    {
        public IActionResult CreateRecipeWithOpenAI()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipeWithOpenAI(string prompt)
        {
            var apiKey = HttpContext.RequestServices.GetService<IConfiguration>()["GoogleApiKey"];
            // Google AI Studio'dan aldığın key

            using var client = new HttpClient();

            var requestData = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = $"Sen bir restoran için yemek önerileri yapan bir yapay zekâsın. Malzemeler: {prompt}. Bana uygun bir yemek tarifi öner." }
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
            requestData);



            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var recipe = result.candidates[0].content.parts[0].text;
                ViewBag.recipe = recipe;
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                ViewBag.recipe = "Bir hata oluştu: " + response.StatusCode + " - " + errorText;
            }

            return View();
        }

        // --- Google Gemini Response Model ---
        public class GeminiResponse
        {
            public Candidate[] candidates { get; set; }
        }

        public class Candidate
        {
            public Content content { get; set; }
        }

        public class Content
        {
            public Part[] parts { get; set; }
        }

        public class Part
        {
            public string text { get; set; }
        }




        public class OpenAIResponse
        {
            public List<Choice> choices { get; set; }
        }
        public class Choice
        {
            public Message message { get; set; }
        }
        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }
    }
}





//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http.Headers;

//namespace ApiProjeKampi.WebUI.Controllers
//{
//    public class AIController : Controller
//    {
//        public IActionResult CreateRecipeWithOpenAI()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateRecipeWithOpenAI(string prompt)
//        {
//            var apiKey = "";

//            using var client = new HttpClient();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

//            var requestData = new
//            {
//                model = "gpt-4o-mini",
//                messages = new[]
//                {
//                    new
//                    {
//                        role="system",
//                        content="Sen bir restoran için yemek önerilerini yapan bir yapay zeka aracısın. Amacımız kullanıcı tarafından girilen malzemelere göre yemek tarifi önerisinde bulunmak."
//                    },
//                    new
//                    {
//                        role="user",
//                        content= prompt
//                    }
//                },
//                temperature = 0.5
//            };

//            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestData);

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
//                var content = result.choices[0].message.content;
//                ViewBag.recipe=content;
//            }
//            else
//            {
//                ViewBag.recipe = "Bir hata oluştu: " + response.StatusCode;
//            }

//            return View();
//        }

//        public class OpenAIResponse
//        {
//            public List<Choice> choices { get; set; }
//        }
//        public class Choice
//        {
//            public Message message { get; set; }
//        }
//        public class Message
//        {
//            public string role { get; set; }
//            public string content { get; set; }
//        }
//    }
//}
