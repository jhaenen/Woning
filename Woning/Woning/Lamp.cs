using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Woning {
    class Lamp {
        public uint IDX { get; internal set; }
        public string Name { get; internal set; }
        public bool Status { get; internal set; }
        public string ImageUri { get; internal set; }

        public bool Dimmable { get; internal set; }
        public bool ColorLamp { get; internal set; }
        public uint Brightness { get; set; }
        public float[] Color { get; set; }

        public Lamp(uint idx, string name, string status, bool dimmable, bool colorLamp) {
            IDX = idx;
            Name = name;
            Color = new float[3];
            Dimmable = dimmable;
            ColorLamp = colorLamp;
            if (status == "Off") {
                ImageUri = "Images/lamp-off.svg";
                Status = false;
            } else {
                ImageUri = "Images/lamp-on.svg";
                Status = true;

                if(dimmable) {
                    Brightness = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                }
            }
        }

        public void Switch(bool status) {
            Status = status;
            if(status) ImageUri = "Images/lamp-on.svg";
            else ImageUri = "Images/lamp-off.svg";
        }
        public void SetColor(float r, float g, float b) { if (ColorLamp) { Color[0] = r; Color[1] = g; Color[2] = b; } }

        public void SetStatus(string status) {
            if (status == "Off") {
                if (Status) {
                    ImageUri = "Images/lamp-off.svg";
                    Status = false;
                    if (Dimmable) Brightness = 0;
                    Debug.WriteLine(IDX + " has turned off");
                }
            } else {
                if (Dimmable) {
                    uint _tmpBright = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                    if(!Status || Brightness != _tmpBright) {
                        ImageUri = "Images/lamp-on.svg";
                        Status = true;
                        Brightness = _tmpBright;
                        Debug.WriteLine(IDX + " has turned on or changed brighntess");
                    }
                } else {
                    if (!Status) {
                        ImageUri = "Images/lamp-on.svg";
                        Status = true;
                        Debug.WriteLine(IDX + " has turned on");
                    }
                }
            }
        }
    }
}
