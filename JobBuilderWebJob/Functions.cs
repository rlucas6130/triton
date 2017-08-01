using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Engine;

namespace JobBuilderWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("buildqueue")] string jobId, TextWriter log)
        {
            log.WriteLine(jobId);
            Task.Factory.StartNew(() =>
            {
                LSA.ProcessAndStore(int.Parse(jobId));
            });
        }
    }
}
