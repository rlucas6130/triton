using System.Collections.Generic;

namespace UI.ViewModels.Dtos
{
    public class ClusterCalculation : Calculable
    {
        public int Id { get; set; }
        public int? ClusterCount { get; set; }
        public float? GlobalSi { get; set; }
        public float? ClusterSi { get; set; }
        public int JobId { get; set; }
        public int MinimumClusterCount { get; set; }
        public int MaximumClusterCount { get; set; }
        public int IterationsPerCluster { get; set; }
        public int MaximumOptimizationsCount { get; set; }
        public Engine.Contracts.ClusterStatus Status { get; set; }
        public virtual IEnumerable<Cluster> Clusters { get; set; }
    }
}