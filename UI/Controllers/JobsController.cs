using Engine;
using Engine.Core;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Post([FromBody]IEnumerable<int> docIds)
        {
            var job = LSA.CreateNewJob(docIds.Count());

            SendProcessingRequestMessage("buildqueue", job.Id, docIds);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        [HttpPost]
        [Route("api/jobs/clusterAnalysis")]
        public void ClusterAnalysis(int jobId, Contracts.ClusterAnalysisParameters clusterParams)
        {
            SendProcessingRequestMessage("clusterqueue", jobId, clusterParams);
        }

        private void SendProcessingRequestMessage(string queueName, int jobId, object serializableRequestPayload)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(Tuple.Create(jobId, serializableRequestPayload)));

            queue.AddMessageAsync(message);
        }
    }
}