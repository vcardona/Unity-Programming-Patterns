using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubclassSandbox
{
    //This is the base class
    public abstract class Superpower
    {
        //This is the sandbox method that a subclass has to have its own version of
        public abstract void Activate();

        //All of the operations a derived class needs to perform - called from Activate()
        protected void Move(float speed)
        {
            Debug.Log("Moving with speed " + speed);
        }

        protected void PlaySound(string coolSound)
        {
            Debug.Log("Playing sound " + coolSound);
        }

        protected void SpawnParticles()
        {

        }
    }


    //Subclasses
    public class SkyLaunch : Superpower
    {
        //Has to have its own version of Activate()
        public override void Activate()
        {
            //Add operations this class has to perform
            Move(10f);
            PlaySound("SkyLaunch");
            SpawnParticles();
        }
    }

    public class GroundDive : Superpower
    {
        //Has to have its own version of Activate()
        public override void Activate()
        {
            //Add operations this class has to perform
            Move(15f);
            PlaySound("GroundDive");
            SpawnParticles();
        }
    }
}
