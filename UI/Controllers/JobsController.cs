using AutoMapper;
using Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using UI.Controllers.Helpers;
using UI.ViewModels;

namespace UI.Controllers
{
    public class JobsController : ApiController
    {
        private Engine.SvdEntities _context = new Engine.SvdEntities();

        // GET api/<controller>
        public IEnumerable<Job> Get()
        {
            var jobs = LSA.GetJobs(_context).OrderByDescending(i => i.Created);

            return Mapper.Map<IEnumerable<Engine.Job>, IEnumerable<Job>>(jobs);
        }

        // GET api/<controller>/5
        public Job Get(int id)
        {
            var job = LSA.GetJob(_context, id);

            return Mapper.Map<Engine.Job, Job>(job);
        }

        // POST api/<controller>
        public void Post([FromBody]IEnumerable<int> docIds)
        {
            var job = LSA.CreateNewJob(_context, docIds.Count());
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