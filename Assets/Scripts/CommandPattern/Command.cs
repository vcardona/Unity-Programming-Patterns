using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandPattern
{
    //The parent class
    public abstract class Command
    {
        //How far should the box move when we press a button
        protected float moveDistance = 1f;

        //Move and maybe save command
        public abstract void Execute(Transform boxTrans, Command command);

        //Undo an old command
        public virtual void Undo(Transform boxTrans) { }

        //Move the box
        public virtual void Move(Transform boxTrans) { }
    }


    //
    // Child classes
    //

    public class MoveForward : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.forward * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.forward * moveDistance);
        }
    }


    public class MoveReverse : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.forward * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.forward * moveDistance);
        }
    }


    public class MoveLeft : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.right * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.right * moveDistance);
        }
    }


    public class MoveRight : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.right * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.right * moveDistance);
        }
    }


    //For keys with no binding
    public class DoNothing : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Nothing will happen if we press this key
        }
    }


    //Undo one command
    public class UndoCommand : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            List<Command> oldCommands = InputHandler.oldCommands;

            if (oldCommands.Count > 0)
            {
                Command latestCommand = oldCommands[oldCommands.Count - 1];

                //Move the box with this command
                latestCommand.Undo(boxTrans);

                //Remove the command from the list
                oldCommands.RemoveAt(oldCommands.Count - 1);
            }
        }
    }


    //Replay all commands
    public class ReplayCommand : Command
    {
        public override void Execute(Transform boxTrans, Command command)
        {
            InputHandler.shouldStartReplay = true;
        }
    }
}
