using Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void LoadMatrixData()
        {
            try
            {
                LSA.GetMatrixContainer();
            }
            catch (Exception e) { }
            
        }

        public JsonResult GetClusters(int k = 2)
        {
            var nameList = new Dictionary<string, List<string>>();

            var start = DateTime.Now;

            var cluster = ClusterOptimizer.OptimizeRange(90, 100, 1, 500);

            cluster.BuildCategoryNameMap();

            Debug.WriteLine($"Total Optimization Time: {DateTime.Now.Subtract(start).TotalMilliseconds} Milliseconds");

            for (var i = 0; i < cluster.ClusterMap.Count; i++)
            {
                var fileName = LSA.MatrixContainer.DocNameMap[i];

                if (!nameList.ContainsKey(cluster.ClusterMap[i].ToString()))
                {
                    nameList[cluster.ClusterMap[i].ToString()] = new List<string>();
                }

                nameList[cluster.ClusterMap[i].ToString()].Add(fileName);
            }

            return Json(new {
                GlobalSI = cluster.GlobalSi,
                ClusterSIAverage = cluster.ClusterSiAverage,
                ClusterSiList = cluster.ClusterSiAverages,
                NameList = nameList,
                CategoryNameMap = cluster.CategoryNameMap,
                NumClusters = cluster.Clusters
            }, JsonRequestBehavior.AllowGet);
        }
    }
}