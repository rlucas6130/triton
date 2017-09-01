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
        private SvdEntities _context = new SvdEntities();

        public ClusterCalculationsController()
        {
            _context.Configuration.ProxyCreationEnabled = false;
        }

        // GET api/<controller>
        [HttpGet]
        [Route("api/clusterCalculations/getAll")]
        public IEnumerable<ClusterCalculation> GetAll(int jobId)
        {
            return ClusterCalculationManager.GetAll(_context, jobId)
                .OrderByDescending(i => i.Created);
        }

        public ClusterCalculation Get(int id)
        {
            return ClusterCalculationManager.Get(_context, id);
        }

        // POST api/<controller>
        public void Post([FromBody]Contracts.ClusterAnalysisParameters clusterParams)
        {
            var clusterCalculation = ClusterCalculationManager.CreateCalculation(_context, clusterParams);

            AzureHelper.SendQueueMessage("clusterqueue", clusterCalculation.Id);
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