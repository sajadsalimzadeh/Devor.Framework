using System.ComponentModel;
using System.Reflection;
using Dorbit.Attributes;
using Dorbit.Database.Abstractions;
using Dorbit.Entities;
using Dorbit.Services.Abstractions;

namespace Dorbit.Services;

[ServiceRegister]
internal class LookupService : ILookupService
{
    private readonly IEnumerable<IDbContext> _dbContexts;

    public LookupService(IEnumerable<IDbContext> dbContexts)
    {
        _dbContexts = dbContexts;
    }

    public void Initiate()
    {
        foreach (var dbContext in _dbContexts)
        {
            using var transaction = dbContext.BeginTransaction();
            foreach (var type in dbContext.GetLookupEntities())
            {
                foreach (var value in Enum.GetValues(type))
                {
                    var name = Enum.GetName(type, value);
                    var enumValueMemberInfo = type.GetMember(name).FirstOrDefault(m => m.DeclaringType == type);
                    var descriptionAttr = enumValueMemberInfo.GetCustomAttribute<DescriptionAttribute>();
                    var rec = dbContext.DbSet<Lookup>().FirstOrDefault(x => x.Entity == type.Name && x.Key == name);
                    if (rec is null)
                    {
                            
                        dbContext.InsertEntityAsync(new Lookup()
                        {
                            Entity = type.Name,
                            Key = name,
                            Value = Convert.ToInt32(value),
                            DisplayName = descriptionAttr?.Description
                        });
                    }
                    else if(rec.DisplayName != descriptionAttr?.Description)
                    {
                        rec.DisplayName = descriptionAttr?.Description;
                        dbContext.UpdateEntityAsync(rec);
                    }
                }
            }
            transaction.Commit();
        }
    }
}