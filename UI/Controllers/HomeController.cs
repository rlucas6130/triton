using Engine;
using Engine.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public JsonResult GetClusters(int jobId, int k = 2)
        {
            var nameList = new Dictionary<string, List<string>>();

            var start = DateTime.Now;

            var cluster = ClusterOptimizer.OptimizeRange(jobId, new Contracts.ClusterAnalysisParameters() {
                MinimumClusterCount = 20,
                MaximumClusterCount = 20,
                IterationsPerCluster = 1,
                MaximumOptimizationsCount = 200
            });

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
                ClusterSIAverage = cluster.GlobalClusterSiAverage,
                ClusterSiList = cluster.ClusterSiAverages,
                NameList = nameList,
                NumClusters = cluster.Clusters
            }, JsonRequestBehavior.AllowGet);
        }
    }
}