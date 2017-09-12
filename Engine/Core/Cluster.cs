using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Engine.Core
{
    public class Cluster
    {
        public ConcurrentDictionary<int, float> Distances { get; set; } = new ConcurrentDictionary<int, float>();
        public ConcurrentDictionary<int, int> ClusterMap { get; set; } = new ConcurrentDictionary<int, int>();
        public Dictionary<int, int> DocsPerCluster { get; set; } = new Dictionary<int, int>();
        public ConcurrentDictionary<int, float[]> Centers { get; set; } = new ConcurrentDictionary<int, float[]>();
        public ConcurrentBag<Tuple<int, float>> ClusterSi { get; set; } = new ConcurrentBag<Tuple<int, float>>();
        public Dictionary<int, float> ClusterSiAverages { get; set; } = new Dictionary<int, float>();
        public ConcurrentDictionary<int, float> DocumentSi { get; set; } = new ConcurrentDictionary<int, float>();
        public bool IsOptimized { get; set; } = false;
        public float OptimizationVarianceThreshold { get; set; } = .00000002F;
        public int MaxIterations { get; set; }
        public float GlobalSi { get; set; }
        public float GlobalClusterSiAverage { get; set; }
        public int Clusters { get; set; }
        public int JobId { get; set; }

        public Cluster(Random generator, int jobId, int k = 2, int maxIteration = 200)
        {
            Clusters = k;
            MaxIterations = maxIteration;
            JobId = jobId;

            LSA.GetMatrixContainer(JobId);

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

            // Silhouette (clustering) 
            // https://en.wikipedia.org/wiki/Silhouette_(clustering)

            ClusterMap.AsParallel().ForAll(kvp =>
            {
                // Get aI
                var currentCluster = kvp.Value;

                var targetCluster = ClusterMap.Where(c => c.Value == currentCluster).ToList();
                var distanceTotal = new List<float>();

                for(var i = 0; i < targetCluster.Count; i++)
                {
                    var distance = LSA.MatrixContainer.DistanceMap[kvp.Key, targetCluster[i].Key];

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
                        var distanceOther = LSA.MatrixContainer.DistanceMap[kvp.Key, otherTargetCluster[m].Key];

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
                    DocumentSi.AddOrUpdate(kvp.Key, sI, (i, f) => sI);
                    ClusterSi.Add(Tuple.Create(currentCluster, sI));
                }
            });
            
            // Calc Cluster Si Averages
            for (var m = 0; m < Clusters; m++)
            {
                if(ClusterSi.Any(c => c.Item1 == m))
                {
                    ClusterSiAverages[m] = ClusterSi
                        .Where(c => c.Item1 == m)
                        .Select(c => c.Item2)
                        .Average();
                }
            }

            // Calculate Si Averages
            GlobalSi = DocumentSi.Average(kvp => kvp.Value);
            GlobalClusterSiAverage = ClusterSiAverages.Average((kvp) => kvp.Value);

            Debug.WriteLine($"****{Clusters}***** GlobalSi: {GlobalSi}");
            Debug.WriteLine($"****{Clusters}***** GlobalClusterSiAverage: {GlobalClusterSiAverage}");
            Debug.WriteLine($"----{Clusters}---- Total Cluster SI Calc: {DateTime.Now.Subtract(calcSiStart).TotalMilliseconds} Milliseconds");
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

        public static ClusterCalculation CreateCalculation(SvdEntities context, Contracts.ClusterCalculationParameters clusterParams)
        {
            var clusterCalculationEntity = context.ClusterCalculations.Add(new ClusterCalculation()
            {
                JobId = clusterParams.JobId.GetValueOrDefault(),
                MinimumClusterCount = clusterParams.MinimumClusterCount,
                MaximumClusterCount = clusterParams.MaximumClusterCount,
                IterationsPerCluster = clusterParams.IterationsPerCluster,
                MaximumOptimizationsCount = clusterParams.MaximumOptimizationsCount,
            });

            SetCalculationStatus(context, clusterCalculationEntity, Contracts.ClusterCalculationStatus.New);

            context.SaveChanges();

            return clusterCalculationEntity;
        }

        public static void SetCalculationStatus(SvdEntities context, ClusterCalculation clusterCalculationEntity, Contracts.ClusterCalculationStatus status)
        {
            if (status == Contracts.ClusterCalculationStatus.New)
                clusterCalculationEntity.Created = DateTime.Now;

            if (status == Contracts.ClusterCalculationStatus.Completed || status == Contracts.ClusterCalculationStatus.Failed)
                clusterCalculationEntity.Completed = DateTime.Now;

            clusterCalculationEntity.Status = status;
            context.SaveChanges();
        }

        public void Save(SvdEntities context, ClusterCalculation clusterCalculationEntity)
        {
            var binaryFormatter = new BinaryFormatter();

            var jobDocs = context.JobDocuments.Where(jd => jd.JobId == JobId).ToLookup(jd => jd.OrdinalIndex);
            var jobTerms = context.JobTerms.Where(jd => jd.JobId == JobId).ToLookup(jt => jt.Term.Value);
            var clusterEntities = new Dictionary<int, Engine.Cluster>();

            clusterCalculationEntity.ClusterCount = Clusters;
            clusterCalculationEntity.GlobalSi = GlobalSi;
            clusterCalculationEntity.ClusterSi = GlobalClusterSiAverage;

            // Update Cluster Calculation
            context.SaveChanges();

            Enumerable.Range(0, Clusters).ToList().ForEach(cluster =>
            {
                using (var memoryStreamCenterVector = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStreamCenterVector, Centers[cluster]);

                    memoryStreamCenterVector.Position = 0;

                    clusterEntities.Add(cluster, new Engine.Cluster()
                    {
                        JobId = JobId,
                        ClusterCalculationId = clusterCalculationEntity.Id,
                        Si = ClusterSiAverages[cluster],
                        CenterVectorSerialized = memoryStreamCenterVector.ToArray()
                    });
                }
            });

            // Insert Clusters
            context.BulkInsert(clusterEntities.Select(kvp => kvp.Value));

            var clusterJobDocumentEntities = new ConcurrentBag<ClusterJobDocument>();
            var clusterJobTermEntities = new ConcurrentBag<ClusterJobTerm>();

            clusterEntities.AsParallel().ForAll(clusterEntity =>
            {
                using (var memoryStreamCenterVector = new MemoryStream())
                {
                    var termDistanceMap = new Dictionary<string, float>();
                    var centerVector = Centers[clusterEntity.Key];

                    foreach (var kvp in ClusterMap.Where(kvp => kvp.Value == clusterEntity.Key))
                    {
                        var docIndex = kvp.Key;
                        var jobDocument = jobDocs[docIndex];

                        if (jobDocument != null)
                        {
                            clusterJobDocumentEntities.Add(new ClusterJobDocument()
                            {
                                ClusterCalculationId = clusterCalculationEntity.Id,
                                ClusterId = clusterEntity.Value.Id,
                                JobId = JobId,
                                Si = DocumentSi.ContainsKey(docIndex) ? DocumentSi[docIndex] : 0,
                                JobDocumentId = jobDocument.First().Id
                            });
                        }
                    }

                    for (var i = 0; i < LSA.MatrixContainer.UMatrix.RowCount; i++)
                    {
                        termDistanceMap[LSA.MatrixContainer.Terms[i]] = Distance.Cosine(centerVector, LSA.MatrixContainer.UMatrix.Row(i).ToArray());
                    }

                    foreach (var term in termDistanceMap.OrderBy(t => t.Value).Take(20))
                    {
                        var jobTermLookup = jobTerms[term.Key];

                        if (jobTermLookup != null)
                        {
                            clusterJobTermEntities.Add(new ClusterJobTerm()
                            {
                                ClusterCalculationId = clusterCalculationEntity.Id,
                                ClusterId = clusterEntity.Value.Id,
                                JobId = JobId,
                                JobTermId = jobTermLookup.First().Id,
                                DistanceToClusterCenter = term.Value
                            });
                        }
                    }
                }
            });

            // Insert Cluster Documents & Terms
            context.BulkInsert(clusterJobTermEntities);
            context.BulkInsert(clusterJobDocumentEntities);

            SetCalculationStatus(context, clusterCalculationEntity, Contracts.ClusterCalculationStatus.Completed);
        }

        public static IEnumerable<ClusterCalculation> GetAll(SvdEntities context, int jobId)
        {
            return context.ClusterCalculations.Where(cc => cc.JobId == jobId).ToList(); 
        }

        public static ClusterCalculation Get(SvdEntities context, int clusterCalculationId)
        {
            return context.ClusterCalculations
                .FirstOrDefault(cc => cc.Id == clusterCalculationId);
        }
    }
}
