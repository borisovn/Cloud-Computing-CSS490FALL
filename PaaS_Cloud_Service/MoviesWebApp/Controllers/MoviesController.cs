using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MoviesDBCommon;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.IO;
using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Web.Script.Serialization;

namespace MoviesWebApp.Controllers
{
    public class MoviesController : Controller
    {
        private MovieContext db = new MovieContext();
        private CloudQueue imagesQueue;
        private static CloudBlobContainer imagesBlobContainer;
      

        public MoviesController()
        {
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            imagesBlobContainer = blobClient.GetContainerReference("images");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the queue.
            imagesQueue = queueClient.GetQueueReference("images");
        }

        public ActionResult PopulateDB()
        {
            return View();
        }

        /// <summary>
        /// PopulteDB Page:
        /// Creates data base based on user input such as
        /// moviename (string), and protocol(string).
        /// </summary>
        /// <param name="moviename"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PopulateDB([Bind(Include = "MovieTitle,Protocol")] Populate pop)
        {
            if (ModelState.IsValid)
            {
                switch (pop.Protocol)
                {
                    case "JSON":
                        await parseByJSONDocument(pop.MovieTitle);
                        break;
                    case "XML":
                        await parseByXMLDocument(pop.MovieTitle);
                        break;
                }
                return View("PopulateDB");
            }
            return View(pop);
        }


        /// <summary>
        /// Index Page:
        /// List all  database instances
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            return View(await db.Movies.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = await db.Movies.FindAsync(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }



        /// <summary>
        /// Delete Function
        /// delete db row 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = await db.Movies.FindAsync(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        /// <summary>
        /// Delete Function
        /// delete blob instance
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // retrive the movie
            Movie movie = await db.Movies.FindAsync(id);
            await DeleteMovieBlobsAsync(movie);
            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
            Trace.TraceInformation("Deleted Movie {0}", movie.MovieId);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// SearchMovies function
        /// filter movies by given title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<ActionResult> SearchMovies(string title, string year, string type)
        {
            var movies = from m in db.Movies
                        select m;

            if (!String.IsNullOrEmpty(title))
            {
                movies = movies.Where(s => s.Title.Contains(title));
            }

            if (!String.IsNullOrEmpty(year))
            {
                movies = movies.Where(s => s.Year.Contains(year));
            }

            if (!String.IsNullOrEmpty(type))
            {
                movies = movies.Where(s => s.Type.Contains(type));
            }

            return View(await movies.ToListAsync());
        }


        /// <summary>
        /// Dispose function
        /// dispose database
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        //=====================================================================================
        // Blob Heandler Section:
        // Current Section takes care of creating, deleting the blobs instance
        // that contains image url 
        //=====================================================================================

        /// <summary>
        ///  UploadAndSaveBlobAsyncByWeb Fucntion:
        ///  upload and save imahge to blob
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        private async Task<CloudBlockBlob> UploadAndSaveBlobAsyncByWeb(string imageFile)
        {
            Trace.TraceInformation("Uploading image file {0}", imageFile);

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile);
            // Retrieve reference to a blob. 
            CloudBlockBlob imageBlob = imagesBlobContainer.GetBlockBlobReference(blobName);
            // Create the blob by uploading a local file.

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageFile);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream inputStream = response.GetResponseStream();

            if ((response.StatusCode == HttpStatusCode.OK ||
       response.StatusCode == HttpStatusCode.Moved ||
       response.StatusCode == HttpStatusCode.Redirect) &&
       response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                using (var fileStream = inputStream)
                {
                    await imageBlob.UploadFromStreamAsync(fileStream);
                }

                Trace.TraceInformation("Uploaded image file to {0}", imageBlob.Uri.ToString());

                return imageBlob;
            }

            return null;
        }

        /// <summary>
        ///  DeleteMovieBlobsAsync Fucntion:
        ///  delete blob from the storage
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private async Task DeleteMovieBlobsAsync(Movie movie)
        {
            if (!string.IsNullOrWhiteSpace(movie.Poster))
            {
                Uri blobUri = new Uri(movie.Poster);
                await DeleteMovieBlobAsync(blobUri);
            }
            if (!string.IsNullOrWhiteSpace(movie.Thumbnail))
            {
                Uri blobUri = new Uri(movie.Poster);
                await DeleteMovieBlobAsync(blobUri);
            }
        }
        private static async Task DeleteMovieBlobAsync(Uri blobUri)
        {
            string blobName = blobUri.Segments[blobUri.Segments.Length - 1];
            Trace.TraceInformation("Deleting image blob {0}", blobName);
            CloudBlockBlob blobToDelete = imagesBlobContainer.GetBlockBlobReference(blobName);
            await blobToDelete.DeleteAsync();
        }
        //=====================================================================================
        // End of Blob Heandler Section   
        //=====================================================================================


