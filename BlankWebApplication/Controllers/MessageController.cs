using Google.Cloud.Storage.V1;
using JosephJulianMuscatMSD63ASynoptic.DataAccess.Interfaces;
using JosephJulianMuscatMSD63ASynoptic.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JosephJulianMuscatMSD63ASynoptic.Controllers
{
    public class MessageController : Controller
    {
        private readonly IPubSubRepository pubsubrepo;
        public MessageController(IPubSubRepository _pubsubrepo)
        {
            pubsubrepo = _pubsubrepo;
        }

        public IActionResult Compose()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Compose(Message m, IFormFile image)
        {
            try
            {
                string json = ""; string im = "";
                try
                {
                    if (!image.Equals(null))
                    {
                        string bucketName = "jjmsynopticimagesbucket";
                        string uniqueFileName = Guid.NewGuid() + System.IO.Path.GetExtension(image.FileName);

                        var storage = StorageClient.Create();

                        using (var myStream = image.OpenReadStream())
                        {
                            storage.UploadObject(bucketName, uniqueFileName, null, myStream);
                        }

                        m.Url = $"https://storage.googleapis.com/{bucketName}/{uniqueFileName}";

                        json = "{\"image\":\"true\",\"URL\":\"" + m.Url + "\",\"message\":\"" + m.msg + "\"}";
                        im = "true";
                    }
                }
                catch(NullReferenceException ex)
                {
                    json = "{\"image\":\"false\",\"message\":\"" + m.msg + "\"}";
                    im = "false";
                }

                pubsubrepo.PublishMessage(json, im);

                TempData["message"] = "Message published successfully";

                return View();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["error"] = "Message went up in smoke, try again";
                return View(m);
            }
        }

        [HttpGet]
        public IActionResult Pull()
        {
            string msgJson = pubsubrepo.PullMessage();
            if (msgJson == string.Empty)
                return View(new Message { msg = "No message available" });

            dynamic deserialized = JsonConvert.DeserializeObject(msgJson);
            Message m = new Message();
            m.msg = deserialized.message;
            if (Convert.ToBoolean(deserialized.image))
                m.Url = deserialized.URL;

            return View(m);
        }
    }
}