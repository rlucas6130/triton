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
    
    public partial class JobDocument
    {
        public int JobId { get; set; }
        public int DocumentId { get; set; }
        public int OrdinalIndex { get; set; }
    
        public virtual Document Document { get; set; }
        public virtual Job Job { get; set; }
    }
}
