using Microsoft.Restier.Core;
using Microsoft.Restier.Core.Submit;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace CloudNimble.RestierEssentials
{

    /// <summary>
    /// Handles submitting Entities through a DbContext when some entities are hidden from the user but not actually deleted.
    /// </summary>
    public class SoftDeleteSubmitExecutor : ISubmitExecutor
    {

        #region Static Members

        internal static string SoftDeletePropertyNameKey = "CloudNimble.Restier.SoftDelete.PropertyName";
        internal static string SoftDeleteAdminRoleKey = "CloudNimble.Restier.SoftDelete.AdminRole";

        #endregion

        #region Private Members

        private string SoftDeletePropertyName = string.Empty;
        private string AdminRole = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the SoftDeleteSubmitExecutor class.
        /// </summary>
        public SoftDeleteSubmitExecutor()
        {
            var config = GlobalConfiguration.Configuration;
            if (!config.Properties.TryGetValue(SoftDeletePropertyNameKey, out object propertyName))
            {
                throw new Exception("");
            }
            SoftDeletePropertyName = propertyName as string;

            // Checking for an admin role claim is optional.
            if (config.Properties.TryGetValue(SoftDeleteAdminRoleKey, out object adminRole))
            {
                AdminRole = adminRole as string;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes any potential soft-deletes and saves the changes to the resulting database.
        /// </summary>
        /// <param name="context">The <see cref="SubmitContext"/> to be processed.</param>
        /// <param name="cancellationToken">The Task's <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        public async Task<SubmitResult> ExecuteSubmitAsync(SubmitContext context, CancellationToken cancellationToken)
        {
            var dbContext = context.GetApiService<DbContext>();

            if (!string.IsNullOrWhiteSpace(AdminRole))
            {
                if (ClaimsPrincipal.Current.IsInRole(AdminRole))
                {
                    // If you're an admin, you can hard delete. If you need to soft-delete as an admin,
                    // you should change the Delete property yourself and update instead of delete.
                    return await SaveChanges(dbContext, context.ChangeSet, cancellationToken);
                }
            }

            foreach (var entry in context.ChangeSet.Entries)
            {
                // Are we deleting something?
                if (entry is DataModificationItem item && item.DataModificationItemAction == DataModificationItemAction.Remove)
                {
                    var entity = item.Resource;
                    var deletedProperty = entity.GetType().GetProperties().FirstOrDefault(p => p.Name == SoftDeletePropertyName);

                    // If Entity has the configured Deleted property, set value to True and update EntityState to Modified instead of Deleted
                    if (deletedProperty != null)
                    {
                        deletedProperty.SetValue(entity, true);
                        dbContext.Entry(entity).State = EntityState.Modified;
                    }
                }
            }

            return await SaveChanges(dbContext, context.ChangeSet, cancellationToken);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Commits the changes to the data to the database and returns a SubmitResult to be passed to the client.
        /// </summary>
        /// <param name="dbContext">The <see cref="DbContext"/> to save changes to.</param>
        /// <param name="changeSet">The Submit ChangeSet to process.</param>
        /// <param name="cancellationToken">The Task's <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        private async Task<SubmitResult> SaveChanges(DbContext dbContext, ChangeSet changeSet, CancellationToken cancellationToken)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new SubmitResult(changeSet);
        }

        #endregion

    }

}