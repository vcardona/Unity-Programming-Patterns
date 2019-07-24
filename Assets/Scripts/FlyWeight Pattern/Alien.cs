using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlyweightPattern
{
    //Class that includes lists with position of body parts
    public class Alien
    {
        public List<Vector3> eyePositions;
        public List<Vector3> legPositions;
        public List<Vector3> armPositions;
    }
}