        //=====================================================================================
        // Parsing Heandler Section:
        // Current Section takes parsing XML and JSON  objects that
        // will be add to the data base
        //=====================================================================================

        /// <summary>
        /// parseByXMLDocument Fucntion:
        /// Parse XML object 
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        private async Task parseByXMLDocument(string temp)
        {
            // build search string
            string xmlUrl = "http://www.omdbapi.com/?s=" + temp + "&r=xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlUrl);
            XmlNodeList MovieNodeList =
                doc.SelectNodes("/root/result");

            // get root


            // check if xml file return anything
            if (MovieNodeList != null)
            {

                // pasre the xml object
                foreach (XmlNode node in MovieNodeList)
                {


                        var Movie = new Movie();
                        CloudBlockBlob imageBlob = null;
                        Movie.Title = node.Attributes.GetNamedItem("Title").Value;
                        Movie.Year = node.Attributes.GetNamedItem("Year").Value;
                        Movie.imdbID = node.Attributes.GetNamedItem("imdbID").Value;
                        Movie.Type = node.Attributes.GetNamedItem("Type").Value;

                        if (!String.IsNullOrWhiteSpace(node.Attributes.GetNamedItem("Poster").Value))
                        {
                            imageBlob = await UploadAndSaveBlobAsyncByWeb(node.Attributes.GetNamedItem("Poster").Value);
                            Movie.Poster = imageBlob.Uri.ToString();
                        }

                        db.Movies.Add(Movie);
                        await db.SaveChangesAsync();

                        // work on queue
                        if (imageBlob != null)
                        {
                            var queueMessage = new CloudQueueMessage(Movie.MovieId.ToString());
                            await imagesQueue.AddMessageAsync(queueMessage);
                            Trace.TraceInformation("Created queue message for MovieId {0}", Movie.MovieId);
                        }

                }
            }
            else
            {
                Trace.TraceInformation("Not valid request: " + xmlUrl);
            }
        }


        /// <summary>
        /// parseByJSONDocument Function:
        /// Parse JSON object
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        private async Task parseByJSONDocument(string temp)
        {
            // build search string
            string jsonUrl = "http://www.omdbapi.com/?s=" + temp + "&r=json";
            string json;

            // make web request to retrive json objects
            WebRequest request = WebRequest.Create(jsonUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
           
            // check for valid repsonce
            if ((response.StatusCode == HttpStatusCode.OK))
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    json = sr.ReadToEnd();
                }
                //DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Spell));
                dynamic sData = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
               // JObject sData = JObject.Parse(json);
           
                foreach (var item in sData["Search"])
                {
                    string title = (string)item["Title"];
                    string year = (string)item["Year"];
                    string imdbID = (string)item["imdbID"];
                    string type = (string)item["Type"];
                    string poster = (string)item["Poster"];
               
                    
                    // check for duplicates
                        Movie movie = new Movie();
                        CloudBlockBlob imageBlob = null;

                        movie.Title = title;
                        movie.Year = year;
                        movie.imdbID = imdbID;
                        movie.Type = type;
                        if (!String.IsNullOrWhiteSpace((string)item["Poster"]) && (string)item["Poster"]!="N/A")
                        {
                            imageBlob = await UploadAndSaveBlobAsyncByWeb(poster);
                            movie.Poster = imageBlob.Uri.ToString();
                        }

                        db.Movies.Add(movie);
                        await db.SaveChangesAsync();

                        //  work on queue
                        if (imageBlob != null)
                        {
                            var queueMessage = new CloudQueueMessage(movie.MovieId.ToString());
                            await imagesQueue.AddMessageAsync(queueMessage);
                            Trace.TraceInformation("Created queue message for MovieId {0}", movie.MovieId);
                        }   
                }
            } else
            {
                Trace.TraceInformation("Not valid request: " + jsonUrl);
            }
        }

        /// <summary>
        /// doesContain
        /// check if db contains 
        /// current movie or not
        /// </summary>
        /// <param name="imdbId"></param>
        /// <returns></returns>
        private bool doesContain(string imdbId)
        {
            return db.Movies.Any(m => m.imdbID == imdbId);
        }

    }
    //=====================================================================================
    // End of Parsing Heandler Section   
    //=====================================================================================

}
