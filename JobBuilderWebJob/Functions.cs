using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Engine.Core;
using Engine;

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

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void UploadDocument([QueueTrigger("documentqueue")] string blobName, [Blob("documents/{queueTrigger}", FileAccess.Read)] Stream blobStream, TextWriter log)
        {
            LSA.CreateDocument(blobStream, blobName);
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void StartClusterAnalysis([QueueTrigger("clusterqueue")] Tuple<int, Contracts.ClusterAnalysisParameters> clusterParams, TextWriter log)
        {
            Task.Factory.StartNew(() =>
            {
                var cluster = ClusterOptimizer.OptimizeRange(clusterParams.Item1, clusterParams.Item2);
            });
        }
    }
}
