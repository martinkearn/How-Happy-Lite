using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using HowHappyLite.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Encodings.Web;

namespace HowHappyLite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Result()
        {
            //get form data
            var file = Request.Form.Files[0];
            var luisQuery = String.IsNullOrEmpty(Request.Form["luisquery"].ToString()) ?
                "Show me all the happy people" :
                Request.Form["luisquery"].ToString();

            //get emotion data from Cognitive Services api
            var faces = new List<Face>();
            using (var httpClient = new HttpClient())
            {
                var _emotionApiKey = "1dd1f4e23a5743139399788aa30a7153";
                var _emotionApiUrl = "https://api.projectoxford.ai/emotion/v1.0/recognize";

                //setup HttpClient with content
                httpClient.BaseAddress = new Uri(_emotionApiUrl);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _emotionApiKey);
                var content = new StreamContent(file.OpenReadStream());
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");

                //make request
                var responseMessage = await httpClient.PostAsync(_emotionApiUrl, content);

                //read response as a json string
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                //get faces
                var responseArray = JArray.Parse(responseString);
                foreach (var faceResponse in responseArray)
                {
                    var face = JsonConvert.DeserializeObject<Face>(faceResponse.ToString());
                    faces.Add(face);
                }
            }

            //get luis data
            var luisResult = new LuisResult();
            using (var httpClient = new HttpClient())
            {
                var _luisApiKey = "d004b0b064694dd1bec537e3629863fb";
                var _luisApiAppId = "203dc1be-487f-4aff-b873-2ffa25e4e86b";
                var _luisApiUrl = "https://api.projectoxford.ai/luis/v1/application?";

                //setup HttpClient
                httpClient.BaseAddress = new Uri(_luisApiUrl);
                 var queryUrl = _luisApiUrl + "id=" + _luisApiAppId + "&subscription-key=" + _luisApiKey + "&q=" + UrlEncoder.Default.Encode(luisQuery);

                //make request
                var responseMessage = await httpClient.GetAsync(queryUrl);

                //read response as a json string
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                //deserialise json to luis response
                luisResult = JsonConvert.DeserializeObject<LuisResult>(responseString);

                //sort faces by emotion entity
                var entity = luisResult.entities.Where(o => o.type.Contains("emotion::")).FirstOrDefault();
                var emotion = entity.type.Substring(9);
                switch (emotion)
                {
                    case "happiness":
                        faces = faces.OrderByDescending(o => o.scores.happiness).ToList();
                        break;
                    case "anger":
                        faces = faces.OrderByDescending(o => o.scores.anger).ToList();
                        break;
                    case "contempt":
                        faces = faces.OrderByDescending(o => o.scores.contempt).ToList();
                        break;
                    case "disgust":
                        faces = faces.OrderByDescending(o => o.scores.disgust).ToList();
                        break;
                    case "fear":
                        faces = faces.OrderByDescending(o => o.scores.fear).ToList();
                        break;
                    case "neutral":
                        faces = faces.OrderByDescending(o => o.scores.neutral).ToList();
                        break;
                    case "sadness":
                        faces = faces.OrderByDescending(o => o.scores.sadness).ToList();
                        break;
                    case "surprise":
                        faces = faces.OrderByDescending(o => o.scores.surprise).ToList();
                        break;
                }
            }


            return Json(faces);
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
