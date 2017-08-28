//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Engine
{
    using System;
    using System.Collections.Generic;
    
    public partial class Job
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Job()
        {
            this.ClusterCalculations = new HashSet<ClusterCalculation>();
            this.JobDocuments = new HashSet<JobDocument>();
            this.JobTerms = new HashSet<JobTerm>();
            this.ClusterJobDocuments = new HashSet<ClusterJobDocument>();
            this.ClusterJobTerms = new HashSet<ClusterJobTerm>();
            this.Clusters = new HashSet<Cluster>();
        }
    
        public int Id { get; set; }
        public int DocumentCount { get; set; }
        public System.DateTime Created { get; set; }
        public int Dimensions { get; set; }
        public Engine.JobStatus Status { get; set; }
        public Nullable<System.DateTime> Completed { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterCalculation> ClusterCalculations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JobDocument> JobDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JobTerm> JobTerms { get; set; }
        public virtual UMatrix UMatrix { get; set; }
        public virtual VMatrix VMatrix { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterJobDocument> ClusterJobDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterJobTerm> ClusterJobTerms { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cluster> Clusters { get; set; }
    }
}
