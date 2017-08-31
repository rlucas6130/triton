using Engine;
using Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using UI.Controllers.Helpers;

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
            AzureHelper.SendQueueMessage("buildqueue", Tuple.Create(job.Id, docIds));
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}