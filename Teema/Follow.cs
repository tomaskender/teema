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
    
    public partial class Follow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public int FollowedId { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual User FollowedUser { get; set; }
        public virtual User FollowerUser { get; set; }
    }
}
