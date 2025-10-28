using Web.Models;

namespace Web.Service
{
    public interface ILogin
    {
        Customer? loginCustomer(string username, string password);
        Employee? loginEmployee(string username, string password);
    }
}