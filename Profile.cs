using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Profile
{
    public string? ProfileBrowserVersion { get; set; }
    public string? SimulatedOperatingSystem { get; set; }
    public string? ProfileName { get; set; }
    public string? BrowserId { get; set; }
    public string? ProfileNotes { get; set; }
    public string? ProfileGroup { get; set; }
    public DateTime ProfileLastEdited { get; set; }
}

public class RootObject
{
    public Profile? Profile { get; set; }
}


