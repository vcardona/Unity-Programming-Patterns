using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Flyweight design pattern main class
namespace FlyweightPattern
{
    public class Flyweight : MonoBehaviour
    {
        //The list that stores all aliens
        List<Alien> allAliens = new List<Alien>();

        List<Vector3> eyePositions;
        List<Vector3> legPositions;
        List<Vector3> armPositions;


        void Start()
        {
            //List used when flyweight is enabled
            eyePositions = GetBodyPartPositions();
            legPositions = GetBodyPartPositions();
            armPositions = GetBodyPartPositions();

            //Create all aliens
            for (int i = 0; i < 10000; i++)
            {
                Alien newAlien = new Alien();

                //Add eyes and leg positions
                //Without flyweight
                //newAlien.eyePositions = GetBodyPartPositions();
                //newAlien.armPositions = GetBodyPartPositions();
                //newAlien.legPositions = GetBodyPartPositions();

                //With flyweight
                newAlien.eyePositions = eyePositions;
                newAlien.armPositions = legPositions;
                newAlien.legPositions = armPositions;

                allAliens.Add(newAlien);
            }
        }


        //Generate a list with body part positions
        List<Vector3> GetBodyPartPositions()
        {
            //Create a new list
            List<Vector3> bodyPartPositions = new List<Vector3>();

            //Add body part positions to the list
            for (int i = 0; i < 1000; i++)
            {
                bodyPartPositions.Add(new Vector3());
            }

            return bodyPartPositions;
        }
    }
}
