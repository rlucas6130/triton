using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Engine;
using Microsoft.WindowsAzure.Storage.Blob;

namespace JobBuilderWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void StartProcessingJob([QueueTrigger("buildqueue")] string jobId, TextWriter log)
        {
            log.WriteLine(jobId);
            Task.Factory.StartNew(() =>
            {
                LSA.ProcessAndStore(int.Parse(jobId));
            });
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void UploadDocument([QueueTrigger("documentqueue")] string blobName, [Blob("documents/{queueTrigger}", FileAccess.Read)] Stream blobStream, TextWriter log)
        {
            LSA.CreateDocument(blobStream, blobName);
        }
    }
}
