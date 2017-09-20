using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Engine.Core;
using Engine;
using Engine.Contracts;
using System.Runtime.Serialization.Formatters.Binary;

namespace JobBuilderWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void StartProcessingJob([QueueTrigger("buildqueue")] Tuple<int, IEnumerable<int>> jobTuple, TextWriter log)
        {
            log.WriteLine(jobTuple.Item1);
            Task.Factory.StartNew(() =>
            {
                LSA.ProcessAndStore(jobTuple.Item1, jobTuple.Item2);
            });
        }

        public static void UploadDocument([QueueTrigger("uploadqueue")] string blobName, [Blob("uploadbatch/{queueTrigger}", FileAccess.Read)] Stream blobStream, TextWriter log)
        {
            try
            {
                var binaryFormatter = new BinaryFormatter();

                var uploadDocs = binaryFormatter.Deserialize(blobStream) as UploadDocumentParameter[];

                uploadDocs.AsParallel().ForAll(doc => LSA.CreateDocument(doc.StreamData, doc.FileName));
            }
            catch (Exception)
            { 
                throw;
            }
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void StartClusterAnalysis([QueueTrigger("clusterqueue")] int clusterCalculationId, TextWriter log)
        {
            Task.Factory.StartNew(() =>
            {
                using (var context = new SvdEntities())
                {
                    ClusterOptimizer.OptimizeRange(context, clusterCalculationId);
                }
            });
        }
    }
}
