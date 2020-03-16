using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Player : IPlayer
{
    private string _id;
    private string _name;

    public Player(string id, string name)
    {
        _id = id;
        _name = name;
    }

    public string GetId()
    {
        return _id;
    }

    public string GetName()
    {
        return _name;
    }
}
