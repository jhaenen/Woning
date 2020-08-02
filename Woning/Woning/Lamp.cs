using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woning
{
    class Lamp {
        private uint _idx;
        private string _name;
        private bool _status;

        public Lamp(uint idx, string name) { _idx = idx; _name = name; }
        public void Switch(bool status) { _status = status; }
    }

    class DimmableLamp : Lamp {
        private uint _idx;
        private string _name;
        private bool _status;

        private uint _brightness;

        public DimmableLamp(uint idx, string name) : base(idx, name) { _idx = idx; _name = name; }

        public void SetBrightness(uint brightness) { _brightness = brightness; }
    }
    
    class ColorLamp : DimmableLamp {
        private uint _idx;
        private string _name;
        private bool _status;

        private uint _brightness;
        private float[] _color;

        public ColorLamp(uint idx, string name) : base(idx, name) { _idx = idx; _name = name; _color = new float[3]; }

        public void SetColor(float r, float g, float b) { _color[0] = r; _color[1] = g; _color[2] = b; }
    }
}
