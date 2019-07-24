using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandPattern
{

    public class InputHandler : MonoBehaviour
    {
        //The box we control with keys
        public Transform boxTrans;
        //The different keys we need
        private Command buttonW, buttonS, buttonA, buttonD, buttonB, buttonZ, buttonR;
        //Stores all commands for replay and undo
        public static List<Command> oldCommands = new List<Command>();
        //Box start position to know where replay begins
        private Vector3 boxStartPos;
        //To reset the coroutine
        private Coroutine replayCoroutine;
        //If we should start the replay
        public static bool shouldStartReplay;
        //So we cant press keys while replaying
        private bool isReplaying;


        void Start()
        {
            //Bind keys with commands
            buttonB = new DoNothing();
            buttonW = new MoveForward();
            buttonS = new MoveReverse();
            buttonA = new MoveLeft();
            buttonD = new MoveRight();
            buttonZ = new UndoCommand();
            buttonR = new ReplayCommand();

            boxStartPos = boxTrans.position;
        }



        void Update()
        {
            if (!isReplaying)
            {
                HandleInput();
            }

            StartReplay();
        }


        //Check if we press a key, if so do what the key is binded to 
        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                buttonA.Execute(boxTrans, buttonA);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                buttonB.Execute(boxTrans, buttonB);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                buttonD.Execute(boxTrans, buttonD);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                buttonR.Execute(boxTrans, buttonZ);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                buttonS.Execute(boxTrans, buttonS);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                buttonW.Execute(boxTrans, buttonW);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                buttonZ.Execute(boxTrans, buttonZ);
            }
        }


        //Checks if we should start the replay
        void StartReplay()
        {
            if (shouldStartReplay && oldCommands.Count > 0)
            {
                shouldStartReplay = false;

                //Stop the coroutine so it starts from the beginning
                if (replayCoroutine != null)
                {
                    StopCoroutine(replayCoroutine);
                }

                //Start the replay
                replayCoroutine = StartCoroutine(ReplayCommands(boxTrans));
            }
        }


        //The replay coroutine
        IEnumerator ReplayCommands(Transform boxTrans)
        {
            //So we can't move the box with keys while replaying
            isReplaying = true;

            //Move the box to the start position
            boxTrans.position = boxStartPos;

            for (int i = 0; i < oldCommands.Count; i++)
            {
                //Move the box with the current command
                oldCommands[i].Move(boxTrans);

                yield return new WaitForSeconds(0.3f);
            }

            //We can move the box again
            isReplaying = false;
        }
    }
}

