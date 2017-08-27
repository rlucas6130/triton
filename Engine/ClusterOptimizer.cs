using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
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
                .ThenByDescending(c => c.ClusterSiAverage).First();
        }

        public static Cluster OptimizeRange(int jobId, int kStart = 2, int kEnd = 100, int iterations = 4, int maxOptimizationIterations = 200)
        {
            var randGen = new Random();

            var clusters = (from k in Enumerable.Range(kStart, (kEnd - kStart) + 1)
                            select Optimize(randGen, jobId, k, iterations, maxOptimizationIterations)).ToList();

            var optimizedCluster = clusters
                .OrderByDescending(c => c.GlobalSi)
                .ThenByDescending(c => c.ClusterSiAverage).First();

            optimizedCluster.BuildCategoryNameMap();

            return optimizedCluster;
        }
    }
}
