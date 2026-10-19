using System.Threading.Tasks;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}