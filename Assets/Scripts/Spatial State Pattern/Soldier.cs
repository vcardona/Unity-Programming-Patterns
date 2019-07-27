using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    //The soldier base class for enemies and friendly
    public class Soldier
    {
        //To change material
        public MeshRenderer soldierMeshRenderer;
        //To move the soldier
        public Transform soldierTrans;
        //The speed the soldier is walking with
        protected float walkSpeed;
        //Has to do with the grid, so we can avoid storing all soldiers in an array
        //Instead we are going to use a linked list where all soldiers in the cell 
        //Are linked to each other
        public Soldier previousSoldier;
        public Soldier nextSoldier;

        //The enemy doesnt need any outside information
        public virtual void Move()
        { }

        //The friendly has to move which soldier is the closest
        public virtual void Move(Soldier soldier)
        { }
    }
}
