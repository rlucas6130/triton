using AutoMapper;
using Engine.Contracts;
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
using UI.ViewModels.Dtos;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;

namespace UI.Controllers
{
    public class DocumentsController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<Document> Get(int page = 1, int docsPerPage = 20)
        {
            using (var context = new Engine.SvdEntities())
            {
                var documents = LSA.GetDocuments(context, page, docsPerPage);

                return Mapper.Map<IEnumerable<Engine.Document>, IEnumerable<Document>>(documents, opt => opt.Items["context"] = context); ;
            }
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

            try
            {
                // Read the form data and return an async task. 
                await Request.Content.ReadAsMultipartAsync(provider); 

                var task = Task.Factory.StartNew((fd) =>
                {
                    var blobContainer = AzureHelper.GetBlobContainer("uploadbatch");

                    var typedFd = fd as Collection<MultipartFileData>;

                    var fileDataQueue = new List<UploadDocumentParameter>();

                    var batchGuid = Guid.NewGuid().ToString();

                    foreach (var file in typedFd)
                    {
                        fileDataQueue.Add(new UploadDocumentParameter()
                        {
                            FileName = file.Headers.ContentDisposition.FileName,
                            StreamData = File.ReadAllBytes(file.LocalFileName)
                        });
                    }

                    var blob = blobContainer.GetBlockBlobReference(batchGuid);

                    var binaryFormatter = new BinaryFormatter();

                    using (var memoryStream = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStream, fileDataQueue.ToArray());

                        memoryStream.Position = 0;

                        var batchBytes = memoryStream.ToArray();

                        blob.UploadFromByteArray(batchBytes, 0, batchBytes.Length);
                    }

                    AzureHelper.SendQueueMessage(batchGuid, "uploadqueue");

                }, provider.FileData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}