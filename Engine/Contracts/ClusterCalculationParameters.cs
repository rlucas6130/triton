namespace Engine.Contracts
{
    public class ClusterCalculationParameters
    {
        public int MinimumClusterCount { get; set; }
        public int MaximumClusterCount { get; set; }
        public int IterationsPerCluster { get; set; }
        public int MaximumOptimizationsCount { get; set; }
        public int? JobId { get; set; }
        public int? ClusterCalculationId { get; set; }
    }
}
