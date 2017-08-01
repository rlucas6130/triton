using Engine;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace UI.Controllers
{
    public class JobsController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<Job> Get()
        {
            return LSA.GetJobs().OrderByDescending(i => i.Created);
        }

        // GET api/<controller>/5
        public Job Get(int id)
        {
            return LSA.GetJob(id);
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
            var job = LSA.CreateNewJob();

            SendProcessingRequestMessage(job.Id);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        private void SendProcessingRequestMessage(int jobId)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("buildqueue");

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(jobId.ToString());
            queue.AddMessage(message);
        }
    }
}