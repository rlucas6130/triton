﻿using System;
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

        public static Cluster Optimize(Random randomGenerator, StringBuilder csv, int k = 2, int iterations = 4, int maxOptimizationIterations = 100)
        {
            var clusters = (from c in Enumerable.Range(0, iterations).AsParallel()
                           select new Cluster(randomGenerator, csv, k, maxOptimizationIterations)).ToList();

            return clusters
                .OrderByDescending(c => c.GlobalSi)
                .ThenByDescending(c => c.ClusterSiAverage).First();
        }

        public static Cluster OptimizeRange(int kStart = 2, int kEnd = 100, int iterations = 4, int maxOptimizationIterations = 200)
        {
            var randGen = new Random();
            var csv = new StringBuilder();

            csv.AppendLine($"Clusters,Global Si,Cluster Si");

            var clusters = (from k in Enumerable.Range(kStart, (kEnd - kStart)).AsParallel()
                            select Optimize(randGen, csv, k, iterations, maxOptimizationIterations)).ToList();

            File.WriteAllText($"D:/Wiki/{LSA.NumDocs}/{DateTime.Now.ToString(@"MM-dd-yyyy-HH-mm")}.csv", csv.ToString());

            return clusters
                .OrderByDescending(c => c.GlobalSi)
                .ThenByDescending(c => c.ClusterSiAverage).First();
        }
    }
}
