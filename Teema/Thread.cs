//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Teema
{
    using System;
    using System.Collections.Generic;
    
    public partial class Thread
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Thread()
        {
            this.Posts = new HashSet<Post>();
        }
    
        public int Id { get; set; }
        public string LinkId { get; set; }
        public string Name { get; set; }
        public System.DateTime Created { get; set; }
        public int TeemaId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Post> Posts { get; set; }
        public virtual Teema Teema { get; set; }
    }
}
