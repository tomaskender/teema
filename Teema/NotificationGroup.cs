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
    
    public partial class NotificationGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NotificationGroup()
        {
            this.NotificationGroupMembers = new HashSet<NotificationGroupMember>();
        }
    
        public int Id { get; set; }
        public int TargetId { get; set; }
        public int EventType { get; set; }
        public int EventId { get; set; }
        public bool Seen { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotificationGroupMember> NotificationGroupMembers { get; set; }
        public virtual User User { get; set; }
    }
}