using AutoMapper;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UI.ViewModels.Dtos;
using LSA = Engine.Core.LSA;

namespace UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(config =>
            {
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            });

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Mapper.Initialize(config => {
                config.CreateMap<Engine.Job, Job>();
                config.CreateMap<Engine.Document, Document>()
                    .ForMember(dest => dest.TotalTermDocCount, opt => opt.ResolveUsing<TermDocCountResolver>());
                config.CreateMap<Engine.ClusterCalculation, ClusterCalculation>();
                config.CreateMap<Engine.Cluster, Cluster>()
                    .ForMember(dest => dest.CenterVector, opt => opt.ResolveUsing<BinaryVectorResolver, byte[]>(src => src.CenterVectorSerialized));

                config.CreateMap<Engine.ClusterJobDocument, ClusterJobDocument>()
                    .ForMember(dest => dest.Name, opt => opt.ResolveUsing(c => c.JobDocument.Document.Name))
                    .ForMember(dest => dest.Vector, opt => opt.ResolveUsing<BinaryVMatrixResolver>());

                config.CreateMap<Engine.ClusterJobTerm, ClusterJobTerm>()
                    .ForMember(dest => dest.Value, opt => opt.ResolveUsing(c => c.JobTerm.Term.Value))
                    .ForMember(dest => dest.Vector, opt => opt.ResolveUsing<BinaryUMatrixResolver>());
            });
        }

        private static BinaryFormatter _binaryFormmater = new BinaryFormatter();

        private class BinaryVectorResolver : IMemberValueResolver<Engine.Cluster, Cluster, byte[], float[]>
        {
            public float[] Resolve(Engine.Cluster source, Cluster destination, byte[] sourceSerializedVector, float[] destinationMember, ResolutionContext context)
            {
                using (var ms = new MemoryStream(sourceSerializedVector))
                {
                    return _binaryFormmater.Deserialize(ms) as float[];
                }
            }
        }

        private class BinaryVMatrixResolver : IValueResolver<Engine.ClusterJobDocument, ClusterJobDocument, float[]>
        {
            public float[] Resolve(Engine.ClusterJobDocument source, ClusterJobDocument destination, float[] destinationMember, ResolutionContext context)
            {
                return LSA.GetDocumentVector(source.JobId, source.JobDocument.OrdinalIndex);
            }
        }

        private class BinaryUMatrixResolver : IValueResolver<Engine.ClusterJobTerm, ClusterJobTerm, float[]>
        {
            public float[] Resolve(Engine.ClusterJobTerm source, ClusterJobTerm destination, float[] destinationMember, ResolutionContext context)
            {
                return LSA.GetTermVector(source.JobId, source.JobTerm.OrdinalIndex);
            }
        }

        private class TermDocCountResolver : IValueResolver<Engine.Document, Document, int?>
        {
            public int? Resolve(Engine.Document source, Document destination, int? destinationMember, ResolutionContext context)
            {
                return context.Items.ContainsKey("context") ? 
                    LSA.GetTotalTermDocCount(context.Items["context"] as Engine.SvdEntities, source.Id) : 0;
            }
        }
    }
}
