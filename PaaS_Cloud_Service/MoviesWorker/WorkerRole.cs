
using System;
using System.Diagnostics;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using MoviesDBCommon;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using MyLogger;
using System.Threading.Tasks;

namespace MoviesWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private CloudQueue imagesQueue;
        private CloudBlobContainer imagesBlobContainer;
        private MoviesDBCommon.MovieContext db;
        private ILogger logger;
        const int _max = 1000000;
        public override void Run()
        {
            logger.Information("Movie entry point called");
            CloudQueueMessage msg = null;

            while (true)
            {
                try
                { 
                    msg = this.imagesQueue.GetMessage();
                    if (msg != null)
                    {
                        ProcessQueueMessage(msg);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (StorageException e)
                {
                    if (msg != null && msg.DequeueCount > 5)
                    {
                        this.imagesQueue.DeleteMessage(msg);
                        logger.Error("Deleting poison queue item due to dequeue count : '{0}'", msg.AsString);
                    }
                    logger.Error("Exception in MoviesWorker: '{0}'", e.Message);
                }
            }
        }

        /// <summary>
        /// Checks whatever db has duplicates rows
        /// based on imdbID
        /// </summary>
        /// <param name="imdbID"></param>
        /// <returns></returns>
        private bool MoviesExists(string imdbID)
        {
            var count = db.Movies.Count(d => d.imdbID == imdbID);
            return count > 1;
        }

        private  void ProcessQueueMessage(CloudQueueMessage msg)
        {

            logger.Information("Start processing queue message {0}", msg);
            var msgTimer = Stopwatch.StartNew(); 
            var adId = int.Parse(msg.AsString);
            Movie movie =  db.Movies.Find(adId);
            CloudBlockBlob imageBlob = null;

            if (movie == null)
            {
                logger.Error("AdId {0} not found, can't create thumbnail and poster", adId.ToString());
                throw new Exception(String.Format("AdId {0} not found, can't create thumbnail and poster", adId.ToString()));
            }

            // delete duplicates if exists
            if (MoviesExists(movie.imdbID))
            {
                var s2 = Stopwatch.StartNew(); 
                s2.Start();
                db.Movies.Remove(movie);
                db.SaveChanges();
                s2.Stop();
                logger.Information("Deleted Duplicate movie Movie {0} with imdbID {1}. Timespan: {2}", movie.MovieId, movie.imdbID,
                     ((double)(s2.Elapsed.TotalMilliseconds * 1000000) / _max).ToString("0.00 ns"));
                this.imagesQueue.DeleteMessage(msg);
                msgTimer.Stop();
                logger.Information("Finished processing queue message {0}. Timespan: {1}", msg, 
                    ((double)(msgTimer.Elapsed.TotalMilliseconds * 1000000) / _max).ToString("0.00 ns"));
                return;
            }

            // add Poster and Thumbnail to blob if exists
            if (!String.IsNullOrWhiteSpace(movie.Poster) && (movie.Poster != "N/A"))
            {
       
                var s1 = Stopwatch.StartNew();  // add timer here
                imageBlob = UploadAndSaveBlobByWeb(movie.Poster);
                s1.Stop();
                logger.Information("Uploaded image file {0} Timespan: {1} ",
                    imageBlob.Uri.ToString(), ((double)(s1.Elapsed.TotalMilliseconds * 1000000) /_max).ToString("0.00 ns"));
                movie.Poster = imageBlob.Uri.ToString();

                // proccess thumbnail
                var timerThumbnail = Stopwatch.StartNew();
                timerThumbnail.Start();
                Uri blobUri = new Uri(movie.Poster);
                string blobName = blobUri.Segments[blobUri.Segments.Length - 1];

                CloudBlockBlob inputBlob = this.imagesBlobContainer.GetBlockBlobReference(blobName);
                string thumbnailName = Path.GetFileNameWithoutExtension(inputBlob.Name) + "thumb.jpg";
                CloudBlockBlob outputBlob = this.imagesBlobContainer.GetBlockBlobReference(thumbnailName);

                using (Stream input = inputBlob.OpenRead())
                using (Stream output = outputBlob.OpenWrite())
                {
                    ConvertImageToThumbnailJPG(input, output);
                    outputBlob.Properties.ContentType = "image/jpeg";
                }
                //add timer
                timerThumbnail.Stop();
                logger.Information("Generated thumbnail in blob {0}. Timespan: {1} ", thumbnailName,
                     ((double)(timerThumbnail.Elapsed.TotalMilliseconds * 1000000) / _max).ToString("0.00 ns"));
                movie.Thumbnail = outputBlob.Uri.ToString();
                logger.Information("Updated thumbnail URL in database: {0}", movie.Poster);
            }

            db.SaveChanges();
            this.imagesQueue.DeleteMessage(msg);
            msgTimer.Stop();
            logger.Information("Finished processing queue message {0}. Timespan: {1}", msg,
                ((double)(msgTimer.Elapsed.TotalMilliseconds * 1000000) / _max).ToString("0.00 ns"));
        }

        public void ConvertImageToThumbnailJPG(Stream input, Stream output)
        {
            int thumbnailsize = 80;
            int width;
            int height;
            var originalImage = new Bitmap(input);

            if (originalImage.Width > originalImage.Height)
            {
                width = thumbnailsize;
                height = thumbnailsize * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = thumbnailsize;
                width = thumbnailsize * originalImage.Width / originalImage.Height;
            }

            Bitmap thumbnailImage = null;
            try
            {
                thumbnailImage = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias; //http://localhost:49606/Service References/
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(originalImage, 0, 0, width, height);
                }

                thumbnailImage.Save(output, ImageFormat.Jpeg);
            }
            finally
            {
                if (thumbnailImage != null)
                {
                    thumbnailImage.Dispose();
                }
            }
        }


        /// <summary>
        ///  upload and save imahge to blob
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        private CloudBlockBlob UploadAndSaveBlobByWeb(string imageFile)
        {
            

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
                    imageBlob.UploadFromStream(fileStream);
                }
                return imageBlob;
            }
            return null;
        }


        // A production app would also include an OnStop override to provide for
        // graceful shut-downs of worker-role VMs.  See
        // http://azure.microsoft.com/en-us/documentation/articles/cloud-services-dotnet-multi-tier-app-storage-3-web-role/#restarts
        public override bool OnStart()
        {

            logger = new Logger(); 

            // Set the maximum number of concurrent connections.
            ServicePointManager.DefaultConnectionLimit = 12;

            // Read database connection string and open database.
            var dbConnString = CloudConfigurationManager.GetSetting("moviesassignment03DbConnectionString");
            db = new MovieContext(dbConnString);

            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse
                (RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            logger.Information("Creating images blob container named images");
            var blobClient = storageAccount.CreateCloudBlobClient();
            imagesBlobContainer = blobClient.GetContainerReference("images");
            if (imagesBlobContainer.CreateIfNotExists())
            {
                // Enable public access on the newly created "images" container.
                imagesBlobContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
            }

            logger.Information("Creating images queue");
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            imagesQueue = queueClient.GetQueueReference("images");
            imagesQueue.CreateIfNotExists();
            logger.Information("Storage initialized");

            return base.OnStart();
        }
    }
}
