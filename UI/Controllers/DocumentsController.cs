using AutoMapper;
using Engine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using UI.Controllers.Helpers;
using UI.ViewModels;

namespace UI.Controllers
{
    public class DocumentsController : ApiController
    {
        private Engine.SvdEntities _context = new Engine.SvdEntities();

        // GET api/<controller>
        public IEnumerable<Document> Get(int page = 1, int docsPerPage = 20)
        {
            var documents = LSA.GetDocuments(_context, page, docsPerPage);

            return Mapper.Map<IEnumerable<Engine.Document>, IEnumerable<Document>>(documents, opt => opt.Items["context"] = _context); ;
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
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
        [Route("api/documents/upload")]
        public async Task<HttpResponseMessage> Upload()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);
            var blobContainer = AzureHelper.GetBlobContainer("documents");

            try
            {
                // Read the form data and return an async task. 
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names for uploaded files. 
                foreach (var fileData in provider.FileData)
                {
                    var filename = fileData.LocalFileName;
                    var blob = blobContainer.GetBlockBlobReference(fileData.Headers.ContentDisposition.FileName);

                    using (var filestream = File.OpenRead(fileData.LocalFileName))
                    {
                        blob.UploadFromStream(filestream);
                    }

                    AzureHelper.SendQueueMessage("documentqueue", fileData.Headers.ContentDisposition.FileName);

                    File.Delete(fileData.LocalFileName);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}