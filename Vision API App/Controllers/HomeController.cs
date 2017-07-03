using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Vision_API_App.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

namespace Vision_API_App.Controllers
{
    public class HomeController : Controller
    {
        const string subscriptionKey = "insert key";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze";

        private readonly DbConnectionContext db = new DbConnectionContext();

        public ActionResult Index()
        {
            var imagesModel = new ImageGallery();
            var imageFiles = Directory.GetFiles(Server.MapPath("~/Upload_Files/"));
            foreach (var item in imageFiles)
            {
                imagesModel.ImageList.Add(Path.GetFileName(item));
            }
            return View(imagesModel);
        }

        [HttpGet]
        public ActionResult UploadImage()
        {
            return View();
        }

        [HttpPost]
        public void UploadImageMethod()
        {
            if (Request.Files.Count != 0)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    int fileSize = file.ContentLength;
                    string fileName = file.FileName;
                    file.SaveAs(Server.MapPath("~/Upload_Files/" + fileName));
                    ImageGallery imageGallery;
                    ImageGallery igTest = db.ImageGallery.Where(x => x.Name == fileName).FirstOrDefault();
                    if (igTest == null)
                    {
                        imageGallery = new ImageGallery();
                        imageGallery.ID = Guid.NewGuid();
                        imageGallery.Name = fileName;
                        imageGallery.ImagePath = "/Upload_Files/" + fileName;
                        db.ImageGallery.Add(imageGallery);
                        db.SaveChanges();
                    }
                }
            }
            Response.Redirect("Index");
        }

        static async Task<string> MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "visualFeatures=Categories,Description,Color&language=en";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                return contentString;
               // return JsonPrettyPrint(contentString);
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        public async Task<ActionResult> ImageInfo(string image)
        {
            IGContainer igContainer = new IGContainer();

            var cos = await HomeController.MakeAnalysisRequest(AppDomain.CurrentDomain.BaseDirectory + "/Upload_Files/" + image);
            var json = JsonConvert.DeserializeObject(cos).ToString();
            //string json = "{ 'categories': [ { 'name': 'others_', 'score': 0.00390625 }, { 'name': 'people_hand', 'score': 0.9609375 } ], 'description': { 'tags': [ 'person', 'man', 'indoor', 'cutting', 'food', 'using', 'holding', 'table', 'black', 'hand', 'glasses', 'wearing', 'close', 'standing', 'phone', 'plate', 'hot', 'cake', 'white', 'eating', 'blue', 'kitchen', 'sandwich', 'hat', 'dog', 'teeth' ], 'captions': [ { 'text': 'a close up of a man', 'confidence': 0.61871840488575 } ] }, 'requestId': '4f72811d - 45b4 - 46d3 - b3c6 - 6b40dd02d27d', 'metadata': { 'width': 1000, 'height': 400, 'format': 'Jpeg' }, 'color': { 'dominantColorForeground': 'Grey', 'dominantColorBackground': 'Black', 'dominantColors': [ 'Black', 'Grey' ], 'accentColor': '714336', 'isBWImg': false } }";
            igContainer.json = json;
            igContainer.imageGalery = db.ImageGallery.Where(x => x.Name == image).FirstOrDefault();
            igContainer.categories = GetCategories(json);
            igContainer.tags = GetTags(json);

            return View(igContainer);
        }

        private List<categories> GetCategories(string json)  //like tags could be more atomic
        {
            List<categories> categories = new List<categories>();
            
            categories passCat;

            JObject array = JObject.Parse(json);

            foreach (JToken token in array.Children())
            {
                if (token is JProperty)
                {
                    var prop = token as JProperty;
                    if (prop.Name == "categories")
                    {

                        foreach (JToken sth in prop.Value)
                        {
                            passCat = new Models.categories();
                            //name
                            string prepStrName = sth.First.ToString().Replace("name","").Replace(":","").Replace(@"\", "").Replace(@"/", "").Replace("\"", "").Replace("\r\n", "").Replace("[", "").Replace("]", ""); 
                            passCat.name = prepStrName;
                            //score
                            string prepStrScore = sth.Last.ToString().Remove(0, 9);
                            passCat.score = prepStrScore;
                            categories.Add(passCat);
                        }
                    }
                }
            }
            return categories;
        }

        private List<string> GetTags(string json)
        {
            List<string> tagsList = new List<string>();

            JObject array = JObject.Parse(json);

            foreach (JToken token in array.Children())
            {
                if (token is JProperty)
                {
                    var prop = token as JProperty;

                    if (prop.Name == "description")
                    {
                        foreach (JToken sth in prop.Value.First) // FIRST for Tags
                        {

                            var prop2 = sth as JProperty;
                            string[] TagsSplit = sth.ToString().Split(',');
                            foreach (var tag in TagsSplit)
                            {
                                string changedTag = tag.Replace(@"\", "").Replace(@"/", "").Replace("\"", "").Replace("\r\n", "").Replace("[", "").Replace("]", "");
                                tagsList.Add(changedTag);
                            }
                        }
                    }
                }
            }
            return tagsList;
        }

        public ActionResult Delete(string image)
        {
            ImageGallery img= db.ImageGallery.Where(x => x.Name == image).FirstOrDefault();
            if (img != null)
            {
                db.ImageGallery.Remove(img);
                db.SaveChanges();
                string path = AppDomain.CurrentDomain.BaseDirectory + img.ImagePath;
                System.IO.File.Delete(path);
            }
            Response.Redirect("Index");
            return View();
        }
    }
}