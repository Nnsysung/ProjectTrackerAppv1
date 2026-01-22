using Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{


    public class ProjectTrackerAppContext : DbContext, IDbContext
    {
        public ProjectTrackerAppContext(DbContextOptions<ProjectTrackerAppContext> options) : base(options)
        {

        }
        #region Tables
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
        public int SaveChanges(string userName, string IP)
        {
            try
            {

                // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
                var changedEntity = this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added ||
                                        p.State == EntityState.Deleted ||
                                        p.State == EntityState.Modified).ToList();

                foreach (var ent in changedEntity)
                {
                    // For each changed record, get the audit record entries and add them
                    var auditRecordsForChange = GetAuditRecordsForChange(ent, userName, IP);

                    foreach (AuditLog x in auditRecordsForChange)
                    {
                        this.AuditLogs.Add(x);
                    }
                }

                // Call the original SaveChanges(), which will save both the changes made and the audit records
                return base.SaveChanges();
            }
            catch (Exception x)
            {
                throw;
                //return 0;
            }
        }

        public async Task<int> SaveChangesAsync(string userName, string IP, CancellationToken token = default)
        {

            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            var changedEntity = this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added ||
                                                                        p.State == EntityState.Deleted ||
                                                                        p.State == EntityState.Modified).ToList();

            foreach (var ent in changedEntity)
            {
                // For each changed record, get the audit record entries and add them
                var auditRecordsForChange = GetAuditRecordsForChange(ent, userName, IP);

                foreach (var x in auditRecordsForChange)
                {
                    this.AuditLogs.Add(x);
                }
            }

            // Call the original SaveChangesAsync(), which will save both the changes made and the audit records
            return await base.SaveChangesAsync(token);

        }




        private List<AuditLog> GetAuditRecordsForChange(EntityEntry dbEntry, string userId, string Ip)
        {
            List<AuditLog> result = new List<AuditLog>();

            DateTime changeTime = DateTime.UtcNow;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            if (dbEntry != null)
            {
                string keyName = dbEntry.Entity.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;
                //string keyName = dbEntry.Entity.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Count() < 0).Name;
                if (dbEntry.State == EntityState.Added)
                {
                    // For Inserts, just add the whole record
                    // If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()
                    var a = new AuditLog();
                    a.Id = Guid.NewGuid();
                    a.UserId = userId;
                    a.EventDateUtc = changeTime;
                    a.EventType = "A"; // Added
                    a.TableName = tableName;
                    a.Ip = Ip;

                    //RecordId = dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
                    //var ri= dbEntry.CurrentValues.GetValue<string>(keyName);
                    // var ri = dbEntry.CurrentValues[keyName];
                    // var r = dbEntry.Property(keyName).CurrentValue;
                    a.RecordId = dbEntry.Property(keyName).CurrentValue.ToString();// r.ToString();// dbEntry.CurrentValues.GetValue<string>(keyName);  // Again, adjust this if you have a multi-column key
                                                                                   //"Couldnt get this, will comeback to this",
                    a.ColumnName = "*ALL";   // Or make it nullable, whatever you want
                                             //NewValue = (dbEntry.CurrentValues.ToObject() is IDescribableEntity) ? (dbEntry.CurrentValues.ToObject() as IDescribableEntity).Describe() : dbEntry.CurrentValues.ToObject().ToString()
                    a.NewValue = JsonConvert.SerializeObject(dbEntry.CurrentValues.ToObject());// "NewEntityEntry";
                    result.Add(a);

                }
                else if (dbEntry.State == EntityState.Deleted)
                {
                    // Same with deletes, do the whole record, and use either the description from Describe() or ToString()
                    result.Add(new AuditLog()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        EventDateUtc = changeTime,
                        EventType = "D", // Deleted
                        TableName = tableName,
                        RecordId = dbEntry.OriginalValues[keyName].ToString(),// dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                        ColumnName = "*ALL",
                        Ip = Ip,
                        NewValue = JsonConvert.SerializeObject(dbEntry.OriginalValues.ToObject())// (dbEntry.OriginalValues.ToObject() is IDescribableEntity) ? (dbEntry.OriginalValues.ToObject() as IDescribableEntity).Describe() : dbEntry.OriginalValues.ToObject().ToString()
                    });
                }
                else if (dbEntry.State == EntityState.Modified)
                {
                    foreach (var propertyName in dbEntry.OriginalValues.Properties)
                    {
                        //var ri = dbEntry.CurrentValues[keyName];
                        //var r = dbEntry.Property(keyName).OriginalValue;
                        // For updates, we only want to capture the columns that actually changed
                        //if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
                        //{ 
                        //var o = dbEntry.OriginalValues[propertyName].ToString();
                        //var n = dbEntry.CurrentValues[propertyName].ToString();
                        if (!object.Equals(dbEntry.OriginalValues[propertyName], dbEntry.CurrentValues[propertyName]))
                        {
                            result.Add(new AuditLog()
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                EventDateUtc = changeTime,
                                EventType = "M",    // Modified
                                TableName = tableName,
                                RecordId = dbEntry.OriginalValues[keyName].ToString(),// dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                                ColumnName = propertyName.Name,
                                Ip = Ip,
                                OriginalValue = dbEntry.OriginalValues[propertyName] == null ? null : dbEntry.OriginalValues[propertyName].ToString(),
                                NewValue = dbEntry.CurrentValues[propertyName] == null ? null : dbEntry.CurrentValues[propertyName].ToString()

                            });
                        }
                    }
                }
            }
            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities

            return result;
        }
    }
}
