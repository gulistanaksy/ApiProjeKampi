using ApiProjeKampi.WebUI.Dtos.MessageDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static ApiProjeKampi.WebUI.Controllers.AIController;

namespace ApiProjeKampi.WebUI.Controllers
{
    public class MessageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public MessageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> MessageList()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7172/api/Messages");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultMessageDto>>(jsonData);
                return View(values);
            }
            return View();
        }

        [HttpGet]
        public IActionResult CreateMessage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7172/api/Messages", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("MessageList");
            }
            return View();
        }

        public async Task<IActionResult> DeleteMessage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync("https://localhost:7172/api/Messages?id=" + id);
            return RedirectToAction("MessageList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateMessage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7172/api/Messages/GetMessage?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetMessageByIdDto>(jsonData);
            return View(value);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateMessage(UpdateMessageDto updateMessageDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            await client.PutAsync("https://localhost:7172/api/Messages/", stringContent);
            return RedirectToAction("MessageList");
        }

        [HttpGet]
        public async Task<IActionResult> AsnwerMessageWithOpenAI(int id)
        {
            // 1. Mesajı getir
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7172/api/Messages/GetMessage?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetMessageByIdDto>(jsonData);
            var prompt = value.MessageDetails;

            // 2. Google API key
            var apiKey = HttpContext.RequestServices
                                     .GetService<IConfiguration>()["GoogleApiKey"];

            using var client2 = new HttpClient();

            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $"Sen bir restoran için kullanıcıların göndermiş oldukları mesajlara detaylı ve olabildiğince olumlu, müşteri memnuniyetini gözeten cevaplar veren bir yapay zekâsın. Kullanıcı mesajı: {prompt}"
                            }
                        }
                    }
                }
            };

            var response = await client2.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
                requestData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var content = result?.candidates?[0]?.content?.parts?[0]?.text ?? "Cevap alınamadı.";
                ViewBag.answerAI = content;
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                ViewBag.answerAI = "Bir hata oluştu: " + response.StatusCode + " - " + errorText;
            }

            return View(value);
        }

        public PartialViewResult SendMessage()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(CreateMessageDto createMessageDto)
        {
            try
            {
                var apiKey = HttpContext.RequestServices
                    .GetService<IConfiguration>()["GoogleApiKey"];

                using var client = new HttpClient();

                // -----------------------
                // 1️⃣ Mesajı İngilizce'ye çevir
                // -----------------------
                var translateRequest = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $"Sen bir çeviri yapay zekâsısın. Aşağıdaki mesajı İngilizceye çevir: {createMessageDto.MessageDetails}"
                        }
                    }
                }
            }
                };

                var translateResponse = await client.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
                    translateRequest
                );

                var translateResponseString = await translateResponse.Content.ReadAsStringAsync();
                Console.WriteLine("=== HAM TRANSLATE RESPONSE ===");
                Console.WriteLine(translateResponseString);

                string englishText = null;
                if (translateResponse.IsSuccessStatusCode)
                {
                    var translateResult = await translateResponse.Content.ReadFromJsonAsync<GeminiResponse>();
                    englishText = translateResult?.candidates?[0]?.content?.parts?[0]?.text;
                    Console.WriteLine("Parsed English text: " + englishText);
                }

                if (string.IsNullOrEmpty(englishText))
                {
                    createMessageDto.Status = "Onay Bekliyor";
                }
                else
                {
                    // -----------------------
                    // 2️⃣ İngilizce mesaj ile toksisite skoru al
                    // -----------------------
                    var toxicRequest = new
                    {
                        contents = new[]
                        {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $@"Görev:
1. Aşağıdaki İngilizce mesajın toksik olup olmadığını değerlendir.
2. Toksisite skorunu 0-1 arasında ver.
3. **Sadece JSON formatında döndür.**

JSON formatı:
{{
  ""translatedText"": ""..."",
  ""toxicity"": ""Toxic / Not Toxic"",
  ""toxicityScore"": 0-1
}}

Mesaj: ""{englishText}"""
                            }
                        }
                    }
                }
                    };

                    var toxicResponse = await client.PostAsJsonAsync(
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
                        toxicRequest
                    );

                    var toxicResponseString = await toxicResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("=== HAM TOXIC RESPONSE ===");
                    Console.WriteLine(toxicResponseString);

                    GeminiModerationResult moderationResult = null;
                    if (toxicResponse.IsSuccessStatusCode)
                    {
                        var toxicResult = await toxicResponse.Content.ReadFromJsonAsync<GeminiResponse>();
                        var toxicText = toxicResult?.candidates?[0]?.content?.parts?[0]?.text;
                        Console.WriteLine("Toxic raw text: " + toxicText);

                        if (!string.IsNullOrEmpty(toxicText))
                        {
                            try
                            {
                                // Backtick ve fazladan karakterleri temizle
                                var cleanedJson = System.Text.RegularExpressions.Regex.Match(
                                    toxicText, @"\{.*\}", System.Text.RegularExpressions.RegexOptions.Singleline
                                ).Value;

                                moderationResult = JsonConvert.DeserializeObject<GeminiModerationResult>(cleanedJson);
                                Console.WriteLine("Parsed toxicity: " + moderationResult.toxicity +
                                                  ", score: " + moderationResult.toxicityScore);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Toxic JSON parse hatası: " + ex.Message);
                            }
                        }
                    }

                    // Status güncelle
                    if (moderationResult != null)
                    {
                        if (moderationResult.toxicity == "Toxic" || moderationResult.toxicityScore > 0.5)
                            createMessageDto.Status = "Toksik Mesaj";
                        else
                            createMessageDto.Status = "Mesaj Alındı";
                    }
                    else
                    {
                        createMessageDto.Status = "Onay Bekliyor";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                createMessageDto.Status = "Onay Bekliyor";
            }

            // Mesajı Web API'ye kaydet
            var client2 = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client2.PostAsync("https://localhost:7172/api/Messages", stringContent);

            if (responseMessage.IsSuccessStatusCode)
                return RedirectToAction("MessageList");

            return View();
        }


        // Gemini çeviri sonucu
        public class GeminiModerationResult
        {
            public string translatedText { get; set; }
            public string toxicity { get; set; }       // "Toxic" / "Not Toxic"
            public double toxicityScore { get; set; }  // 0-1
        }



        //        [HttpPost]
        //        public async Task<IActionResult> SendMessage(CreateMessageDto createMessageDto)
        //        {
        //            try
        //            {
        //                // Google API key
        //                var apiKey = HttpContext.RequestServices
        //                    .GetService<IConfiguration>()["GoogleApiKey"];

        //                using var client = new HttpClient();
        //                client.DefaultRequestHeaders.Authorization =
        //                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        //                // Prompt (Türkçe, stricter JSON)
        //                var prompt = $@"
        //Görev:
        //1. Aşağıdaki mesajı İngilizce'ye çevir.
        //2. Mesajın toksik olup olmadığını değerlendir (Toxic / Not Toxic).
        //3. Toksik olasılık skorunu 0 ile 1 arasında ver.
        //4. **Sadece JSON döndür, başka hiçbir açıklama yazma.**
        //JSON formatı: {{""translatedText"": ""..."", ""toxicity"": ""Toxic / Not Toxic"", ""toxicityScore"": 0-1}}

        //Mesaj: ""{createMessageDto.MessageDetails}""
        //";

        //                var requestBody = new
        //                {
        //                    prompt = prompt,
        //                    max_output_tokens = 500
        //                };

        //                var jsonBody = JsonConvert.SerializeObject(requestBody);
        //                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //                // Gemini REST API endpoint
        //                var response = await client.PostAsync(
        //                    "https://generativelanguage.googleapis.com/v1beta2/models/gemini-1.5-flash:generateText",
        //                    content
        //                );

        //                var responseString = await response.Content.ReadAsStringAsync();

        //                // Ham Gemini cevabını al
        //                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        //                var geminiText = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;

        //                // Raw text logla
        //                Console.WriteLine("Gemini raw text: " + geminiText);

        //                // JSON kısmını regex ile ayıkla
        //                string jsonPart = null;
        //                if (!string.IsNullOrEmpty(geminiText))
        //                {
        //                    var match = System.Text.RegularExpressions.Regex.Match(
        //                        geminiText,
        //                        @"\{.*\}"
        //                    );
        //                    if (match.Success)
        //                        jsonPart = match.Value;
        //                }

        //                GeminiModerationResult moderationResult = null;
        //                if (jsonPart != null)
        //                {
        //                    try
        //                    {
        //                        moderationResult = JsonConvert.DeserializeObject<GeminiModerationResult>(jsonPart);
        //                    }
        //                    catch
        //                    {
        //                        Console.WriteLine("JSON parse edilemedi: " + jsonPart);
        //                    }
        //                }

        //                // Status güncelleme
        //                if (moderationResult != null)
        //                {
        //                    if (moderationResult.toxicity == "Toxic" || moderationResult.toxicityScore > 0.5)
        //                        createMessageDto.Status = "Toksik Mesaj";
        //                    else
        //                        createMessageDto.Status = "Mesaj Alındı";
        //                }
        //                else
        //                {
        //                    createMessageDto.Status = "Onay Bekliyor";
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Hata: " + ex.Message);
        //                createMessageDto.Status = "Onay Bekliyor";
        //            }

        //            // Mesajı Web API'ye kaydet
        //            var client2 = _httpClientFactory.CreateClient();
        //            var jsonData = JsonConvert.SerializeObject(createMessageDto);
        //            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
        //            var responseMessage = await client2.PostAsync("https://localhost:7172/api/Messages", stringContent);

        //            if (responseMessage.IsSuccessStatusCode)
        //            {
        //                return RedirectToAction("MessageList");
        //            }

        //            return View();
        //        }


        // --- Bizim JSON çıktımız için özel model ---


        //[HttpPost]
        //public async Task<IActionResult> SendMessage(CreateMessageDto createMessageDto)
        //{

        //    var client = new HttpClient();
        //    var apiKey = "";
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        //    try
        //    {
        //        var translateRequestBody = new
        //        {
        //            inputs = createMessageDto.MessageDetails
        //        };
        //        var translateJson = System.Text.Json.JsonSerializer.Serialize(translateRequestBody);
        //        var translateContent = new StringContent(translateJson, Encoding.UTF8, "application/json");

        //        var translateResponse = await client.PostAsync("https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-tr-en", translateContent);
        //        var translateResponseString = await translateResponse.Content.ReadAsStringAsync();

        //        string englishText = createMessageDto.MessageDetails;
        //        if (translateResponseString.TrimStart().StartsWith("["))
        //        {
        //            var translateDoc = JsonDocument.Parse(translateResponseString);
        //            englishText = translateDoc.RootElement[0].GetProperty("translation_text").GetString();
        //            //ViewBag.v = englishText;
        //        }

        //        var toxicRequestBody = new
        //        {
        //            inputs = englishText
        //        };

        //        var toxicJson = System.Text.Json.JsonSerializer.Serialize(toxicRequestBody);
        //        var toxicContent = new StringContent(toxicJson, Encoding.UTF8, "application/json");
        //        var toxicResponse = await client.PostAsync("https://api-inference.huggingface.co/models/unitary/toxic-bert", toxicContent);
        //        var toxicResponseString = await toxicResponse.Content.ReadAsStringAsync();

        //        if (toxicResponseString.TrimStart().StartsWith("["))
        //        {
        //            var toxicDoc = JsonDocument.Parse(toxicResponseString);
        //            foreach(var item in toxicDoc.RootElement[0].EnumerateArray())
        //            {
        //                string label=item.GetProperty("label").GetString();
        //                double score = item.GetProperty("score").GetDouble();

        //                if (score > 0.5)
        //                {
        //                    createMessageDto.Status = "Toksik Mesaj";
        //                    break;
        //                }
        //            }
        //        }
        //        if (string.IsNullOrEmpty(createMessageDto.Status))
        //        {
        //            createMessageDto.Status = "Mesaj Alındı";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        createMessageDto.Status = "Onay Bekliyor";
        //    }


        //    var client2 = _httpClientFactory.CreateClient();
        //    var jsonData = JsonConvert.SerializeObject(createMessageDto);
        //    StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
        //    var responseMessage = await client2.PostAsync("https://localhost:7172/api/Messages", stringContent);
        //    if (responseMessage.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("MessageList");
        //    }
        //    return View();
        //}
    }
}
