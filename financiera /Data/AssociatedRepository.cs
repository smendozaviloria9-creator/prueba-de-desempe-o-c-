using System.Collections.Generic;
using System.Linq;
using Financiera.Data;
using financiera.Models;
using Financiera.Models;

namespace financiera.Data;

public class AssociatedRepository
{
    public void Insert(Associated associated)
    {
        InMemoryDatabase.Associateds.Add(associated);
    }
    public Associated? GetByDocument(string documentNumber)
    {
        return InMemoryDatabase.Associateds.FirstOrDefault(a => a.DocumentNumber == documentNumber && a.IsActive);
    }

    public List<Associated> GetByName(string namequery)
    {
        return InMemoryDatabase.Associateds.Where(a=>a.IsActive && a.FullName.ToLower().Contains(namequery.ToLower())).ToList();
    }
    public List<Associated> GetAll()
    {
        return InMemoryDatabase.Associateds.Where(a => a.IsActive).ToList();
    }

    public void Update(Associated associated)
    {
        var existing = GetByDocument(associated.DocumentNumber);
        if (existing != null)
        {
            existing.FullName = associated.FullName;
            existing.Phone = associated.Phone;
            existing.Address = associated.Address;
        }
    }

    public void Dlete(string documentNumber)
    {
        var existing = GetByDocument(documentNumber);
        if (existing != null)
            {
            existing.IsActive = false;
            }
    }
}