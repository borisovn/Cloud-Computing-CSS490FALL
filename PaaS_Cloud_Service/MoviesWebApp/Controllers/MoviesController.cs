using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MoviesDBCommon;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.IO;
using System.Xml;
using MyLogger;

namespace MoviesWebApp.Controllers
{
    public class MoviesController : Controller
    {
        private MovieContext db = new MovieContext();
        private CloudQueue imagesQueue;
        private static CloudBlobContainer imagesBlobContainer;
        private ILogger logger;

        public MoviesController()
        {
            logger = new Logger();
            InitializeStorage();
        }

        private  void InitializeStorage()
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
        /// Creates data base based
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
                return RedirectToAction("Index");
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
        /// delete db row
        /// based on id
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
            logger.Information("Deleted Movie {0}", movie.MovieId);
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

        public ActionResult DeleteDB()
        {
            return View();
        }
        /// <summary>
        /// Delete all movies in data base
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        [HttpPost, ActionName("DeleteDB")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDBConfirmed()
        {
            var movies = await db.Movies.ToListAsync();

            foreach (var movie in movies)
            {
                await DeleteMovieBlobsAsync(movie);
                db.Movies.Remove(movie);
                await db.SaveChangesAsync();
                logger.Information("Deleted Movie {0}", movie.MovieId);
            }

            return RedirectToAction("Index", "Home");
        }


        /// <summary>
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
        // Blob Deleting Section:
        // Current Section takes care of deleting the blobs instance if image exists
        //=====================================================================================
        /// <summary>
        ///  DeleteMovieBlobsAsync Fucntion:
        ///  delete blob from the storage
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private async Task DeleteMovieBlobsAsync(Movie movie)
        {
            if (!string.IsNullOrWhiteSpace(movie.Poster) && (movie.Poster !="N/A"))
            {
                Uri blobUri = new Uri(movie.Poster);
                await DeleteMovieBlobAsync(blobUri);
            }
            if (!string.IsNullOrWhiteSpace(movie.Poster) && (movie.Poster !="N/A"))
            if (!string.IsNullOrWhiteSpace(movie.Thumbnail))
            {
                Uri blobUri = new Uri(movie.Thumbnail);
                await DeleteMovieBlobAsync(blobUri);
            }
        }
        private async Task DeleteMovieBlobAsync(Uri blobUri)
        {
       
            string blobName = blobUri.Segments[blobUri.Segments.Length - 1];
            logger.Information("Deleting image blob {0}", blobName);
            CloudBlockBlob blobToDelete =   imagesBlobContainer.GetBlockBlobReference(blobName);
            await blobToDelete.DeleteAsync();
        }

        //=====================================================================================
        // Parsing Heandler Section:
        // Current Section is parsing XML and JSON  objects 
        //=====================================================================================
        
        /// <summary>
        /// Parse XML object 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task parseByXMLDocument(string param)
        {
            // build search string
            string xmlUrl = "http://www.omdbapi.com/?s=" + param + "&r=xml";
            logger.Information("Creating new object based on XML format. Link: {0}", xmlUrl);
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlUrl);
            XmlNodeList MovieNodeList = doc.SelectNodes("/root/result");

            // check if xml file return anything
            if (MovieNodeList != null)
            {
                // pasre the xml object
                foreach (XmlNode node in MovieNodeList)
                {
                    var Movie = new Movie();
                    Movie.Title = node.Attributes.GetNamedItem("Title").Value;
                    Movie.Year = node.Attributes.GetNamedItem("Year").Value;
                    Movie.imdbID = node.Attributes.GetNamedItem("imdbID").Value;
                    Movie.Type = node.Attributes.GetNamedItem("Type").Value;

                    if (node.Attributes["Poster"] != null)
                    {
                        Movie.Poster = node.Attributes.GetNamedItem("Poster").Value;
                    }
                   
                    db.Movies.Add(Movie);
                    await db.SaveChangesAsync();
                    var queueMessage = new CloudQueueMessage(Movie.MovieId.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);
                    logger.Information("Created queue message for MovieId {0}", Movie.MovieId);
                }
            }
            else
            {
                logger.Information("Not valid request for XML object. Link: {0}", xmlUrl);
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
            logger.Information("Creating new object based on JSON format. Link: {0}", jsonUrl);
            // make web request to retrive json objects
            WebRequest request = WebRequest.Create(jsonUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // check for valid repsonce
            if ((response.StatusCode == HttpStatusCode.OK))
            {
                string json;
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    json = sr.ReadToEnd();
                }

                dynamic sData = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                // JObject sData = JObject.Parse(json);

                foreach (var item in sData["Search"])
                {
                    string title = (string)item["Title"];
                    string year = (string)item["Year"];
                    string imdbID = (string)item["imdbID"];
                    string type = (string)item["Type"];
                    string poster = (string)item["Poster"];

                    Movie movie = new Movie();
                    movie.Title = title;
                    movie.Year = year;
                    movie.imdbID = imdbID;
                    movie.Type = type;
                    movie.Poster = poster;

                    db.Movies.Add(movie);
                    await db.SaveChangesAsync();
                    var queueMessage = new CloudQueueMessage(movie.MovieId.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);
                    logger.Information("Created queue message for MovieId {0}", movie.MovieId);
                }
            }
            else
            {
                logger.Information("Not valid request for JSON object. Link: {0}", jsonUrl);
            }
        }
    }
}
