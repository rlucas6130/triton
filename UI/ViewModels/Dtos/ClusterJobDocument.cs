namespace UI.ViewModels.Dtos
{
    public class ClusterJobDocument
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int ClusterCalculationId { get; set; }
        public int JobDocumentId { get; set; }
        public int ClusterId { get; set; }
        public float Si { get; set; }
        public float[] Vector { get; set; }
        public string Name { get; set; }
    }
}