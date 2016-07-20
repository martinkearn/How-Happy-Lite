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
            //get file from form data
            var file = Request.Form.Files[0];

            //get emotion data from Cognitive Services api
            var faces = new List<Face>();
            using (var httpClient = new HttpClient())
            {
                var _apiKey = "1dd1f4e23a5743139399788aa30a7153";
                var _apiUrl = "https://api.projectoxford.ai/emotion/v1.0/recognize";

                //setup HttpClient with content
                httpClient.BaseAddress = new Uri(_apiUrl);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
                var content = new StreamContent(file.OpenReadStream());
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");

                //make request
                var responseMessage = await httpClient.PostAsync(_apiUrl, content);

                //read response as a json string
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                //get faces
                var responseArray = JArray.Parse(responseString);
                foreach (var faceResponse in responseArray)
                {
                    //deserialise json to face
                    var face = JsonConvert.DeserializeObject<Face>(faceResponse.ToString());

                    //add face to faces list
                    faces.Add(face);
                }
                //sort faces by happiness
                faces = faces.OrderByDescending(o => o.scores.happiness).ToList();
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
