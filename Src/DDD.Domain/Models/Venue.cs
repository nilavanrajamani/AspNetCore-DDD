using System;

using DDD.Domain.Core.Models;

namespace DDD.Domain.Models;

public class Venue : EntityAudit
{
    public Venue(Guid id, string name, string address, int capacity)
    {
        Id = id;
        Name = name;
        Address = address;
        Capacity = capacity;
        CreatedAt = DateTime.UtcNow;
    }

    // Empty constructor for EF
    protected Venue()
    {
    }

    public string Name { get; private set; }

    public string Address { get; private set; }

    public int Capacity { get; private set; }

    public void UpdateDetails(string name, string address, int capacity)
    {
        Name = name;
        Address = address;
        Capacity = capacity;
        UpdatedAt = DateTime.UtcNow;
    }
}