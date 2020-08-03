using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woning {    
    class Lamp {
        public uint IDX { get; internal set; }
        public string Name { get; internal set; }
        public bool Status { get; set; }

        public bool Dimmable { get; internal set; }
        public bool ColorLamp { get; internal set; }
        public uint Brightness { get; set; }
        public float[] Color { get; set; }

        public Lamp(uint idx, string name) { IDX = idx; Name = name; Color = new float[3]; Dimmable = false; ColorLamp = false; }
        public Lamp(uint idx, string name, bool dimmable) { IDX = idx; Name = name; Color = new float[3]; Dimmable = dimmable; ColorLamp = false; }
        public Lamp(uint idx, string name, bool dimmable, bool colorLamp) { IDX = idx; Name = name; Color = new float[3]; Dimmable = dimmable; ColorLamp = colorLamp; }

        public void SetColor(float r, float g, float b) { if (ColorLamp) { Color[0] = r; Color[1] = g; Color[2] = b; } }
    }
}
