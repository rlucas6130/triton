using System;
using System.Linq;

namespace Engine.Core
{
    public class ClusterOptimizer
    {
        public int NumClusters { get; set; }

        public ClusterOptimizer(int k = 2)
        {
            // Method #1
            // Create 'n' number of Cluster(k) objects and select the one with the best Si stats. 

            // Method #2
            // Monitor each cluster Si and remove each cluster from the calculation as they become 'optimized'
        }

        public static Cluster Optimize(Random randomGenerator, int jobId, int k = 2, int iterations = 4, int maxOptimizationIterations = 200)
        {
            var clusters = (from c in Enumerable.Range(0, iterations)
                           select new Cluster(randomGenerator, jobId, k, maxOptimizationIterations)).ToList();

            return clusters
                .OrderByDescending(c => c.GlobalSi)
                .ThenByDescending(c => c.GlobalClusterSiAverage).First();
        }

        public static Cluster OptimizeRange(int jobId, Contracts.ClusterAnalysisParameters clusterParams)
        {
            using (var context = new SvdEntities())
            {
                var clusterCalculationEntity = Cluster.CreateCalculation(context, jobId, clusterParams);

                try
                {
                    var randGen = new Random();

                    Cluster.SetCalculationStatus(context, clusterCalculationEntity, Contracts.ClusterStatus.Clustering);

                    var clusters = (from k in Enumerable.Range(clusterParams.MinimumClusterCount, (clusterParams.MaximumClusterCount - clusterParams.MinimumClusterCount) + 1)
                                    select Optimize(randGen, jobId, k, clusterParams.IterationsPerCluster, clusterParams.MaximumOptimizationsCount)).ToList();

                    var optimizedCluster = clusters
                        .OrderByDescending(c => c.GlobalSi)
                        .ThenByDescending(c => c.GlobalClusterSiAverage).First();

                    optimizedCluster.Save(context, clusterCalculationEntity);

                    return optimizedCluster;
                }
                catch (Exception)
                {
                    Cluster.SetCalculationStatus(context, clusterCalculationEntity, Contracts.ClusterStatus.Failed);
                    throw;
                }
            }
        }
    }
}
