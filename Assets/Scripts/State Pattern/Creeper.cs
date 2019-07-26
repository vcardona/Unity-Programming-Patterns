using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatePattern
{
    //The creeper class
    public class Creeper : Enemy
    {
        EnemyFSM creeperMode = EnemyFSM.Stroll;

        float health = 100f;


        public Creeper(Transform creeperObj)
        {
            base.enemyObj = creeperObj;
        }


        //Update the creeper's state
        public override void UpdateEnemy(Transform playerObj)
        {
            //The distance between the Creeper and the player
            float distance = (base.enemyObj.position - playerObj.position).magnitude;

            switch (creeperMode)
            {
                case EnemyFSM.Attack:
                    if (health < 20f)
                    {
                        creeperMode = EnemyFSM.Flee;
                    }
                    else if (distance > 2f)
                    {
                        creeperMode = EnemyFSM.MoveTowardsPlayer;
                    }
                    break;
                case EnemyFSM.Flee:
                    if (health > 60f)
                    {
                        creeperMode = EnemyFSM.Stroll;
                    }
                    break;
                case EnemyFSM.Stroll:
                    if (distance < 10f)
                    {
                        creeperMode = EnemyFSM.MoveTowardsPlayer;
                    }
                    break;
                case EnemyFSM.MoveTowardsPlayer:
                    if (distance < 1f)
                    {
                        creeperMode = EnemyFSM.Attack;
                    }
                    else if (distance > 15f)
                    {
                        creeperMode = EnemyFSM.Stroll;
                    }
                    break;
            }

            //Move the enemy based on a state
            DoAction(playerObj, creeperMode);
        }
    }
}
