﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TeemaDBEntities : DbContext
    {
        public TeemaDBEntities()
            : base("name=TeemaDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Follow> Follows { get; set; }
        public virtual DbSet<NotificationGroupMember> NotificationGroupMembers { get; set; }
        public virtual DbSet<NotificationGroup> NotificationGroups { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<TeemaAccess> TeemaAccesses { get; set; }
        public virtual DbSet<Teema> Teemas { get; set; }
        public virtual DbSet<Thread> Threads { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Verification> Verifications { get; set; }
        public virtual DbSet<Vote> Votes { get; set; }
    }
}