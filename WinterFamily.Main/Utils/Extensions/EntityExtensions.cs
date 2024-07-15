using Microsoft.EntityFrameworkCore;

namespace WinterFamily.Main.Utils.Extensions;

internal static class EntityExtensions
{
    public static void Clear<T>(this DbSet<T> dbSet) where T : class
    {
        dbSet.RemoveRange(dbSet);
    }
}
