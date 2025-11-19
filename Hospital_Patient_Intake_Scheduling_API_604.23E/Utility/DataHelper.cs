using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Utility

{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            await dbContextSvc.Database.MigrateAsync();
        }
    }
}