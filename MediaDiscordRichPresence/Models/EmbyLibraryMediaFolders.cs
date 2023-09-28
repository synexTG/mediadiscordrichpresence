using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaDiscordRichPresence.Models;
public class EmbyLibraryMediaFolders
{

    public class Rootobject
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Guid { get; set; }
        public Subfolder[] SubFolders { get; set; }
    }

    public class Subfolder
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
    }

}
