using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubclassSandbox
{
    public class GameController : MonoBehaviour
    {
        //A list that will store all superpowers
        List<Superpower> superPowers = new List<Superpower>();

        void Start()
        {
            superPowers.Add(new SkyLaunch());
            superPowers.Add(new GroundDive());
        }

        void Update()
        {
            //Activate each superpower each update
            for (int i = 0; i < superPowers.Count; i++)
            {
                superPowers[i].Activate();
            }
        }
    }
}
