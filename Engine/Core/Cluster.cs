using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Engine.Core
{
    public class Cluster
    {
        public ConcurrentDictionary<int, float> Distances { get; set; }
        public ConcurrentDictionary<int, int> ClusterMap { get; set; }
        public Dictionary<int, int> DocsPerCluster { get; set; }
        public ConcurrentDictionary<int, float[]> Centers { get; set; }
        public ConcurrentBag<Tuple<int, float>> ClusterSi { get; set; }
        public Dictionary<string, float> ClusterSiAverages { get; set; }
        public bool IsOptimized { get; set; }
        public float OptimizationVarianceThreshold { get; set; }
        public int MaxIterations { get; set; }
        public float GlobalSi { get; set; }
        public float ClusterSiAverage { get; set; }
        public int Clusters { get; set; }
        public int JobId { get; set; }

        public Cluster(Random generator, int jobId, int k = 2, int maxIteration = 200)
        {
            Clusters = k;

            Centers = new ConcurrentDictionary<int, float[]>();
            ClusterMap = new ConcurrentDictionary<int, int>();
            DocsPerCluster = new Dictionary<int, int>();
            Distances = new ConcurrentDictionary<int, float>();
            ClusterSi = new ConcurrentBag<Tuple<int, float>>();
            ClusterSiAverages = new Dictionary<string, float>();
            IsOptimized = false;
            OptimizationVarianceThreshold = .00000002F;
            MaxIterations = maxIteration;
            JobId = jobId;

            if (LSA.MatrixContainer == null)
            {
                LSA.GetMatrixContainer(JobId);
            }

            var clusterCentersDocIndex = new HashSet<int>();

            while (clusterCentersDocIndex.Count <= Clusters)
            {
                clusterCentersDocIndex.Add(generator.Next(LSA.MatrixContainer.VMatrix.ColumnCount));
            }

            for (var i = 0; i < Clusters; i++)
            {
                Centers[i] = LSA.MatrixContainer.VMatrix.Column(clusterCentersDocIndex.ElementAt(i)).ToArray();
            }

            var counter = 0;

            var start = DateTime.Now;

            while(!IsOptimized && ++counter < MaxIterations)
            {
                CalcDistancesAndAssign();

                MoveCenters();
            }

            Debug.WriteLine($"----{Clusters}----Total Iterations: {counter}");

            for (var i = 0; i < Clusters; i++)
            {
                DocsPerCluster[i] = ClusterMap.Count(c => c.Value == i);
            }

            Debug.WriteLine($"----{Clusters}----Total Cluster Calc: {DateTime.Now.Subtract(start).TotalMilliseconds} Milliseconds");

            var calcSiStart = DateTime.Now;
            var siList = new ConcurrentBag<float>();

            // Silhouette (clustering) 
            // https://en.wikipedia.org/wiki/Silhouette_(clustering)

            ClusterMap.AsParallel().ForAll(kvp =>
            {
                // Get aI
                var currentCluster = kvp.Value;

                //var targetDoc = LSA.MatrixContainer.VMatrix.Column(kvp.Key).ToArray();

                var targetCluster = ClusterMap.Where(c => c.Value == currentCluster).ToList();
                var distanceTotal = new List<float>();

                for(var i = 0; i < targetCluster.Count; i++)
                {
                    //var docToCompare = LSA.MatrixContainer.VMatrix.Column(targetCluster[i].Key).ToArray();
                    var distance = LSA.MatrixContainer.DistanceMap[kvp.Key, targetCluster[i].Key]; // Distance.Cosine(targetDoc, docToCompare);

                    if (!float.IsInfinity(distance) && !float.IsNaN(distance))
                    {
                        distanceTotal.Add(distance);
                    }
                }

                var aI = distanceTotal.Sum() / DocsPerCluster[currentCluster];

                // Get bI
                var otherClusterDistances = new List<float>();

                for(var i = 0; i < Clusters; i++)
                {
                    if (i == currentCluster) continue;

                    var otherTargetCluster = ClusterMap.Where(c => c.Value == i).ToList();
                    var distanceOtherTotal = new List<float>();

                    for(var m = 0; m < otherTargetCluster.Count; m++)
                    {
                        //var docToCompareOther = LSA.MatrixContainer.VMatrix.Column(otherTargetCluster[m].Key).ToArray();
                        var distanceOther = LSA.MatrixContainer.DistanceMap[kvp.Key, otherTargetCluster[m].Key];//Distance.Cosine(targetDoc, docToCompareOther);

                        if (!float.IsInfinity(distanceOther) && !float.IsNaN(distanceOther))
                        {
                            distanceOtherTotal.Add(distanceOther);
                        }
                    }

                    var otherDistanceAverage = distanceOtherTotal.Sum() / DocsPerCluster[i];

                    if (!float.IsInfinity(otherDistanceAverage) && !float.IsNaN(otherDistanceAverage))
                    {
                        otherClusterDistances.Add(otherDistanceAverage);
                    }
                }

                var bI = otherClusterDistances.Min();

                var sI = (bI - aI) / Math.Max(aI, bI);

                if (!float.IsInfinity(sI) && !float.IsNaN(sI))
                {
                    siList.Add(sI);

                    ClusterSi.Add(Tuple.Create(currentCluster, sI));
                }
            });
            
            // Calc Cluster Si Averages
            for (var m = 0; m < Clusters; m++)
            {
                if(ClusterSi.Any(c => c.Item1 == m))
                {
                    ClusterSiAverages[m.ToString()] = ClusterSi
                        .Where(c => c.Item1 == m)
                        .Select(c => c.Item2)
                        .Average();
                }
            }

            // Calculate Si Averages
            GlobalSi = siList.Average();
            ClusterSiAverage = ClusterSiAverages.Average((kvp) => kvp.Value);

            Debug.WriteLine($"****{Clusters}***** GlobalSi: {GlobalSi}");
            Debug.WriteLine($"****{Clusters}***** ClusterSiAverage: {ClusterSiAverage}");
            Debug.WriteLine($"----{Clusters}---- Total Cluster SI Calc: {DateTime.Now.Subtract(calcSiStart).TotalMilliseconds} Milliseconds");
        }

        public void Save()
        {
            using (var context = new SvdEntities())
            {
                var clusterCalculationEntity = context.ClusterCalculations.Add(new ClusterCalculation()
                {
                    JobId = JobId,
                    ClusterCount = Clusters,
                    GlobalSi = GlobalSi,
                    ClusterSi = ClusterSiAverage
                });

                Enumerable.Range(0, Clusters).ToList().ForEach(cluster =>
                {
                    var clusterEntity = context.Clusters.Add(new Engine.Cluster()
                    {
                        JobId = JobId,
                        ClusterCalculation = clusterCalculationEntity,
                        Si = ClusterSi.First(c => c.Item1 == cluster).Item2
                    });

                    foreach (var kvp in ClusterMap.Where(kvp => kvp.Value == cluster))
                    {
                        var docIndex = kvp.Key;

                        clusterEntity.ClusterJobDocuments.Add(new ClusterJobDocument()
                        {
                            ClusterCalculation = clusterCalculationEntity,
                            Cluster = clusterEntity,
                            JobId = JobId,
                            JobDocument = context.JobDocuments.FirstOrDefault(jd => jd.JobId == JobId && jd.OrdinalIndex == docIndex)
                        });

                    }

                    //Centers.AsParallel().ForAll(centerVector =>
                    //{
                    //    var termDistanceMap = new Dictionary<string, float>();

                    //    for (var i = 0; i < LSA.MatrixContainer.UMatrix.RowCount; i++)
                    //    {
                    //        termDistanceMap[LSA.MatrixContainer.Terms[i]] = Distance.Cosine(centerVector.Value, LSA.MatrixContainer.UMatrix.Row(i).ToArray());
                    //    }

                    //    foreach(var term in termDistanceMap.OrderBy(t => t.Value).Select(t => t.Key).Take(20))
                    //    {
                    //        clusterEntity.ClusterJobTerms.Add(new ClusterJobTerm()
                    //        {
                    //            ClusterCalculation = clusterCalculationEntity,
                    //            Cluster = clusterEntity,
                    //            JobId = JobId,
                    //            JobTerm = context.JobTerms.FirstOrDefault(jd => jd.JobId == JobId && jd.Term.Value == term)
                    //        });
                    //    }
                    //});
                });

                context.SaveChanges();
            }
        }

        public void CalcDistancesAndAssign()
        {
            Enumerable.Range(0, Clusters).AsParallel().ForAll(i =>
            {
                for (var m = 0; m < LSA.MatrixContainer.VMatrix.ColumnCount; m++)
                {
                    var newDistance = Distance.Cosine(Centers[i], LSA.MatrixContainer.VMatrix.Column(m).ToArray());

                    if(!Distances.ContainsKey(m) || newDistance < Distances[m])
                    {
                        ClusterMap[m] = i;
                        Distances[m] = newDistance;
                    }
                }
            });
        }

        public void MoveCenters()
        {
            var isOptimizedMap = new ConcurrentDictionary<int, bool>();

            Enumerable.Range(0, Clusters).AsParallel().ForAll(i =>
            {
                var clusterTotal = ClusterMap.Count(c => c.Value == i);

                if (clusterTotal > 0)
                {
                    DenseVector vectorSum = null;

                    for (var m = 0; m < LSA.MatrixContainer.VMatrix.ColumnCount; m++)
                    {
                        if (ClusterMap[m] == i)
                        {
                            if (vectorSum == null)
                            {
                                vectorSum = (DenseVector)LSA.MatrixContainer.VMatrix.Column(m);
                            }
                            else
                            {
                                vectorSum += (DenseVector)LSA.MatrixContainer.VMatrix.Column(m);
                            }
                        }
                    }

                    var newCenter = (vectorSum / clusterTotal).ToArray();

                    if (Centers[i] != null)
                    {
                        isOptimizedMap[i] = Distance.Cosine(Centers[i], newCenter) < OptimizationVarianceThreshold;
                    }

                    Centers[i] = newCenter;
                }
            });

            IsOptimized = isOptimizedMap.All(v => v.Value == true);
        }
    }
}
