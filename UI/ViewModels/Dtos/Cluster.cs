using System.Collections.Generic;

namespace UI.ViewModels.Dtos
{
    public class Cluster
    { 
        public int Id { get; set; }
        public int JobId { get; set; }
        public int ClusterCalculationId { get; set; }
        public float[] CenterVector { get; set; }
        public float? Si { get; set; }

        public IEnumerable<ClusterJobDocument> ClusterJobDocuments { get; set; }
        public IEnumerable<ClusterJobTerm> ClusterJobTerms { get; set; }
    }
}