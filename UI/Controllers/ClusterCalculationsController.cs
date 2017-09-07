using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using UI.Controllers.Helpers;
using UI.ViewModels.Dtos;
using ClusterCalculationManager = Engine.Core.Cluster;

namespace UI.Controllers
{
    public class ClusterCalculationsController : ApiController
    {
        private Engine.SvdEntities _context = new Engine.SvdEntities();
        private Engine.SvdEntities _noLazyContext = new Engine.SvdEntities();

        public ClusterCalculationsController()
        {
            _noLazyContext.Configuration.LazyLoadingEnabled = false;
            _noLazyContext.Configuration.ProxyCreationEnabled = false;
        }

        // GET api/<controller>
        [HttpGet]
        [Route("api/clusterCalculations/getAll")]
        public IEnumerable<ClusterCalculation> GetAll(int jobId)
        {
            var clusterCalculations = ClusterCalculationManager.GetAll(_noLazyContext, jobId)
                .OrderByDescending(i => i.Created);

            return Mapper.Map<IEnumerable<Engine.ClusterCalculation>, IEnumerable<ClusterCalculation>>(clusterCalculations);
        }

        public ClusterCalculation Get(int id)
        {
            var clusterCalculation = ClusterCalculationManager.Get(_context, id);

            return Mapper.Map<Engine.ClusterCalculation, ClusterCalculation>(clusterCalculation);
        }

        // POST api/<controller>
        public void Post([FromBody]Engine.Contracts.ClusterCalculationParameters clusterParams)
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