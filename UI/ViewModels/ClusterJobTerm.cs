namespace UI.ViewModels
{
    public class ClusterJobTerm
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int ClusterCalculationId { get; set; }
        public int JobTermId { get; set; }
        public int ClusterId { get; set; }
        public float DistanceToClusterCenter { get; set; }
        public float[] Vector { get; set; }
        public string Value { get; set; }
    }
}