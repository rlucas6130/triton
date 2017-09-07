using System;
using System.Linq;

namespace Engine.Core
{
    public class ClusterOptimizer
    {
        public ClusterOptimizer()
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

        public static Cluster OptimizeRange(SvdEntities context, int clusterCalculationId) =>
            OptimizeRange(context, Cluster.Get(context, clusterCalculationId));


        public static Cluster OptimizeRange(SvdEntities context, Contracts.ClusterCalculationParameters clusterAnalysisParameters) =>
            OptimizeRange(context, Cluster.CreateCalculation(context, clusterAnalysisParameters));


        private static Cluster OptimizeRange(SvdEntities context, ClusterCalculation clusterCalculationEntity)
        {
            try
            {
                var randGen = new Random();

                Cluster.SetCalculationStatus(context, clusterCalculationEntity, Contracts.ClusterStatus.Clustering);

                var clusters = (from k in Enumerable.Range(clusterCalculationEntity.MinimumClusterCount, (clusterCalculationEntity.MaximumClusterCount - clusterCalculationEntity.MinimumClusterCount) + 1)
                                select Optimize(randGen, clusterCalculationEntity.JobId, k, clusterCalculationEntity.IterationsPerCluster, clusterCalculationEntity.MaximumOptimizationsCount)).ToList();

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
