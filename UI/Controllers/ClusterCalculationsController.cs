using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using UI.Controllers.Helpers;
using ClusterCalculationManager = Engine.Core.Cluster;

namespace UI.Controllers
{
    public class ClusterCalculationsController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [Route("api/clusterCalculations/getAll")]
        public IEnumerable<ClusterCalculation> GetAll(int jobId)
        {
            return ClusterCalculationManager.GetAll(jobId).OrderByDescending(i => i.Created);
        }

        public ClusterCalculation Get(int id)
        {
            return ClusterCalculationManager.Get(id);
        }

        // POST api/<controller>
        public void Post(int jobId, [FromBody]Contracts.ClusterAnalysisParameters clusterParams)
        {
            AzureHelper.SendQueueMessage("clusterqueue", Tuple.Create(jobId, clusterParams));
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