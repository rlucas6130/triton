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
    
    public partial class UMatrix
    {
        public int JobId { get; set; }
        public byte[] SerializedValues { get; set; }
    
        public virtual Job Job { get; set; }
    }
}
